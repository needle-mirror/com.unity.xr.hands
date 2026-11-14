#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.OpenXR.Meshing;
using UnityEngine.XR.Hands.ProviderImplementation;
using UnityEngine.XR.OpenXR;

#if UNITY_OPENXR_PACKAGE_1_8
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

#if OPENXR_1_19_0_PRE_2_OR_NEWER
using UnityEngine.XR.OpenXR.NativeTypes;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Hand tracking provider for the OpenXR platform.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    [Preserve]
    public unsafe class OpenXRHandProvider : XRHandSubsystemProvider
    {
        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Start"/>.
        /// </summary>
        public override void Start()
        {
#if UNITY_OPENXR_PACKAGE_1_8
            m_ApplyMetaAimRotationOffset = OpenXRRuntime.name == "Oculus";
#endif
        }

        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Stop"/>.
        /// </summary>
        public override void Stop()
        {
        }

        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Destroy"/>.
        /// </summary>
        public override void Destroy() => NativeApi.Destroy();

        /// <inheritdoc/>
        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            if (!NativeApi.TryInitialize())
            {
                Debug.LogError("OpenXR hand provider failed to initialize - no data will be tracked or surfaced!");
                return;
            }

            handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;

            m_IsValid = true;
        }

        /// <inheritdoc/>
        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints)
        {
            if (!m_IsValid)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            var successFlags = NativeApi.TryUpdateHands(
                updateType,
                ref leftHandRootPose,
                leftHandJoints.GetUnsafePtr(),
                ref rightHandRootPose,
                rightHandJoints.GetUnsafePtr());

            if (s_MetaAim != null && updateType == XRHandSubsystem.UpdateType.Dynamic)
            {
                s_MetaAim.OnUpdatedHandsInProvider(successFlags);
                s_MetaAim.FlushMetaAimChanges();

                const XRHandSubsystem.UpdateSuccessFlags leftSuccessFlags = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                const XRHandSubsystem.UpdateSuccessFlags rightSuccessFlags = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;

                var indexLeft = Handedness.Left.ToIndex();
                var indexRight = Handedness.Right.ToIndex();

                m_AgnosticAimStatesValidity[indexLeft] |= (successFlags & leftSuccessFlags) == leftSuccessFlags;
                m_AgnosticAimStatesValidity[indexRight] |= (successFlags & rightSuccessFlags) == rightSuccessFlags;

                if (m_AgnosticAimStatesValidity[indexLeft])
                {
                    s_MetaAim.GetAimState(Handedness.Left, out var leftAimState);
                    m_AgnosticAimStates[indexLeft] = leftAimState;
                }

                if (m_AgnosticAimStatesValidity[indexRight])
                {
                    s_MetaAim.GetAimState(Handedness.Right, out var rightAimState);
                    m_AgnosticAimStates[indexRight] = rightAimState;
                }
            }

            return successFlags;
        }

#if OPENXR_1_19_0_PRE_2_OR_NEWER
        /// <inheritdoc/>
        public override bool canSurfaceUnextrapolatedPoses
        {
            get
            {
                if (m_IsUnextrapolatedPosesExtensionEnabled)
                    return true;

                m_IsUnextrapolatedPosesExtensionEnabled =
                    OpenXRRuntime.IsExtensionEnabled(MetaHandTrackingUnextrapolatedPoses.extensionString);
                return m_IsUnextrapolatedPosesExtensionEnabled;
            }
        }
        bool m_IsUnextrapolatedPosesExtensionEnabled;

        /// <inheritdoc/>
        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateUnextrapolatedPoses(
            Handedness handedness,
            ref Pose rootPose,
            NativeArray<XRHandJoint> joints,
            out long? captureTime)
        {
            captureTime = null;

            if (!m_IsValid)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            var extensibilityManager = HandTracking.extensibilityManager;
            if (extensibilityManager == null)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            var xrHand = handedness == Handedness.Left ? XrHandEXT.Left : XrHandEXT.Right;
            var inputChain = extensibilityManager.GetSecondaryLocateInputChain(xrHand);
            var outputChain = extensibilityManager.GetSecondaryLocateOutputChain(xrHand);
            if (inputChain == null || outputChain == null)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            var successFlags = NativeApi.TryLocateHand(
                XRHandSubsystem.UpdateType.Dynamic,
                handedness,
                inputChain.GetHeadPointer(),
                outputChain.GetHeadPointer(),
                ref rootPose,
                joints.GetUnsafePtr());

            // The runtime leaves captureTime at zero when it returns no valid poses, so it is only
            // meaningful on a locate that succeeded.
            if (successFlags.IsHandTracked(handedness) &&
                outputChain.TryGetNode<XrHandTrackingUnextrapolatedPosesMETA>(
                    XrStructureType.HandTrackingUnextrapolatedPosesMETA, out var unextrapolatedPoses))
                captureTime = unextrapolatedPoses.captureTime;

            return successFlags;
        }
