using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// State for the input device representing Meta Aim Hand data,
    /// surfaced by <see cref="MetaAimHand"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 57)]
    struct MetaAimHandDeviceState : IInputStateTypeInfo
    {
        /// <summary>
        /// Memory format identifier for <see cref="MetaAimHandDeviceState"/>.
        /// </summary>
        /// <seealso cref="InputStateBlock.format"/>
        public static FourCC formatId => new FourCC('X', 'R', 'A', 'D');

        /// <summary>
        /// Data format identifier of the state.
        /// </summary>
        public FourCC format => formatId;

        /// <summary>
        /// Whether the pinch between the index finger and the thumb is mostly pressed.
        /// </summary>
        [InputControl(layout = "Button", offset = 0)]
        [FieldOffset(0)]
        public bool indexPressed;

        /// <summary>
        /// Whether the pinch between the middle finger and the thumb is mostly pressed.
        /// </summary>
        [InputControl(layout = "Button", offset = 1)]
        [FieldOffset(1)]
        public bool middlePressed;

        /// <summary>
        /// Whether the pinch between the ring finger and the thumb is mostly pressed.
        /// </summary>
        [InputControl(layout = "Button", offset = 2)]
        [FieldOffset(2)]
        public bool ringPressed;

        /// <summary>
        /// Whether the pinch between the little finger and the thumb is mostly pressed.
        /// </summary>
        [InputControl(layout = "Button", offset = 3)]
        [FieldOffset(3)]
        public bool littlePressed;

        /// <summary>
        /// The <see cref="MetaAimFlags"/> for the hand, truncated to 32 bits.
        /// </summary>
        [InputControl(layout = "Integer", offset = 4)]
        [FieldOffset(4)]
        public int aimFlags;

        /// <summary>
        /// The pinch strength between the index finger and the thumb.
        /// </summary>
        [InputControl(layout = "Axis", offset = 8)]
        [FieldOffset(8)]
        public float pinchStrengthIndex;

        /// <summary>
        /// The pinch strength between the middle finger and the thumb.
        /// </summary>
        [InputControl(layout = "Axis", offset = 12)]
        [FieldOffset(12)]
        public float pinchStrengthMiddle;

        /// <summary>
        /// The pinch strength between the ring finger and the thumb.
        /// </summary>
        [InputControl(layout = "Axis", offset = 16)]
        [FieldOffset(16)]
        public float pinchStrengthRing;

        /// <summary>
        /// The pinch strength between the little finger and the thumb.
        /// </summary>
        [InputControl(layout = "Axis", offset = 20)]
        [FieldOffset(20)]
        public float pinchStrengthLittle;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the device (aim) position and rotation.
        /// </summary>
        [InputControl(usage = "TrackingState", layout = "Integer", offset = 24)]
        [FieldOffset(24)]
        public int trackingState;

        /// <summary>
        /// Informs to the developer whether the device is currently being tracked.
        /// </summary>
        [InputControl(usage = "IsTracked", layout = "Button", offset = 28)]
        [FieldOffset(28)]
        public bool isTracked;

        /// <summary>
        /// Position of the device, representing the aim pose.
        /// </summary>
        [InputControl(usage = "DevicePosition", offset = 29)]
        [FieldOffset(29)]
        public Vector3 devicePosition;

        /// <summary>
        /// Rotation of the device, representing the aim pose.
        /// </summary>
        [InputControl(usage = "DeviceRotation", offset = 41)]
        [FieldOffset(41)]
        public Quaternion deviceRotation;
    }
}
