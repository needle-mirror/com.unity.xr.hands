#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

using System;

namespace UnityEngine.XR.Hands.OpenXR
{
    public partial class MetaHandTrackingAim
    {
        /// <summary>
        /// (Obsolete) Use <see cref="MetaAimFlags"/> instead.
        /// </summary>
        [Obsolete("AimFlags is obsolete. Use UnityEngine.XR.Hands.MetaAimFlags instead.", true)]
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
            /// <see cref="UnityEngine.InputSystem.TrackedDevice.devicePosition"/> and
            /// <see cref="UnityEngine.InputSystem.TrackedDevice.deviceRotation"/> that
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
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
