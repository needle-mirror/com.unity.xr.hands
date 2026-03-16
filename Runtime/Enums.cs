using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Represents the values being tracked for a particular joint.
    /// </summary>
    [Flags]
    public enum XRHandJointTrackingState
    {
        /// <summary>
        /// No data is currently being tracked for a joint.
        /// </summary>
        None = 0,

        /// <summary>
        /// Radius of current joint.
        /// </summary>
        Radius = 1 << 0,

        /// <summary>
        /// Pose of current joint.
        /// </summary>
        Pose = 1 << 1,

        /// <summary>
        /// Linear velocity of current joint.
        /// </summary>
        LinearVelocity = 1 << 2,

        /// <summary>
        /// Angular velocity of current joint.
        /// </summary>
        AngularVelocity = 1 << 3,

        /// <summary>
        /// Joint was marked as not being part of the hand layout for the current provider.
        /// </summary>
        WillNeverBeValid = 1 << 4,

        /// <summary>
        /// The joint is being actively tracked by the runtime, and the pose is not based on inference.
        /// </summary>
        HighFidelityPose = 1 << 5,
    }

    /// <summary>
    /// Represents the type of a hand joint. If you wish to convert it to an
    /// index, call <c>.ToIndex()</c> on the joint ID.
    /// </summary>
    public enum XRHandJointID
    {
        /// <summary>
        /// Invalid ID.
        /// </summary>
        Invalid,

        /// <summary>
        /// Marks the beginning of joints, or start of an array of data related
        /// to them. Casting this to an integer type will not result in a
        /// correct start. Use <see cref="XRHandJointIDUtility.ToIndex"/> instead.
        /// </summary>
        BeginMarker,

        /// <summary>
        /// Joint for the wrist.
        /// </summary>
        Wrist = BeginMarker,

        /// <summary>
        /// Represents the palm of the hand.
        /// </summary>
        Palm,

        /// <summary>
        /// Metacarpal joint of the thumb.
        /// </summary>
        ThumbMetacarpal,

        /// <summary>
        /// Proximal joint of the thumb.
        /// </summary>
        ThumbProximal,

        /// <summary>
        /// Distal joint of the thumb.
        /// </summary>
        ThumbDistal,

        /// <summary>
        /// Tip of the thumb.
        /// </summary>
        ThumbTip,

        /// <summary>
        /// Metacarpal joint of the index finger.
        /// </summary>
        IndexMetacarpal,

        /// <summary>
        /// Proximal joint of the index finger.
        /// </summary>
        IndexProximal,

        /// <summary>
        /// Intermediate joint of the index finger.
        /// </summary>
        IndexIntermediate,

        /// <summary>
        /// Distal joint of the index finger.
        /// </summary>
        IndexDistal,

        /// <summary>
        /// Tip of the index finger.
        /// </summary>
        IndexTip,

        /// <summary>
        /// Metacarpal joint of the middle finger.
        /// </summary>
        MiddleMetacarpal,

        /// <summary>
        /// Proximal joint of the middle finger.
        /// </summary>
        MiddleProximal,

        /// <summary>
        /// Intermediate joint of the middle finger.
        /// </summary>
        MiddleIntermediate,

        /// <summary>
        /// Distal joint of the middle finger.
        /// </summary>
        MiddleDistal,

        /// <summary>
        /// Tip of the middle finger.
        /// </summary>
        MiddleTip,

        /// <summary>
        /// Metacarpal joint of the ring finger.
        /// </summary>
        RingMetacarpal,

        /// <summary>
        /// Proximal joint of the ring finger.
        /// </summary>
        RingProximal,

        /// <summary>
        /// Intermediate joint of the ring finger.
        /// </summary>
        RingIntermediate,

        /// <summary>
        /// Distal joint of the ring finger.
        /// </summary>
        RingDistal,

        /// <summary>
        /// Tip of the ring finger.
        /// </summary>
        RingTip,

        /// <summary>
        /// Metacarpal joint of the little finger.
        /// </summary>
        LittleMetacarpal,

        /// <summary>
        /// Proximal joint of the little finger.
        /// </summary>
        LittleProximal,

        /// <summary>
        /// Intermediate joint of the little finger.
        /// </summary>
        LittleIntermediate,

        /// <summary>
        /// Distal joint of the little finger.
        /// </summary>
        LittleDistal,

        /// <summary>
        /// Tip of the little finger.
        /// </summary>
        LittleTip,

        /// <summary>
        /// Marks the end of joints, or size of an array of data related to
        /// them. Casting this to an integer type will not result in a correct
        /// count. Use <see cref="XRHandJointIDUtility.ToIndex"/> instead.
        /// </summary>
        EndMarker
    }

    /// <summary>
    /// Represents which hand is being referred to.
    /// </summary>
    public enum Handedness
    {
        /// <summary>
        /// Invalid hand. Will never be usable. Use <see cref="XRHandSubsystem.leftHand"/>
        /// or <see cref="XRHandSubsystem.rightHand"/> to obtain a valid hand.
        /// </summary>
        Invalid,

        /// <summary>
        /// Left hand.
        /// </summary>
        Left,

        /// <summary>
        /// Right hand.
        /// </summary>
        Right
    }

    /// <summary>
    /// Represents a finger on either hand. Useful with the
    /// <see cref="XRHandJointIDUtility.GetFrontJointID"/> and
    /// <see cref="XRHandJointIDUtility.GetBackJointID"/> APIs.
    /// </summary>
    public enum XRHandFingerID
    {
        /// <summary>
        /// Represents the thumb.
        /// </summary>
        Thumb,

        /// <summary>
        /// Represents the index finger.
        /// </summary>
        Index,

        /// <summary>
        /// Represents the middle finger.
        /// </summary>
        Middle,

        /// <summary>
        /// Represents the ring finger.
        /// </summary>
        Ring,

        /// <summary>
        /// Represents the little finger.
        /// </summary>
        Little
    }

    /// <summary>
    /// Flags denoting validity of relevant data in
    /// <see cref="XRCommonHandGestures"/> and its mirroring
    /// <see cref="XRCommonHandGesturesState"/>.
    /// </summary>
    [Flags]
    public enum XRCommonHandGesturesFlags
    {
        /// <summary>
        /// No optional data is valid.
        /// </summary>
        None = 0,

        /// <summary>
        /// Whether the aim pose is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsAimPoseValid</c> is enabled, the aim pose is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetAimPose"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetAimPose"/>
        /// will succeed.
        /// </value>
        IsAimPoseValid = 1 << 0,

        /// <summary>
        /// Whether the aim active value is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsAimActivateValueValid</c> is enabled, the aim activate value is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetAimActivateValue"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetAimActivateValue"/>
        /// will succeed.
        /// </value>
        IsAimActivateValueValid = 1 << 1,

        /// <summary>
        /// Whether the grasp value is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsGraspValueValid</c> is enabled, the grasp value is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetGraspValue"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetGraspValue"/>
        /// will succeed.
        /// </value>
        IsGraspValueValid = 1 << 2,

        /// <summary>
        /// Whether the GripPose is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsGripPoseValid</c> is enabled, the grip pose is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetGripPose"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetGripPose"/>
        /// will succeed.
        /// </value>
        IsGripPoseValid = 1 << 3,

        /// <summary>
        /// Whether the PinchPose is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsPinchPoseValid</c> is enabled, the pinch pose is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetPinchPose"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetPinchPose"/>
        /// will succeed.
        /// </value>
        IsPinchPoseValid = 1 << 4,

        /// <summary>
        /// Whether the PinchValue is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsPinchValueValid</c> is enabled, the pinch value is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetPinchValue"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetPinchValue"/>
        /// will succeed.
        /// </value>
        IsPinchValueValid = 1 << 5,

        /// <summary>
        /// Whether the PokePose is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsPokePoseValid</c> is enabled, the poke pose is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetPokePose"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetPokePose"/>
        /// will succeed.
        /// </value>
        IsPokePoseValid = 1 << 6,

        /// <summary>
        /// Whether the aim activated state is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsAimActivatedStateValid</c> is enabled, the aim activated state is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetAimActivatedState"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetAimActivatedState"/>
        /// will succeed.
        /// </value>
        IsAimActivatedStateValid = 1 << 7,

        /// <summary>
        /// Whether the grasp firm state is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsGraspFirmStateValid</c> is enabled, the grasp firm state is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetGraspFirmState"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetGraspFirmState"/>
        /// will succeed.
        /// </value>
        IsGraspFirmStateValid = 1 << 8,

        /// <summary>
        /// Whether the pinch touched state is valid in the <see cref="XRCommonHandGestures"/>
        /// or <see cref="XRCommonHandGesturesState"/> that these <c>XRCommonHandGesturesFlags</c>
        /// were retrieved from.
        /// </summary>
        /// <value>
        /// If <c>IsPinchTouchedStateValid</c> is enabled, the pinch touched state is valid, which means
        /// that if these <c>XRCommonHandGesturesFlags</c> were just
        /// obtained from <see cref="XRCommonHandGestures"/> or
        /// <see cref="XRCommonHandGesturesState"/>, then the corresponding
        /// <c>XRCommonHandGestures.</c><see cref="XRCommonHandGestures.TryGetPinchTouchedState"/>
        /// and <c>XRCommonHandGesturesState.</c><see cref="XRCommonHandGesturesState.TryGetPinchTouchedState"/>
        /// will succeed.
        /// </value>
        IsPinchTouchedStateValid = 1 << 9,
    }

    /// <summary>
    /// Describes which version of authored hand meshes is detected for use by the provider.
    /// </summary>
    public enum XRDetectedHandMeshLayout
    {
        /// <summary>
        /// The system was unable to detect a hand mesh layout to use.
        /// </summary>
        Unknown,

        /// <summary>
        /// The originally shipped version of sample meshes provided by the XR Hands package, compatible with Meta Quest in OpenXR.
        /// </summary>
        OpenXRMetaQuest,

        /// <summary>
        /// The version of sample meshes that is meant for use with the Android XR runtime in OpenXR.
        /// </summary>
        OpenXRAndroidXR,
    }

    /// <summary>
    /// Houses extension and utility methods for <see cref="XRHandJointID"/>.
    /// </summary>
    public static class XRHandJointIDUtility
    {
        /// <summary>
        /// Call <c>.ToIndex()</c> on a <see cref="XRHandJointID"/> to get its
        /// corresponding index into an array of joint data.
        /// </summary>
        /// <param name="jointId">ID of the joint to convert to an index.</param>
        /// <returns>
        /// The index matching the ID passed in.
        /// </returns>
        public static int ToIndex(this XRHandJointID jointId) => (int)jointId - 1;

        /// <summary>
        /// Call this to get the corresponding <see cref="XRHandJointID"/> from
        /// an index into an array of associated data.
        /// </summary>
        /// <param name="index">Index to convert to an ID.</param>
        /// <returns>
        /// The ID matching the index passed in.
        /// </returns>
        public static XRHandJointID FromIndex(int index) => (XRHandJointID)(index + 1);

        /// <summary>
        /// Gets the metacarpal <see cref="XRHandJointID"/> of a given <see cref="XRHandFingerID"/>.
        /// </summary>
        /// <param name="fingerId">ID of the finger of which you want the first <see cref="XRHandJointID"/>.</param>
        /// <returns>
        /// First <see cref="XRHandJointID"/> for the given finger in an <see cref="XRHand"/>
        /// object's list of joints.
        /// </returns>
        /// <example>
        /// <para>
        /// You can use <c>GetFrontJointID</c> and <see cref="GetBackJointID(XRHandFingerID)"/> to iterate
        /// through the joints of a specific finger:
        /// </para>
        /// <code>
        /// <![CDATA[
        ///     for(var i = XRHandFingerID.Thumb.GetFrontJointID().ToIndex(); // metacarpal
        ///             i <= XRHandFingerID.Thumb.GetBackJointID().ToIndex();  // tip
        ///             i++)
        ///     {
        ///         // hand is an XRHand object
        ///         XRHandJoint fingerJoint = hand.GetJoint(XRHandJointIDUtility.FromIndex(i));
        ///         // Use data...
        ///     }
        /// ]]>
        /// </code>
        /// </example>
        public static XRHandJointID GetFrontJointID(this XRHandFingerID fingerId)
        {
            switch (fingerId)
            {
                case XRHandFingerID.Thumb:
                    return XRHandJointID.ThumbMetacarpal;

                case XRHandFingerID.Index:
                    return XRHandJointID.IndexMetacarpal;

                case XRHandFingerID.Middle:
                    return XRHandJointID.MiddleMetacarpal;

                case XRHandFingerID.Ring:
                    return XRHandJointID.RingMetacarpal;

                case XRHandFingerID.Little:
                    return XRHandJointID.LittleMetacarpal;
            }

            throw new ArgumentException(nameof(fingerId) + " must be a valid value!");
        }

        /// <summary>
        /// Gets the tip <see cref="XRHandJointID"/> of a given <see cref="XRHandFingerID"/>.
        /// </summary>
        /// <param name="fingerId">ID of the finger you want the last <see cref="XRHandJointID"/> of.</param>
        /// <returns>Last <see cref="XRHandJointID"/> for the given finger in an <see cref="XRHand"/>
        /// object's list of joints.</returns>
        /// <remarks>Use with <see cref="GetFrontJointID(XRHandFingerID)"/> to iterate through the joints
        /// of a finger.</remarks>
        public static XRHandJointID GetBackJointID(this XRHandFingerID fingerId)
        {
            switch (fingerId)
            {
                case XRHandFingerID.Thumb:
                    return XRHandJointID.ThumbTip;

                case XRHandFingerID.Index:
                    return XRHandJointID.IndexTip;

                case XRHandFingerID.Middle:
                    return XRHandJointID.MiddleTip;

                case XRHandFingerID.Ring:
                    return XRHandJointID.RingTip;

                case XRHandFingerID.Little:
                    return XRHandJointID.LittleTip;
            }

            throw new ArgumentException(nameof(fingerId) + " must be a valid value!");
        }
    }
}