#endif // OPENXR_1_19_0_PRE_2_OR_NEWER

        /// <inheritdoc/>
        public override bool canSurfaceCommonPoseData
        {
            get
            {
                if (m_IsHandInteractionProfileEnabled || m_IsMetaHandTrackingAimEnabled)
                    return true;

                m_IsHandInteractionProfileEnabled =
#if UNITY_OPENXR_PACKAGE_1_8
                    OpenXRRuntime.IsExtensionEnabled(HandInteractionProfile.extensionString);
#else
                    false;
#endif

                m_IsMetaHandTrackingAimEnabled =
#if UNITY_OPENXR_PACKAGE_1_8
                    OpenXRRuntime.IsExtensionEnabled(MetaHandTrackingAim.extensionString);
#else
                    false;
#endif

                return m_IsHandInteractionProfileEnabled || m_IsMetaHandTrackingAimEnabled;
            }
        }
        bool m_IsHandInteractionProfileEnabled;
        bool m_IsMetaHandTrackingAimEnabled;

        /// <inheritdoc/>
        internal override bool TryGetCommonGesturesState(Handedness handedness, out XRCommonHandGesturesState commonGestures)
        {
#if UNITY_OPENXR_PACKAGE_1_8
            commonGestures = new XRCommonHandGesturesState
            {
                handedness = handedness,
                // The "Is Tracked" data is explicitly provided by this method, so set this flag
                // since otherwise it would be based purely on Valid ("Tracking State").
                flags = XRCommonHandGesturesFlags.HasExplicitIsTracked,
            };

            // If there's no hand device, all other property values of the state should remain default.
            if (!TryGetHandDevice(handedness, out var handDevice))
            {
                // Provide the Meta Aim Pose if available as a fallback to Aim Pose.
                if (TryGetMetaAim(handedness, out var metaAimPose, out var isAimPoseValid, out var isAimPoseTracked))
                {
                    commonGestures.aimPoseInternal = metaAimPose;

                    if (isAimPoseValid)
                        commonGestures.flags = commonGestures.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);

                    if (isAimPoseTracked)
                        commonGestures.flags = commonGestures.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseTracked);
                }

                return true;
            }

            var flags = commonGestures.flags;

            // Aim Pose
            var aimPose = Pose.identity;
            if (handDevice.TryGetFeatureValue(Usages.pointerPosition, out var position))
                aimPose.position = position;
            if (handDevice.TryGetFeatureValue(Usages.pointerRotation, out var rotation))
                aimPose.rotation = rotation;
            if (handDevice.TryGetFeatureValue(Usages.pointerTrackingState, out var trackingState) && IsValid(trackingState))
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);
            if (handDevice.TryGetFeatureValue(Usages.pointerIsTracked, out var isTracked) && isTracked)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseTracked);

            // Grip Pose
            var gripPose = Pose.identity;
            if (handDevice.TryGetFeatureValue(Usages.devicePosition, out position))
                gripPose.position = position;
            if (handDevice.TryGetFeatureValue(Usages.deviceRotation, out rotation))
                gripPose.rotation = rotation;
            if (handDevice.TryGetFeatureValue(Usages.deviceTrackingState, out trackingState) && IsValid(trackingState))
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid);
            if (handDevice.TryGetFeatureValue(Usages.deviceIsTracked, out isTracked) && isTracked)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseTracked);

            // Pinch Pose
            var pinchPose = Pose.identity;
            if (handDevice.TryGetFeatureValue(Usages.pinchPosition, out position))
                pinchPose.position = position;
            if (handDevice.TryGetFeatureValue(Usages.pinchRotation, out rotation))
                pinchPose.rotation = rotation;
            if (handDevice.TryGetFeatureValue(Usages.pinchTrackingState, out trackingState) && IsValid(trackingState))
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid);
            if (handDevice.TryGetFeatureValue(Usages.pinchIsTracked, out isTracked) && isTracked)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseTracked);

            // Poke Pose
            var pokePose = Pose.identity;
            if (handDevice.TryGetFeatureValue(Usages.pokePosition, out position))
                pokePose.position = position;
            if (handDevice.TryGetFeatureValue(Usages.pokeRotation, out rotation))
                pokePose.rotation = rotation;
            if (handDevice.TryGetFeatureValue(Usages.pokeTrackingState, out trackingState) && IsValid(trackingState))
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid);
            if (handDevice.TryGetFeatureValue(Usages.pokeIsTracked, out isTracked) && isTracked)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseTracked);

            // Aim Activate
            handDevice.TryGetFeatureValue(Usages.pointerActivateValue, out var aimActivateValue);
            handDevice.TryGetFeatureValue(Usages.pointerActivated, out var isAimActivated);
            if (handDevice.TryGetFeatureValue(Usages.pointerActivateReady, out var isReady) && isReady)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid | XRCommonHandGesturesFlags.IsAimActivatedStateValid);

            // Grasp
            handDevice.TryGetFeatureValue(Usages.graspValue, out var graspValue);
            handDevice.TryGetFeatureValue(Usages.graspFirm, out var isGraspFirm);
            if (handDevice.TryGetFeatureValue(Usages.graspReady, out isReady) && isReady)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid | XRCommonHandGesturesFlags.IsGraspFirmStateValid);

            // Pinch
            handDevice.TryGetFeatureValue(Usages.pinchValue, out var pinchValue);
            handDevice.TryGetFeatureValue(Usages.pinchTouched, out var isPinchTouched);
            if (handDevice.TryGetFeatureValue(Usages.pinchReady, out isReady) && isReady)
                flags = flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid | XRCommonHandGesturesFlags.IsPinchTouchedStateValid);

            // Microgestures
            var microgestures = XRHandMicrogestureFlags.None;
