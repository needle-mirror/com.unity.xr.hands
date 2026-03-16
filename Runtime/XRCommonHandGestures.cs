using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Access to common hand gesture data and callbacks.
    /// </summary>
    public class XRCommonHandGestures
    {
        /// <summary>
        /// Event-args type for when the aim pose updates.
        /// </summary>
        public class AimPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the aim pose.
            /// </summary>
            /// <param name="aimPose">
            /// Will be filled out with the aim pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetAimPose(out Pose aimPose)
            {
                aimPose = m_IsAimPoseTracked ? m_AimPose : Pose.identity;
                return m_IsAimPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal AimPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_AimPose;
            internal bool m_IsAimPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the aim activation value updates.
        /// </summary>
        public class AimActivateValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the aim activate value.
            /// </summary>
            /// <param name="aimActivateValue">
            /// Will be filled out with the aim activate value, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> and a valid value is filled out.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetAimActivateValue(out float aimActivateValue)
            {
                aimActivateValue = m_IsAimActivateValueReady ? m_AimActivateValue : 0f;
                return m_IsAimActivateValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal AimActivateValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_AimActivateValue;
            internal bool m_IsAimActivateValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the aim activate state updates.
        /// </summary>
        public class AimActivatedStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the aim is fully activated.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the aim activation state might not be available when this event is dispatched.
            /// When data is available, the function returns <c>true</c> and sets <paramref name="isAimActivated"/>
            /// to indicate whether the aim is fully activated. If this function returns <c>false</c>,
            /// <paramref name="isAimActivated"/> will also be <c>false</c> (whether or not the aim is actually activated).
            /// </remarks>
            /// <param name="isAimActivated">
            /// Will be set to <c>true</c> if aim is fully activated,
            /// otherwise <c>false</c>.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if a valid evaluation of the aim activation state is available.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetAimActivatedState(out bool isAimActivated)
            {
                isAimActivated = m_IsAimActivatedStateReady ? m_IsAimActivated : false;
                return m_IsAimActivatedStateReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal AimActivatedStateUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal bool m_IsAimActivated;
            internal bool m_IsAimActivatedStateReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the grasp value updates.
        /// </summary>
        public class GraspValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the grasp value.
            /// </summary>
            /// <param name="graspValue">
            /// Will be filled out with the grasp value, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> and a valid value is filled out.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetGraspValue(out float graspValue)
            {
                graspValue = m_IsGraspValueReady ? m_GraspValue : 0f;
                return m_IsGraspValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal GraspValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_GraspValue;
            internal bool m_IsGraspValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the firm grasp state updates.
        /// </summary>
        public class GraspFirmStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the user is making a fist.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the gesture might not be available when this event is dispatched. When data is available,
            /// the function returns <c>true</c> and sets <paramref name="isGraspFirm"/> to indicate
            /// whether the user is making a fist (firm grasp). If this function returns <c>false</c>,
            /// <paramref name="isGraspFirm"/> will also be <c>false</c> (whether or not the user is making a fist).
            /// </remarks>
            /// <param name="isGraspFirm">
            /// Will be set to <c>true</c> if the user is making a fist,
            /// otherwise <c>false</c>.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if a valid evaluation of the gesture is available.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetGraspFirmState(out bool isGraspFirm)
            {
                isGraspFirm = m_IsGraspFirmStateReady ? m_IsGraspFirm : false;
                return m_IsGraspFirmStateReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal GraspFirmStateUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal bool m_IsGraspFirm;
            internal bool m_IsGraspFirmStateReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the grip pose updates.
        /// </summary>
        public class GripPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the grip pose.
            /// </summary>
            /// <param name="gripPose">
            /// Will be filled out with the grip pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetGripPose(out Pose gripPose)
            {
                gripPose = m_IsGripPoseTracked ? m_GripPose : Pose.identity;
                return m_IsGripPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal GripPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_GripPose;
            internal bool m_IsGripPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the pinch pose updates.
        /// </summary>
        public class PinchPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the pinch pose.
            /// </summary>
            /// <param name="pinchPose">
            /// Will be filled out with the pinch pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetPinchPose(out Pose pinchPose)
            {
                pinchPose = m_IsPinchPoseTracked ? m_PinchPose : Pose.identity;
                return m_IsPinchPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PinchPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_PinchPose;
            internal bool m_IsPinchPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the pinch value updates.
        /// </summary>
        public class PinchValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the pinch value.
            /// </summary>
            /// <param name="pinchValue">
            /// Will be filled out with the pinch value, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> and a valid value is filled out.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetPinchValue(out float pinchValue)
            {
                pinchValue = m_IsPinchValueReady ? m_PinchValue : 0f;
                return m_IsPinchValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PinchValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_PinchValue;
            internal bool m_IsPinchValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the pinch touched state updates.
        /// </summary>
        public class PinchTouchedStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the hand is performing a pinch gesture.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the gesture might not be available when you call this function. When data is available,
            /// the function returns <c>true</c> and sets <paramref name="isPinched"/> to indicate
            /// whether the hand is currently pinching. If this function returns <c>false</c>,
            /// <paramref name="isPinched"/> will be <c>false</c> whether or not the hand is pinching.
            /// </remarks>
            /// <param name="isPinched">
            /// Will be set to <c>true</c> if the hand is pinching,
            /// otherwise <c>false</c>.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if a valid evaluation of the gesture is available.
            /// Returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetPinchTouchedState(out bool isPinched)
            {
                isPinched = m_IsPinchTouchedStateReady ? m_IsPinchTouched : false;
                return m_IsPinchTouchedStateReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PinchTouchedStateUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal bool m_IsPinchTouched;
            internal bool m_IsPinchTouchedStateReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the poke pose updates.
        /// </summary>
        public class PokePoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the poke pose.
            /// </summary>
            /// <param name="pokePose">
            /// Will be filled out with the poke pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
            /// </returns>
            public bool TryGetPokePose(out Pose pokePose)
            {
                pokePose = m_IsPokePoseTracked ? m_PokePose : Pose.identity;
                return m_IsPokePoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PokePoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_PokePose;
            internal bool m_IsPokePoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Attempts to get the aim pose.
        /// </summary>
        /// <param name="aimPose">
        /// Will be filled out with the aim pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetAimPose(out Pose aimPose)
        {
            if (m_AimPose == null)
            {
                aimPose = Pose.identity;
                return false;
            }

            return m_AimPose.TryGetAimPose(out aimPose);
        }

        /// <summary>
        /// Attempts to get the aim activate value.
        /// </summary>
        /// <param name="aimActivateValue">
        /// Will be filled out with the aim activate value, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> and a valid value is filled out.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetAimActivateValue(out float aimActivateValue)
        {
            if (m_AimActivateValue == null)
            {
                aimActivateValue = 0f;
                return false;
            }

            return m_AimActivateValue.TryGetAimActivateValue(out aimActivateValue);
        }

        /// <summary>
        /// Attempts to get whether the aim is fully activated.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the aim activation state might not be available when you call this function.
        /// When data is available, the function returns <c>true</c> and sets <paramref name="isAimActivated"/>
        /// to indicate whether the aim is fully activated. If this function returns <c>false</c>,
        /// <paramref name="isAimActivated"/> will be <c>false</c> whether or not the aim is actually activated.
        /// </remarks>
        /// <param name="isAimActivated">
        /// Will be set to <c>true</c> if the aim is fully activated,
        /// otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if a valid evaluation of the activation state is available.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetAimActivatedState(out bool isAimActivated)
        {
            if (m_AimActivatedState == null)
            {
                isAimActivated = false;
                return false;
            }

            return m_AimActivatedState.TryGetAimActivatedState(out isAimActivated);
        }

        /// <summary>
        /// Attempts to get the grasp value.
        /// </summary>
        /// <param name="graspValue">
        /// Will be filled out with the grasp value, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> and a valid value is filled out.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetGraspValue(out float graspValue)
        {
            if (m_GraspValue == null)
            {
                graspValue = 0f;
                return false;
            }

            return m_GraspValue.TryGetGraspValue(out graspValue);
        }

        /// <summary>
        /// Attempts to get whether the user is making a fist.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the gesture might not be available when you call this function. When data is available,
        /// the function returns <c>true</c> and sets <paramref name="isGraspFirm"/> to indicate
        /// whether the user is making a fist (firm grasp). If this function returns <c>false</c>,
        /// <paramref name="isGraspFirm"/> will be <c>false</c> whether or not the user is making a fist.
        /// </remarks>
        /// <param name="isGraspFirm">
        /// Will be set to <c>true</c> if the user is making a fist,
        /// otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if a valid evaluation of the gesture is available.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetGraspFirmState(out bool isGraspFirm)
        {
            if (m_GraspFirmState == null)
            {
                isGraspFirm = false;
                return false;
            }
            return m_GraspFirmState.TryGetGraspFirmState(out isGraspFirm);
        }

        /// <summary>
        /// Attempts to get the grip pose.
        /// </summary>
        /// <param name="gripPose">
        /// Will be filled out with the grip pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetGripPose(out Pose gripPose)
        {
            if (m_GripPose == null)
            {
                gripPose = Pose.identity;
                return false;
            }

            return m_GripPose.TryGetGripPose(out gripPose);
        }

        /// <summary>
        /// Attempts to get the pinch pose.
        /// </summary>
        /// <param name="pinchPose">
        /// Will be filled out with the pinch pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetPinchPose(out Pose pinchPose)
        {
            if (m_PinchPose == null)
            {
                pinchPose = Pose.identity;
                return false;
            }

            return m_PinchPose.TryGetPinchPose(out pinchPose);
        }

        /// <summary>
        /// Attempts to get the pinch value.
        /// </summary>
        /// <param name="pinchValue">
        /// Will be filled out with the pinch value, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> and a valid value is filled out.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetPinchValue(out float pinchValue)
        {
            if (m_PinchValue == null)
            {
                pinchValue = 0f;
                return false;
            }

            return m_PinchValue.TryGetPinchValue(out pinchValue);
        }

        /// <summary>
        /// Attempts to get whether the hand is performing a pinch gesture.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the pinch gesture might not be available when you call this function. When data is available,
        /// the function returns <c>true</c> and sets <paramref name="isPinchTouched"/> to indicate
        /// whether the hand is currently pinching. If this function returns <c>false</c>,
        /// <paramref name="isPinchTouched"/> will be <c>false</c> whether or not the hand is pinching.
        /// </remarks>
        /// <param name="isPinchTouched">
        /// Will be set to <c>true</c> if the hand is pinching,
        /// otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if a valid evaluation of the gesture is available.
        /// Returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetPinchTouchedState(out bool isPinchTouched)
        {
            if (m_PinchTouchedState == null)
            {
                isPinchTouched = false;
                return false;
            }

            return m_PinchTouchedState.TryGetPinchTouchedState(out isPinchTouched);
        }

        /// <summary>
        /// Attempts to get the poke pose.
        /// </summary>
        /// <param name="pokePose">
        /// Will be filled out with the poke pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful, returns <c>false</c> otherwise.
        /// </returns>
        public bool TryGetPokePose(out Pose pokePose)
        {
            if (m_PokePose == null)
            {
                pokePose = Pose.identity;
                return false;
            }

            return m_PokePose.TryGetPokePose(out pokePose);
        }

        /// <summary>
        /// Called when the aim pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<AimPoseUpdatedEventArgs> aimPoseUpdated;

        /// <summary>
        /// Called when the aim activate value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<AimActivateValueUpdatedEventArgs> aimActivateValueUpdated;

        /// <summary>
        /// Called when the aim activate state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<AimActivatedStateUpdatedEventArgs> aimActivatedStateUpdated;

        /// <summary>
        /// Called when the grasp value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<GraspValueUpdatedEventArgs> graspValueUpdated;

        /// <summary>
        /// Called when the firm grasp state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<GraspFirmStateUpdatedEventArgs> graspFirmStateUpdated;

        /// <summary>
        /// Called when the grip pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<GripPoseUpdatedEventArgs> gripPoseUpdated;

        /// <summary>
        /// Called when the pinch pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PinchPoseUpdatedEventArgs> pinchPoseUpdated;

        /// <summary>
        /// Called when the pinch value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PinchValueUpdatedEventArgs> pinchValueUpdated;

        /// <summary>
        /// Called when the pinch state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PinchTouchedStateUpdatedEventArgs> pinchTouchedStateUpdated;

        /// <summary>
        /// Called when the poke pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PokePoseUpdatedEventArgs> pokePoseUpdated;

        /// <summary>
        /// Describes the validity of data found in this <c>XRCommonHandGesturesState</c>.
        /// </summary>
        public XRCommonHandGesturesFlags flags => m_CommonGesturesFlags;

        internal void OnSwitchDelegationType(ref XRCommonHandGesturesState copyTo, in XRCommonHandGesturesState copyFrom)
        {
            copyTo = copyFrom;
            if (!copyFrom.handedness.IsValid())
                return;

            if (m_Handedness != copyFrom.handedness)
                Debug.LogWarning("Copying from incorrect state block! Copying from '" + copyFrom.handedness + "' while our handedness is '" + m_Handedness + "'.");

            m_CommonGesturesFlags = copyFrom.flags;

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimPoseValid) != 0)
                UpdateAimPose(copyFrom.possiblyInvalidAimPose, false);
            else
                InvalidateAimPose(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivateValueValid) != 0)
                UpdateAimActivateValue(copyFrom.possiblyInvalidAimActivateValue, false);
            else
                InvalidateAimActivateValue(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspValueValid) != 0)
                UpdateGraspValue(copyFrom.possiblyInvalidGraspValue, false);
            else
                InvalidateGraspValue(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGripPoseValid) != 0)
                UpdateGripPose(copyFrom.possiblyInvalidGripPose, false);
            else
                InvalidateGripPose(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchPoseValid) != 0)
                UpdatePinchPose(copyFrom.possiblyInvalidPinchPose, false);
            else
                InvalidatePinchPose(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchValueValid) != 0)
                UpdatePinchValue(copyFrom.possiblyInvalidPinchValue, false);
            else
                InvalidatePinchValue(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPokePoseValid) != 0)
                UpdatePokePose(copyFrom.possiblyInvalidPokePose, false);
            else
                InvalidatePokePose(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsAimActivatedStateValid) != 0)
                UpdateAimActivatedState(copyFrom.possiblyInvalidIsAimActivated, false);
            else
                InvalidateAimActivatedState(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsGraspFirmStateValid) != 0)
                UpdateGraspFirmState(copyFrom.possiblyInvalidIsGraspFirm, false);
            else
                InvalidateGraspFirmState(false);

            if ((m_CommonGesturesFlags & XRCommonHandGesturesFlags.IsPinchTouchedStateValid) != 0)
                UpdatePinchTouchedState(copyFrom.possiblyInvalidIsPinchTouched, false);
            else
                InvalidatePinchTouchedState(false);
        }

        internal void UpdateAimPose(Pose aimPose, bool allowFireCallback = true)
        {
            if (m_AimPose == null)
                m_AimPose = new AimPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimPoseUpdated != null &&
                (!m_AimPose.m_IsAimPoseTracked || aimPose != m_AimPose.m_AimPose);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsAimPoseValid;
            m_AimPose.m_IsAimPoseTracked = true;
            m_AimPose.m_AimPose = aimPose;

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void InvalidateAimPose(bool allowFireCallback = true)
        {
            if (m_AimPose == null)
                m_AimPose = new AimPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimPoseUpdated != null && m_AimPose.m_IsAimPoseTracked;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsAimPoseValid;
            m_AimPose.m_IsAimPoseTracked = false;

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void UpdateAimActivateValue(float aimActivateValue, bool allowFireCallback = true)
        {
            if (m_AimActivateValue == null)
                m_AimActivateValue = new AimActivateValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimActivateValueUpdated != null &&
                (!m_AimActivateValue.m_IsAimActivateValueReady || aimActivateValue != m_AimActivateValue.m_AimActivateValue);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsAimActivateValueValid;
            m_AimActivateValue.m_IsAimActivateValueReady = true;
            m_AimActivateValue.m_AimActivateValue = aimActivateValue;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void InvalidateAimActivateValue(bool allowFireCallback = true)
        {
            if (m_AimActivateValue == null)
                m_AimActivateValue = new AimActivateValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimActivateValueUpdated != null && m_AimActivateValue.m_IsAimActivateValueReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsAimActivateValueValid;
            m_AimActivateValue.m_IsAimActivateValueReady = false;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void UpdateAimActivatedState(bool isAimActivated, bool allowFireCallback = true)
        {
            if (m_AimActivatedState == null)
                m_AimActivatedState = new AimActivatedStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimActivatedStateUpdated != null &&
                (!m_AimActivatedState.m_IsAimActivatedStateReady || isAimActivated != m_AimActivatedState.m_IsAimActivated);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsAimActivatedStateValid;
            m_AimActivatedState.m_IsAimActivatedStateReady = true;
            m_AimActivatedState.m_IsAimActivated = isAimActivated;

            if (fire)
                aimActivatedStateUpdated.Invoke(m_AimActivatedState);
        }

        internal void InvalidateAimActivatedState(bool allowFireCallback = true)
        {
            if (m_AimActivatedState == null)
                m_AimActivatedState = new AimActivatedStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && aimActivatedStateUpdated != null && m_AimActivatedState.m_IsAimActivatedStateReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsAimActivatedStateValid;
            m_AimActivatedState.m_IsAimActivatedStateReady = false;

            if (fire)
                aimActivatedStateUpdated.Invoke(m_AimActivatedState);
        }

        internal void UpdateGraspValue(float graspValue, bool allowFireCallback = true)
        {
            if (m_GraspValue == null)
                m_GraspValue = new GraspValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && graspValueUpdated != null &&
                (!m_GraspValue.m_IsGraspValueReady || graspValue != m_GraspValue.m_GraspValue);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsGraspValueValid;
            m_GraspValue.m_IsGraspValueReady = true;
            m_GraspValue.m_GraspValue = graspValue;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void InvalidateGraspValue(bool allowFireCallback = true)
        {
            if (m_GraspValue == null)
                m_GraspValue = new GraspValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && graspValueUpdated != null && m_GraspValue.m_IsGraspValueReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsGraspValueValid;
            m_GraspValue.m_IsGraspValueReady = false;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void UpdateGraspFirmState(bool isGraspFirm, bool allowFireCallback = true)
        {
            if (m_GraspFirmState == null)
                m_GraspFirmState = new GraspFirmStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && graspFirmStateUpdated != null &&
                (!m_GraspFirmState.m_IsGraspFirmStateReady || isGraspFirm != m_GraspFirmState.m_IsGraspFirm);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsGraspFirmStateValid;
            m_GraspFirmState.m_IsGraspFirmStateReady = true;
            m_GraspFirmState.m_IsGraspFirm = isGraspFirm;

            if (fire)
                graspFirmStateUpdated.Invoke(m_GraspFirmState);
        }

        internal void InvalidateGraspFirmState(bool allowFireCallback = true)
        {
            if (m_GraspFirmState == null)
                m_GraspFirmState = new GraspFirmStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && graspFirmStateUpdated != null && m_GraspFirmState.m_IsGraspFirmStateReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsGraspFirmStateValid;
            m_GraspFirmState.m_IsGraspFirmStateReady = false;

            if (fire)
                graspFirmStateUpdated.Invoke(m_GraspFirmState);
        }

        internal void UpdateGripPose(Pose gripPose, bool allowFireCallback = true)
        {
            if (m_GripPose == null)
                m_GripPose = new GripPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && gripPoseUpdated != null &&
                (!m_GripPose.m_IsGripPoseTracked || gripPose != m_GripPose.m_GripPose);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsGripPoseValid;
            m_GripPose.m_IsGripPoseTracked = true;
            m_GripPose.m_GripPose = gripPose;

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void InvalidateGripPose(bool allowFireCallback = true)
        {
            if (m_GripPose == null)
                m_GripPose = new GripPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && gripPoseUpdated != null && m_GripPose.m_IsGripPoseTracked;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsGripPoseValid;
            m_GripPose.m_IsGripPoseTracked = false;

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void UpdatePinchPose(Pose pinchPose, bool allowFireCallback = true)
        {
            if (m_PinchPose == null)
                m_PinchPose = new PinchPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchPoseUpdated != null &&
                (!m_PinchPose.m_IsPinchPoseTracked || pinchPose != m_PinchPose.m_PinchPose);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsPinchPoseValid;
            m_PinchPose.m_IsPinchPoseTracked = true;
            m_PinchPose.m_PinchPose = pinchPose;

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void InvalidatePinchPose(bool allowFireCallback = true)
        {
            if (m_PinchPose == null)
                m_PinchPose = new PinchPoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchPoseUpdated != null && m_PinchPose.m_IsPinchPoseTracked;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsPinchPoseValid;
            m_PinchPose.m_IsPinchPoseTracked = false;

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void UpdatePinchValue(float pinchValue, bool allowFireCallback = true)
        {
            if (m_PinchValue == null)
                m_PinchValue = new PinchValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchValueUpdated != null &&
                (!m_PinchValue.m_IsPinchValueReady || pinchValue != m_PinchValue.m_PinchValue);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsPinchValueValid;
            m_PinchValue.m_IsPinchValueReady = true;
            m_PinchValue.m_PinchValue = pinchValue;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void InvalidatePinchValue(bool allowFireCallback = true)
        {
            if (m_PinchValue == null)
                m_PinchValue = new PinchValueUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchValueUpdated != null && m_PinchValue.m_IsPinchValueReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsPinchValueValid;
            m_PinchValue.m_IsPinchValueReady = false;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void UpdatePinchTouchedState(bool isPinchTouched, bool allowFireCallback = true)
        {
            if (m_PinchTouchedState == null)
                m_PinchTouchedState = new PinchTouchedStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchTouchedStateUpdated != null &&
                (!m_PinchTouchedState.m_IsPinchTouchedStateReady ||
                    isPinchTouched != m_PinchTouchedState.m_IsPinchTouched);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsPinchTouchedStateValid;
            m_PinchTouchedState.m_IsPinchTouchedStateReady = true;
            m_PinchTouchedState.m_IsPinchTouched = isPinchTouched;

            if (fire)
                pinchTouchedStateUpdated.Invoke(m_PinchTouchedState);
        }

        internal void InvalidatePinchTouchedState(bool allowFireCallback = true)
        {
            if (m_PinchTouchedState == null)
                m_PinchTouchedState = new PinchTouchedStateUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pinchTouchedStateUpdated != null && m_PinchTouchedState.m_IsPinchTouchedStateReady;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsPinchTouchedStateValid;
            m_PinchTouchedState.m_IsPinchTouchedStateReady = false;

            if (fire)
                pinchTouchedStateUpdated.Invoke(m_PinchTouchedState);
        }

        internal void UpdatePokePose(Pose pokePose, bool allowFireCallback = true)
        {
            if (m_PokePose == null)
                m_PokePose = new PokePoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pokePoseUpdated != null &&
                (!m_PokePose.m_IsPokePoseTracked || pokePose != m_PokePose.m_PokePose);

            m_CommonGesturesFlags |= XRCommonHandGesturesFlags.IsPokePoseValid;
            m_PokePose.m_IsPokePoseTracked = true;
            m_PokePose.m_PokePose = pokePose;

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal void InvalidatePokePose(bool allowFireCallback = true)
        {
            if (m_PokePose == null)
                m_PokePose = new PokePoseUpdatedEventArgs(m_Handedness);

            bool fire = allowFireCallback && pokePoseUpdated != null && m_PokePose.m_IsPokePoseTracked;

            m_CommonGesturesFlags &= ~XRCommonHandGesturesFlags.IsPokePoseValid;
            m_PokePose.m_IsPokePoseTracked = false;

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal XRCommonHandGestures(Handedness handedness) => m_Handedness = handedness;

        internal Handedness handedness => m_Handedness;
        internal Pose aimPose => m_AimPose != null ? m_AimPose.m_AimPose : Pose.identity;
        internal float aimActivateValue => m_AimActivateValue != null ? m_AimActivateValue.m_AimActivateValue : 0f;
        internal float graspValue => m_GraspValue != null ? m_GraspValue.m_GraspValue : 0f;
        internal Pose gripPose => m_GripPose != null ? m_GripPose.m_GripPose : Pose.identity;
        internal Pose pinchPose => m_PinchPose != null ? m_PinchPose.m_PinchPose : Pose.identity;
        internal float pinchValue => m_PinchValue != null ? m_PinchValue.m_PinchValue : 0f;
        internal Pose pokePose => m_PokePose != null ? m_PokePose.m_PokePose : Pose.identity;

        readonly Handedness m_Handedness;
        AimPoseUpdatedEventArgs m_AimPose;
        AimActivateValueUpdatedEventArgs m_AimActivateValue;
        AimActivatedStateUpdatedEventArgs m_AimActivatedState;
        GraspValueUpdatedEventArgs m_GraspValue;
        GraspFirmStateUpdatedEventArgs m_GraspFirmState;
        GripPoseUpdatedEventArgs m_GripPose;
        PinchPoseUpdatedEventArgs m_PinchPose;
        PinchValueUpdatedEventArgs m_PinchValue;
        PinchTouchedStateUpdatedEventArgs m_PinchTouchedState;
        PokePoseUpdatedEventArgs m_PokePose;
        XRCommonHandGesturesFlags m_CommonGesturesFlags;
    }
}
