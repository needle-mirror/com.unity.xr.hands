using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Hands.Capture;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// A static representation of the data in <see cref="XRCommonHandGestures"/>.
    /// </summary>
    [Serializable]
    struct XRCommonHandGesturesState : IEquatable<XRCommonHandGesturesState>
    {
        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRCommonHandGesturesState"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRCommonHandGesturesState"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool Equals(in XRCommonHandGesturesState other)
        {
            return m_Handedness == other.m_Handedness &&
                m_CommonGesturesFlags == other.m_CommonGesturesFlags &&
                m_AimPose == other.m_AimPose &&
                m_AimActivateValue == other.m_AimActivateValue &&
                m_GraspValue == other.m_GraspValue &&
                m_GripPose == other.m_GripPose &&
                m_PinchPose == other.m_PinchPose &&
                m_PinchValue == other.m_PinchValue &&
                m_PokePose == other.m_PokePose &&
                m_IsAimActivated == other.m_IsAimActivated &&
                m_IsGraspFirm == other.m_IsGraspFirm &&
                m_IsPinchTouched == other.m_IsPinchTouched;
        }

        /// <inheritdoc cref="Equals(in XRCommonHandGesturesState)"/>
        readonly bool IEquatable<XRCommonHandGesturesState>.Equals(XRCommonHandGesturesState other) => Equals(in other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRCommonHandGesturesState"/> and
        /// <see cref="Equals(in XRCommonHandGesturesState)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public readonly override bool Equals(object obj) => obj is XRCommonHandGesturesState other && Equals(in other);

        /// <summary>
        /// Computes a hash code from all fields of this <c>XRCommonHandGesturesState</c>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public readonly override int GetHashCode()
        {
            return HashCodeUtil.Combine(
                m_Handedness.GetHashCode(),
                m_CommonGesturesFlags.GetHashCode(),
                m_AimPose.GetHashCode(),
                m_AimActivateValue.GetHashCode(),
                m_GraspValue.GetHashCode(),
                m_GripPose.GetHashCode(),
                m_PinchPose.GetHashCode(),
                m_PinchValue.GetHashCode(),
                m_PokePose.GetHashCode(),
                m_IsAimActivated.GetHashCode(),
                m_IsGraspFirm.GetHashCode(),
                m_IsPinchTouched.GetHashCode());
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(in XRCommonHandGesturesState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(in XRCommonHandGesturesState lhs, in XRCommonHandGesturesState rhs) => lhs.Equals(in rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(in XRCommonHandGesturesState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(in XRCommonHandGesturesState lhs, in XRCommonHandGesturesState rhs) => !lhs.Equals(in rhs);

        /// <summary>
        /// Denotes the hand this <c>XRCommonHandGesturesState</c> is associated with.
        /// </summary>
        /// <value>
        /// If this was retrieved from a valid source, such as a successful call to
        /// <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetCommonGestures"/>,
        /// this can only ever be <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>.
        /// </value>
        public Handedness handedness
        {
            readonly get => m_Handedness;
            internal set => m_Handedness = value;
        }

        /// <summary>
        /// Describes the validity of data found in this <c>XRCommonHandGesturesState</c>.
        /// </summary>
        public XRCommonHandGesturesFlags flags
        {
            readonly get => m_CommonGesturesFlags;
            internal set => m_CommonGesturesFlags = value;
        }

        /// <summary>
        /// Attempts to get the aim pose.
        /// </summary>
        /// <param name="aimPose">
        /// Will be filled out with the aim pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetAimPose(out Pose aimPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimPoseValid) != 0;
            aimPose = ret ? m_AimPose : Pose.identity;
            return ret;
        }

        /// <summary>
        /// Gets whether the aim pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the aim pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public readonly bool GetAimPoseIsTracked() =>
            m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.HasExplicitIsTracked)
                ? m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseTracked)
                : m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);

        /// <summary>
        /// Attempts to get the aim activate value.
        /// </summary>
        /// <param name="aimActivateValue">
        /// Will be filled out with the aim activate value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetAimActivateValue(out float aimActivateValue)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivateValueValid) != 0;
            aimActivateValue = ret ? m_AimActivateValue : 0f;
            return ret;
        }

        /// <summary>
        /// Attempts to get the grasp value.
        /// </summary>
        /// <param name="graspValue">
        /// Will be filled out with the grasp value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetGraspValue(out float graspValue)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspValueValid) != 0;
            graspValue = ret ? m_GraspValue : 0f;
            return ret;
        }

        /// <summary>
        /// Attempts to get the grip pose.
        /// </summary>
        /// <param name="gripPose">
        /// Will be filled out with the grip pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetGripPose(out Pose gripPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGripPoseValid) != 0;
            gripPose = ret ? m_GripPose : Pose.identity;
            return ret;
        }

        /// <summary>
        /// Gets whether the grip pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the grip pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public readonly bool GetGripPoseIsTracked() =>
            m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.HasExplicitIsTracked)
                ? m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseTracked)
                : m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid);

        /// <summary>
        /// Attempts to get the pinch pose.
        /// </summary>
        /// <param name="pinchPose">
        /// Will be filled out with the pinch pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetPinchPose(out Pose pinchPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchPoseValid) != 0;
            pinchPose = ret ? m_PinchPose : Pose.identity;
            return ret;
        }

        /// <summary>
        /// Gets whether the pinch pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the pinch pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public readonly bool GetPinchPoseIsTracked() =>
            m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.HasExplicitIsTracked)
                ? m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseTracked)
                : m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid);

        /// <summary>
        /// Attempts to get the pinch value.
        /// </summary>
        /// <param name="pinchValue">
        /// Will be filled out with the pinch value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetPinchValue(out float pinchValue)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchValueValid) != 0;
            pinchValue = ret ? m_PinchValue : 0f;
            return ret;
        }

        /// <summary>
        /// Attempts to get the poke pose.
        /// </summary>
        /// <param name="pokePose">
        /// Will be filled out with the poke pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetPokePose(out Pose pokePose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPokePoseValid) != 0;
            pokePose = ret ? m_PokePose : Pose.identity;
            return ret;
        }

        /// <summary>
        /// Gets whether the poke pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the poke pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public readonly bool GetPokePoseIsTracked() =>
            m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.HasExplicitIsTracked)
                ? m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseTracked)
                : m_CommonGesturesFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid);

        /// <summary>
        /// Attempts to get whether aim is fully activated.
        /// </summary>
        /// <param name="isAimActivated">
        /// Will be set to <see langword="true"/> if aim is fully activated, otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the aim activation state is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetAimActivatedState(out bool isAimActivated)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivatedStateValid) != 0;
            isAimActivated = ret && m_IsAimActivated;
            return ret;
        }

        /// <summary>
        /// Attempts to get whether the grasp is firm.
        /// </summary>
        /// <param name="isGraspFirm">
        /// Will be set to <see langword="true"/> if the grasp is firm, otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the grasp firm state is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetGraspFirmState(out bool isGraspFirm)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspFirmStateValid) != 0;
            isGraspFirm = ret && m_IsGraspFirm;
            return ret;
        }

        /// <summary>
        /// Attempts to get whether pinch is touched.
        /// </summary>
        /// <param name="isPinchTouched">
        /// Will be set to <see langword="true"/> if pinch is touched, otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the pinch touched state is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public readonly bool TryGetPinchTouchedState(out bool isPinchTouched)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchTouchedStateValid) != 0;
            isPinchTouched = ret && m_IsPinchTouched;
            return ret;
        }

        internal Pose aimPoseInternal
        {
            readonly get => m_AimPose;
            set => m_AimPose = value;
        }

        internal float aimActivateValueInternal
        {
            readonly get => m_AimActivateValue;
            set => m_AimActivateValue = value;
        }

        internal float graspValueInternal
        {
            readonly get => m_GraspValue;
            set => m_GraspValue = value;
        }

        internal Pose gripPoseInternal
        {
            readonly get => m_GripPose;
            set => m_GripPose = value;
        }

        internal Pose pinchPoseInternal
        {
            readonly get => m_PinchPose;
            set => m_PinchPose = value;
        }

        internal float pinchValueInternal
        {
            readonly get => m_PinchValue;
            set => m_PinchValue = value;
        }

        internal Pose pokePoseInternal
        {
            readonly get => m_PokePose;
            set => m_PokePose = value;
        }

        internal bool isAimActivatedInternal
        {
            readonly get => m_IsAimActivated;
            set => m_IsAimActivated = value;
        }

        internal bool isGraspFirmInternal
        {
            readonly get => m_IsGraspFirm;
            set => m_IsGraspFirm = value;
        }

        internal bool isPinchTouchedInternal
        {
            readonly get => m_IsPinchTouched;
            set => m_IsPinchTouched = value;
        }

        [SerializeField]
        Handedness m_Handedness;

        [SerializeField]
        XRCommonHandGesturesFlags m_CommonGesturesFlags;

        [SerializeField]
        Pose m_AimPose;

        [SerializeField]
        float m_AimActivateValue;

        [SerializeField]
        float m_GraspValue;

        [SerializeField]
        Pose m_GripPose;

        [SerializeField]
        Pose m_PinchPose;

        [SerializeField]
        float m_PinchValue;

        [SerializeField]
        Pose m_PokePose;

        [SerializeField]
        bool m_IsAimActivated;

        [SerializeField]
        bool m_IsGraspFirm;

        [SerializeField]
        bool m_IsPinchTouched;
    }
}