#if OPENXR_7_0_0_PRE_1_OR_NEWER
            if (s_RegisteredDescriptorCinfo.HasValue && s_RegisteredDescriptorCinfo.Value.supportsMicrogestures)
                microgestures = ReadMicrogestures(handDevice);
#endif

            commonGestures.flags = flags;

            commonGestures.aimPoseInternal = aimPose;
            commonGestures.gripPoseInternal = gripPose;
            commonGestures.pinchPoseInternal = pinchPose;
            commonGestures.pokePoseInternal = pokePose;

            commonGestures.aimActivateValueInternal = aimActivateValue;
            commonGestures.isAimActivatedInternal = isAimActivated;

            commonGestures.graspValueInternal = graspValue;
            commonGestures.isGraspFirmInternal = isGraspFirm;

            commonGestures.pinchValueInternal = pinchValue;
            commonGestures.isPinchTouchedInternal = isPinchTouched;

            commonGestures.microgesturesInternal = microgestures;

            return true;
#else
            return base.TryGetCommonGesturesState(handedness, out commonGestures);
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetAimPose(Handedness handedness, out Pose aimPose)
        {
            aimPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (TryGetHandDevice(handedness, out var handDevice))
            {
                if (handDevice.TryGetFeatureValue(Usages.pointerPosition, out var position))
                    aimPose.position = position;

                if (handDevice.TryGetFeatureValue(Usages.pointerRotation, out var rotation))
                    aimPose.rotation = rotation;

                return handDevice.TryGetFeatureValue(Usages.pointerTrackingState, out var trackingState) && IsValid(trackingState);
            }

            // Provide the Meta Aim Pose if available as a fallback to Aim Pose.
            if (TryGetMetaAim(handedness, out var metaAimPose, out var isAimPoseValid, out _))
            {
                aimPose = metaAimPose;

                return isAimPoseValid;
            }

            return false;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetAimActivateValue(Handedness handedness, out float aimActivateValue)
        {
            aimActivateValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.pointerActivateValue, out aimActivateValue);
            return handDevice.TryGetFeatureValue(Usages.pointerActivateReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetAimActivatedState(Handedness handedness, out bool isAimActivated)
        {
            isAimActivated = false;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.pointerActivated, out isAimActivated);
            return handDevice.TryGetFeatureValue(Usages.pointerActivateReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetGraspValue(Handedness handedness, out float graspValue)
        {
            graspValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.graspValue, out graspValue);
            return handDevice.TryGetFeatureValue(Usages.graspReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetGraspFirmState(Handedness handedness, out bool isGraspFirm)
        {
            isGraspFirm = false;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.graspFirm, out isGraspFirm);
            return handDevice.TryGetFeatureValue(Usages.graspReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetGripPose(Handedness handedness, out Pose gripPose)
        {
            gripPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.devicePosition, out var position))
                gripPose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.deviceRotation, out var rotation))
                gripPose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.deviceTrackingState, out var trackingState) && IsValid(trackingState);
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPinchPose(Handedness handedness, out Pose pinchPose)
        {
            pinchPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.pinchPosition, out var position))
                pinchPose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.pinchRotation, out var rotation))
                pinchPose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.pinchTrackingState, out var trackingState) && IsValid(trackingState);
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPinchValue(Handedness handedness, out float pinchValue)
        {
            pinchValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.pinchValue, out pinchValue);
            return handDevice.TryGetFeatureValue(Usages.pinchReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPinchTouchedState(Handedness handedness, out bool isPinched)
        {
            isPinched = false;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.pinchTouched, out isPinched);
            return handDevice.TryGetFeatureValue(Usages.pinchReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPokePose(Handedness handedness, out Pose pokePose)
        {
            pokePose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.pokePosition, out var position))
                pokePose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.pokeRotation, out var rotation))
                pokePose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.pokeTrackingState, out var trackingState) && IsValid(trackingState);
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <see cref="XRHandSubsystem"/> reads microgestures for this provider through the common gestures state, so
        /// it doesn't call this method during normal updates.
        /// </remarks>
        public override bool IsMicrogestureActive(Handedness handedness, XRHandMicrogesture microgesture)
        {
#if OPENXR_7_0_0_PRE_1_OR_NEWER
            return TryGetHandDevice(handedness, out var handDevice) &&
                TryGetMicrogestureUsage(microgesture, out var usage) &&
                handDevice.TryGetFeatureValue(usage, out var isActive) &&
                isActive;
#else
            return false;
#endif
        }

