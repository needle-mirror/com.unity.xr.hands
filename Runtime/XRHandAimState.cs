using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.XR.Hands.Capture.Recording;

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
        public bool Equals(in XRHandAimState other)
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
        bool IEquatable<XRHandAimState>.Equals(XRHandAimState other) => Equals(in other);

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
        public override bool Equals(object obj) => obj is XRHandAimState other && Equals(in other);

        /// <summary>
        /// Computes a hash code from all fields of this <c>XRHandAimState</c>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
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
        public readonly Handedness handedness => m_Handedness;

        /// <summary>
        /// Whether the hand is currently tracked.
        /// </summary>
        public readonly bool isTracked => (m_AimFlags & AimFlags.IsTracked) != 0;

        /// <summary>
        /// Determines which properties of the hand are being tracked as per <see cref="InputTrackingState"/>.
        /// </summary>
        public readonly InputTrackingState trackingState => m_TrackingState;

        /// <summary>
        /// Reserved.
        /// </summary>
        public readonly int reserved0 => m_Reserved0;

        /// <summary>
        /// Reserved.
        /// </summary>
        public readonly int reserved1 => m_Reserved1;

        /// <summary>
        /// Whether the index finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool indexPressed => (m_AimFlags & AimFlags.IsIndexPressed) != 0;

        /// <summary>
        /// Whether the middle finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool middlePressed => (m_AimFlags & AimFlags.IsMiddlePressed) != 0;

        /// <summary>
        /// Whether the ring finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool ringPressed => (m_AimFlags & AimFlags.IsRingPressed) != 0;

        /// <summary>
        /// Whether the little finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool littlePressed => (m_AimFlags & AimFlags.IsLittlePressed) != 0;

        /// <summary>
        ///  The strength of the pinch between the index finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthIndex
        {
            get => m_PinchStrengthIndex;
            internal set => m_PinchStrengthIndex = value;
        }

        /// <summary>
        /// The strength of the pinch between the middle finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthMiddle
        {
            get => m_PinchStrengthMiddle;
            internal set => m_PinchStrengthMiddle = value;
        }

        /// <summary>
        /// The strength of the pinch between the ring finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthRing
        {
            get => m_PinchStrengthRing;
            internal set => m_PinchStrengthRing = value;
        }

        /// <summary>
        /// The strength of the pinch between the little finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public float pinchStrengthLittle
        {
            get => m_PinchStrengthLittle;
            internal set => m_PinchStrengthLittle = value;
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
        public bool TryGetAimPose(out Pose aimPose)
        {
            bool ret = (m_AimFlags & AimFlags.IsAimPoseValid) != 0;
            aimPose = ret ? m_AimPose : Pose.identity;
            return ret;
        }

        internal void SetAimPose(Pose aimPose)
        {
            m_AimPose = aimPose;
        }

        internal XRHandAimState(BinaryReader reader)
        {
            m_Handedness = reader.ReadHandedness();
            m_AimFlags = reader.ReadAimFlags();
            m_TrackingState = reader.ReadInputTrackingState();

            m_Reserved0 = reader.ReadInt32();
            m_Reserved1 = reader.ReadInt32();

            m_PinchStrengthIndex = reader.ReadSingle();
            m_PinchStrengthMiddle = reader.ReadSingle();
            m_PinchStrengthRing = reader.ReadSingle();
            m_PinchStrengthLittle = reader.ReadSingle();

            if ((m_AimFlags & AimFlags.IsAimPoseValid) != 0)
                reader.ReadPose(out m_AimPose);
            else
                m_AimPose = Pose.identity;
        }

        internal readonly AimFlags flags => m_AimFlags;

        internal readonly Pose possiblyInvalidAimPose => m_AimPose;

        internal void UpdateToAimRepresentation(
            Handedness handedness,
            MetaAimFlags metaAimFlags,
            Pose aimPose,
            float pinchIndex,
            float pinchMiddle,
            float pinchRing,
            float pinchLittle)
        {
            m_Handedness = handedness;
            m_TrackingState = InputTrackingState.None;

            m_Reserved0 = (int)(((ulong)metaAimFlags) & 0xffffffff);
            m_Reserved1 = (int)(((ulong)metaAimFlags) >> 32);

            m_PinchStrengthIndex = pinchIndex;
            m_PinchStrengthMiddle = pinchMiddle;
            m_PinchStrengthRing = pinchRing;
            m_PinchStrengthLittle = pinchLittle;

            m_AimFlags = AimFlags.None;
            if ((metaAimFlags & MetaAimFlags.IndexPinching) != 0)
                m_AimFlags |= AimFlags.IsIndexPressed;

            if ((metaAimFlags & MetaAimFlags.MiddlePinching) != 0)
                m_AimFlags |= AimFlags.IsMiddlePressed;

            if ((metaAimFlags & MetaAimFlags.RingPinching) != 0)
                m_AimFlags |= AimFlags.IsRingPressed;

            if ((metaAimFlags & MetaAimFlags.LittlePinching) != 0)
                m_AimFlags |= AimFlags.IsLittlePressed;

            if ((metaAimFlags & MetaAimFlags.Valid) != 0)
            {
                m_AimPose = aimPose;
                m_AimFlags |= AimFlags.IsAimPoseValid;
            }
        }

        internal void SetTrackingStateToValidPose()
            => m_TrackingState = InputTrackingState.Position | InputTrackingState.Rotation;

        [SerializeField]
        Handedness m_Handedness;

        [SerializeField]
        AimFlags m_AimFlags;

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
