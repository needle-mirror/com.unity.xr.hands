using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// An <see cref="InputDevice"/> that surfaces common controller data
    /// populated by hand joint poses. Devices will only be created if
    /// hand-tracking is enabled in the build settings for the target platform.
    /// </summary>
    /// <remarks>
    /// The <see cref="TrackedDevice.devicePosition"/> and
    /// <see cref="TrackedDevice.deviceRotation"/> inherited from <see cref="TrackedDevice"/>
    /// represent the wrist pose. This is reported in session space,
    /// relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
    /// </remarks>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    [Preserve, InputControlLayout(displayName = "XR Hand Device", commonUsages = new[] { "LeftHand", "RightHand" })]
    public class XRHandDevice : TrackedDevice
    {
        /// <summary>
        /// The left-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface common controller data
        /// populated by hand joint poses.
        /// </summary>
        public static XRHandDevice leftHand { get; internal set; }

        /// <summary>
        /// The right-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface common controller data
        /// populated by hand joint poses.
        /// </summary>
        public static XRHandDevice rightHand { get; internal set; }

        /// <summary>
        /// Position of the grip pose, representing the palm. This is reported in session
        /// space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public Vector3Control gripPosition { get; private set; }

        /// <summary>
        /// Rotation of the grip pose, representing the palm. This is reported in session
        /// space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public QuaternionControl gripRotation { get; private set; }

        /// <summary>
        /// Position of the poke pose, representing the index finger's tip. This is reported
        /// in session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public Vector3Control pokePosition { get; private set; }

        /// <summary>
        /// Rotation of the poke pose, representing the index finger's tip. This is reported
        /// in session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public QuaternionControl pokeRotation { get; private set; }

        /// <summary>
        /// Position of the pinch pose, representing the thumb's tip. This is reported in
        /// session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public Vector3Control pinchPosition { get; private set; }

        /// <summary>
        /// Rotation of the pinch pose, representing the thumb's tip. This is reported in
        /// session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve, InputControl]
        public QuaternionControl pinchRotation { get; private set; }

        /// <summary>
        /// Perform final initialization tasks after the control hierarchy has been put into place.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            gripPosition = GetChildControl<Vector3Control>("gripPosition");
            gripRotation = GetChildControl<QuaternionControl>("gripRotation");
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
            pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.RightHand);
            }
        }

        internal static XRHandDevice Create(
            XRHandSubsystem subsystem,
            Handedness handedness,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            var extraCharacteristics = handedness == Handedness.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;

            var desc = new InputDeviceDescription
            {
                product = deviceProductName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.HandTracking | extraCharacteristics,
                    inputFeatures = new List<XRFeatureDescriptor>()
                    {
                        new XRFeatureDescriptor
                        {
                            name = "grip_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grip_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "poke_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "poke_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_rotation",
                            featureType = FeatureType.Rotation
                        }
                    }
                }.ToJson()
            };
            var handDevice = InputSystem.InputSystem.AddDevice(desc) as XRHandDevice;
            subsystem.updatedHands += handDevice.OnUpdatedHands;
            handDevice.m_Handedness = handedness;

            handDevice.OnUpdatedHands(subsystem, updateSuccessFlags, updateType);
            return handDevice;
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            XRHand hand;
            bool areJointsValid = false;
            if (m_Handedness == Handedness.Left)
            {
                hand = subsystem.leftHand;
                areJointsValid = (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None;
            }
            else
            {
                hand = subsystem.rightHand;
                areJointsValid = (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None;
            }

            if (!m_WereJointsValid && !areJointsValid)
                return;

            if (!areJointsValid)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                return;
            }

            if (!m_WereJointsValid)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, true);
                InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
            }

            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(devicePosition, wristPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(deviceRotation, wristPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(gripPosition, palmPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(gripRotation, palmPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var indexTipPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(pokePosition, indexTipPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(pokeRotation, indexTipPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumbTipPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchPosition, thumbTipPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchRotation, thumbTipPose.rotation);
            }
        }

        static XRHandDevice() => Initialize();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.InputSystem.RegisterLayout<XRHandDevice>(
                matches: new InputDeviceMatcher()
                .WithProduct(deviceProductName));
#endif // ENABLE_INPUT_SYSTEM
        }

        const string deviceProductName = "XRHandDevice";

        Handedness m_Handedness;
        bool m_WereJointsValid;
    }
}