#if UNITY_OPENXR_PACKAGE_1_8
#if OPENXR_7_0_0_PRE_1_OR_NEWER
        static bool TryGetMicrogestureUsage(XRHandMicrogesture microgesture, out InputFeatureUsage<bool> usage)
        {
            switch (microgesture)
            {
                case XRHandMicrogesture.SwipeLeft:
                    usage = Usages.swipeLeft;
                    return true;
                case XRHandMicrogesture.SwipeRight:
                    usage = Usages.swipeRight;
                    return true;
                case XRHandMicrogesture.SwipeForward:
                    usage = Usages.swipeForward;
                    return true;
                case XRHandMicrogesture.SwipeBackward:
                    usage = Usages.swipeBackward;
                    return true;
                case XRHandMicrogesture.TapThumb:
                    usage = Usages.tapThumb;
                    return true;
                default:
                    usage = default;
                    return false;
            }
        }

        static XRHandMicrogestureFlags ReadMicrogestures(InputDevice handDevice)
        {
            var microgestures = XRHandMicrogestureFlags.None;
            for (var gestureIndex = (int)XRHandMicrogestureUtility.k_FirstMicrogesture; gestureIndex <= (int)XRHandMicrogestureUtility.k_LastMicrogesture; gestureIndex++)
            {
                var microgesture = (XRHandMicrogesture)gestureIndex;
                if (TryGetMicrogestureUsage(microgesture, out var usage) && handDevice.TryGetFeatureValue(usage, out var isActive) && isActive)
                    microgestures |= microgesture.ToMicrogestureFlag();
            }

            return microgestures;
        }
#endif

        static bool IsValid(uint trackingState)
        {
            return ((InputTrackingState)trackingState & (InputTrackingState.Position | InputTrackingState.Rotation)) == (InputTrackingState.Position | InputTrackingState.Rotation);
        }

        bool TryGetMetaAim(Handedness handedness, out Pose aimPose, out bool isAimPoseValid, out bool isAimPoseTracked)
        {
            aimPose = Pose.identity;
            isAimPoseValid = false;
            isAimPoseTracked = false;

            if (s_MetaAim == null)
                return false;

            var handednessAsIndex = handedness.ToIndex();
            var aimState = m_AgnosticAimStates[handednessAsIndex];

            aimPose = aimState.aimPoseInternal;

            if (m_ApplyMetaAimRotationOffset)
                aimPose.rotation *= Quaternion.AngleAxis(90f, Vector3.forward);

            if (m_AgnosticAimStatesValidity[handednessAsIndex])
            {
                isAimPoseValid = IsValid((uint)aimState.trackingState);
                isAimPoseTracked = aimState.isTracked;
            }

            return true;
        }
