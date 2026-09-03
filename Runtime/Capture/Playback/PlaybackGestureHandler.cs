using Unity.Profiling;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Handles retrieval and interpolation of common gesture data during playback.
    /// </summary>
    class PlaybackGestureHandler
    {
        readonly Handedness m_Handedness;
        IPlaybackCoordinateTransform m_CoordinateTransform;
        IPlaybackTimeController m_TimeController;

        XRHandCaptureFrame m_CurrentFrame;
        XRHandCaptureFrame m_NextFrame;
        bool m_IsCurrentFrameValid;
        bool m_IsNextFrameValid;

        float currentElapsedTime => m_TimeController?.ElapsedTime ?? 0f;

        static readonly ProfilerMarker k_TryGetAimPoseMarker = new("PlaybackGestureHandler.TryGetAimPose");
        static readonly ProfilerMarker k_TryGetTwoCommonGesturesMarker = new("PlaybackGestureHandler.TryGetCommonGestureStates");

        public PlaybackGestureHandler(
            Handedness handedness,
            IPlaybackCoordinateTransform coordinateTransform,
            IPlaybackTimeController timeController)
        {
            m_Handedness = handedness;
            m_CoordinateTransform = coordinateTransform;
            m_TimeController = timeController;
        }

        /// <summary>
        /// Updates the current and next frame state for gesture retrieval.
        /// Should be called before retrieving gesture data for a frame.
        /// </summary>
        public void UpdateFrameState(
            XRHandCaptureFrame currentFrame,
            XRHandCaptureFrame nextFrame,
            bool isCurrentFrameValid,
            bool isNextFrameValid)
        {
            m_CurrentFrame = currentFrame;
            m_NextFrame = nextFrame;
            m_IsCurrentFrameValid = isCurrentFrameValid;
            m_IsNextFrameValid = isNextFrameValid;
        }

        /// <summary>
        /// Generic helper for retrieving and interpolating common gesture data.
        /// </summary>
        delegate bool TryGetFromGestures<T>(XRCommonHandGesturesState gestures, out T value);
        delegate T InterpolateFunc<T>(in T current, in T next, float blendScalar);

        // Cache interpolation functions to use as delegate to avoid GC Alloc
        readonly InterpolateFunc<Pose> m_InterpolatePose = PlaybackInterpolator.InterpolatePose;
        readonly InterpolateFunc<float> m_InterpolateValue = PlaybackInterpolator.InterpolateValue;

        /// <summary>
        /// Retrieves common gesture data and interpolates when applicable.
        /// </summary>
        /// <typeparam name="T">Type of the common gesture data.</typeparam>
        /// <param name="getter">Delegate used to retrieve common gesture data.</param>
        /// <param name="interpolator">Delegate used to interpolate between two frames of common gesture data.</param>
        /// <param name="result">Result of the common gesture data.</param>
        /// <param name="defaultValue">Default value for result when common gesture data cannot be retrieved.</param>
        /// <returns>Returns <see langword="false"/> if the current frame is invalid or common gesture data cannot be retrieved from current the current frame. Otherwise returns <see langword="true"/>.</returns>
        /// <remarks>If the next frame is invalid or if common gesture data cannot be retrieved from the next frame, this function still returns true but does not interpolate.</remarks>
        bool TryGetCommonGestureHelper<T>(
            TryGetFromGestures<T> getter,
            InterpolateFunc<T> interpolator,
            out T result,
            T defaultValue)
        {
            if (!TryGetCommonGesturesStates(out bool isNextCommonGesturesValid, out var currentCommonGestures, out var nextCommonGestures, out float blendScalar))
            {
                result = defaultValue;
                return false;
            }

            // Get current frame common gesture data
            if (!getter(currentCommonGestures, out result))
                return false;

            // Interpolate if applicable
            if (isNextCommonGesturesValid && getter(nextCommonGestures, out var nextValue))
                result = interpolator(result, nextValue, blendScalar);

            return true;
        }

        /// <summary>
        /// Retrieves common gesture data without interpolation.
        /// </summary>
        /// <typeparam name="T">Type of the common gesture data.</typeparam>
        /// <param name="getter">Delegate used to retrieve common gesture data.</param>
        /// <param name="result">Result of the common gesture data.</param>
        /// <param name="defaultValue">Default value for result when common gesture data cannot be retrieved.</param>
        /// <returns>Returns <see langword="false"/> if the current frame is invalid or common gesture data cannot be retrieved from current the current frame. Otherwise returns <see langword="true"/>.</returns>
        /// <remarks>If the next frame is invalid or if common gesture data cannot be retrieved from the next frame, this function still returns true but does not interpolate.</remarks>
        bool TryGetCommonGestureHelper<T>(
            TryGetFromGestures<T> getter,
            out T result,
            T defaultValue)
        {
            if (!TryGetCommonGesturesStates(out _, out var currentCommonGestures, out _, out _))
            {
                result = defaultValue;
                return false;
            }

            // Get current frame common gesture data
            if (!getter(currentCommonGestures, out result))
                return false;

            return true;
        }

        /// <summary>
        /// Retrieves common gesture data and interpolates when applicable.
        /// </summary>
        /// <param name="getter">Delegate used to retrieve common gesture data.</param>
        /// <param name="interpolator">Delegate used to interpolate between two frames of common gesture data.</param>
        /// <param name="result">Result of the common gesture data.</param>
        /// <param name="defaultValue">Default value for result when common gesture data cannot be retrieved.</param>
        /// <param name="applyCoordinateTransform">Whether to transform thew result.</param>
        /// <returns>Returns <see langword="false"/> if the current frame is invalid or common gesture data cannot be retrieved from current the current frame. Otherwise returns <see langword="true"/>.</returns>
        /// <remarks>If the next frame is invalid or if common gesture data cannot be retrieved from the next frame, this function still returns true but does not interpolate.</remarks>
        bool TryGetCommonGestureHelper(
            TryGetFromGestures<Pose> getter,
            InterpolateFunc<Pose> interpolator,
            out Pose result,
            Pose defaultValue,
            bool applyCoordinateTransform)
        {
            if (!TryGetCommonGestureHelper(getter, interpolator, out result, defaultValue))
                return false;

            if (applyCoordinateTransform)
                result = m_CoordinateTransform.TransformPose(result);

            return true;
        }

        public bool TryGetAimPose(out Pose aimPose)
        {
            using (k_TryGetAimPoseMarker.Auto())
            {
                return TryGetCommonGestureHelper(
                    (XRCommonHandGesturesState g, out Pose p) => g.TryGetAimPose(out p),
                    m_InterpolatePose,
                    out aimPose,
                    Pose.identity,
                    true);
            }
        }

        public bool TryGetAimActivateValue(out float aimActivateValue)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out float v) => g.TryGetAimActivateValue(out v),
                m_InterpolateValue,
                out aimActivateValue,
                0f);
        }

        public bool TryGetAimActivatedState(out bool isAimActivated)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out bool v) => g.TryGetAimActivatedState(out v),
                out isAimActivated,
                false);
        }

        public bool TryGetGraspValue(out float graspValue)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out float v) => g.TryGetGraspValue(out v),
                m_InterpolateValue,
                out graspValue,
                0f);
        }

        public bool TryGetGraspFirmState(out bool isGraspFirm)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out bool v) => g.TryGetGraspFirmState(out v),
                out isGraspFirm,
                false);
        }

        public bool TryGetGripPose(out Pose gripPose)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out Pose p) => g.TryGetGripPose(out p),
                m_InterpolatePose,
                out gripPose,
                Pose.identity,
                true);
        }

        public bool TryGetPinchPose(out Pose pinchPose)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out Pose p) => g.TryGetPinchPose(out p),
                m_InterpolatePose,
                out pinchPose,
                Pose.identity,
                true);
        }

        public bool TryGetPinchValue(out float pinchValue)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out float v) => g.TryGetPinchValue(out v),
                m_InterpolateValue,
                out pinchValue,
                0f);
        }

        public bool TryGetPinchTouchedState(out bool isPinched)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out bool v) => g.TryGetPinchTouchedState(out v),
                out isPinched,
                false);
        }

        public bool TryGetPokePose(out Pose pokePose)
        {
            return TryGetCommonGestureHelper(
                (XRCommonHandGesturesState g, out Pose p) => g.TryGetPokePose(out p),
                m_InterpolatePose,
                out pokePose,
                Pose.identity,
                true);
        }

        public bool TryGetAimState(out XRHandAimState aimState)
        {
            // Try to get current frame aim state
            if (!m_IsCurrentFrameValid || !m_CurrentFrame.TryGetAimState(m_Handedness, out aimState))
            {
                aimState = default;
                return false;
            }

            // Try to get the next frame and interpolate between them
            if (m_IsNextFrameValid && m_NextFrame.TryGetAimState(m_Handedness, out var nextAimState))
            {
                float blendScalar = PlaybackInterpolator.CalculateBlendScalar(m_CurrentFrame, m_NextFrame, currentElapsedTime);
                aimState = PlaybackInterpolator.InterpolateAimState(aimState, nextAimState, blendScalar);
            }

            // Apply coordinate transform to aim pose of frame before returning
            aimState.aimPoseInternal = m_CoordinateTransform.TransformPose(aimState.aimPoseInternal);

            return true;
        }

        public bool TryGetCommonGesturesState(out XRCommonHandGesturesState commonGestures)
        {
            // Try to get current frame common gestures state
            if (!m_IsCurrentFrameValid || !m_CurrentFrame.TryGetCommonGestures(m_Handedness, out commonGestures))
            {
                commonGestures = default;
                return false;
            }

            // Try to get the next frame and interpolate between them
            if (m_IsNextFrameValid && m_NextFrame.TryGetCommonGestures(m_Handedness, out var nextCommonGesturesState))
            {
                float blendScalar = PlaybackInterpolator.CalculateBlendScalar(m_CurrentFrame, m_NextFrame, currentElapsedTime);
                commonGestures = PlaybackInterpolator.InterpolateCommonGesturesState(commonGestures, nextCommonGesturesState, blendScalar);
            }

            // Apply coordinate transform to all poses of current frame before returning
            commonGestures.aimPoseInternal = m_CoordinateTransform.TransformPose(commonGestures.aimPoseInternal);
            commonGestures.gripPoseInternal = m_CoordinateTransform.TransformPose(commonGestures.gripPoseInternal);
            commonGestures.pinchPoseInternal = m_CoordinateTransform.TransformPose(commonGestures.pinchPoseInternal);
            commonGestures.pokePoseInternal = m_CoordinateTransform.TransformPose(commonGestures.pokePoseInternal);

            return true;
        }

        /// <summary>
        /// Attempts to get <see cref="XRCommonHandGesturesState"/> for current and next frame.
        /// </summary>
        /// <param name="isNextValid">Whether the next frame has valid <seealso cref="XRCommonHandGesturesState"/>.</param>
        /// <param name="current">The common gesture state for the current frame.</param>
        /// <param name="next">The common gesture state for the next frame.</param>
        /// <param name="blendScalar">The blend scalar value for interpolation.</param>
        /// <returns></returns>
        bool TryGetCommonGesturesStates(out bool isNextValid, out XRCommonHandGesturesState current, out XRCommonHandGesturesState next, out float blendScalar)
        {
            using (k_TryGetTwoCommonGesturesMarker.Auto())
            {
                next = default;
                isNextValid = false;
                blendScalar = 0f;

                if (!m_IsCurrentFrameValid || !m_CurrentFrame.TryGetCommonGestures(m_Handedness, out current))
                {
                    current = default;
                    return false;
                }

                if (!m_IsNextFrameValid)
                    return true;

                isNextValid = m_NextFrame.TryGetCommonGestures(m_Handedness, out next);
                if (!isNextValid)
                    return true;

                blendScalar = PlaybackInterpolator.CalculateBlendScalar(m_CurrentFrame, m_NextFrame, currentElapsedTime);
                return true;
            }
        }

        public void SetCoordinateTransform(IPlaybackCoordinateTransform coordinateTransform)
        {
            m_CoordinateTransform = coordinateTransform;
        }

        public void SetTimeController(IPlaybackTimeController timeController)
        {
            m_TimeController = timeController;
        }
    }
}
