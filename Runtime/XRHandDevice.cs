using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.Hands.Configuration;

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
    /// represent the wrist pose.
    ///
    /// Use the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) in the scene to position and orient the device
    /// and gesture poses properly. If you are using this data to set the Transform of a GameObject in
    /// the scene hierarchy, you can set the local position and rotation of the Transform and make
    /// it a child of the <c>CameraOffset</c> object below the <c>XROrigin</c>. Otherwise, you can use the
    /// Transform of the <c>CameraOffset</c> to transform the data into world space.
    /// </remarks>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(stateType = typeof(XRHandDeviceState), displayName = "XR Hand Device", commonUsages = new[] { "LeftHand", "RightHand" })]
    public class XRHandDevice : TrackedDevice
    {
        /// <summary>
        /// Left-hand <see cref="InputDevice"/> that contains <see cref="InputControl"/>s that surface
        /// common hand data populated by hand joint poses.
        /// </summary>
        public static XRHandDevice leftHand { get; internal set; }

        /// <summary>
        /// Right-hand <see cref="InputDevice"/> that contains <see cref="InputControl"/>s that surface
        /// common hand data populated by hand joint poses.
        /// </summary>
        public static XRHandDevice rightHand { get; internal set; }

        /// <summary>
        /// Tracking status of the grip position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl gripTrackingState { get; private set; }

        /// <summary>
        /// Position of the grip pose, representing the palm.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control gripPosition { get; private set; }

        /// <summary>
        /// Rotation of the grip pose, representing the palm.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl gripRotation { get; private set; }

        /// <summary>
        /// [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that indicates the extent to which a user is making a fist.
        /// </summary>
        public AxisControl graspValue { get; private set; }

        /// <summary>
        /// [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that indicates whether or not the user is making a fist.
        /// </summary>
        public ButtonControl graspFirm { get; private set; }

        /// <summary>
        /// Whether the hand performing the grasp action is properly tracked by the hand tracking device
        /// and it is observed to be ready to perform or is performing the grasp action.
        /// </summary>
        public ButtonControl graspReady { get; private set; }

        /// <summary>
        /// Tracking status of the poke position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl pokeTrackingState { get; private set; }

        /// <summary>
        /// Position of the poke pose, representing the index finger's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control pokePosition { get; private set; }

        /// <summary>
        /// Rotation of the poke pose, representing the index finger's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl pokeRotation { get; private set; }

        /// <summary>
        /// Tracking status of the pinch position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl pinchTrackingState { get; private set; }

        /// <summary>
        /// Position of the pinch pose, representing the thumb's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control pinchPosition { get; private set; }

        /// <summary>
        /// Rotation of the pinch pose, representing the thumb's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl pinchRotation { get; private set; }

        /// <summary>
        /// [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the extent
        /// to which the user is bringing their finger and thumb together to perform a "pinch" gesture.
        /// </summary>
        public AxisControl pinchValue { get; private set; }

        /// <summary>
        /// [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents whether or not a
        /// pinch action is actively occurring.
        /// </summary>
        public ButtonControl pinchTouched { get; private set; }

        /// <summary>
        /// Whether the fingers used to perform the "pinch" gesture are properly tracked by the hand tracking device
        /// and the hand shape is observed to be ready to perform or is performing a "pinch" gesture.
        /// </summary>
        public ButtonControl pinchReady { get; private set; }

        /// <summary>
        /// Tracking status of the aim position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl aimTrackingState { get; private set; }

        /// <summary>
        /// Position of the aim pose, representing an aiming ray cast to a target.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control aimPosition { get; private set; }

        /// <summary>
        /// Rotation of the aim pose, representing an aiming ray cast to a target.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl aimRotation { get; private set; }

        /// <summary>
        /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that indicates the extent to which a
        /// user activated the aim action on the target that the user is pointing at with the aim pose.
        /// </summary>
        public AxisControl aimActivateValue { get; private set; }

        /// <summary>
        /// [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that indicates whether or not
        /// the user activated the aim action on the target that the user is pointing at with the aim pose.
        /// </summary>
        public ButtonControl aimActivated { get; private set; }

        /// <summary>
        /// Whether the fingers to perform the aim activate gesture are properly tracked by the hand tracking device
        /// and the hand shape is observed to be ready to perform or is performing an aim activate gesture.
        /// </summary>
        public ButtonControl aimActivateReady { get; private set; }

        Action<XRHandSubsystem, XRHandSubsystem.UpdateSuccessFlags, XRHandSubsystem.UpdateType> m_UpdateBehavior;

        XRHandDeviceState m_DeviceState;
        bool m_DeviceStateDirty = true;

        /// <summary>
        /// Perform final initialization tasks after the control hierarchy has been put into place.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            gripPosition = GetChildControl<Vector3Control>("gripPosition");
            gripRotation = GetChildControl<QuaternionControl>("gripRotation");
            gripTrackingState = GetChildControl<IntegerControl>("gripTrackingState");
            graspValue = GetChildControl<AxisControl>("graspValue");
            graspFirm = GetChildControl<ButtonControl>("graspFirm");
            graspReady = GetChildControl<ButtonControl>("graspReady");
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            pokeTrackingState = GetChildControl<IntegerControl>("pokeTrackingState");
            pinchTrackingState = GetChildControl<IntegerControl>("pinchTrackingState");
            pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
            pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");
            pinchValue = GetChildControl<AxisControl>("pinchValue");
            pinchTouched = GetChildControl<ButtonControl>("pinchTouched");
            pinchReady = GetChildControl<ButtonControl>("pinchReady");
            aimTrackingState = GetChildControl<IntegerControl>("aimTrackingState");
            aimPosition = GetChildControl<Vector3Control>("aimPosition");
            aimRotation = GetChildControl<QuaternionControl>("aimRotation");
            aimActivateValue = GetChildControl<AxisControl>("aimActivateValue");
            aimActivated = GetChildControl<ButtonControl>("aimActivated");
            aimActivateReady = GetChildControl<ButtonControl>("aimActivateReady");

            // Explicitly initialize all rotations in the device state to identity (Quaternion(0f, 0f, 0f, 1f))
            // instead of the struct default (Quaternion(0f, 0f, 0f, 0f)).
            m_DeviceState.gripRotation = Quaternion.identity;
            m_DeviceState.pokeRotation = Quaternion.identity;
            m_DeviceState.pinchRotation = Quaternion.identity;
            m_DeviceState.aimRotation = Quaternion.identity;
            m_DeviceState.deviceRotation = Quaternion.identity;

            // Ensure these initial rotation state changes are pushed even when the hand is not valid
            m_DeviceStateDirty = true;

#if ENABLE_VR || UNITY_GAMECORE // UnityEngine.InputSystem.XR.XRDeviceDescriptor.characteristics is guarded with these defines starting with com.unity.inputsystem@1.14.2
            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.RightHand);
            }
