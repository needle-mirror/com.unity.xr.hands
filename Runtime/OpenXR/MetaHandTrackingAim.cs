#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Meta's
    /// hand-tracking aim data in OpenXR. It will not work without also enabling
    /// the <see cref="HandTracking"/> feature. It enables
    /// <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_hand_tracking_aim">
    /// XR_FB_hand_tracking_aim</see> in the underlying runtime. This creates
    /// new <see cref="InputDevice"/>s with the <see cref="InputDeviceCharacteristics.HandTracking"/>
    /// characteristic where the <see cref="TrackedDevice.devicePosition"/>
    /// and <see cref="TrackedDevice.devicePosition"/> represent the aim pose
    /// exposed by this extension.
    /// </summary>
    /// <remarks>
    /// For this extension to be available, you must install the
    /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@latest/manual/index.html">
    /// XR Hands package</see>.
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Meta Hand Tracking Aim",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows for mapping input to the aim pose extension data. Will create an InputDevice for each hand if this and HandTracking are enabled.",
        DocumentationLink = "https://docs.unity3d.com/Packages/com.unity.xr.hands@1.0/manual/features/metahandtrackingaim.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId)]
#endif
    public class MetaHandTrackingAim : OpenXRInteractionFeature
    {
        /// <summary>
        /// The left-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface data in the Meta Hand
        /// Tracking Aim extension.
        /// </summary>
        public static MetaAimHand leftHand { get; private set; }

        /// <summary>
        /// The right-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface data in the Meta Hand
        /// Tracking Aim extension.
        /// </summary>
        public static MetaAimHand rightHand { get; private set; }

        /// <summary>
        /// The feature ID string. This is used to give the feature a well known
        /// ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.metahandtrackingaim";

        /// <summary>
        /// The OpenXR Extension string. OpenXR uses this to check if this
        /// extension is available or enabled. See
        /// <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_hand_tracking_aim">Meta hand-tracking aim</see>
        /// documentation for more information on this OpenXR extension.
        /// </summary>
        public const string extensionString = "XR_FB_hand_tracking_aim";

        /// <summary>
        /// The device product name used when creating the <see cref="InputDevice"/>s
        /// that represent each hand in the extension.
        /// </summary>
        const string deviceProductName = "Meta Aim Hand Tracking";

        /// <summary>
        /// The device manufacturer name used when creating the <see cref="InputDevice"/>s
        /// that represent each hand in the extension.
        /// </summary>
        const string deviceManufacturerName = "OpenXR Meta";

        /// <summary>
        /// The flags in the extension for each hand that can be read from
        /// <see cref="MetaAimHand.aimFlags"/> and casting to this type.
        /// </summary>
        /// <remarks>
        /// For this type to be available, you must install the
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@latest/manual/index.html">
        /// XR Hands package</see>.
        /// </remarks>
        [Flags]
        public enum AimFlags : ulong
        {
            /// <summary>
            /// No flags are valid.
            /// </summary>
            None = 0,

            /// <summary>
            /// Data for this hand has been computed.
            /// </summary>
            Computed = 1 << 0,

            /// <summary>
            /// The aim pose is valid. Retrieve this data from
            /// <see cref="TrackedDevice.devicePosition"/> and
            /// <see cref="TrackedDevice.deviceRotation"/> that
            /// <see cref="MetaAimHand"/> inherits.
            /// </summary>
            Valid = 1 << 1,

            /// <summary>
            /// Indicates whether the index finger is pinching with the thumb.
            /// Only valid when the pinch strength retrieved from
            /// <see cref="MetaAimHand.pinchStrengthIndex"/> is
            /// at a full strength of <c>1.0</c>.
            /// </summary>
            IndexPinching = 1 << 2,

            /// <summary>
            /// Indicates whether the middle finger is pinching with the thumb.
            /// Only valid when the pinch strength retrieved from
            /// <see cref="MetaAimHand.pinchStrengthMiddle"/> is
            /// at a full strength of <c>1.0</c>.
            /// </summary>
            MiddlePinching = 1 << 3,

            /// <summary>
            /// Indicates whether the ring finger is pinching with the thumb.
            /// Only valid when the pinch strength retrieved from
            /// <see cref="MetaAimHand.pinchStrengthRing"/> is
            /// at a full strength of <c>1.0</c>.
            /// </summary>
            RingPinching = 1 << 4,

            /// <summary>
            /// Indicates whether the little finger is pinching with the thumb.
            /// Only valid when the pinch strength retrieved from
            /// <see cref="MetaAimHand.pinchStrengthLittle"/> is
            /// at a full strength of <c>1.0</c>.
            /// </summary>
            LittlePinching = 1 << 5,

            /// <summary>
            /// Indicates whether a system gesture is being performed (when the
            /// palm of the hand is facing the headset).
            /// </summary>
            SystemGesture = 1 << 6,

            /// <summary>
            /// Indicates whether the hand these flags were retrieved from is
            /// the dominant hand.
            /// </summary>
            DominantHand = 1 << 7,

            /// <summary>
            /// Indicates whether the menu gesture button is pressed.
            /// </summary>
            MenuPressed = 1 << 8,
        }

        /// <summary>
        /// A <see cref="TrackedDevice"/> based off the data exposed in the <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_hand_tracking_aim">Meta Hand Tracking Aim extension</see>. Enabled through <see cref="MetaHandTrackingAim"/>.
        /// </summary>
        /// <remarks>
        /// For this type to be available, you must install the
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@latest/manual/index.html">
        /// XR Hands package</see>.
        /// </remarks>
        [Preserve, InputControlLayout(displayName = "Meta Aim Hand (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class MetaAimHand : TrackedDevice
        {
            /// <summary>
            /// The pinch amount required to register as being pressed for the
            /// purposes of <see cref="indexPressed"/>, <see cref="middlePressed"/>,
            /// <see cref="ringPressed"/>, and <see cref="littlePressed"/>.
            /// </summary>
            public const float pressThreshold = 0.8f;

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl)
            /// that represents whether the pinch between the index finger and
            /// the thumb is mostly pressed (greater than a threshold of <c>0.8</c>
            /// contained in <see cref="pressThreshold"/>.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            public ButtonControl indexPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl)
            /// that represents whether the pinch between the middle finger and
            /// the thumb is mostly pressed (greater than a threshold of <c>0.8</c>
            /// contained in <see cref="pressThreshold"/>.
            /// </summary>
            [Preserve, InputControl(offset = 1)]
            public ButtonControl middlePressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl)
            /// that represents whether the pinch between the ring finger and
            /// the thumb is mostly pressed (greater than a threshold of <c>0.8</c>
            /// contained in <see cref="pressThreshold"/>.
            /// </summary>
            [Preserve, InputControl(offset = 2)]
            public ButtonControl ringPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl)
            /// that represents whether the pinch between the little finger and
            /// the thumb is mostly pressed (greater than a threshold of <c>0.8</c>
            /// contained in <see cref="pressThreshold"/>.
            /// </summary>
            [Preserve, InputControl(offset = 3)]
            public ButtonControl littlePressed { get; private set; }

            /// <summary>
            /// Cast the result of reading this to <see cref="AimFlags"/> to examine the value.
            /// </summary>
            [Preserve, InputControl]
            public IntegerControl aimFlags { get; private set; }

            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl)
            /// that represents the pinch strength between the index finger and
            /// the thumb.
            /// </summary>
            /// <remarks>
            /// A value of <c>0</c> denotes no pinch at all, while a value of
            /// <c>1</c> denotes a full pinch.
            /// </remarks>
            [Preserve, InputControl]
            public AxisControl pinchStrengthIndex { get; private set; }

            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl)
            /// that represents the pinch strength between the middle finger and
            /// the thumb.
            /// </summary>
            /// <remarks>
            /// A value of <c>0</c> denotes no pinch at all, while a value of
            /// <c>1</c> denotes a full pinch.
            /// </remarks>
            [Preserve, InputControl]
            public AxisControl pinchStrengthMiddle { get; private set; }

            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl)
            /// that represents the pinch strength between the ring finger and
            /// the thumb.
            /// </summary>
            /// <remarks>
            /// A value of <c>0</c> denotes no pinch at all, while a value of
            /// <c>1</c> denotes a full pinch.
            /// </remarks>
            [Preserve, InputControl]
            public AxisControl pinchStrengthRing { get; private set; }

            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl)
            /// that represents the pinch strength between the little finger and
            /// the thumb.
            /// </summary>
            /// <remarks>
            /// A value of <c>0</c> denotes no pinch at all, while a value of
            /// <c>1</c> denotes a full pinch.
            /// </remarks>
            [Preserve, InputControl]
            public AxisControl pinchStrengthLittle { get; private set; }

            /// <summary>
            /// Perform final initialization tasks after the control hierarchy has been put into place.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();

                indexPressed = GetChildControl<ButtonControl>(nameof(indexPressed));
                middlePressed = GetChildControl<ButtonControl>(nameof(middlePressed));
                ringPressed = GetChildControl<ButtonControl>(nameof(ringPressed));
                littlePressed = GetChildControl<ButtonControl>(nameof(littlePressed));
                aimFlags = GetChildControl<IntegerControl>(nameof(aimFlags));
                pinchStrengthIndex = GetChildControl<AxisControl>(nameof(pinchStrengthIndex));
                pinchStrengthMiddle = GetChildControl<AxisControl>(nameof(pinchStrengthMiddle));
                pinchStrengthRing = GetChildControl<AxisControl>(nameof(pinchStrengthRing));
                pinchStrengthLittle = GetChildControl<AxisControl>(nameof(pinchStrengthLittle));

                var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
                if (deviceDescriptor != null)
                {
                    if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                        UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.LeftHand);
                    else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                        UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.RightHand);
                }
            }

            internal static MetaAimHand CreateHand(InputDeviceCharacteristics extraCharacteristics)
            {
                var desc = new InputDeviceDescription
                {
                    product = deviceProductName,
                    manufacturer = deviceManufacturerName,
                    capabilities = new XRDeviceDescriptor
                    {
                        characteristics = InputDeviceCharacteristics.HandTracking | extraCharacteristics,
                        inputFeatures = new List<XRFeatureDescriptor>()
                        {
                            new XRFeatureDescriptor
                            {
                                name = "index_pressed",
                                featureType = FeatureType.Binary
                            },
                            new XRFeatureDescriptor
                            {
                                name = "middle_pressed",
                                featureType = FeatureType.Binary
                            },
                            new XRFeatureDescriptor
                            {
                                name = "ring_pressed",
                                featureType = FeatureType.Binary
                            },
                            new XRFeatureDescriptor
                            {
                                name = "little_pressed",
                                featureType = FeatureType.Binary
                            },
                            new XRFeatureDescriptor
                            {
                                name = "aim_flags",
                                featureType = FeatureType.DiscreteStates
                            },
                            new XRFeatureDescriptor
                            {
                                name = "aim_pose_position",
                                featureType = FeatureType.Axis3D
                            },
                            new XRFeatureDescriptor
                            {
                                name = "aim_pose_rotation",
                                featureType = FeatureType.Rotation
                            },
                            new XRFeatureDescriptor
                            {
                                name = "pinch_strength_index",
                                featureType = FeatureType.Axis1D
                            },
                            new XRFeatureDescriptor
                            {
                                name = "pinch_strength_middle",
                                featureType = FeatureType.Axis1D
                            },
                            new XRFeatureDescriptor
                            {
                                name = "pinch_strength_ring",
                                featureType = FeatureType.Axis1D
                            },
                            new XRFeatureDescriptor
                            {
                                name = "pinch_strength_little",
                                featureType = FeatureType.Axis1D
                            }
                        }
                    }.ToJson()
                };
                return InputSystem.InputSystem.AddDevice(desc) as MetaAimHand;
            }

            internal void UpdateHand(bool isLeft)
            {
                UnityOpenXRHands_RetrieveMetaAim(
                    isLeft,
                    out AimFlags aimFlags,
                    out Vector3 aimPosePosition,
                    out Quaternion aimPoseRotation,
                    out float pinchIndex,
                    out float pinchMiddle,
                    out float pinchRing,
                    out float pinchLittle);

                InputSystem.InputSystem.QueueDeltaStateEvent(this.aimFlags, (int)aimFlags);

                bool isIndexPressed = pinchIndex > pressThreshold;
                if (isIndexPressed != m_WasIndexPressed)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(indexPressed, isIndexPressed);
                    m_WasIndexPressed = isIndexPressed;
                }

                bool isMiddlePressed = pinchMiddle > pressThreshold;
                if (isMiddlePressed != m_WasMiddlePressed)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(middlePressed, isMiddlePressed);
                    m_WasMiddlePressed = isMiddlePressed;
                }

                bool isRingPressed = pinchRing > pressThreshold;
                if (isRingPressed != m_WasRingPressed)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(ringPressed, isRingPressed);
                    m_WasRingPressed = isRingPressed;
                }

                bool isLittlePressed = pinchLittle > pressThreshold;
                if (isLittlePressed != m_WasLittlePressed)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(littlePressed, isLittlePressed);
                    m_WasLittlePressed = isLittlePressed;
                }

                InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthIndex, pinchIndex);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthMiddle, pinchMiddle);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthRing, pinchRing);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthLittle, pinchLittle);

                if ((aimFlags & AimFlags.Computed) == AimFlags.None)
                {
                    if (m_WasTracked)
                    {
                        InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                        InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                        m_WasTracked = false;
                    }

                    return;
                }

                if ((aimFlags & AimFlags.Valid) != AimFlags.None)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(devicePosition, aimPosePosition);
                    InputSystem.InputSystem.QueueDeltaStateEvent(deviceRotation, aimPoseRotation);

                    if (!m_WasTracked)
                    {
                        InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                        InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, true);
                    }

                    m_WasTracked = true;
                }
                else if (m_WasTracked)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                    InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                    m_WasTracked = false;
                }
            }

            bool m_WasTracked;
            bool m_WasIndexPressed;
            bool m_WasMiddlePressed;
            bool m_WasRingPressed;
            bool m_WasLittlePressed;
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            if (UnityOpenXRHands_ToggleMetaAim(true))
            {
                CreateHands();
                var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
                if (subsystem != null)
                    subsystem.handsUpdated += OnHandsUpdated;
            }
            else
            {
                Debug.LogError("Couldn't enable Meta aim retrieval in plugin - please ensure you enabled the Hand Tracking feature.");
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            UnityOpenXRHands_ToggleMetaAim(false);

            var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (subsystem != null)
                subsystem.handsUpdated -= OnHandsUpdated;

            DestroyHands();
        }

        /// <summary>
        /// Registers the <see cref="MetaAimHand"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!HandTracking.OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            if (leftHand == null && rightHand == null)
            {
                InputSystem.InputSystem.RegisterLayout(typeof(MetaHandTrackingAim.MetaAimHand),
                        matches: new InputDeviceMatcher()
                        .WithProduct(deviceProductName)
                        .WithManufacturer(deviceManufacturerName));
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
#endif
            CreateHands();
        }

        /// <summary>
        /// Removes the <see cref="MetaAimHand"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!HandTracking.OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(MetaHandTrackingAim.MetaAimHand));
            DestroyHands();
        }

        void CreateHands()
        {
            if (leftHand == null)
                leftHand = MetaAimHand.CreateHand(InputDeviceCharacteristics.Left);

            if (rightHand == null)
                rightHand = MetaAimHand.CreateHand(InputDeviceCharacteristics.Right);
        }

        void DestroyHands()
        {
            if (leftHand != null)
            {
                InputSystem.InputSystem.RemoveDevice(leftHand);
                leftHand = null;
            }

            if (rightHand != null)
            {
                InputSystem.InputSystem.RemoveDevice(rightHand);
                rightHand = null;
            }
        }

        void OnHandsUpdated(XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType)
        {
            leftHand.UpdateHand(true);
            rightHand.UpdateHand(false);
        }

        [DllImport("UnityOpenXRHands")]
        static extern bool UnityOpenXRHands_ToggleMetaAim(bool enable);

        [DllImport("UnityOpenXRHands")]
        static extern void UnityOpenXRHands_RetrieveMetaAim(
            bool isLeft,
            out AimFlags aimFlags,
            out Vector3 aimPosePosition,
            out Quaternion aimPoseRotation,
            out float pinchStrengthIndex,
            out float pinchStrengthMiddle,
            out float pinchStrengthRing,
            out float pinchStrengthLittle);
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
