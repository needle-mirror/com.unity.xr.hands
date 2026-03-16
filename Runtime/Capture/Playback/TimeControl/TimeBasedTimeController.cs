namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Time-based controller that advances playback based on deltaTime.
    /// Supports interpolation between frames when elapsed time falls between two frame timestamps.
    /// Frame index advances automatically based on elapsed time relative to frame timestamps.
    /// This controller is used when <see cref="XRHandPlaybackOptions.ProgressPlaybackBasedOnFrames"/> is disabled.
    /// </summary>
    class TimeBasedTimeController : IPlaybackTimeController
    {
        int m_FrameIndex;
        float m_ElapsedTime;

        /// <summary>
        /// Gets or sets the current elapsed time in seconds.
        /// Setting this will update the frame index to match the time.
        /// </summary>
        public float ElapsedTime
        {
            get => m_ElapsedTime;
            set => m_ElapsedTime = value;
        }

        /// <summary>
        /// Gets or sets the current frame index.
        /// This is synchronized with elapsed time automatically during playback.
        /// </summary>
        public int FrameIndex
        {
            get => m_FrameIndex;
            set => m_FrameIndex = value;
        }

        /// <summary>
        /// Resets playback to the beginning (frame 0, elapsed time 0).
        /// </summary>
        public void Reset()
        {
            m_FrameIndex = 0;
            m_ElapsedTime = 0f;
        }

        /// <summary>
        /// Updates elapsed time based on the current frame index.
        /// Used when frame index is set directly to synchronize elapsed time.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        public void UpdateTimeBasedOnFrameIndex(XRHandCaptureSequence sequence)
        {
            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
            {
                m_ElapsedTime = 0f;
                return;
            }

            // Clamp frame index to valid range
            m_FrameIndex = Mathf.Clamp(m_FrameIndex, 0, sequence.frames.Count - 1);

            // Update elapsed time to match the timestamp of the current frame
            m_ElapsedTime = sequence.frames[m_FrameIndex].timestamp;
        }

        /// <summary>
        /// Advances elapsed time by deltaTime and updates frame index to match.
        /// Frame index is determined by finding the closest frame to the current elapsed time.
        /// </summary>
        /// <param name="deltaTime">The time to advance (typically Time.deltaTime).</param>
        /// <param name="sequence">The capture sequence being played back.</param>
        public void ApplyFrameDelta(float deltaTime, XRHandCaptureSequence sequence)
        {
            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
                return;

            m_ElapsedTime += deltaTime;
            m_FrameIndex = sequence.GetClosestFrameIndex(m_ElapsedTime);
        }

        /// <summary>
        /// Determines if playback has reached the end.
        /// In time-based mode, completion is determined by elapsed time exceeding the last frame's timestamp.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        /// <param name="looping">If true, playback never completes.</param>
        /// <returns>True if elapsed time exceeds the sequence duration and not looping, false otherwise.</returns>
        public bool IsComplete(XRHandCaptureSequence sequence, bool looping)
        {
            if (looping)
                return false;

            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
                return true;

            // Check if elapsed time has exceeded the last frame's timestamp
            var lastFrame = sequence.frames[sequence.frames.Count - 1];
            return m_ElapsedTime >= lastFrame.timestamp;
        }

        /// <summary>
        /// Determines if interpolation is needed at the current elapsed time.
        /// Interpolation is needed when elapsed time is between the current frame and the next frame.
        /// </summary>
        /// <param name="currentFrame">The current frame to check against.</param>
        /// <returns>True if interpolation should be performed, false otherwise.</returns>
        public bool NeedsInterpolation(XRHandCaptureFrame currentFrame)
        {
            // If elapsed time exactly matches or is before the current frame, no interpolation needed
            if (m_ElapsedTime <= currentFrame.timestamp + Constants.k_Epsilon)
                return false;

            // If we're at the last frame, no next frame to interpolate to
            // This check requires sequence context, but we'll handle it in TryGetInterpolationFrames
            return true;
        }

        /// <summary>
        /// Attempts to get the two frames and blend scalar needed for interpolation.
        /// Returns true when elapsed time is between two frames and both frames are available.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        /// <param name="currentFrame">Output: The frame before the current elapsed time.</param>
        /// <param name="nextFrame">Output: The frame after the current elapsed time.</param>
        /// <param name="blendScalar">Output: Value between 0-1 indicating interpolation position.</param>
        /// <returns>True if interpolation frames are available, false otherwise.</returns>
        public bool TryGetInterpolationFrames(
            XRHandCaptureSequence sequence,
            out XRHandCaptureFrame currentFrame,
            out XRHandCaptureFrame nextFrame,
            out float blendScalar)
        {
            currentFrame = default;
            nextFrame = default;
            blendScalar = 0f;

            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
                return false;

            // Check if we're at or past the last frame
            if (m_FrameIndex >= sequence.frames.Count - 1)
                return false;

            currentFrame = sequence.frames[m_FrameIndex];
            nextFrame = sequence.frames[m_FrameIndex + 1];

            // If elapsed time is at or before the current frame, no interpolation needed
            if (m_ElapsedTime <= currentFrame.timestamp + Constants.k_Epsilon)
                return false;

            // If elapsed time is at or after the next frame, no interpolation needed
            if (m_ElapsedTime >= nextFrame.timestamp - Constants.k_Epsilon)
                return false;

            // Calculate blend scalar between the two frames
            float frameDuration = nextFrame.timestamp - currentFrame.timestamp;
            if (frameDuration < Constants.k_Epsilon)
                return false;

            blendScalar = (m_ElapsedTime - currentFrame.timestamp) / frameDuration;
            return true;
        }
    }
}
