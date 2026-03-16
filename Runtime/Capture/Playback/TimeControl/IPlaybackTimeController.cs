namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Interface for controlling time progression and frame advancement during playback.
    /// Implementations handle the relationship between elapsed time and frame index,
    /// supporting both frame-based (discrete) and time-based (interpolated) playback modes.
    /// </summary>
    interface IPlaybackTimeController
    {
        /// <summary>
        /// Gets or sets the current elapsed time in seconds since playback started.
        /// </summary>
        float ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the current frame index being played back.
        /// </summary>
        int FrameIndex { get; set; }

        /// <summary>
        /// Resets the controller state to the beginning of playback.
        /// Sets both elapsed time and frame index to their initial values.
        /// </summary>
        void Reset();

        /// <summary>
        /// Updates elapsed time based on the current frame index.
        /// Used when the frame index is set directly to synchronize elapsed time.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        void UpdateTimeBasedOnFrameIndex(XRHandCaptureSequence sequence);

        /// <summary>
        /// Advances time and/or frame index based on the time delta.
        /// The behavior depends on the implementation (frame-based vs time-based).
        /// </summary>
        /// <param name="deltaTime">The time delta to apply (typically Time.deltaTime).</param>
        /// <param name="sequence">The capture sequence being played back.</param>
        void ApplyFrameDelta(float deltaTime, XRHandCaptureSequence sequence);

        /// <summary>
        /// Determines whether playback has reached the end of the sequence.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        /// <param name="looping">Whether looping is enabled.</param>
        /// <returns>True if playback is complete and not looping, false otherwise.</returns>
        bool IsComplete(XRHandCaptureSequence sequence, bool looping);

        /// <summary>
        /// Determines whether interpolation between frames is needed at the current time.
        /// Frame-based controllers always return false, while time-based controllers
        /// return true when elapsed time is between two frame timestamps.
        /// </summary>
        /// <param name="currentFrame">The current frame being evaluated.</param>
        /// <returns>True if interpolation is needed, false otherwise.</returns>
        bool NeedsInterpolation(XRHandCaptureFrame currentFrame);

        /// <summary>
        /// Attempts to get the two frames and blend scalar needed for interpolation.
        /// Only succeeds when time-based playback is between two frames.
        /// </summary>
        /// <param name="sequence">The capture sequence being played back.</param>
        /// <param name="currentFrame">Output: The frame before the current time.</param>
        /// <param name="nextFrame">Output: The frame after the current time.</param>
        /// <param name="blendScalar">Output: Value between 0-1 indicating interpolation position.</param>
        /// <returns>True if interpolation frames are available, false otherwise.</returns>
        bool TryGetInterpolationFrames(
            XRHandCaptureSequence sequence,
            out XRHandCaptureFrame currentFrame,
            out XRHandCaptureFrame nextFrame,
            out float blendScalar);
    }
}
