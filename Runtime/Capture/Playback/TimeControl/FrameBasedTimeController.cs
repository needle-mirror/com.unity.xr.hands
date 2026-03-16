namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Frame-based time controller that advances playback one frame at a time.
    /// Does not support interpolation between frames. Each update advances to the next frame,
    /// and elapsed time is synchronized to match the current frame's timestamp.
    /// This controller is used when <see cref="XRHandPlaybackOptions.ProgressPlaybackBasedOnFrames"/> is enabled.
    /// </summary>
    class FrameBasedTimeController : IPlaybackTimeController
    {
        int m_FrameIndex;
        float m_ElapsedTime;

        /// <summary>
        /// Gets or sets the current elapsed time in seconds.
        /// In frame-based mode, this is synchronized to the current frame's timestamp.
        /// </summary>
        public float ElapsedTime
        {
            get => m_ElapsedTime;
            set => m_ElapsedTime = value;
        }

        /// <summary>
        /// Gets or sets the current frame index.
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
        /// Used when the frame index is set directly to synchronize elapsed time.
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

            // Update elapsed time to match the current frame's timestamp
            m_ElapsedTime = sequence.frames[m_FrameIndex].timestamp;
        }

        /// <summary>
        /// Advances to the next frame in the sequence.
        /// Ensures frame index stays within range of the number of frames in the sequence.
        /// Updates elapsed time to match the new frame's timestamp.
        /// </summary>
        /// <param name="deltaTime">Not used in frame-based mode.</param>
        /// <param name="sequence">The capture sequence being played back.</param>
        public void ApplyFrameDelta(float deltaTime, XRHandCaptureSequence sequence)
        {
            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
                return;

            int frameCount = sequence.frames.Count;

            // Increment frame index, but ensure it does not exceed frame count of the sequence
            m_FrameIndex = Mathf.Clamp(m_FrameIndex + 1, 0, frameCount - 1);

            // Sync elapsed time to the current frame's timestamp
            m_ElapsedTime = sequence.frames[m_FrameIndex].timestamp;
        }

        /// <summary>
        /// Determines if playback has reached the end.
        /// In frame-based mode, completion is determined by frame index.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        /// <param name="looping">If true, playback never completes.</param>
        /// <returns>True if at the last frame and not looping or if the sequence is null or empty. Otherwise, returns false.</returns>
        public bool IsComplete(XRHandCaptureSequence sequence, bool looping)
        {
            if (looping)
                return false;

            if (sequence == null || sequence.frames == null || sequence.frames.Count == 0)
                return true;

            return m_FrameIndex >= sequence.frames.Count - 1;
        }

        /// <summary>
        /// Frame-based playback never needs interpolation.
        /// </summary>
        /// <param name="currentFrame">The current frame (ignored).</param>
        /// <returns>Always returns false.</returns>
        public bool NeedsInterpolation(XRHandCaptureFrame currentFrame)
        {
            return false;
        }

        /// <summary>
        /// Frame-based playback does not support interpolation.
        /// </summary>
        /// <param name="sequence">The capture sequence (ignored).</param>
        /// <param name="currentFrame">Output parameter (set to default).</param>
        /// <param name="nextFrame">Output parameter (set to default).</param>
        /// <param name="blendScalar">Output parameter (set to 0).</param>
        /// <returns>Always returns false.</returns>
        public bool TryGetInterpolationFrames(
            XRHandCaptureSequence sequence,
            out XRHandCaptureFrame currentFrame,
            out XRHandCaptureFrame nextFrame,
            out float blendScalar)
        {
            currentFrame = default;
            nextFrame = default;
            blendScalar = 0f;
            return false;
        }
    }
}