#endif // ENABLE_VR || UNITY_GAMECORE
        }

        /// <inheritdoc />
        protected override unsafe long ExecuteCommand(InputDeviceCommand* commandPtr)
        {
            return XRHandDeviceUtility.TryExecuteCommand(commandPtr, out var result)
                ? result
                : base.ExecuteCommand(commandPtr);
        }

        internal static XRHandDevice Create(
            XRHandSubsystem subsystem,
            Handedness handedness,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            var desc = new InputDeviceDescription
            {
                product = k_DeviceProductName,
                capabilities = new XRDeviceDescriptor
                {
#if ENABLE_VR || UNITY_GAMECORE // UnityEngine.InputSystem.XR.XRDeviceDescriptor.characteristics is guarded with these defines starting with com.unity.inputsystem@1.14.2
                    characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice |
                        (handedness == Handedness.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right),
#endif // ENABLE_VR || UNITY_GAMECORE
                    inputFeatures = new List<XRFeatureDescriptor>
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
                            name = "grip_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grasp_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grasp_firm",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grasp_ready",
                            featureType = FeatureType.Binary
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
                            name = "poke_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_tracking_state",
                            featureType = FeatureType.DiscreteStates
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
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_touched",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_ready",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_activate_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_activated",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_activate_ready",
                            featureType = FeatureType.Binary
                        },
                    }
                }.ToJson()
            };
            var handDevice = InputSystem.InputSystem.AddDevice(desc) as XRHandDevice;
            if (handDevice != null)
            {
                handDevice.m_Handedness = handedness;

#if !(ENABLE_VR || UNITY_GAMECORE) // Ensure the device usage is set when the characteristics property is unavailable
                if (handedness == Handedness.Left)
                    InputSystem.InputSystem.SetDeviceUsage(handDevice, InputSystem.CommonUsages.LeftHand);
                else if (handedness == Handedness.Right)
                    InputSystem.InputSystem.SetDeviceUsage(handDevice, InputSystem.CommonUsages.RightHand);
#endif // !(ENABLE_VR || UNITY_GAMECORE)

                subsystem.updatedHands += handDevice.OnUpdatedHands;
                subsystem.configurationUpdated += handDevice.OnXRHandSubsystemConfigUpdated;

                // Initialize the update method we will use for each frame and trigger initial update
                handDevice.SetDevicePoseSource(subsystem.handSubsystemConfiguration.xrHandDevicePoseSource);
                handDevice.OnUpdatedHands(subsystem, updateSuccessFlags, updateType);
            }

            return handDevice;
        }

        void SetDevicePoseSource(XRHandDevicePoseSource poseSource)
        {
            if (poseSource == XRHandDevicePoseSource.LegacyJointRecognition)
                m_UpdateBehavior = OnUpdatedHandsLegacy;
            else if (poseSource == XRHandDevicePoseSource.CommonGestures)
                m_UpdateBehavior = OnUpdatedHandsCommonGesture;
            else
                throw new ArgumentException($"Unhandled {typeof(XRHandDevicePoseSource)}={poseSource}", nameof(poseSource));
        }

        void OnXRHandSubsystemConfigUpdated(XRHandSubsystemConfigurationUpdatedEventArgs args)
        {
            var poseSource = args.newConfiguration.xrHandDevicePoseSource;
            SetDevicePoseSource(poseSource);

            if (poseSource == XRHandDevicePoseSource.LegacyJointRecognition)
            {
                // Reset all fields that are potentially set during the CommonGestures path and never in the Legacy path
                // to ensure all fields are not frozen in place at some last driven CommonGestures value.
                m_DeviceState.graspValue = default;
                m_DeviceState.graspReady = default;
                m_DeviceState.graspFirm = default;
                m_DeviceState.pinchValue = default;
                m_DeviceState.pinchReady = default;
                m_DeviceState.pinchTouched = default;
                m_DeviceState.aimActivateValue = default;
                m_DeviceState.aimActivateReady = default;
                m_DeviceState.aimActivated = default;
                m_DeviceState.aimPosition = default;
                m_DeviceState.aimRotation = Quaternion.identity;
                m_DeviceState.aimTrackingState = (int)InputTrackingState.None;

                m_DeviceStateDirty = true;
            }
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            m_UpdateBehavior(subsystem, updateSuccessFlags, updateType);
        }

        void OnUpdatedHandsLegacy(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            XRHand hand;
            bool isValid;
            if (m_Handedness == Handedness.Left)
            {
                hand = subsystem.leftHand;
                const XRHandSubsystem.UpdateSuccessFlags success = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }
            else
            {
                hand = subsystem.rightHand;
                const XRHandSubsystem.UpdateSuccessFlags success = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }

            if (!isValid)
            {
                if (m_WasValid)
                {
                    m_DeviceState.isTracked = false;
                    m_DeviceState.trackingState = (int)InputTrackingState.None;
                    m_DeviceState.gripTrackingState = (int)InputTrackingState.None;
                    m_DeviceState.pokeTrackingState = (int)InputTrackingState.None;
                    m_DeviceState.pinchTrackingState = (int)InputTrackingState.None;
                    m_WasValid = false;

                    m_DeviceStateDirty = true;
                }

                if (m_DeviceStateDirty)
                    QueueStateEvent();

                return;
            }

            m_WasValid = true;

            const int poseFullyTracked = (int)(InputTrackingState.Position | InputTrackingState.Rotation);

            m_DeviceState.isTracked = true;
            m_DeviceState.trackingState = poseFullyTracked;
            m_DeviceState.gripTrackingState = poseFullyTracked;
            m_DeviceState.pokeTrackingState = poseFullyTracked;
            m_DeviceState.pinchTrackingState = poseFullyTracked;

            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose))
            {
                m_DeviceState.devicePosition = wristPose.position;
                m_DeviceState.deviceRotation = wristPose.rotation;
            }

            if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmPose))
            {
                m_DeviceState.gripPosition = palmPose.position;
                m_DeviceState.gripRotation = palmPose.rotation;
            }

            if (hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var indexTipPose))
            {
                m_DeviceState.pokePosition = indexTipPose.position;
                m_DeviceState.pokeRotation = indexTipPose.rotation;
            }

            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumbTipPose))
            {
                m_DeviceState.pinchPosition = thumbTipPose.position;
                m_DeviceState.pinchRotation = thumbTipPose.rotation;
            }

            QueueStateEvent();
        }

        void OnUpdatedHandsCommonGesture(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            XRCommonHandGestures commonGestures;
            bool isValid;
            if (m_Handedness == Handedness.Left)
            {
                commonGestures = subsystem.leftHandCommonGestures;
                const XRHandSubsystem.UpdateSuccessFlags success = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }
            else
            {
                commonGestures = subsystem.rightHandCommonGestures;
                const XRHandSubsystem.UpdateSuccessFlags success = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }

            if (!isValid)
            {
                if (m_WasValid)
                {
                    m_DeviceState.isTracked = false;
                    m_DeviceState.trackingState = (int)InputTrackingState.None;
                    m_DeviceState.gripTrackingState = (int)InputTrackingState.None;
                    m_DeviceState.pokeTrackingState = (int)InputTrackingState.None;
                    m_DeviceState.pinchTrackingState = (int)InputTrackingState.None;
                    m_DeviceState.aimTrackingState = (int)InputTrackingState.None;
                    m_WasValid = false;

                    m_DeviceStateDirty = true;
                }

                if (m_DeviceStateDirty)
                    QueueStateEvent();

                return;
            }

            m_WasValid = true;

            const int poseFullyTracked = (int)(InputTrackingState.Position | InputTrackingState.Rotation);

            m_DeviceState.isTracked = true;
            m_DeviceState.trackingState = poseFullyTracked;

            // Grip Pose
            if (commonGestures.TryGetGripPose(out var gripPose))
            {
                m_DeviceState.devicePosition = gripPose.position;
                m_DeviceState.deviceRotation = gripPose.rotation;

                m_DeviceState.gripPosition = gripPose.position;
                m_DeviceState.gripRotation = gripPose.rotation;
                m_DeviceState.gripTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.gripTrackingState = (int)InputTrackingState.None;
            }

            // Grasp
            if (commonGestures.TryGetGraspValue(out var currentGraspValue))
            {
                m_DeviceState.graspValue = currentGraspValue;
                m_DeviceState.graspReady = true;
            }
            else
            {
                m_DeviceState.graspValue = 0f;
                m_DeviceState.graspReady = false;
            }

            m_DeviceState.graspFirm = commonGestures.TryGetGraspFirmState(out var isGraspFirm) && isGraspFirm;

            // Poke Pose
            if (commonGestures.TryGetPokePose(out var pokePose))
            {
                m_DeviceState.pokePosition = pokePose.position;
                m_DeviceState.pokeRotation = pokePose.rotation;
                m_DeviceState.pokeTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.pokeTrackingState = (int)InputTrackingState.None;
            }

            // Pinch Pose
            if (commonGestures.TryGetPinchPose(out var pinchPose))
            {
                m_DeviceState.pinchPosition = pinchPose.position;
                m_DeviceState.pinchRotation = pinchPose.rotation;
                m_DeviceState.pinchTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.pinchTrackingState = (int)InputTrackingState.None;
            }

            // Pinch
            if (commonGestures.TryGetPinchValue(out var currentPinchValue))
            {
                m_DeviceState.pinchValue = currentPinchValue;
                m_DeviceState.pinchReady = true;
            }
            else
            {
                m_DeviceState.pinchValue = 0f;
                m_DeviceState.pinchReady = false;
            }

            m_DeviceState.pinchTouched = commonGestures.TryGetPinchTouchedState(out var isPinchTouched) && isPinchTouched;

            // Aim Pose
            if (commonGestures.TryGetAimPose(out var aimPose))
            {
                m_DeviceState.aimPosition = aimPose.position;
                m_DeviceState.aimRotation = aimPose.rotation;
                m_DeviceState.aimTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.aimTrackingState = (int)InputTrackingState.None;
            }

            // Aim
            if (commonGestures.TryGetAimActivateValue(out var currentAimValue))
            {
                m_DeviceState.aimActivateValue = currentAimValue;
                m_DeviceState.aimActivateReady = true;
            }
            else
            {
                m_DeviceState.aimActivateValue = 0f;
                m_DeviceState.aimActivateReady = false;
            }

            m_DeviceState.aimActivated = commonGestures.TryGetAimActivatedState(out var isAimActivated) && isAimActivated;

            QueueStateEvent();
        }

        void QueueStateEvent()
        {
            m_DeviceStateDirty = false;
            InputSystem.InputSystem.QueueStateEvent(this, m_DeviceState);
        }

#if UNITY_EDITOR
        static XRHandDevice() => RegisterLayout();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterLayout()
        {
            // "ResetStaticsOnLoad()" section of this RuntimeInitializeOnLoadMethod
            leftHand = null;
            rightHand = null;

#if ENABLE_INPUT_SYSTEM
            InputSystem.InputSystem.RegisterLayout<XRHandDevice>(
                matches: new InputDeviceMatcher()
                    .WithProduct(k_DeviceProductName));
#endif
        }

        const string k_DeviceProductName = "XRHandDevice";

        Handedness m_Handedness;
        bool m_WasValid;
    }
}
