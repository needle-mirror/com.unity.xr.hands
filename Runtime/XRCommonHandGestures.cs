using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

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
        /// <seealso cref="aimPoseUpdated"/>
        public class AimPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the aim pose.
            /// </summary>
            /// <param name="aimPose">
            /// Will be filled out with the aim pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetAimPose(out Pose aimPose) => m_CommonGestures.TryGetAimPose(out aimPose);

            /// <summary>
            /// Gets whether the aim pose is tracked.
            /// </summary>
            /// <returns>
            /// Returns <see langword="true"/> if the aim pose is tracked.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            /// <seealso cref="TrackedDevice.isTracked"/>
            /// <seealso cref="PoseControl.isTracked"/>
            public bool GetAimPoseIsTracked() => m_CommonGestures.GetAimPoseIsTracked();

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal AimPoseUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the aim activation value updates.
        /// </summary>
        /// <seealso cref="aimActivateValueUpdated"/>
        public class AimActivateValueUpdatedEventArgs
        {
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
            public bool TryGetAimActivateValue(out float aimActivateValue) => m_CommonGestures.TryGetAimActivateValue(out aimActivateValue);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal AimActivateValueUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the aim activate state updates.
        /// </summary>
        /// <seealso cref="aimActivatedStateUpdated"/>
        public class AimActivatedStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the aim is fully activated.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the aim activation state might not be available when this event is dispatched.
            /// When data is available, the function returns <see langword="true"/> and sets <paramref name="isAimActivated"/>
            /// to indicate whether the aim is fully activated. If this function returns <see langword="false"/>,
            /// <paramref name="isAimActivated"/> will also be <see langword="false"/> (whether or not the aim is actually activated).
            /// </remarks>
            /// <param name="isAimActivated">
            /// Will be set to <see langword="true"/> if aim is fully activated,
            /// otherwise <see langword="false"/>.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if a valid evaluation of the aim activation state is available.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetAimActivatedState(out bool isAimActivated) => m_CommonGestures.TryGetAimActivatedState(out isAimActivated);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal AimActivatedStateUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the grasp value updates.
        /// </summary>
        /// <seealso cref="graspValueUpdated"/>
        public class GraspValueUpdatedEventArgs
        {
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
            public bool TryGetGraspValue(out float graspValue) => m_CommonGestures.TryGetGraspValue(out graspValue);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal GraspValueUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the firm grasp state updates.
        /// </summary>
        /// <seealso cref="graspFirmStateUpdated"/>
        public class GraspFirmStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the user is making a fist.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the gesture might not be available when this event is dispatched. When data is available,
            /// the function returns <see langword="true"/> and sets <paramref name="isGraspFirm"/> to indicate
            /// whether the user is making a fist (firm grasp). If this function returns <see langword="false"/>,
            /// <paramref name="isGraspFirm"/> will also be <see langword="false"/> (whether or not the user is making a fist).
            /// </remarks>
            /// <param name="isGraspFirm">
            /// Will be set to <see langword="true"/> if the user is making a fist,
            /// otherwise <see langword="false"/>.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if a valid evaluation of the gesture is available.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetGraspFirmState(out bool isGraspFirm) => m_CommonGestures.TryGetGraspFirmState(out isGraspFirm);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal GraspFirmStateUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the grip pose updates.
        /// </summary>
        /// <seealso cref="gripPoseUpdated"/>
        public class GripPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the grip pose.
            /// </summary>
            /// <param name="gripPose">
            /// Will be filled out with the grip pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetGripPose(out Pose gripPose) => m_CommonGestures.TryGetGripPose(out gripPose);

            /// <summary>
            /// Gets whether the grip pose is tracked.
            /// </summary>
            /// <returns>
            /// Returns <see langword="true"/> if the grip pose is tracked.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            /// <seealso cref="TrackedDevice.isTracked"/>
            /// <seealso cref="PoseControl.isTracked"/>
            public bool GetGripPoseIsTracked() => m_CommonGestures.GetGripPoseIsTracked();

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal GripPoseUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the pinch pose updates.
        /// </summary>
        /// <seealso cref="pinchPoseUpdated"/>
        public class PinchPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the pinch pose.
            /// </summary>
            /// <param name="pinchPose">
            /// Will be filled out with the pinch pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPinchPose(out Pose pinchPose) => m_CommonGestures.TryGetPinchPose(out pinchPose);

            /// <summary>
            /// Gets whether the pinch pose is tracked.
            /// </summary>
            /// <returns>
            /// Returns <see langword="true"/> if the pinch pose is tracked.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            /// <seealso cref="TrackedDevice.isTracked"/>
            /// <seealso cref="PoseControl.isTracked"/>
            public bool GetPinchPoseIsTracked() => m_CommonGestures.GetPinchPoseIsTracked();

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal PinchPoseUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the pinch value updates.
        /// </summary>
        /// <seealso cref="pinchValueUpdated"/>
        public class PinchValueUpdatedEventArgs
        {
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
            public bool TryGetPinchValue(out float pinchValue) => m_CommonGestures.TryGetPinchValue(out pinchValue);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal PinchValueUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the pinch touched state updates.
        /// </summary>
        /// <seealso cref="pinchTouchedStateUpdated"/>
        public class PinchTouchedStateUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get whether the hand is performing a pinch gesture.
            /// </summary>
            /// <remarks>
            /// Data to evaluate the gesture might not be available when you call this function. When data is available,
            /// the function returns <see langword="true"/> and sets <paramref name="isPinched"/> to indicate
            /// whether the hand is currently pinching. If this function returns <see langword="false"/>,
            /// <paramref name="isPinched"/> will be <see langword="false"/> whether or not the hand is pinching.
            /// </remarks>
            /// <param name="isPinched">
            /// Will be set to <see langword="true"/> if the hand is pinching,
            /// otherwise <see langword="false"/>.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if a valid evaluation of the gesture is available.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPinchTouchedState(out bool isPinched) => m_CommonGestures.TryGetPinchTouchedState(out isPinched);

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal PinchTouchedStateUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
        }

        /// <summary>
        /// Event-args type for when the poke pose updates.
        /// </summary>
        /// <seealso cref="pokePoseUpdated"/>
        public class PokePoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the poke pose.
            /// </summary>
            /// <param name="pokePose">
            /// Will be filled out with the poke pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPokePose(out Pose pokePose) => m_CommonGestures.TryGetPokePose(out pokePose);

            /// <summary>
            /// Gets whether the poke pose is tracked.
            /// </summary>
            /// <returns>
            /// Returns <see langword="true"/> if the poke pose is tracked.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            /// <seealso cref="TrackedDevice.isTracked"/>
            /// <seealso cref="PoseControl.isTracked"/>
            public bool GetPokePoseIsTracked() => m_CommonGestures.GetPokePoseIsTracked();

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_CommonGestures.handedness;

            /// <summary>
            /// The common gestures that raised this event.
            /// </summary>
            public XRCommonHandGestures commonGestures => m_CommonGestures;

            internal PokePoseUpdatedEventArgs(XRCommonHandGestures commonGestures) => m_CommonGestures = commonGestures;

            readonly XRCommonHandGestures m_CommonGestures;
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
        public bool TryGetAimPose(out Pose aimPose) => m_State.TryGetAimPose(out aimPose);

        /// <summary>
        /// Gets whether the aim pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the aim pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public bool GetAimPoseIsTracked() => m_State.GetAimPoseIsTracked();

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
        public bool TryGetAimActivateValue(out float aimActivateValue) => m_State.TryGetAimActivateValue(out aimActivateValue);

        /// <summary>
        /// Attempts to get whether the aim is fully activated.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the aim activation state might not be available when you call this function.
        /// When data is available, the function returns <see langword="true"/> and sets <paramref name="isAimActivated"/>
        /// to indicate whether the aim is fully activated. If this function returns <see langword="false"/>,
        /// <paramref name="isAimActivated"/> will be <see langword="false"/> whether or not the aim is actually activated.
        /// </remarks>
        /// <param name="isAimActivated">
        /// Will be set to <see langword="true"/> if the aim is fully activated,
        /// otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the activation state is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetAimActivatedState(out bool isAimActivated) => m_State.TryGetAimActivatedState(out isAimActivated);

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
        public bool TryGetGraspValue(out float graspValue) => m_State.TryGetGraspValue(out graspValue);

        /// <summary>
        /// Attempts to get whether the user is making a fist.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the gesture might not be available when you call this function. When data is available,
        /// the function returns <see langword="true"/> and sets <paramref name="isGraspFirm"/> to indicate
        /// whether the user is making a fist (firm grasp). If this function returns <see langword="false"/>,
        /// <paramref name="isGraspFirm"/> will be <see langword="false"/> whether or not the user is making a fist.
        /// </remarks>
        /// <param name="isGraspFirm">
        /// Will be set to <see langword="true"/> if the user is making a fist,
        /// otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the gesture is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetGraspFirmState(out bool isGraspFirm) => m_State.TryGetGraspFirmState(out isGraspFirm);

        /// <summary>
        /// Attempts to get the grip pose.
        /// </summary>
        /// <param name="gripPose">
        /// Will be filled out with the grip pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetGripPose(out Pose gripPose) => m_State.TryGetGripPose(out gripPose);

        /// <summary>
        /// Gets whether the grip pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the grip pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public bool GetGripPoseIsTracked() => m_State.GetGripPoseIsTracked();

        /// <summary>
        /// Attempts to get the pinch pose.
        /// </summary>
        /// <param name="pinchPose">
        /// Will be filled out with the pinch pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPinchPose(out Pose pinchPose) => m_State.TryGetPinchPose(out pinchPose);

        /// <summary>
        /// Gets whether the pinch pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the pinch pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public bool GetPinchPoseIsTracked() => m_State.GetPinchPoseIsTracked();

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
        public bool TryGetPinchValue(out float pinchValue) => m_State.TryGetPinchValue(out pinchValue);

        /// <summary>
        /// Attempts to get whether the hand is performing a pinch gesture.
        /// </summary>
        /// <remarks>
        /// Data to evaluate the pinch gesture might not be available when you call this function. When data is available,
        /// the function returns <see langword="true"/> and sets <paramref name="isPinchTouched"/> to indicate
        /// whether the hand is currently pinching. If this function returns <see langword="false"/>,
        /// <paramref name="isPinchTouched"/> will be <see langword="false"/> whether or not the hand is pinching.
        /// </remarks>
        /// <param name="isPinchTouched">
        /// Will be set to <see langword="true"/> if the hand is pinching,
        /// otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid evaluation of the gesture is available.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPinchTouchedState(out bool isPinchTouched) => m_State.TryGetPinchTouchedState(out isPinchTouched);

        /// <summary>
        /// Attempts to get the poke pose.
        /// </summary>
        /// <param name="pokePose">
        /// Will be filled out with the poke pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPokePose(out Pose pokePose) => m_State.TryGetPokePose(out pokePose);

        /// <summary>
        /// Gets whether the poke pose is tracked.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if the poke pose is tracked.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="PoseControl.isTracked"/>
        public bool GetPokePoseIsTracked() => m_State.GetPokePoseIsTracked();

        /// <summary>
        /// Called when the aim pose is updated. Either the pose changed,
        /// the ability to retrieve it changed, or its tracking status changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="AimPoseUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<AimPoseUpdatedEventArgs> aimPoseUpdated;

        /// <summary>
        /// Called when the aim activate value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="AimActivateValueUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<AimActivateValueUpdatedEventArgs> aimActivateValueUpdated;

        /// <summary>
        /// Called when the aim activate state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="AimActivatedStateUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<AimActivatedStateUpdatedEventArgs> aimActivatedStateUpdated;

        /// <summary>
        /// Called when the grasp value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="GraspValueUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<GraspValueUpdatedEventArgs> graspValueUpdated;

        /// <summary>
        /// Called when the firm grasp state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="GraspFirmStateUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<GraspFirmStateUpdatedEventArgs> graspFirmStateUpdated;

        /// <summary>
        /// Called when the grip pose is updated. Either the pose changed,
        /// the ability to retrieve it changed, or its tracking status changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="GripPoseUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<GripPoseUpdatedEventArgs> gripPoseUpdated;

        /// <summary>
        /// Called when the pinch pose is updated. Either the pose changed,
        /// the ability to retrieve it changed, or its tracking status changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="PinchPoseUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<PinchPoseUpdatedEventArgs> pinchPoseUpdated;

        /// <summary>
        /// Called when the pinch value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="PinchValueUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<PinchValueUpdatedEventArgs> pinchValueUpdated;

        /// <summary>
        /// Called when the pinch state is updated. Either the state changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="PinchTouchedStateUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<PinchTouchedStateUpdatedEventArgs> pinchTouchedStateUpdated;

        /// <summary>
        /// Called when the poke pose is updated. Either the pose changed,
        /// the ability to retrieve it changed, or its tracking status changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="PokePoseUpdatedEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        public Action<PokePoseUpdatedEventArgs> pokePoseUpdated;

        /// <summary>
        /// Denotes which hand this represents common gestures data for.
        /// </summary>
        public Handedness handedness => m_Handedness;

        /// <summary>
        /// Describes the validity of data found in this common gestures data.
        /// </summary>
        public XRCommonHandGesturesFlags flags => m_State.flags;

        /// <summary>
        /// A copy of the state that backs this common gestures data.
        /// </summary>
        internal XRCommonHandGesturesState stateInternal => m_State;

        internal void UpdateState(XRCommonHandGesturesState newState)
        {
            // Compare old state with new state to determine which events need to fire.
            var oldFlags = m_State.flags;
            var newFlags = newState.flags;

            // Poses are frozen in place when the tracking state does not have the flags for position and rotation.
            // The provider indicates this by combining both InputTrackingState.Position | InputTrackingState.Rotation
            // into a single gesture flag (such as XRCommonHandGesturesFlags.IsAimPoseValid). (As an aside, we should
            // ideally have stored the tracking state instead of combining into a single boolean flag to allow position
            // and rotation to be updated separately, such as for controller devices which may still update rotation but
            // not position when occluded, but currently we can only use the single Valid flag for both pose components).
            //
            // Recorded hand tracking data does not store the pose each frame that the pose does not have the Valid flag,
            // so the pose value coming in could be the default Pose.identity. This may be done for efficiency in storing
            // the recording blob of captured frames.
            //
            // To allow the historical last valid pose to always be queryable each frame, we replace the incoming new state
            // pose data with the old poses if the Valid flag is not set in the new state since the new pose would likely
            // be Pose.identity instead of the last valid pose.
            //
            // We don't need to do this for the other float/bool values gated on a ready signal because the OpenXR spec
            // requires that the value is 0 when ready is false.
            if (!newFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid))
                newState.aimPoseInternal = m_State.aimPoseInternal;
            if (!newFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid))
                newState.gripPoseInternal = m_State.gripPoseInternal;
            if (!newFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid))
                newState.pinchPoseInternal = m_State.pinchPoseInternal;
            if (!newFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid))
                newState.pokePoseInternal = m_State.pokePoseInternal;

            var fireAimPoseUpdated = aimPoseUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsAimPoseValid | XRCommonHandGesturesFlags.IsAimPoseTracked) ||
                    m_State.aimPoseInternal != newState.aimPoseInternal);
            var fireAimActivateValueUpdated = aimActivateValueUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsAimActivateValueValid) || m_State.aimActivateValueInternal != newState.aimActivateValueInternal);
            var fireAimActivatedStateUpdated = aimActivatedStateUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsAimActivatedStateValid) || m_State.isAimActivatedInternal != newState.isAimActivatedInternal);
            var fireGraspValueUpdated = graspValueUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsGraspValueValid) || m_State.graspValueInternal != newState.graspValueInternal);
            var fireGraspFirmStateUpdated = graspFirmStateUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsGraspFirmStateValid) || m_State.isGraspFirmInternal != newState.isGraspFirmInternal);
            var fireGripPoseUpdated = gripPoseUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsGripPoseValid | XRCommonHandGesturesFlags.IsGripPoseTracked) ||
                    m_State.gripPoseInternal != newState.gripPoseInternal);
            var firePinchPoseUpdated = pinchPoseUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsPinchPoseValid | XRCommonHandGesturesFlags.IsPinchPoseTracked) ||
                    m_State.pinchPoseInternal != newState.pinchPoseInternal);
            var firePinchValueUpdated = pinchValueUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsPinchValueValid) || m_State.pinchValueInternal != newState.pinchValueInternal);
            var firePinchTouchedStateUpdated = pinchTouchedStateUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsPinchTouchedStateValid) || m_State.isPinchTouchedInternal != newState.isPinchTouchedInternal);
            var firePokePoseUpdated = pokePoseUpdated != null &&
                (FlagsDiffer(oldFlags, newFlags, XRCommonHandGesturesFlags.IsPokePoseValid | XRCommonHandGesturesFlags.IsPokePoseTracked) ||
                    m_State.pokePoseInternal != newState.pokePoseInternal);

            m_State = newState;

            if (fireAimPoseUpdated)
                aimPoseUpdated.Invoke(m_AimPose);
            if (fireAimActivateValueUpdated)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
            if (fireAimActivatedStateUpdated)
                aimActivatedStateUpdated.Invoke(m_AimActivatedState);
            if (fireGraspValueUpdated)
                graspValueUpdated.Invoke(m_GraspValue);
            if (fireGraspFirmStateUpdated)
                graspFirmStateUpdated.Invoke(m_GraspFirmState);
            if (fireGripPoseUpdated)
                gripPoseUpdated.Invoke(m_GripPose);
            if (firePinchPoseUpdated)
                pinchPoseUpdated.Invoke(m_PinchPose);
            if (firePinchValueUpdated)
                pinchValueUpdated.Invoke(m_PinchValue);
            if (firePinchTouchedStateUpdated)
                pinchTouchedStateUpdated.Invoke(m_PinchTouchedState);
            if (firePokePoseUpdated)
                pokePoseUpdated.Invoke(m_PokePose);

            return;

            static bool FlagsDiffer(XRCommonHandGesturesFlags a, XRCommonHandGesturesFlags b, XRCommonHandGesturesFlags gestureFlag)
            {
                return ((a ^ b) & gestureFlag) != 0;
            }
        }

        internal void UpdateAimPose(Pose aimPose, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimPoseUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid) || m_State.aimPoseInternal != aimPose);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);
            m_State.aimPoseInternal = aimPose;

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void InvalidateAimPose(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimPoseUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);

            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid);

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void UpdateAimActivateValue(float aimActivateValue, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimActivateValueUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid) || m_State.aimActivateValueInternal != aimActivateValue);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid);
            m_State.aimActivateValueInternal = aimActivateValue;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void InvalidateAimActivateValue(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimActivateValueUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid);
            m_State.aimActivateValueInternal = 0f;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void UpdateAimActivatedState(bool isAimActivated, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimActivatedStateUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivatedStateValid) || m_State.isAimActivatedInternal != isAimActivated);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsAimActivatedStateValid);
            m_State.isAimActivatedInternal = isAimActivated;

            if (fire)
                aimActivatedStateUpdated.Invoke(m_AimActivatedState);
        }

        internal void InvalidateAimActivatedState(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && aimActivatedStateUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivatedStateValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsAimActivatedStateValid);
            m_State.isAimActivatedInternal = false;

            if (fire)
                aimActivatedStateUpdated.Invoke(m_AimActivatedState);
        }

        internal void UpdateGraspValue(float graspValue, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && graspValueUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid) || m_State.graspValueInternal != graspValue);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid);
            m_State.graspValueInternal = graspValue;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void InvalidateGraspValue(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && graspValueUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid);
            m_State.graspValueInternal = 0f;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void UpdateGraspFirmState(bool isGraspFirm, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && graspFirmStateUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspFirmStateValid) || m_State.isGraspFirmInternal != isGraspFirm);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGraspFirmStateValid);
            m_State.isGraspFirmInternal = isGraspFirm;

            if (fire)
                graspFirmStateUpdated.Invoke(m_GraspFirmState);
        }

        internal void InvalidateGraspFirmState(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && graspFirmStateUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspFirmStateValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsGraspFirmStateValid);
            m_State.isGraspFirmInternal = false;

            if (fire)
                graspFirmStateUpdated.Invoke(m_GraspFirmState);
        }

        internal void UpdateGripPose(Pose gripPose, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && gripPoseUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid) || m_State.gripPoseInternal != gripPose);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid);
            m_State.gripPoseInternal = gripPose;

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void InvalidateGripPose(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && gripPoseUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid);

            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid);

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void UpdatePinchPose(Pose pinchPose, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchPoseUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid) || m_State.pinchPoseInternal != pinchPose);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid);
            m_State.pinchPoseInternal = pinchPose;

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void InvalidatePinchPose(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchPoseUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid);

            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid);

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void UpdatePinchValue(float pinchValue, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchValueUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid) || m_State.pinchValueInternal != pinchValue);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid);
            m_State.pinchValueInternal = pinchValue;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void InvalidatePinchValue(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchValueUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid);
            m_State.pinchValueInternal = 0f;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void UpdatePinchTouchedState(bool isPinchTouched, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchTouchedStateUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchTouchedStateValid) || m_State.isPinchTouchedInternal != isPinchTouched);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPinchTouchedStateValid);
            m_State.isPinchTouchedInternal = isPinchTouched;

            if (fire)
                pinchTouchedStateUpdated.Invoke(m_PinchTouchedState);
        }

        internal void InvalidatePinchTouchedState(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pinchTouchedStateUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchTouchedStateValid);

            // When ready is false, the value must be 0, so explicitly clear it rather than freezing it in place like poses.
            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsPinchTouchedStateValid);
            m_State.isPinchTouchedInternal = false;

            if (fire)
                pinchTouchedStateUpdated.Invoke(m_PinchTouchedState);
        }

        internal void UpdatePokePose(Pose pokePose, bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pokePoseUpdated != null &&
                (!m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid) || m_State.pokePoseInternal != pokePose);

            m_State.flags = m_State.flags.WithGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid);
            m_State.pokePoseInternal = pokePose;

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal void InvalidatePokePose(bool allowFireCallback = true)
        {
            var fire = allowFireCallback && pokePoseUpdated != null &&
                m_State.flags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid);

            m_State.flags = m_State.flags.WithoutGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid);

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal XRCommonHandGestures(Handedness handedness)
        {
            m_Handedness = handedness;
            m_State.handedness = handedness;

            // Explicitly initialize all rotations in the state to identity (Quaternion(0f, 0f, 0f, 1f))
            // instead of the struct default (Quaternion(0f, 0f, 0f, 0f)).
            m_State.aimPoseInternal = Pose.identity;
            m_State.gripPoseInternal = Pose.identity;
            m_State.pinchPoseInternal = Pose.identity;
            m_State.pokePoseInternal = Pose.identity;

            m_AimPose = new AimPoseUpdatedEventArgs(this);
            m_AimActivateValue = new AimActivateValueUpdatedEventArgs(this);
            m_AimActivatedState = new AimActivatedStateUpdatedEventArgs(this);
            m_GraspValue = new GraspValueUpdatedEventArgs(this);
            m_GraspFirmState = new GraspFirmStateUpdatedEventArgs(this);
            m_GripPose = new GripPoseUpdatedEventArgs(this);
            m_PinchPose = new PinchPoseUpdatedEventArgs(this);
            m_PinchValue = new PinchValueUpdatedEventArgs(this);
            m_PinchTouchedState = new PinchTouchedStateUpdatedEventArgs(this);
            m_PokePose = new PokePoseUpdatedEventArgs(this);
        }

        readonly Handedness m_Handedness;
        XRCommonHandGesturesState m_State;

        readonly AimPoseUpdatedEventArgs m_AimPose;
        readonly AimActivateValueUpdatedEventArgs m_AimActivateValue;
        readonly AimActivatedStateUpdatedEventArgs m_AimActivatedState;
        readonly GraspValueUpdatedEventArgs m_GraspValue;
        readonly GraspFirmStateUpdatedEventArgs m_GraspFirmState;
        readonly GripPoseUpdatedEventArgs m_GripPose;
        readonly PinchPoseUpdatedEventArgs m_PinchPose;
        readonly PinchValueUpdatedEventArgs m_PinchValue;
        readonly PinchTouchedStateUpdatedEventArgs m_PinchTouchedState;
        readonly PokePoseUpdatedEventArgs m_PokePose;
    }
}
