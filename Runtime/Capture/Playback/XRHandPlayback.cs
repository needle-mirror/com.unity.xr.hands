using Unity.Collections;
using Unity.Profiling;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// This type provides access to playback of <see cref="XRHandCaptureSequence"/>
    /// data through the <c>XRHandPlayback</c>'s associated <see cref="XRHandSubsystem"/>,
    /// and can be retrieved with <c>XRHandSubsystem</c>.<see cref="XRHandPlaybackExtensions.GetPlayback"/>
    /// if that script is <c>using UnityEngine.XR.Hands.Capture.Playback</c>.
    /// Anything you can retrieve from <c>XRHandPlayback</c> reflects the state
    /// that the <c>XRHandSubsystem</c> will try to use for surfacing
    /// <see cref="XRHandCaptureSequence"/> data through
    /// <c>XRHandSubsystem</c>.<see cref="XRHandSubsystem.TryUpdateHands"/>
    /// as of its next <see cref="XRHandSubsystem.UpdateType.Dynamic"/> step.
    /// Each frame, that state starts with what was used and will continue to be
    /// used for <see cref="XRHandSubsystem.TryUpdateHands"/> this frame, but
    /// can be overwritten through further use of <c>XRHandPlayback</c>.
    /// </summary>
    class XRHandPlayback
    {
        /// <summary>
        /// Denotes which hand this controls playback for.
        /// </summary>
        public Handedness handedness => m_Handedness;

        /// <summary>
        /// Captured hand data to surface through this <c>XRHandPlayback</c>'s
        /// attached <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.TryUpdateHands"/>
        /// that will be used as of the next <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// step.
        /// </summary>
        /// <value>
        /// Unlike <see cref="isRunning"/>, <see cref="isPlaying"/>,
        /// <see cref="elapsedTime"/>, and <see cref="frameIndex"/>, this can
        /// be set directly.
        /// </value>
        /// <remarks>
        /// Setting this can result in <see cref="elapsedTime"/>, <see cref="frameIndex"/>,
        /// and <see cref="isPlaying"/> changing to ensure that whenever
        /// <c>sourceCaptureSequence</c> is valid and <see cref="isRunning"/> is
        /// <see langword="true"/>, timing controls are kept in sync.
        /// </remarks>
        public XRHandCaptureSequence sourceCaptureSequence
        {
            get => m_SourceCaptureSequence;
            set
            {
                m_SourceCaptureSequence = value;
                m_FrameProvider?.UpdateSequence(value);
            }
        }

        /// <summary>
        /// Adjust the anchor <see cref="Pose"/> from which captured hand data is reported
        /// to <see cref="XRHandSubsystem"/> through <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// </summary>
        /// <value>
        /// The way this value is used is based on whether <see cref="XRHandPlaybackOptions.RootPoseLockedToAnchor"/>
        /// is set on <see cref="options"/>. The default is that this flag is clear, which results in <c>anchor</c> controlling
        /// where in the current Unity scene gets treated as if it were the origin when the hand data was captured.
        /// Setting the <see cref="XRHandPlaybackOptions.RootPoseLockedToAnchor"/> flag on <see cref="options"/>
        /// results in <c>anchor</c> defining the root pose of the hand on the first frame of playback based on the first
        /// frame used for playback when starts as a result of a call to <see cref="Play"/>. This means that, if this value
        /// has been changed to something other than <c>Pose.</c><see cref="Pose.identity"/>, calling <see cref="Play"/>
        /// with different states for <see cref="frameIndex"/> or <see cref="elapsedTime"/> can result in different offsets
        /// for how the hand data is played back.
        /// </value>
        public Pose anchor
        {
            get => m_CoordinateTransform?.CurrentAnchor ?? Pose.identity;
            set
            {
                // Update each transformer with the new anchor
                for (var i = 0; i < m_CoordinateTransformers?.Length; i++)
                    m_CoordinateTransformers?[i].UpdateAnchor(value);
            }
        }

        /// <summary>
        /// Starts playback of captured hand data in <see cref="sourceCaptureSequence"/>
        /// through the attached <see cref="XRHandSubsystem"/>'s <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// <c>XRHandPlayback</c> will not, however, be able to attempt to surface
        /// and hand data without a valid <see cref="sourceCaptureSequence"/> that
        /// has at least one frame.
        /// </summary>
        /// <param name="resetIfSequenceEnded">If <see langword="true"/>, the sequence will be reset to first frame to ensure the sequence plays.</param>
        /// <remarks>
        /// Calling this can result in <see cref="elapsedTime"/>, <see cref="frameIndex"/>,
        /// and <see cref="isPlaying"/> changing to ensure that whenever
        /// <see cref="sourceCaptureSequence"/> is valid and <see cref="isRunning"/>
        /// is <see langword="true"/>, timing controls are kept in sync.
        /// If sequence has ended and playback is set to not loop, calling <see cref="Play"/>
        /// alone will not automatically reset it. Call <see cref="Play"/> with
        /// <see cref="resetIfSequenceEnded"/> set to true to ensure it will reset the sequence and play.
        /// </remarks>
        /// <seealso cref="Stop"/>
        /// <seealso cref="Pause"/>
        public void Play(bool resetIfSequenceEnded = false)
        {
            m_IsRunning = true;
            SyncIsPlaying();

            // Ensure sequence is reset to guarantee sequence will play if looping is disabled and the sequence is complete.
            if (resetIfSequenceEnded && !isPlaying)
            {
                m_TimeController?.Reset();
                SyncIsPlaying();
            }
        }

        /// <summary>
        /// Stops playback of captured hand data in <see cref="sourceCaptureSequence"/>
        /// through the attached <see cref="XRHandSubsystem"/>'s <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// </summary>
        /// <remarks>
        /// Calling this will result in <see cref="isRunning"/> and <see cref="isPlaying"/>
        /// becoming <see langword="false"/> and the time controller will be reset.
        /// </remarks>
        /// <seealso cref="Play"/>
        /// <seealso cref="Pause"/>
        public void Stop()
        {
            m_IsRunning = false;
            m_IsPlaying = false;
            m_TimeController?.Reset();
        }

        /// <summary>
        /// Pauses playback of captured hand data in <see cref="sourceCaptureSequence"/>
        /// through the attached <see cref="XRHandSubsystem"/>'s <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// </summary>
        /// <remarks>
        /// Calling this will result in <see cref="isRunning"/> and <see cref="isPlaying"/>
        /// becoming <see langword="false"/>. This does not reset the time controller.
        /// </remarks>
        /// <seealso cref="Play"/>
        /// <seealso cref="Stop"/>
        public void Pause()
        {
            m_IsRunning = false;
            m_IsPlaying = false;
        }

        /// <summary>
        /// If <c>isRunning</c> is <see langword="true"/>, playback of
        /// <see cref="sourceCaptureSequence"/> hand data each frame through
        /// <c>XRHandSubsystem</c>.<see cref="XRHandSubsystem.TryUpdateHands"/>
        /// will be attempted, based on the state of <see cref="sourceCaptureSequence"/>,
        /// <see cref="elapsedTime"/>, <see cref="frameIndex"/>,
        /// and <see cref="options"/>.
        /// </summary>
        /// <value>
        /// Reports whether <see cref="Play"/> has been called without a
        /// later <see cref="Stop"/> call.
        /// </value>
        /// <seealso cref="isPlaying"/>
        public bool isRunning => m_IsRunning;

        /// <summary>
        /// Reports whether the next frame will result in further playback
        /// progression from the source <see cref="XRHandCaptureSequence"/>
        /// data in <see cref="sourceCaptureSequence"/>.
        /// </summary>
        /// <value>
        /// This being <see langword="true"/> is not enough for playback to
        /// occur, as the <see cref="XRHandSubsystem"/> must also have
        /// <see cref="XRHandSubsystem.Start"/> called on it (without a later
        /// <see cref="XRHandSubsystem.Stop"/> call, meaning <see cref="XRHandSubsystem.isRunning"/>
        /// returns <see langword="true"/>).
        /// </value>
        /// <remarks>
        /// This can only be <see langword="true"/> when <see cref="isRunning"/>
        /// is as well. However, for <c>isPlaying</c> to also be <c>true</c>,
        /// there had to be valid data to surface based on <see cref="sourceCaptureSequence"/>,
        /// <see cref="elapsedTime"/>, <see cref="frameIndex"/>, and <see cref="options"/>.
        /// </remarks>
        public bool isPlaying => m_IsPlaying;

        /// <summary>
        /// Elapsed time that will be used as of the next
        /// <see cref="XRHandSubsystem.UpdateType.Dynamic"/> step of the attached
        /// <see cref="XRHandSubsystem"/>'s <see cref="XRHandSubsystem.TryUpdateHands"/>
        /// step. If a negative value is supplied, the value will be set to zero
        /// instead. If <see cref="sourceCaptureSequence"/> is valid, <c>elapsedTime</c>
        /// will also have its upper bound capped by <see cref="sourceCaptureSequence"/>'s
        /// <see cref="XRHandCaptureSequence.durationInSeconds"/> and <see cref="frameIndex"/>
        /// will change to be the frame index before but as close as possible to
        /// <c>elapsedTime</c>.
        /// </summary>
        /// <value>
        /// Calling this can result in <see cref="frameIndex"/> and
        /// <see cref="isPlaying"/> changing in order to ensure that whenever
        /// <see cref="sourceCaptureSequence"/> is valid and <see cref="isRunning"/>
        /// is <see langword="true"/>, timing controls are kept in sync.
        /// </value>
        public float elapsedTime
        {
            get => m_TimeController?.ElapsedTime ?? 0f;
            set { if (m_TimeController != null) m_TimeController.ElapsedTime = value; }
        }

        /// <summary>
        /// Frame index that will be used as of the next
        /// <see cref="XRHandSubsystem.UpdateType.Dynamic"/> step of the attached
        /// <see cref="XRHandSubsystem"/>'s <see cref="XRHandSubsystem.TryUpdateHands"/>
        /// step.
        /// </summary>
        /// <value>
        /// Calling this can result in <see cref="elapsedTime"/> and
        /// <see cref="isPlaying"/> changing to ensure that whenever
        /// <see cref="sourceCaptureSequence"/> is valid and <see cref="isRunning"/>
        /// is <see langword="true"/>, timing controls are kept in sync.
        /// </value>
        /// <remarks>
        /// If playback progression is time-based (the <see cref="XRHandPlaybackOptions.ProgressPlaybackBasedOnFrames"/>
        /// option is disabled on <see cref="options"/>), this will usually reflect the lower of the two indices
        /// used to interpolate between to present pose data - otherwise, in rarer
        /// scenarios like the first frame of playback, the <c>frameIndex</c>
        /// will still be entirely accurate either way.
        /// </remarks>
        public int frameIndex
        {
            get => m_TimeController?.FrameIndex ?? 0;
            set
            {
                if (m_TimeController != null)
                {
                    m_TimeController.FrameIndex = value;
                    m_TimeController.UpdateTimeBasedOnFrameIndex(m_SourceCaptureSequence);
                }
            }
        }

        /// <summary>
        /// Optional behavior changes to how playback should behave with respect to
        /// timing and interaction with <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.TryUpdateHands"/>,
        /// such as whether to loop playback automatically when the end is reached.
        /// </summary>
        /// <value>
        /// Setting this can result in <see cref="elapsedTime"/>, <see cref="frameIndex"/>,
        /// and <see cref="isPlaying"/> changing to ensure that whenever
        /// <c>sourceCaptureSequence</c> is valid and <see cref="isRunning"/> is
        /// <see langword="true"/>, timing controls are kept in sync.
        /// </value>
        public XRHandPlaybackOptions options
        {
            get => m_Options;
            set
            {
                if (m_Options == value)
                    return;

                float currentElapsedTime = elapsedTime;
                int currentFrameIndex = frameIndex;
                m_Options = value;
                ReflectOptionsToHandlers();
                if (m_TimeController != null)
                {
                    m_TimeController.ElapsedTime = currentElapsedTime;
                    m_TimeController.FrameIndex = currentFrameIndex;
                }
            }
        }

        internal XRHandPlayback(ref XRHandSubsystemActions actions, Handedness handedness)
        {
            m_Handedness = handedness;
            m_CoordinateTransform = CreateCoordinateTransform(m_Options);
            foreach (var coordinateTransformer in m_CoordinateTransformers)
                coordinateTransformer.UpdateAnchor(Pose.identity); // UpdateRootPose with identity anchor

            m_TimeController = CreateTimeController(m_Options);
            m_FrameProvider = new PlaybackFrameProvider(m_SourceCaptureSequence, m_Handedness);
            m_GestureHandler = new PlaybackGestureHandler(m_Handedness, m_CoordinateTransform, m_TimeController);
            actions.beginTryUpdateHands += OnBeginTryUpdateHands;
        }

        internal SequenceFlags GetActiveSequenceFlags() => m_SourceCaptureSequence.flags;

        internal bool TryGetActiveFrame(out XRHandCaptureFrame frame)
        {
            int currentFrameIndex = m_TimeController?.FrameIndex ?? 0;
            return m_FrameProvider.TryGetCurrentFrame(currentFrameIndex, out frame);
        }

        internal bool TryGetNextActiveFrame(out XRHandCaptureFrame frame)
        {
            int currentFrameIndex = m_TimeController?.FrameIndex ?? 0;
            return m_FrameProvider.TryGetNextFrame(currentFrameIndex, out frame);
        }

        internal XRHandSubsystem.UpdateSuccessFlags TryUpdateHand(
            XRHandSubsystem.UpdateType updateType,
            ref Pose handRootPose,
            NativeArray<XRHandJoint> handJoints)
        {
            using (k_TryUpdateHandMarker.Auto())
            {
                m_MostRecentSuccessFlags = TryUpdateHandImpl(updateType, ref handRootPose, handJoints);

                // TryUpdateHandImpl needs to be called before early returning to ensure hands render when no sequence is playing.
                if (!m_IsPlaying)
                    return m_MostRecentSuccessFlags;

                if (updateType == XRHandSubsystem.UpdateType.BeforeRender && m_IsPlaying)
                    m_TimeController?.ApplyFrameDelta(Time.deltaTime, m_SourceCaptureSequence);

                return m_MostRecentSuccessFlags;
            }
        }

        XRHandSubsystem.UpdateSuccessFlags TryUpdateHandImpl(
            XRHandSubsystem.UpdateType updateType,
            ref Pose handRootPose,
            NativeArray<XRHandJoint> handJoints)
        {
            using (k_TryUpdateHandImplMarker.Auto())
            {
                if (!HasAnyFrames())
                    return XRHandSubsystem.UpdateSuccessFlags.None;

                if (updateType == XRHandSubsystem.UpdateType.BeforeRender)
                    return m_MostRecentSuccessFlags;

                XRHandSubsystem.UpdateSuccessFlags returnFlags = XRHandSubsystem.UpdateSuccessFlags.None;
                UpdateOriginPose();
                returnFlags |= m_Handedness.ToRootPoseUpdateSuccessFlag();
                m_IsCurrentFrameValid = TryGetActiveFrame(out m_CurrentFrame);
                if (!m_IsCurrentFrameValid)
                    return XRHandSubsystem.UpdateSuccessFlags.None;

                m_IsNextFrameValid = TryGetNextActiveFrame(out m_NextFrame);

                // Update gesture handler with current frame state
                m_GestureHandler?.UpdateFrameState(m_CurrentFrame, m_NextFrame, m_IsCurrentFrameValid, m_IsNextFrameValid);

                // Use time controller to determine if interpolation is needed
                if (m_TimeController != null && m_TimeController.NeedsInterpolation(m_CurrentFrame))
                {
                    return returnFlags | HandleTimeBasedHandsUpdate(updateType, ref handRootPose, handJoints);
                }

                return returnFlags | TryUpdateHandFromOneFrame(updateType, ref handRootPose, handJoints);
            }
        }

        /// <summary>
        /// Helper to find a hand frame searching backwards from a starting index.
        /// Returns true if a valid tracked hand is found.
        /// </summary>
        bool TryFindHandBackward(
            int startFrameIndex,
            XRHandSubsystem.UpdateType updateType,
            out XRHand hand,
            out float timestamp)
        {
            hand = default;
            timestamp = Constants.k_InvalidTime;

            for (int index = startFrameIndex; index >= 0; --index)
            {
                if (!m_FrameProvider.TryGetCurrentFrame(index, out var frame))
                    continue;

                if (TryGetEitherHand(frame, updateType, out hand))
                {
                    timestamp = frame.timestamp;
                    return true;
                }

                hand.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Helper to find a hand frame searching forward from a starting index.
        /// Returns true if a valid tracked hand is found.
        /// </summary>
        bool TryFindHandForward(
            int startFrameIndex,
            XRHandSubsystem.UpdateType updateType,
            out XRHand hand,
            out float timestamp)
        {
            hand = default;
            timestamp = Constants.k_InvalidTime;

            for (int index = startFrameIndex; index < m_FrameProvider.frameCount; ++index)
            {
                if (!m_FrameProvider.TryGetCurrentFrame(index, out var frame))
                    continue;

                if (TryGetEitherHand(frame, updateType, out hand))
                {
                    timestamp = frame.timestamp;
                    return true;
                }

                hand.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Updates the hands when using <see cref="TimeBasedTimeController"/>.
        /// </summary>
        /// <param name="updateType">The current <see cref="XRHandSubsystem.UpdateType"/>.</param>
        /// <param name="handRootPose">The current <see cref="Pose"/> of the hand root.</param>
        /// <param name="handJoints">Array of hand joint data to be updated.</param>
        /// <returns><see cref="XRHandSubsystem.UpdateSuccessFlags"/> indicating the state of this update.</returns>
        XRHandSubsystem.UpdateSuccessFlags HandleTimeBasedHandsUpdate(
            XRHandSubsystem.UpdateType updateType,
            ref Pose handRootPose,
            NativeArray<XRHandJoint> handJoints)
        {
            float currentElapsedTime = m_TimeController?.ElapsedTime ?? 0f;
            int currentFrameIndex = m_TimeController?.FrameIndex ?? 0;

            // Use single frame update if current time is behind the frame and interpolation is not needed
            if (currentElapsedTime == 0f || currentElapsedTime <= m_CurrentFrame.timestamp || currentFrameIndex == GetFrameCount() - 1)
                return TryUpdateHandFromOneFrame(updateType, ref handRootPose, handJoints);

            var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;

            // Find hand frames for interpolation
            bool isHandBeforeValid = TryFindHandBackward(currentFrameIndex, updateType, out var handBefore, out float elapsedTimeAtHandBefore);
            bool isHandAfterValid = TryFindHandForward(currentFrameIndex + 1, updateType, out var handAfter, out float elapsedTimeAtHandAfter);

            if (isHandBeforeValid && isHandAfterValid)
            {
                float blendScalar = PlaybackInterpolator.CalculateBlendScalar(elapsedTimeAtHandBefore, elapsedTimeAtHandAfter,
                    currentElapsedTime);

                // Use PlaybackInterpolator to interpolate joints
                if (PlaybackInterpolator.TryInterpolateJoints(handBefore, handAfter, blendScalar, m_Handedness, handJoints))
                {
                    // Transforms joints in place using the data from the hand joints array because TryInterpolateJoints has modified the data in the hand joint array.
                    // Note: This differs from UpdateHandFromFrame which uses the hand capture snapshot data.
                    TransformJointsInPlaceHelper(handJoints);

                    successFlags |= m_Handedness.ToJointsUpdateSuccessFlag();

                    if (handJoints[XRHandJointID.Wrist.ToIndex()].TryGetPose(out var wristPose))
                    {
                        handRootPose = wristPose;
                        successFlags |= m_Handedness.ToRootPoseUpdateSuccessFlag();
                    }
                }
            }

            handBefore.Dispose();
            handAfter.Dispose();
            return successFlags;
        }

        /// <summary>
        /// Updates the hands when using <see cref="FrameBasedTimeController"/>.
        /// </summary>
        /// <param name="frame">The current frame of the captured hand data.</param>
        /// <param name="updateType">The current <see cref="XRHandSubsystem.UpdateType"/>.</param>
        /// <param name="handRootPose">The current <see cref="Pose"/> of the hand root.</param>
        /// <param name="handJoints">Array of hand joint data to be updated.</param>
        /// <returns><see cref="XRHandSubsystem.UpdateSuccessFlags"/> indicating the state of this update.</returns>
        XRHandSubsystem.UpdateSuccessFlags UpdateHandFromFrame(
            in XRHandCaptureFrame frame,
            XRHandSubsystem.UpdateType updateType,
            ref Pose handRootPose,
            NativeArray<XRHandJoint> handJoints)
        {
            using (k_TryUpdateHandByFrameMarker.Auto())
            {
                // Try to extract the hand capture snapshot from the current frame.
                if (!frame.TryGetHandSnapshot(m_Handedness, out var handCaptureSnapshot))
                    return XRHandSubsystem.UpdateSuccessFlags.None;

                // Try to extract the XRHand from the snapshot to use to update the joint data.
                if (!handCaptureSnapshot.TryGetHand(updateType, Allocator.Temp, out var handFromCaptureSnapshot))
                {
                    handFromCaptureSnapshot.Dispose();
                    return XRHandSubsystem.UpdateSuccessFlags.None;
                }

                var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;

                // Transforms joints in place using the joint data from the hand retrieved from the capture snapshot.
                // Note: This differs from HandleTimeBasedHandsUpdate which uses the joint data from the hand joint array.
                TransformJointsInPlaceHelper(handJoints, handFromCaptureSnapshot);

                successFlags |= m_Handedness.ToJointsUpdateSuccessFlag();

                if (handJoints[XRHandJointID.Wrist.ToIndex()].TryGetPose(out var wristPose))
                {
                    handRootPose = wristPose;
                    successFlags |= m_Handedness.ToRootPoseUpdateSuccessFlag();
                }

                handFromCaptureSnapshot.Dispose();
                return successFlags;
            }
        }

        /// <summary>
        /// Helper to transform joints in place by applying ShiftByOrigin to each joint's pose.
        /// Result are updated in the hand joints aray that is passed in as a parameter.
        /// </summary>
        /// <param name="handJoints">Hand joints to be transformed.</param>
        /// <remarks>Iterates through the hand joints array via index and updates the
        /// hand joints array with results.</remarks>
        void TransformJointsInPlaceHelper(NativeArray<XRHandJoint> handJoints)
        {
            for (int i = 0; i < XRHandJointID.EndMarker.ToIndex(); ++i)
            {
                var joint = handJoints[i];
                if ((joint.trackingState & XRHandJointTrackingState.Pose) == 0)
                    continue;

                joint.m_Pose = ShiftByOrigin(joint.m_Pose);
                handJoints[i] = joint;
            }
        }

        /// <summary>
        /// Helper to transform joints in place by applying ShiftByOrigin to each joint's pose.
        /// Result are updated in the hand joints aray that is passed in as a parameter.
        /// <param name="handJoints">Hand joints to be transformed.</param>
        /// <param name="hand">Hand used to access joints.</param>
        /// </summary>
        /// <remarks>Iterates through the hand joints array via joint IDs retrieved from
        /// the hand that is passed in and updates the hand joints array with results.</remarks>
        void TransformJointsInPlaceHelper(NativeArray<XRHandJoint> handJoints, XRHand hand)
        {
            for (var jointID = XRHandJointID.BeginMarker; jointID < XRHandJointID.EndMarker; jointID++)
            {
                var joint = hand.GetJoint(jointID);
                if ((joint.trackingState & XRHandJointTrackingState.Pose) == 0)
                    continue;

                joint.m_Pose = ShiftByOrigin(joint.m_Pose);
                handJoints[jointID.ToIndex()] = joint;
            }
        }

        XRHandSubsystem.UpdateSuccessFlags TryUpdateHandFromOneFrame(
            XRHandSubsystem.UpdateType updateType,
            ref Pose handRootPose,
            NativeArray<XRHandJoint> handJoints)
            => UpdateHandFromFrame(m_CurrentFrame, updateType, ref handRootPose, handJoints);

        bool TryGetEitherHand(in XRHandCaptureFrame frame, XRHandSubsystem.UpdateType preferredUpdateType, out XRHand hand)
        {
            if (!frame.TryGetHandSnapshot(m_Handedness, out var handCaptureSnapshot))
            {
                hand = default;
                return false;
            }

            return handCaptureSnapshot.TryGetHand(preferredUpdateType, Allocator.Temp, out hand)
                || handCaptureSnapshot.TryGetHand(preferredUpdateType.GetOtherUpdateType(), Allocator.Temp, out hand);
        }

        internal bool TryGetAimPose(ref Pose aimPose) => m_GestureHandler?.TryGetAimPose(ref aimPose) ?? false;

        internal bool TryGetAimActivateValue(ref float aimActivateValue) => m_GestureHandler?.TryGetAimActivateValue(ref aimActivateValue) ?? false;

        internal bool TryGetGraspValue(ref float graspValue) => m_GestureHandler?.TryGetGraspValue(ref graspValue) ?? false;

        internal bool TryGetGripPose(ref Pose gripPose) => m_GestureHandler?.TryGetGripPose(ref gripPose) ?? false;

        internal bool TryGetPinchPose(ref Pose pinchPose) => m_GestureHandler?.TryGetPinchPose(ref pinchPose) ?? false;

        internal bool TryGetPinchValue(ref float pinchValue) => m_GestureHandler?.TryGetPinchValue(ref pinchValue) ?? false;

        internal bool TryGetPokePose(ref Pose pokePose) => m_GestureHandler?.TryGetPokePose(ref pokePose) ?? false;

        internal XRDetectedHandMeshLayout detectedHandMeshLayout
            => m_SourceCaptureSequence?.detectedHandMeshLayout ?? XRDetectedHandMeshLayout.Unknown;

        internal bool TryGetAimState(ref XRHandAimState aimState) => m_GestureHandler?.TryGetAimState(ref aimState) ?? false;

        void ReflectOptionsToHandlers()
        {
            // Set handlers based on current options using the same bit math as Create methods
            m_CoordinateTransform = m_CoordinateTransformers[((int)m_Options >> 3) & 1];
            m_TimeController = m_TimeControllers[(int)m_Options & 1];
            m_GestureHandler.SetCoordinateTransform(m_CoordinateTransform);
            m_GestureHandler.SetTimeController(m_TimeController);
        }

        void OnBeginTryUpdateHands(XRHandSubsystem.UpdateType updateType)
        {
            if (updateType != XRHandSubsystem.UpdateType.Dynamic)
                return;

            LoopToStartIfNeeded();
            SyncIsPlaying();
            ReflectOptionsToHandlers();
            UpdateOriginPose();
        }

        void UpdateOriginPose()
        {
            // Update the coordinate transform with the current anchor and the current frame
            // This ensures consistent anchoring regardless of current playback position
            if (m_CoordinateTransform != null && m_FrameProvider.TryGetCurrentFrame(m_TimeController.FrameIndex, out var currentFrame))
            {
                m_CoordinateTransform.UpdateRootPose(m_CoordinateTransform.CurrentAnchor, currentFrame, m_Handedness);
            }
        }

        internal int GetFrameCount() => m_FrameProvider.frameCount;
        internal bool HasAnyFrames() => m_FrameProvider.hasFrames;

        void LoopToStartIfNeeded()
        {
            if (!m_IsPlaying)
                return;

            if (m_Options.IsAnyFlagEnabled(XRHandPlaybackOptions.LoopCaptureWhenPlaybackEnds))
            {
                if (m_TimeController != null
                    && m_TimeController.IsComplete(m_SourceCaptureSequence, false))
                    SeekToStart();
            }
        }

        void SyncIsPlaying()
        {
            if (!m_IsRunning)
            {
                m_IsPlaying = false;
                return;
            }

            if (m_Options.IsAnyFlagEnabled(XRHandPlaybackOptions.LoopCaptureWhenPlaybackEnds))
            {
                m_IsPlaying = true;
            }
            else if (m_Options.IsAnyFlagEnabled(XRHandPlaybackOptions.ProgressPlaybackBasedOnFrames))
            {
                int currentFrameIndex = m_TimeController?.FrameIndex ?? 0;
                // Assumption is if we have our final frame (count-1), playing is stopped.
                // If the sequence is set to loop this will be reset and play again.
                int lastFrameIndex = m_SourceCaptureSequence != null ? m_SourceCaptureSequence.frames.Count - 1 : 0;
                m_IsPlaying = currentFrameIndex < lastFrameIndex;
            }
            else
            {
                float currentElapsedTime = m_TimeController?.ElapsedTime ?? 0f;
                float duration = m_SourceCaptureSequence?.durationInSeconds ?? 0f;
                m_IsPlaying = currentElapsedTime < duration;
            }
        }

        void SeekToStart()
        {
            m_TimeController?.Reset();
        }

        Pose ShiftByOrigin(in Pose pose)
        {
            using (k_ShiftByOriginMarker.Auto())
            {
                // Delegate to the coordinate transform component
                if (m_CoordinateTransform != null)
                    return m_CoordinateTransform.TransformPose(pose);

                // If no transform is initialized, return the pose unchanged
                return pose;
            }
        }

        IPlaybackCoordinateTransform[] m_CoordinateTransformers = { new WorldSpaceCoordinateTransform(), new RelativeToInitialRootTransform() };
        IPlaybackTimeController[] m_TimeControllers = { new TimeBasedTimeController(), new FrameBasedTimeController() };

        IPlaybackCoordinateTransform CreateCoordinateTransform(XRHandPlaybackOptions playbackOptions)
        {
            return m_CoordinateTransformers[((int)playbackOptions >> 3) & 1];
        }

        IPlaybackTimeController CreateTimeController(XRHandPlaybackOptions playbackOptions)
        {
            return m_TimeControllers[(int)playbackOptions & 1];
        }

        // prevent public construction
        XRHandPlayback() { }

        // Profiler markers
        static readonly ProfilerMarker k_TryUpdateHandMarker = new("XRHandPlayback.TryUpdateHand");
        static readonly ProfilerMarker k_TryUpdateHandImplMarker = new("XRHandPlayback.TryUpdateHandImpl");
        static readonly ProfilerMarker k_TryUpdateHandByFrameMarker = new("XRHandPlayback.TryUpdateHandByFrame");
        static readonly ProfilerMarker k_ShiftByOriginMarker = new("XRHandPlayback.ShiftByOrigin");

        Handedness m_Handedness;
        XRHandCaptureSequence m_SourceCaptureSequence;
        XRHandPlaybackOptions m_Options;

        bool m_IsRunning;
        bool m_IsPlaying;

        PlaybackFrameProvider m_FrameProvider;

        XRHandSubsystem.UpdateSuccessFlags m_MostRecentSuccessFlags;
        XRHandCaptureFrame m_CurrentFrame;
        XRHandCaptureFrame m_NextFrame;
        bool m_IsCurrentFrameValid;
        bool m_IsNextFrameValid;

        IPlaybackCoordinateTransform m_CoordinateTransform;
        IPlaybackTimeController m_TimeController;
        PlaybackGestureHandler m_GestureHandler;
    }
}
