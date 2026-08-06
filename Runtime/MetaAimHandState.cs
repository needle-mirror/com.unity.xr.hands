using System;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.OpenXR;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// <c>MetaAimHandState</c> represents a snapshot of a hand's worth of
    /// data in a single frame that can be successfully retrieved if the
    /// <see cref="MetaHandTrackingAim"/> feature
    /// is enabled, or was enabled during capture when producing an
    /// <see cref="XRHandCaptureSequence"/> asset and retrieving that data from
    /// there.
    /// </summary>
    [Serializable]
    public struct MetaAimHandState : IEquatable<MetaAimHandState>
    {
        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="MetaAimHandState"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandAimState"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool Equals(MetaAimHandState other)
        {
            return m_Handedness == other.m_Handedness
                && m_AgnosticFlags == other.m_AgnosticFlags
                && m_TrackingState == other.m_TrackingState
                && m_MetaFlags == other.m_MetaFlags
                && m_PinchStrengthIndex == other.m_PinchStrengthIndex
                && m_PinchStrengthMiddle == other.m_PinchStrengthMiddle
                && m_PinchStrengthRing == other.m_PinchStrengthRing
                && m_PinchStrengthLittle == other.m_PinchStrengthLittle
                && m_AimPose == other.m_AimPose;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="MetaAimHandState"/> and
        /// <see cref="Equals(MetaAimHandState)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public readonly override bool Equals(object obj) => obj is MetaAimHandState other && Equals(other);

        /// <summary>
        /// Computes a hash code from all fields of this <c>MetaAimHandState</c>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public readonly override int GetHashCode()
        {
            int hash = HashCodeUtil.Combine(
                m_Handedness.GetHashCode(),
                m_AgnosticFlags.GetHashCode(),
                m_TrackingState.GetHashCode(),
                m_MetaFlags.GetHashCode(),
                m_PinchStrengthIndex.GetHashCode());

            return HashCodeUtil.Combine(
                hash,
                m_PinchStrengthMiddle.GetHashCode(),
                m_PinchStrengthRing.GetHashCode(),
                m_PinchStrengthLittle.GetHashCode(),
                m_AimPose.GetHashCode());
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(MetaAimHandState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(MetaAimHandState lhs, MetaAimHandState rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(MetaAimHandState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(MetaAimHandState lhs, MetaAimHandState rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Create the Meta representation of aim data from the given <see cref="MetaAimHandState"/>.
        /// </summary>
        /// <param name="aimState">
        /// Platform-agnostic representation of aim data to copy from.
        /// </param>
        public MetaAimHandState(in XRHandAimState aimState)
        {
            m_Handedness = aimState.handedness;
            m_AgnosticFlags = aimState.aimStateFlags;
            m_TrackingState = aimState.trackingState;
            m_MetaFlags = aimState.metaAimFlags;

            m_PinchStrengthIndex = aimState.pinchStrengthIndex;
            m_PinchStrengthMiddle = aimState.pinchStrengthMiddle;
            m_PinchStrengthRing = aimState.pinchStrengthRing;
            m_PinchStrengthLittle = aimState.pinchStrengthLittle;

            m_AimPose = aimState.aimPoseInternal;
        }

        /// <summary>
        /// Denotes which hand this <c>MetaAimHandState</c> is associated with.
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
        public readonly bool isTracked => (m_AgnosticFlags & AimStateFlags.IsTracked) != 0;

        /// <summary>
        /// Determines which properties of the hand are being tracked as per <see cref="InputTrackingState"/>.
        /// </summary>
        public readonly InputTrackingState trackingState => m_TrackingState;

        /// <summary>
        /// Mirrors the <c>XrFlags64</c> in the <c>XrHandTrackingAimStateFB</c> struct from OpenXR.
        /// </summary>
        public readonly MetaAimFlags aimFlags => m_MetaFlags;

        /// <summary>
        /// Whether the index finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool indexPressed => (m_AgnosticFlags & AimStateFlags.IsIndexPressed) != 0;

        /// <summary>
        /// Whether the middle finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool middlePressed => (m_AgnosticFlags & AimStateFlags.IsMiddlePressed) != 0;

        /// <summary>
        /// Whether the ring finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool ringPressed => (m_AgnosticFlags & AimStateFlags.IsRingPressed) != 0;

        /// <summary>
        /// Whether the little finger and thumb are currently pressed together.
        /// </summary>
        public readonly bool littlePressed => (m_AgnosticFlags & AimStateFlags.IsLittlePressed) != 0;

        /// <summary>
        ///  The strength of the pinch between the index finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public readonly float pinchStrengthIndex => m_PinchStrengthIndex;

        /// <summary>
        /// The strength of the pinch between the middle finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public readonly float pinchStrengthMiddle => m_PinchStrengthMiddle;

        /// <summary>
        /// The strength of the pinch between the ring finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public readonly float pinchStrengthRing => m_PinchStrengthRing;

        /// <summary>
        /// The strength of the pinch between the little finger and thumb. Ranges from 0.0 to 1.0.
        /// </summary>
        public readonly float pinchStrengthLittle => m_PinchStrengthLittle;

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
            bool ret = (m_AgnosticFlags & AimStateFlags.HasAimPose) != 0;
            aimPose = ret ? m_AimPose : Pose.identity;
            return ret;
        }

        [SerializeField]
        internal Handedness m_Handedness;

        [SerializeField]
        internal AimStateFlags m_AgnosticFlags;

        [SerializeField]
        internal InputTrackingState m_TrackingState;

#pragma warning disable UAC1011 // Flags should never exceed the 32-bit limit, but this is required for OpenXR compatibility.
        [SerializeField]
        internal MetaAimFlags m_MetaFlags;
#pragma warning restore UAC1011

        [SerializeField]
        internal float m_PinchStrengthIndex;

        [SerializeField]
        internal float m_PinchStrengthMiddle;

        [SerializeField]
        internal float m_PinchStrengthRing;

        [SerializeField]
        internal float m_PinchStrengthLittle;

        [SerializeField]
        internal Pose m_AimPose;
    }
}
