using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.Processing;

namespace UnityEngine.XR.Hands.ProviderImplementation
{
    /// <summary>
    /// Methods to implement by the implementing provider for an <see cref="XRHandSubsystem"/>.
    /// </summary>
    public abstract class XRHandSubsystemProvider
        : SubsystemProvider<XRHandSubsystem>
    {
        /// <summary>
        /// Gets the layout of hand joints for this provider, by having the
        /// provider mark each index corresponding to a <see cref="XRHandJointID"/>
        /// get marked as <c>true</c> if the provider attempts to track
        /// that joint.
        /// </summary>
        /// <remarks>
        /// Called once on creation so that before the subsystem is even started,
        /// the user can immediately create a valid hierarchical structure as
        /// soon as they get a reference to the subsystem without even needing to
        /// start it. This is called before any call to
        /// <see cref="GetFingerShapeConfiguration"/>.
        /// </remarks>
        /// <param name="handJointsInLayout">
        /// Each index corresponds to a <see cref="XRHandJointID"/>. For each
        /// joint that the provider will attempt to track, mark that spot as
        /// <c>true</c> by calling <c>.ToIndex()</c> on that ID.
        /// </param>
        public abstract void GetHandLayout(NativeArray<bool> handJointsInLayout);

        /// <summary>
        /// Gets the <see cref="XRFingerShapeConfiguration"/> on the current
        /// device for the given <see cref="XRHandFingerID"/>.
        /// </summary>
        /// <remarks>
        /// Called once for each finger on creation so that the subsystem will
        /// always have valid configurations to base detection math off of. If
        /// the provider does not override this, defaults will be reported -
        /// this means that if the device is more constrained in reporting joint
        /// data than the defaults, gestures and poses may not be detected correctly.
        /// Called after <see cref="GetHandLayout"/> for each finger, but before
        /// the subsystem is returned during a call to
        /// <see cref="XRHandSubsystemDescriptor.Create"/>.
        /// </remarks>
        /// <param name="fingerID">
        /// Which finger to get the <see cref="XRFingerShapeConfiguration"/> for.
        /// </param>
        /// <returns>
        /// A populated <see cref="XRFingerShapeConfiguration"/> representing
        /// range of motion for the given <see cref="XRHandFingerID"/>.
        /// </returns>
        public virtual XRFingerShapeConfiguration GetFingerShapeConfiguration(XRHandFingerID fingerID)
            => FingerConfigDefaults.configurations[fingerID.ToIndex()];

        /// <summary>
        /// Attempts to retrieve current hand-tracking data from the provider.
        /// </summary>
        /// <param name="updateType">
        /// Informs the provider which kind of timing the update is being
        /// requested under.
        /// </param>
        /// <param name="leftHandRootPose">
        /// Update this and include <c>XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose</c>
        /// in the return value to update the left hand's root pose.
        /// </param>
        /// <param name="leftHandJoints">
        /// Array of hand joints to fill out for the left hand. These are
        /// initialized with a copy of the current joint data for the left hand,
        /// so if the last known tracking data for a particular joint is still
        /// fine, you don't need to fill out that data again. If you update
        /// these, include <c>XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints</c>
        /// in the return value to have the changes reflected in the subsystem.
        /// </param>
        /// <param name="rightHandRootPose">
        /// Update this and include <c>XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose</c>
        /// in the return value to update the right hand's root pose.
        /// </param>
        /// <param name="rightHandJoints">
        /// Array of hand joints to fill out for the right hand. These are
        /// initialized with a copy of the current joint data for the right hand,
        /// so if the last known tracking data for a particular joint is still
        /// fine, you don't need to fill out that data again. If you update
        /// these, include <c>XRHandSubsystem.UpdateSuccessFlags.RightHandJoints</c>
        /// in the return value to have the changes reflected in the subsystem.
        /// </param>
        /// <returns>
        /// Returns <see cref="XRHandSubsystem.UpdateSuccessFlags"/> to describe which tracking
        /// data was successfully updated.
        /// </returns>
        public abstract XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints);