#endif

        /// <summary>
        /// The <see cref="OpenXRHandProvider"/> calls into this when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <value>
        /// This is only useful for developers exposing hand mesh data for their
        /// platform. If you are a user making a game or app, you do not need to
        /// worry about this.
        /// </value>
        public IOpenXRHandMeshDataSupplier handMeshDataSupplier { get; set; }

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
        /// Returns <see langword="true"/> if successful and either hand has
        /// valid data. Otherwise, returns <see langword="false"/>.
        /// </returns>
        public override bool TryGetMeshData(ref XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams)
        {
            if (handMeshDataSupplier == null)
                return false;

            return handMeshDataSupplier.TryGetMeshData(ref result, ref queryParams);
        }

        /// <inheritdoc/>
        public override XRDetectedHandMeshLayout detectedHandMeshLayout => NativeApi.GetDetectedHandMeshLayout();

        /// <inheritdoc/>
        public override bool TryGetAimState(Handedness handedness, out XRHandAimState aimState)
        {
            int handednessAsIndex = handedness.ToIndex();
            bool ret = m_AgnosticAimStatesValidity[handednessAsIndex];
            aimState = ret ? m_AgnosticAimStates[handednessAsIndex] : default;
            return ret;
        }

#if UNITY_OPENXR_PACKAGE_1_8
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static class Usages
        {
            // Action poses for hand interactions:
            // Aim Pose ("Pointer")
            internal static readonly InputFeatureUsage<bool> pointerIsTracked = new InputFeatureUsage<bool>("PointerIsTracked");
            internal static readonly InputFeatureUsage<uint> pointerTrackingState = new InputFeatureUsage<uint>("PointerTrackingState");
            internal static readonly InputFeatureUsage<Vector3> pointerPosition = new InputFeatureUsage<Vector3>("PointerPosition");
            internal static readonly InputFeatureUsage<Quaternion> pointerRotation = new InputFeatureUsage<Quaternion>("PointerRotation");

            // Grip Pose ("Device")
            internal static readonly InputFeatureUsage<bool> deviceIsTracked = new InputFeatureUsage<bool>("DeviceIsTracked");
            internal static readonly InputFeatureUsage<uint> deviceTrackingState = new InputFeatureUsage<uint>("DeviceTrackingState");
            internal static readonly InputFeatureUsage<Vector3> devicePosition = new InputFeatureUsage<Vector3>("DevicePosition");
            internal static readonly InputFeatureUsage<Quaternion> deviceRotation = new InputFeatureUsage<Quaternion>("DeviceRotation");

            // Pinch Pose
            internal static readonly InputFeatureUsage<bool> pinchIsTracked = new InputFeatureUsage<bool>("PinchIsTracked");
            internal static readonly InputFeatureUsage<uint> pinchTrackingState = new InputFeatureUsage<uint>("PinchTrackingState");
            internal static readonly InputFeatureUsage<Vector3> pinchPosition = new InputFeatureUsage<Vector3>("PinchPosition");
            internal static readonly InputFeatureUsage<Quaternion> pinchRotation = new InputFeatureUsage<Quaternion>("PinchRotation");

            // Poke Pose
            internal static readonly InputFeatureUsage<bool> pokeIsTracked = new InputFeatureUsage<bool>("PokeIsTracked");
            internal static readonly InputFeatureUsage<uint> pokeTrackingState = new InputFeatureUsage<uint>("PokeTrackingState");
            internal static readonly InputFeatureUsage<Vector3> pokePosition = new InputFeatureUsage<Vector3>("PokePosition");
            internal static readonly InputFeatureUsage<Quaternion> pokeRotation = new InputFeatureUsage<Quaternion>("PokeRotation");

            // Action inputs:
            // Aim activate action
            internal static readonly InputFeatureUsage<bool> pointerActivateReady = new InputFeatureUsage<bool>("PointerActivateReady");
            internal static readonly InputFeatureUsage<float> pointerActivateValue = new InputFeatureUsage<float>("PointerActivateValue");
            internal static readonly InputFeatureUsage<bool> pointerActivated = new InputFeatureUsage<bool>("PointerActivated");

            // Grasp action
            internal static readonly InputFeatureUsage<bool> graspReady = new InputFeatureUsage<bool>("GraspReady");
            internal static readonly InputFeatureUsage<float> graspValue = new InputFeatureUsage<float>("GraspValue");
            internal static readonly InputFeatureUsage<bool> graspFirm = new InputFeatureUsage<bool>("GraspFirm");

            // Pinch action
            internal static readonly InputFeatureUsage<bool> pinchReady = new InputFeatureUsage<bool>("PinchReady");
            internal static readonly InputFeatureUsage<float> pinchValue = new InputFeatureUsage<float>("PinchValue");
            internal static readonly InputFeatureUsage<bool> pinchTouched = new InputFeatureUsage<bool>("PinchTouched");

#if OPENXR_7_0_0_PRE_1_OR_NEWER
            // Microgesture actions
            internal static readonly InputFeatureUsage<bool> swipeLeft = new InputFeatureUsage<bool>("swipeLeft");
            internal static readonly InputFeatureUsage<bool> swipeRight = new InputFeatureUsage<bool>("swipeRight");
            internal static readonly InputFeatureUsage<bool> swipeForward = new InputFeatureUsage<bool>("swipeForward");
            internal static readonly InputFeatureUsage<bool> swipeBackward = new InputFeatureUsage<bool>("swipeBackward");
            internal static readonly InputFeatureUsage<bool> tapThumb = new InputFeatureUsage<bool>("tapThumb");
#endif
        }

        InputDevice m_LeftHandInteractionDevice;
        InputDevice m_RightHandInteractionDevice;
        static readonly List<InputDevice> s_DevicesReuse = new List<InputDevice>();
        bool TryGetHandDevice(Handedness handedness, out InputDevice device)
        {
            if (handedness == Handedness.Left && m_LeftHandInteractionDevice.isValid)
            {
                device = m_LeftHandInteractionDevice;
                return true;
            }

            if (handedness == Handedness.Right && m_RightHandInteractionDevice.isValid)
            {
                device = m_RightHandInteractionDevice;
                return true;
            }

            InputDevices.GetDevicesWithCharacteristics(
                handedness == Handedness.Left
                    ? InputDeviceCharacteristics.Left
                    : InputDeviceCharacteristics.Right,
                s_DevicesReuse);

            for (int deviceIndex = 0; deviceIndex < s_DevicesReuse.Count; ++deviceIndex)
            {
                device = s_DevicesReuse[deviceIndex];
                if (device.name != k_HandInteractionDeviceName)
                    continue;

                if (handedness == Handedness.Left)
                    m_LeftHandInteractionDevice = device;
                else
                    m_RightHandInteractionDevice = device;

                return true;
            }

            device = default;
            return false;
        }

        /// <summary>
        /// Whether to modify the rotation of the Meta Aim Pose when providing the common aim pose
        /// to make it conform to the Hand Interaction Profile convention.
        /// </summary>
        /// <remarks>
        /// On Oculus only, the Meta Aim Pose has a -90 degree offset from the Hand Interaction Profile aim rotation.
        /// Since the primary way of populating the common hand gestures data is with the Hand Interaction Profile rather
        /// than the Meta Aim Pose, when this is true the rotation is fixed up to match what the Hand Interaction Profile
        /// aim pose would be.
        /// <br/>
        /// This makes the aim pose rotation consistent in the XRHandDevice no matter which of the Hand Interaction Profile
        /// or Meta Hand Tracking Aim OpenXR features are enabled if users depend on the rotation.
        /// However, in practice, users shouldn't place much importance on the orientation of the pose and instead
        /// just use the forward direction in their logic since for the XR_EXT_hand_interaction aim pose (emphasis added):
        /// "The +Y direction is a <b>runtime defined direction</b> based on the hand tracking device or ergonomics of the controller in the user’s hand."
        /// See https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_aim_pose.
        /// </remarks>
        bool m_ApplyMetaAimRotationOffset;
