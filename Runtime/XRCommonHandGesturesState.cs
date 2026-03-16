using System;
using System.IO;
using UnityEngine.XR.Hands.Capture.Recording;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// A static representation of the data in <see cref="XRCommonHandGestures"/>.
    /// </summary>
    [Serializable]
    struct XRCommonHandGesturesState : IEquatable<XRCommonHandGesturesState>
    {
        /// <summary>
        /// Initializes an <c>XRCommonHandGesturesState</c> as a copy of the state in
        /// the given <see cref="XRCommonHandGestures"/>.
        /// </summary>
        /// <param name="copyFrom">
        /// The <see cref="XRCommonHandGestures"/> retrieved from an <see cref="XRHandSubsystem"/>
        /// through <see cref="XRHandSubsystem.GetHand"/>, <see cref="XRHandSubsystem.leftHandCommonGestures"/>,
        /// <see cref="XRHandSubsystem.rightHandCommonGestures"/>, or successfully retrieved from
        /// <see cref="XRHandSubsystem.TryGetHand"/>.
        /// </param>
        public XRCommonHandGesturesState(XRCommonHandGestures copyFrom)
        {
            m_Handedness = copyFrom.handedness;
            m_CommonGesturesFlags = copyFrom.flags;

            m_AimPose = copyFrom.TryGetAimPose(out var aimPose) ? aimPose : Pose.identity;
            m_AimActivateValue = copyFrom.TryGetAimActivateValue(out var aimActivateValue) ? aimActivateValue : 0f;
            m_GraspValue = copyFrom.TryGetGraspValue(out var graspValue) ? graspValue : 0f;
            m_GripPose = copyFrom.TryGetGripPose(out var gripPose) ? gripPose : Pose.identity;
            m_PinchPose = copyFrom.TryGetPinchPose(out var pinchPose) ? pinchPose : Pose.identity;
            m_PinchValue = copyFrom.TryGetPinchValue(out var pinchValue) ? pinchValue : 0f;
            m_PokePose = copyFrom.TryGetPokePose(out var pokePose) ? pokePose : Pose.identity;

            if (copyFrom.TryGetAimActivatedState(out var isAimActivated))
            {
                m_IsAimActivated = isAimActivated;
                m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsAimActivatedStateValid;
            }
            else
            {
                m_IsAimActivated = false;
            }

            if (copyFrom.TryGetGraspFirmState(out var isGraspFirm))
            {
                m_IsGraspFirm = isGraspFirm;
                m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsGraspFirmStateValid;
            }
            else
            {
                m_IsGraspFirm = false;
            }

            if (copyFrom.TryGetPinchTouchedState(out var isPinchTouched))
            {
                m_IsPinchTouched = isPinchTouched;
                m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsPinchTouchedStateValid;
            }
            else
            {
                m_IsPinchTouched = false;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRCommonHandGesturesState"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRCommonHandGesturesState"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(in XRCommonHandGesturesState other)
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
        bool IEquatable<XRCommonHandGesturesState>.Equals(XRCommonHandGesturesState other) => Equals(in other);

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
        public override bool Equals(object obj) => obj is XRCommonHandGesturesState other && Equals(in other);

        /// <summary>
        /// Computes a hash code from all fields of this <c>XRCommonHandGesturesState</c>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
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
        /// Denotes the hand this <c>XRCommonHandGesturesState</c> is associated
        /// with.
        /// </summary>
        /// <value>
        /// If this was retrieved from a valid source, such as a successful call to
        /// <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetCommonGestures"/>,
        /// this can only ever be <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>.
        /// </value>
        public Handedness handedness => m_Handedness;

        /// <summary>
        /// Describes the validity of data found in this <c>XRCommonHandGesturesState</c>.
        /// </summary>
        public XRCommonHandGesturesFlags flags => m_CommonGesturesFlags;

        /// <summary>
        /// Attempts to get the aim pose.
        /// </summary>
        /// <param name="aimPose">
        /// Will be filled out with the aim pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetAimPose(out Pose aimPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimPoseValid) != 0;
            aimPose = ret ? m_AimPose : Pose.identity;
            return ret;
        }

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
        public bool TryGetAimActivateValue(out float aimActivateValue)
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
        public bool TryGetGraspValue(out float graspValue)
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
        public bool TryGetGripPose(out Pose gripPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGripPoseValid) != 0;
            gripPose = ret ? m_GripPose : Pose.identity;
            return ret;
        }

        /// <summary>
        /// Attempts to get the pinch pose.
        /// </summary>
        /// <param name="pinchPose">
        /// Will be filled out with the pinch pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPinchPose(out Pose pinchPose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchPoseValid) != 0;
            pinchPose = ret ? m_PinchPose : Pose.identity;
            return ret;
        }

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
        public bool TryGetPinchValue(out float pinchValue)
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
        public bool TryGetPokePose(out Pose pokePose)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPokePoseValid) != 0;
            pokePose = ret ? m_PokePose : Pose.identity;
            return ret;
        }

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
        public bool TryGetAimActivatedState(out bool isAimActivated)
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
        public bool TryGetGraspFirmState(out bool isGraspFirm)
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
        public bool TryGetPinchTouchedState(out bool isPinchTouched)
        {
            bool ret = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchTouchedStateValid) != 0;
            isPinchTouched = ret && m_IsPinchTouched;
            return ret;
        }

        internal XRCommonHandGesturesState(BinaryReader reader)
        {
            m_Handedness = reader.ReadHandedness();
            m_CommonGesturesFlags = reader.ReadCommonGesturesFlags();

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimPoseValid) != 0)
                reader.ReadPose(out m_AimPose);
            else
                m_AimPose = Pose.identity;

            m_AimActivateValue = ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivateValueValid) != 0)
                ? reader.ReadSingle()
                : 0f;

            m_GraspValue = ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspValueValid) != 0)
                ? reader.ReadSingle()
                : 0f;

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGripPoseValid) != 0)
                reader.ReadPose(out m_GripPose);
            else
                m_GripPose = Pose.identity;

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchPoseValid) != 0)
                reader.ReadPose(out m_PinchPose);
            else
                m_PinchPose = Pose.identity;

            m_PinchValue = ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchValueValid) != 0)
                ? reader.ReadSingle()
                : 0f;

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPokePoseValid) != 0)
                reader.ReadPose(out m_PokePose);
            else
                m_PokePose = Pose.identity;

            m_IsAimActivated = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivatedStateValid) != 0 && reader.ReadBoolean();
            m_IsGraspFirm = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspFirmStateValid) != 0 && reader.ReadBoolean();
            m_IsPinchTouched = (m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchTouchedStateValid) != 0 && reader.ReadBoolean();
        }

        internal Pose possiblyInvalidAimPose => m_AimPose;
        internal float possiblyInvalidAimActivateValue => m_AimActivateValue;
        internal float possiblyInvalidGraspValue => m_GraspValue;
        internal Pose possiblyInvalidGripPose => m_GripPose;
        internal Pose possiblyInvalidPinchPose => m_PinchPose;
        internal float possiblyInvalidPinchValue => m_PinchValue;
        internal Pose possiblyInvalidPokePose => m_PokePose;
        internal bool possiblyInvalidIsAimActivated => m_IsAimActivated;
        internal bool possiblyInvalidIsGraspFirm => m_IsGraspFirm;
        internal bool possiblyInvalidIsPinchTouched => m_IsPinchTouched;

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
