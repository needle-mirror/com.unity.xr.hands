using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// State for input device representing XR hand gestures.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 216)]
    struct XRHandDeviceState : IInputStateTypeInfo
    {
        /// <summary>
        /// Memory format identifier for <see cref="XRHandDeviceState"/>.
        /// </summary>
        /// <seealso cref="InputStateBlock.format"/>
        public static FourCC formatId => new FourCC('X', 'R', 'H', 'D');

        /// <summary>
        /// Data format identifier of the state.
        /// </summary>
        public FourCC format => formatId;

        /// <summary>
        /// Informs whether the grip pose is currently being tracked.
        /// </summary>
        [InputControl(usage = "GripIsTracked", layout = "Button", offset = 0)]
        [FieldOffset(0)]
        public bool gripIsTracked;

        /// <summary>
        /// Informs whether the poke pose is currently being tracked.
        /// </summary>
        [InputControl(usage = "PokeIsTracked", layout = "Button", offset = 1)]
        [FieldOffset(1)]
        public bool pokeIsTracked;

        /// <summary>
        /// Informs whether the pinch pose is currently being tracked.
        /// </summary>
        [InputControl(usage = "PinchIsTracked", layout = "Button", offset = 2)]
        [FieldOffset(2)]
        public bool pinchIsTracked;

        /// <summary>
        /// Informs whether the aim pose is currently being tracked.
        /// </summary>
        [InputControl(usage = "AimIsTracked", layout = "Button", offset = 3)]
        [FieldOffset(3)]
        public bool aimIsTracked;

        /// <summary>
        /// Informs whether the wrist pose is currently being tracked.
        /// </summary>
        [InputControl(usage = "WristIsTracked", layout = "Button", offset = 4)]
        [FieldOffset(4)]
        public bool wristIsTracked;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the grip position and rotation.
        /// </summary>
        [InputControl(usage = "GripTrackingState", layout = "Integer", offset = 5)]
        [FieldOffset(5)]
        public int gripTrackingState;

        /// <summary>
        /// Position of the grip pose.
        /// </summary>
        [InputControl(usage = "GripPosition", offset = 9)]
        [FieldOffset(9)]
        public Vector3 gripPosition;

        /// <summary>
        /// Rotation of the grip pose.
        /// </summary>
        [InputControl(usage = "GripRotation", offset = 21)]
        [FieldOffset(21)]
        public Quaternion gripRotation;

        /// <summary>
        /// Value corresponding to the grip pose.
        /// </summary>
        [InputControl(usage = "GraspValue", layout = "Axis", offset = 37)]
        [FieldOffset(37)]
        public float graspValue;

        /// <summary>
        /// Informs whether or not the user is making a fist.
        /// </summary>
        [InputControl(usage = "GraspFirm", layout = "Button", offset = 41)]
        [FieldOffset(41)]
        public bool graspFirm;

        /// <summary>
        /// Informs whether the grasp values are ready.
        /// </summary>
        [InputControl(usage = "GraspReady", layout = "Button", offset = 42)]
        [FieldOffset(42)]
        public bool graspReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the poke position and rotation.
        /// </summary>
        [InputControl(usage = "pokeTrackingState", layout = "Integer", offset = 43)]
        [FieldOffset(43)]
        public int pokeTrackingState;

        /// <summary>
        /// Position of the poke pose.
        /// </summary>
        [InputControl(usage = "PokePosition", offset = 47)]
        [FieldOffset(47)]
        public Vector3 pokePosition;

        /// <summary>
        /// Rotation of the poke pose.
        /// </summary>
        [InputControl(usage = "PokeRotation", offset = 59)]
        [FieldOffset(59)]
        public Quaternion pokeRotation;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the pinch position and rotation.
        /// </summary>
        [InputControl(usage = "pinchTrackingState", layout = "Integer", offset = 75)]
        [FieldOffset(75)]
        public int pinchTrackingState;

        /// <summary>
        /// Position of the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchPosition", offset = 79)]
        [FieldOffset(79)]
        public Vector3 pinchPosition;

        /// <summary>
        /// Rotation of the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchRotation", offset = 91)]
        [FieldOffset(91)]
        public Quaternion pinchRotation;

        /// <summary>
        /// Value corresponding to the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchValue", layout = "Axis", offset = 107)]
        [FieldOffset(107)]
        public float pinchValue;

        /// <summary>
        /// Informs whether or not the user actively pinching.
        /// </summary>
        [InputControl(usage = "PinchTouched", layout = "Button", offset = 111)]
        [FieldOffset(111)]
        public bool pinchTouched;

        /// <summary>
        /// Informs whether the pinch pose is currently ready.
        /// </summary>
        [InputControl(usage = "PinchReady", layout = "Button", offset = 112)]
        [FieldOffset(112)]
        public bool pinchReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the aim position and rotation.
        /// </summary>
        [InputControl(usage = "aimTrackingState", layout = "Integer", offset = 113)]
        [FieldOffset(113)]
        public int aimTrackingState;

        /// <summary>
        /// Position of the aim pose.
        /// </summary>
        [InputControl(usage = "AimPosition", alias = "pointerPosition", offset = 117)]
        [FieldOffset(117)]
        public Vector3 aimPosition;

        /// <summary>
        /// Rotation of the aim pose.
        /// </summary>
        [InputControl(usage = "AimRotation", alias = "pointerRotation", offset = 129)]
        [FieldOffset(129)]
        public Quaternion aimRotation;

        /// <summary>
        /// Value corresponding to activation by the aim pose.
        /// </summary>
        [InputControl(usage = "PointerActivateValue", layout = "Axis", alias = "pointerActivateValue", offset = 145)]
        [FieldOffset(145)]
        public float aimActivateValue;

        /// <summary>
        /// Informs whether or not aim is activated.
        /// </summary>
        [InputControl(usage = "PointerActivated", alias = "pointerActivated", layout = "Button", offset = 149)]
        [FieldOffset(149)]
        public bool aimActivated;

        /// <summary>
        /// Informs whether the aim activation values are ready.
        /// </summary>
        [InputControl(usage = "PointerActivateReady", layout = "Button", offset = 150)]
        [FieldOffset(150)]
        public bool aimActivateReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the wrist position and rotation.
        /// </summary>
        [InputControl(usage = "WristTrackingState", layout = "Integer", offset = 151)]
        [FieldOffset(151)]
        public int wristTrackingState;

        /// <summary>
        /// Position of the wrist pose.
        /// </summary>
        [InputControl(usage = "WristPosition", offset = 155)]
        [FieldOffset(155)]
        public Vector3 wristPosition;

        /// <summary>
        /// Rotation of the wrist pose.
        /// </summary>
        [InputControl(usage = "WristRotation", offset = 167)]
        [FieldOffset(167)]
        public Quaternion wristRotation;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the device position and rotation.
        /// </summary>
        [InputControl(usage = "TrackingState", layout = "Integer", offset = 183)]
        [FieldOffset(183)]
        public int trackingState;

        /// <summary>
        /// Informs to the developer whether the device is currently being tracked.
        /// </summary>
        [InputControl(usage = "IsTracked", layout = "Button", offset = 187)]
        [FieldOffset(187)]
        public bool isTracked;

        /// <summary>
        /// Position of the device.
        /// </summary>
        [InputControl(usage = "DevicePosition", offset = 188)]
        [FieldOffset(188)]
        public Vector3 devicePosition;

        /// <summary>
        /// Rotation of this device.
        /// </summary>
        [InputControl(usage = "DeviceRotation", offset = 200)]
        [FieldOffset(200)]
        public Quaternion deviceRotation;
    }
}