#endif

        bool m_IsValid;
        readonly bool[] m_AgnosticAimStatesValidity = new bool[2];
        readonly XRHandAimState[] m_AgnosticAimStates = new XRHandAimState[2];

        internal static string id { get; }

        internal static void SetMetaAim(MetaHandTrackingAim metaAim) => s_MetaAim = metaAim;
        static MetaHandTrackingAim s_MetaAim;
        static bool s_SubsystemRegistered;

        // This static field should not be reset between Play mode sessions since registration list of subsystem descriptors are not cleared.
        static XRHandSubsystemDescriptor.Cinfo? s_RegisteredDescriptorCinfo;

        static OpenXRHandProvider() => id = "OpenXR Hands";

        const string k_HandInteractionDeviceName = "Hand Interaction OpenXR";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            // "ResetStaticsOnLoad()" section of this RuntimeInitializeOnLoadMethod
            s_MetaAim = null;

            var settings = OpenXRSettings.Instance;
            if (settings == null)
                return;

            var handTracking = settings.GetFeature<HandTracking>();
            if (handTracking == null || !handTracking.enabled)
                return;

#if UNITY_OPENXR_PACKAGE_1_8
            var handInteractionProfile = settings.GetFeature<HandInteractionProfile>();
            var commonPosesEnabled = handInteractionProfile != null && handInteractionProfile.enabled;

            var metaHandTrackingAim = settings.GetFeature<MetaHandTrackingAim>();
            var metaAimPoseEnabled = metaHandTrackingAim != null && metaHandTrackingAim.enabled;