        /// <summary>
        /// Whether the provider is currently able to surface data from any of <see cref="TryGetAimPose"/>,
        /// <see cref="TryGetAimActivateValue"/>, <see cref="TryGetGraspValue"/>, <see cref="TryGetGripPose"/>,
        /// <see cref="TryGetPinchPose"/>, <see cref="TryGetPinchValue"/>, or <see cref="TryGetPinchPose"/>.
        /// </summary>
        public virtual bool canSurfaceCommonPoseData => false;

        /// <summary>
        /// Gets the aim pose. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsAimPose"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="aimPose">
        /// The pose to update the aim pose to, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the aim pose was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetAimPose(Handedness handedness, out Pose aimPose)
        {
            aimPose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Gets the aim activate value. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsAimActivateValue"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="aimActivateValue">
        /// The aim activate value, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the aim activate value was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetAimActivateValue(Handedness handedness, out float aimActivateValue)
        {
            aimActivateValue = 0f;
            return false;
        }

        /// <summary>
        /// Gets whether the aim is fully activated.
        /// Will only be called if <see cref="XRHandSubsystemDescriptor.supportsAimActivateValue"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the gesture might not be available when you call this function. When data is available,
        /// the function returns <c>true</c> and sets <paramref name="isAimActivated"/> to indicate
        /// whether the aim gesture is fully activated. If this function returns <c>false</c>,
        /// <paramref name="isAimActivated"/> will be <c>false</c> whether or not the aim gesture is activated.
        /// </remarks>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="isAimActivated">
        /// Will be set to <c>true</c> if aim is fully activated;
        /// otherwise, <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the <paramref name="isAimActivated"/> value was
        /// set based on valid data. Returns <c>false</c> if the state of the user's hand wasn't available.
        /// </returns>
        public virtual bool TryGetAimActivatedState(Handedness handedness, out bool isAimActivated)
        {
            isAimActivated = false;
            return false;
        }

        /// <summary>
        /// Gets the grasp value. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsGraspValue"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="graspValue">
        /// The grasp value, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the grasp value was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetGraspValue(Handedness handedness, out float graspValue)
        {
            graspValue = 0f;
            return false;
        }

        /// <summary>
        /// Gets whether the user is making a fist.
        /// Will only be called if <see cref="XRHandSubsystemDescriptor.supportsGraspValue"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the gesture might not be available when you call this function. When data is available,
        /// the function returns <c>true</c> and sets <paramref name="isGraspFirm"/> to indicate
        /// whether the user is making a fist (firm grasp). If this function returns <c>false</c>,
        /// <paramref name="isGraspFirm"/> will also be <c>false</c> (whether or not the user is making a fist).
        /// </remarks>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="isGraspFirm">
        /// Will be set to <c>true</c> if the user is making a fist,
        /// otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the <paramref name="isGraspFirm"/> value was
        /// set based on valid data. Returns <c>false</c> if the state of the user's hand wasn't available.
        /// </returns>
        public virtual bool TryGetGraspFirmState(Handedness handedness, out bool isGraspFirm)
        {
            isGraspFirm = false;
            return false;
        }

        /// <summary>
        /// Gets the grip pose. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsGripPose"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="gripPose">
        /// The pose to update the aim pose to, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the grip pose was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetGripPose(Handedness handedness, out Pose gripPose)
        {
            gripPose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Gets the pinch pose. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsPinchPose"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="pinchPose">
        /// The pose to update the pinch pose to, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the pinch pose was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetPinchPose(Handedness handedness, out Pose pinchPose)
        {
            pinchPose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Gets the pinch value. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsPinchValue"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="pinchValue">
        /// The pinch value, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the grasp value was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetPinchValue(Handedness handedness, out float pinchValue)
        {
            pinchValue = 0f;
            return false;
        }

        /// <summary>
        /// Gets whether the hand is performing a pinch gesture.
        /// Will only be called if <see cref="XRHandSubsystemDescriptor.supportsPinchValue"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the gesture might not be available when you call this function. When data is available,
        /// the function returns <c>true</c> and sets <paramref name="isPinched"/> to indicate
        /// whether the hand is currently pinching. If this function returns <c>false</c>,
        /// <paramref name="isPinched"/> will be <c>false</c> whether or not the hand is pinching.
        /// </remarks>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="isPinched">
        /// Will be set to <c>true</c> if the user is pinching;
        /// otherwise, <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the <paramref name="isPinched"/> value was
        /// set based on valid data. Returns <c>false</c> if the state of the user's hand wasn't available.
        /// </returns>
        public virtual bool TryGetPinchTouchedState(Handedness handedness, out bool isPinched)
        {
            isPinched = false;
            return false;
        }

        /// <summary>
        /// Gets the poke pose. Will only be called if <see cref="XRHandSubsystemDescriptor.supportsPokePose"/>
        /// is enabled.
        /// </summary>
        /// <param name="handedness">
        /// Which hand to retrieve data for.
        /// </param>
        /// <param name="pokePose">
        /// The pose to update the poke pose to, if available. Will not be used if
        /// <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and the poke pose was
        /// filled out, returns <c>false</c> otherwise.
        /// </returns>
        public virtual bool TryGetPokePose(Handedness handedness, out Pose pokePose)
        {
            pokePose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Attempt to retrieve hand mesh data from the platform. Only called when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <param name="result">
        /// Output data for hand meshes.
        /// </param>
        /// <param name="queryParams">
        /// Input data for hand meshes.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful and either hand has
        /// valid data. Otherwise, returns <c>false</c>.
        /// </returns>
        public virtual bool TryGetMeshData(ref XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams)
            => false;

        /// <summary>
        /// Describes which version of authored hand meshes is detected for use.
        /// </summary>
        public virtual XRDetectedHandMeshLayout detectedHandMeshLayout => XRDetectedHandMeshLayout.OpenXRMetaQuest;

        /// <summary>
        /// Attempts to get aim state for the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// Denotes the hand you wish to obtain aim state for.
        /// </param>
        /// <param name="aimState">
        /// If <c>TryGetAimState</c> returns <see langword="true"/>, this will be
        /// filled out with aim state data for the given <see cref="Handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if <c>TryGetAimState</c> succeeds and
        /// fills out aim state for the given <see cref="Handedness"/>.
        /// </returns>
        public virtual bool TryGetAimState(Handedness handedness, out XRHandAimState aimState)
        {
            aimState = default;
            return false;
        }

        /// <summary>
        /// Method that determines whether joint processing is allowed for this provider.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="XRHandSubsystem"/> should allow <see cref="IXRHandProcessor"/>
        /// to process joints.</returns>
        protected internal virtual bool AllowJointProcessing() => true;

        /// <summary>
        /// Allows the provider to subscribe to subsystem actions.
        /// </summary>
        /// <param name="actions">The actions structure provided by the subsystem for the provider to subscribe to.</param>
        internal virtual void SubscribeToSubsystemActions(ref XRHandSubsystemActions actions)
        {
            // Do nothing by default
        }

        // these defaults were captured using a Meta Quest 2
        internal static class FingerConfigDefaults
        {
            internal static XRFingerShapeConfiguration[] configurations => s_Configurations;
            internal static XRFingerShapeConfigurationState[] configurationStates => s_ConfigurationStates;

            static FingerConfigDefaults()
            {
                s_ConfigurationStates = new XRFingerShapeConfigurationState[]
                {
                    // XRHandFingerID.Thumb
                    new XRFingerShapeConfigurationState
                    {
                        minimumFullCurlDegrees1 = 132f,
                        maximumFullCurlDegrees1 = 162f,
                        minimumFullCurlDegrees2 = 129f,
                        maximumFullCurlDegrees2 = 180f,
                        minimumFullCurlDegrees3 = -1f,
                        maximumFullCurlDegrees3 = -1f,

                        minimumBaseCurlDegrees = 2f,
                        maximumBaseCurlDegrees = 60f,

                        minimumTipCurlDegrees1 = 119f,
                        maximumTipCurlDegrees1 = 169f,
                        minimumTipCurlDegrees2 = 129f,
                        maximumTipCurlDegrees2 = 180f,

                        minimumPinchDistance = -1f,
                        maximumPinchDistance = -1f,

                        minimumSpreadDegrees = 3f,
                        maximumSpreadDegrees = 57f,
                    },
                    // XRHandFingerID.Index
                    new XRFingerShapeConfigurationState
                    {
                        minimumFullCurlDegrees1 = 102f,
                        maximumFullCurlDegrees1 = 180f,
                        minimumFullCurlDegrees2 = 90f,
                        maximumFullCurlDegrees2 = 174f,
                        minimumFullCurlDegrees3 = 120f,
                        maximumFullCurlDegrees3 = 180f,

                        minimumBaseCurlDegrees = 102f,
                        maximumBaseCurlDegrees = 180f,

                        minimumTipCurlDegrees1 = 90f,
                        maximumTipCurlDegrees1 = 174f,
                        minimumTipCurlDegrees2 = 120f,
                        maximumTipCurlDegrees2 = 180f,

                        minimumPinchDistance = 0.01f,
                        maximumPinchDistance = 0.045f,

                        minimumSpreadDegrees = 3f,
                        maximumSpreadDegrees = 18f,
                    },
                    // XRHandFingerID.Middle
                    new XRFingerShapeConfigurationState
                    {
                        minimumFullCurlDegrees1 = 92f,
                        maximumFullCurlDegrees1 = 180f,
                        minimumFullCurlDegrees2 = 90f,
                        maximumFullCurlDegrees2 = 174f,
                        minimumFullCurlDegrees3 = 116f,
                        maximumFullCurlDegrees3 = 180f,

                        minimumBaseCurlDegrees = 92f,
                        maximumBaseCurlDegrees = 180f,

                        minimumTipCurlDegrees1 = 90f,
                        maximumTipCurlDegrees1 = 174f,
                        minimumTipCurlDegrees2 = 116f,
                        maximumTipCurlDegrees2 = 180f,

                        minimumPinchDistance = 0.01f,
                        maximumPinchDistance = 0.045f,

                        minimumSpreadDegrees = 3f,
                        maximumSpreadDegrees = 20f,
                    },
                    // XRHandFingerID.Ring
                    new XRFingerShapeConfigurationState
                    {
                        minimumFullCurlDegrees1 = 90f,
                        maximumFullCurlDegrees1 = 180f,
                        minimumFullCurlDegrees2 = 90f,
                        maximumFullCurlDegrees2 = 174f,
                        minimumFullCurlDegrees3 = 112f,
                        maximumFullCurlDegrees3 = 180f,

                        minimumBaseCurlDegrees = 95f,
                        maximumBaseCurlDegrees = 180f,

                        minimumTipCurlDegrees1 = 90f,
                        maximumTipCurlDegrees1 = 174f,
                        minimumTipCurlDegrees2 = 112f,
                        maximumTipCurlDegrees2 = 180f,

                        minimumPinchDistance = 0.01f,
                        maximumPinchDistance = 0.045f,

                        minimumSpreadDegrees = 3f,
                        maximumSpreadDegrees = 20f,
                    },
                    // XRHandFingerID.Little
                    new XRFingerShapeConfigurationState
                    {
                        minimumFullCurlDegrees1 = 90f,
                        maximumFullCurlDegrees1 = 180f,
                        minimumFullCurlDegrees2 = 90f,
                        maximumFullCurlDegrees2 = 164f,
                        minimumFullCurlDegrees3 = 116f,
                        maximumFullCurlDegrees3 = 180f,

                        minimumBaseCurlDegrees = 95f,
                        maximumBaseCurlDegrees = 180f,

                        minimumTipCurlDegrees1 = 90f,
                        maximumTipCurlDegrees1 = 164f,
                        minimumTipCurlDegrees2 = 116f,
                        maximumTipCurlDegrees2 = 180f,

                        minimumPinchDistance = 0.01f,
                        maximumPinchDistance = 0.045f,

                        minimumSpreadDegrees = -1f,
                        maximumSpreadDegrees = -1f,
                    },
                };

                s_Configurations = new XRFingerShapeConfiguration[Constants.k_NumFingersPerHand];
                foreach (var fingerID in HandsUtility.validFingerIDs)
                {
                    int fingerIndex = fingerID.ToIndex();
                    s_Configurations[fingerIndex] = new XRFingerShapeConfiguration(s_ConfigurationStates[fingerIndex]);
                }
            }

            static readonly XRFingerShapeConfiguration[] s_Configurations;
            static readonly XRFingerShapeConfigurationState[] s_ConfigurationStates;
        }
    }
}
