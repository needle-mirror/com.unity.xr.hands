using System;
using UnityEngine.XR.Hands.Capture;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// <c>XRHandAimState</c> is a cross-platform struct representation of the <see cref="MetaAimHand"/> class to store all
    /// the state information related to hand aim on a per-frame basis.
    /// </summary>
    [Serializable]
    public struct XRHandAimState : IEquatable<XRHandAimState>
    {
        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandAimState"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandAimState"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool Equals(in XRHandAimState other)
        {
            return m_Handedness == other.m_Handedness &&
                m_AimFlags == other.m_AimFlags &&
                m_TrackingState == other.m_TrackingState &&
                m_Reserved0 == other.m_Reserved0 &&
                m_Reserved1 == other.m_Reserved1 &&
                m_PinchStrengthIndex == other.m_PinchStrengthIndex &&
                m_PinchStrengthMiddle == other.m_PinchStrengthMiddle &&
                m_PinchStrengthRing == other.m_PinchStrengthRing &&
                m_PinchStrengthLittle == other.m_PinchStrengthLittle &&
                m_AimPose == other.m_AimPose;
        }

        /// <inheritdoc cref="Equals(in XRHandAimState)"/>
        readonly bool IEquatable<XRHandAimState>.Equals(XRHandAimState other) => Equals(in other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandAimState"/> and
        /// <see cref="Equals(in XRHandAimState)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public readonly override bool Equals(object obj) => obj is XRHandAimState other && Equals(in other);

        /// <summary>
        /// Computes a hash code from all fields of this <c>XRHandAimState</c>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public readonly override int GetHashCode()
        {
            int hash = HashCodeUtil.Combine(
                m_Handedness.GetHashCode(),
                m_AimFlags.GetHashCode(),
                m_TrackingState.GetHashCode(),
                m_Reserved0.GetHashCode(),
                m_Reserved1.GetHashCode());

            return HashCodeUtil.Combine(
                hash,
                m_PinchStrengthIndex.GetHashCode(),
                m_PinchStrengthMiddle.GetHashCode(),
                m_PinchStrengthRing.GetHashCode(),
                m_PinchStrengthLittle.GetHashCode(),
                m_AimPose.GetHashCode());
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(in XRHandAimState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(in XRHandAimState lhs, in XRHandAimState rhs) => lhs.Equals(in rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(in XRHandAimState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(in XRHandAimState lhs, in XRHandAimState rhs) => !lhs.Equals(in rhs);

        /// <summary>
        /// Denotes which hand this <c>XRHandAimState</c> is associated with.
        /// </summary>
        /// <value>
        /// If this was retrieved from a valid source, such as a successful call to
        /// <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetAimState"/>,
        /// this can only ever be <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>.
        /// </value>
        public Handedness handedness
        {
            readonly get => m_Handedness;
            internal set => m_Handedness = value;
        }

        /// <summary>
        /// Whether the hand is currently tracked.
        /// </summary>
        public readonly bool isTracked => (m_AimFlags & AimStateFlags.IsTracked) != 0;

        /// <summary>
        /// Determines which properties of the hand are being tracked as per <see cref="InputTrackingState"/>.
        /// </summary>
        public InputTrackingState trackingState
        {
            readonly get => m_TrackingState;
            internal set => m_TrackingState = value;
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        public int reserved0
        {
            readonly get => m_Reserved0;
            internal set => m_Reserved0 = value;
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        public int reserved1
        {
            readonly get => m_Reserved1;
            internal set => m_Reserved1 = value;
        }

        /// <summary>
        /// Whether the index finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool indexPressed => (m_AimFlags & AimStateFlags.IsIndexPressed) != 0;

        /// <summary>
        /// Whether the middle finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool middlePressed => (m_AimFlags & AimStateFlags.IsMiddlePressed) != 0;

        /// <summary>
        /// Whether the ring finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool ringPressed => (m_AimFlags & AimStateFlags.IsRingPressed) != 0;

        /// <summary>
        /// Whether the little finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool littlePressed => (m_AimFlags & AimStateFlags.IsLittlePressed) != 0;

        /// <summary>
        ///  The strength of the pinch between the index finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthIndex
        {
            readonly get => m_PinchStrengthIndex;
            internal set => m_PinchStrengthIndex = value;
        }

        /// <summary>
        /// The strength of the pinch between the middle finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthMiddle
        {
            readonly get => m_PinchStrengthMiddle;
            internal set => m_PinchStrengthMiddle = value;
        }

        /// <summary>
        /// The strength of the pinch between the ring finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthRing
        {
            readonly get => m_PinchStrengthRing;
            internal set => m_PinchStrengthRing = value;
        }

        /// <summary>
        /// The strength of the pinch between the little finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthLittle
        {
            readonly get => m_PinchStrengthLittle;
            internal set => m_PinchStrengthLittle = value;
        }

        /// <summary>
        /// Internal flags that pack some of the dependent properties of this representation.
        /// </summary>
        internal AimStateFlags aimStateFlags
        {
            readonly get => m_AimFlags;
            set => m_AimFlags = value;
        }

        /// <summary>
        /// Flags that describe the availability and state of other signals, coming from the OpenXR status bitmask.
        /// </summary>
        /// <remarks>
        /// See https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XrHandTrackingAimFlagBitsFB.
        /// </remarks>
        /// <seealso cref="MetaAimHand.aimFlags"/>
        // The 64-bit flags is captured with two 32-bit int fields,
        // where reserved0 is the lower 32-bits and reserved1 is the upper 32-bits.
        internal MetaAimFlags metaAimFlags => (MetaAimFlags)((((ulong)reserved1) << 32) | ((uint)reserved0));

        /// <summary>
        /// Internal raw access to the aim <see cref="Pose"/>.
        /// </summary>
        /// <remarks>
        /// Note that this property does not check/set <see cref="AimStateFlags.HasAimPose"/>.
        /// This property is intended to be available for transforming the Pose to a different reference space
        /// or for copy constructors to access the Pose field.
        /// </remarks>
        /// <seealso cref="TryGetAimPose"/>
        internal Pose aimPoseInternal
        {
            readonly get => m_AimPose;
            set => m_AimPose = value;
        }

        /// <summary>
        /// Attempts to retrieve the aim <see cref="Pose"/>.
        /// </summary>
        /// <param name="aimPose">
        /// If <c>TryGetAimPose</c> returns <see langword="true"/>, this will
        /// be filled out with a valid aim <see cref="Pose"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the pose was successfully retrieved.
        /// Otherwise, this returns <see langword="false"/>, and you should not use
        /// the resulting pose.
        /// </returns>
        public readonly bool TryGetAimPose(out Pose aimPose)
        {
            bool ret = (m_AimFlags & AimStateFlags.HasAimPose) != 0;
            aimPose = ret ? m_AimPose : Pose.identity;
            return ret;
        }

        internal void UpdateToAimRepresentation(
            Handedness handedness,
            bool isHandRootTracked,
            MetaAimFlags metaAimFlags,
            Pose aimPose,
            float pinchIndex,
            float pinchMiddle,
            float pinchRing,
            float pinchLittle)
        {
            m_Handedness = handedness;
            m_AimFlags = AimStateFlags.HasAimPose;
            m_TrackingState = InputTrackingState.None;

            if (isHandRootTracked)
            {
                m_AimFlags |= AimStateFlags.IsTracked;
                m_TrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
            }

            if ((metaAimFlags & MetaAimFlags.IndexPinching) != 0)
                m_AimFlags |= AimStateFlags.IsIndexPressed;

            if ((metaAimFlags & MetaAimFlags.MiddlePinching) != 0)
                m_AimFlags |= AimStateFlags.IsMiddlePressed;

            if ((metaAimFlags & MetaAimFlags.RingPinching) != 0)
                m_AimFlags |= AimStateFlags.IsRingPressed;

            if ((metaAimFlags & MetaAimFlags.LittlePinching) != 0)
                m_AimFlags |= AimStateFlags.IsLittlePressed;

            // Store 64-bit flags as two 32-bit int fields
            // where reserved0 is the lower 32-bits and reserved1 is the upper 32-bits.
            m_Reserved0 = unchecked((int)((ulong)metaAimFlags));
            m_Reserved1 = unchecked((int)(((ulong)metaAimFlags) >> 32));

            m_PinchStrengthIndex = pinchIndex;
            m_PinchStrengthMiddle = pinchMiddle;
            m_PinchStrengthRing = pinchRing;
            m_PinchStrengthLittle = pinchLittle;
            m_AimPose = aimPose;
        }

        [SerializeField]
        Handedness m_Handedness;

        [SerializeField]
        AimStateFlags m_AimFlags;

        [SerializeField]
        InputTrackingState m_TrackingState;

        [SerializeField]
        int m_Reserved0;

        [SerializeField]
        int m_Reserved1;

        [SerializeField]
        float m_PinchStrengthIndex;

        [SerializeField]
        float m_PinchStrengthMiddle;

        [SerializeField]
        float m_PinchStrengthRing;

        [SerializeField]
        float m_PinchStrengthLittle;

        [SerializeField]
        Pose m_AimPose;
    }
}