#else
            var commonPosesEnabled = false;
            var metaAimPoseEnabled = false;
#endif

#if OPENXR_7_0_0_PRE_1_OR_NEWER
            var microgesturesFeature = settings.GetFeature<HandInteractionProfileMicrogestures>();
            var microgesturesEnabled = commonPosesEnabled && microgesturesFeature != null && microgesturesFeature.enabled;
#else
            var microgesturesEnabled = false;
#endif
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = id,
                providerType = typeof(OpenXRHandProvider),
                supportsAimPose = commonPosesEnabled || metaAimPoseEnabled,
                supportsAimActivateValue = commonPosesEnabled,
                supportsGraspValue = commonPosesEnabled,
                supportsGripPose = commonPosesEnabled,
                supportsPinchPose = commonPosesEnabled,
                supportsPinchValue = commonPosesEnabled,
                supportsPokePose = commonPosesEnabled,
                supportsMicrogestures = microgesturesEnabled,
            };

            // Determine if we need to register or replace the subsystem descriptor.
            var registerSubsystemDescriptor = false;
            if (!s_RegisteredDescriptorCinfo.HasValue)
            {
                // Has never registered the subsystem descriptor.
                registerSubsystemDescriptor = true;
            }
            else if (s_RegisteredDescriptorCinfo.Value != handsSubsystemCinfo)
            {
                // Has previously registered the subsystem descriptor from a previous Play mode but parameters have changed.
                // We must replace the subsystem descriptor for the changed parameters.
                // Warn the user that they will see the following warning logged:
                // "Registering subsystem descriptor with duplicate ID 'OpenXR Hands' - overwriting previous entry."
                // There is no API in the subsystem module for avoiding the warning that will be logged
                // during the Register method when replacing the subsystem descriptor.
                Debug.Log($"The registered subsystem descriptor with ID '{id}' will be overwritten" +
                    " since Hand Interaction Profile, Hand Interaction Profile Microgestures, or Meta Hand Tracking Aim has changed enabled state since last Play mode.");

                registerSubsystemDescriptor = true;
            }

            if (registerSubsystemDescriptor)
            {
                XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
                s_RegisteredDescriptorCinfo = handsSubsystemCinfo;
            }
        }

        static class NativeApi
        {
            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_TryInitialize")]
            [return: MarshalAs(UnmanagedType.I1)]
            internal static extern bool TryInitialize();

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_Destroy")]
            internal static extern void Destroy();

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_TryUpdateHands")]
            internal static extern unsafe XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
                XRHandSubsystem.UpdateType updateType,
                ref Pose leftRootPose,
                void* leftHandJoints,
                ref Pose rightRootPose,
                void* rightHandJoints);

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_TryLocateHand")]
            internal static extern unsafe XRHandSubsystem.UpdateSuccessFlags TryLocateHand(
                XRHandSubsystem.UpdateType updateType,
                Handedness handedness,
                void* inputChain,
                void* outputChain,
                ref Pose rootPose,
                void* handJoints);

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_GetDetectedHandMeshLayout")]
            internal static extern XRDetectedHandMeshLayout GetDetectedHandMeshLayout();
        }
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
