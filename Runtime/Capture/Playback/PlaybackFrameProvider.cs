using Unity.Collections;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Provides centralized access to frame data from an XRHandCaptureSequence.
    /// Encapsulates frame retrieval, validation, and hand data extraction to reduce
    /// direct sequence manipulation and improve testability.
    /// </summary>
    /// <remarks>
    /// Thread Safety: This class is not thread-safe. Frame access should occur on the
    /// same thread that updates the sequence. Updating the sequence during frame access
    /// may result in undefined behavior.
    /// </remarks>
    class PlaybackFrameProvider
    {
        XRHandCaptureSequence m_Sequence;
        Handedness m_Handedness;

        /// <summary>
        /// Gets the number of frames in the current sequence.
        /// Returns 0 if the sequence is null or has no frames.
        /// </summary>
        public int frameCount => m_Sequence?.frames?.Count ?? 0;

        /// <summary>
        /// Gets whether the current sequence has any frames available.
        /// </summary>
        public bool hasFrames => frameCount > 0;

        /// <summary>
        /// Gets whether the current sequence is valid and has a frames collection.
        /// </summary>
        public bool isSequenceValid => m_Sequence != null && m_Sequence.frames != null;

        /// <summary>
        /// Initializes a new instance of PlaybackFrameProvider.
        /// </summary>
        /// <param name="sequence">The capture sequence to provide frames from. Can be null.</param>
        /// <param name="handedness">The handedness of the hand to extract data for.</param>
        public PlaybackFrameProvider(XRHandCaptureSequence sequence, Handedness handedness)
        {
            m_Sequence = sequence;
            m_Handedness = handedness;
        }

        /// <summary>
        /// Updates the sequence source for frame data.
        /// </summary>
        /// <param name="newSequence">The new sequence to use. Can be null.</param>
        /// <remarks>
        /// Calling this while frames are being accessed may result in undefined behavior.
        /// It is recommended to stop playback before changing the sequence.
        /// </remarks>
        public void UpdateSequence(XRHandCaptureSequence newSequence)
        {
            m_Sequence = newSequence;
        }

        /// <summary>
        /// Attempts to retrieve the frame at the specified index.
        /// </summary>
        /// <param name="frameIndex">The index of the frame to retrieve.</param>
        /// <param name="frame">Output parameter for the frame if retrieval succeeds.</param>
        /// <returns>True if the frame was successfully retrieved, false otherwise.</returns>
        /// <remarks>
        /// Returns false if:
        /// - The sequence is null or has no frames
        /// - The frame index is negative
        /// - The frame index is greater than or equal to the frame count
        /// </remarks>
        public bool TryGetCurrentFrame(int frameIndex, out XRHandCaptureFrame frame)
        {
            frame = default;

            if (m_Sequence == null || m_Sequence.frames == null)
                return false;

            if (frameIndex < 0 || frameIndex >= m_Sequence.frames.Count)
                return false;

            frame = m_Sequence.frames[frameIndex];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the frame after the specified index.
        /// </summary>
        /// <param name="frameIndex">The index of the current frame. The next frame (frameIndex + 1) will be retrieved.</param>
        /// <param name="frame">Output parameter for the next frame if retrieval succeeds.</param>
        /// <returns>True if the next frame was successfully retrieved, false otherwise.</returns>
        public bool TryGetNextFrame(int frameIndex, out XRHandCaptureFrame frame)
        {
            return TryGetCurrentFrame(frameIndex + 1, out frame);
        }

        /// <summary>
        /// Attempts to retrieve a pair of frames for interpolation.
        /// </summary>
        /// <param name="frameIndex">The index of the current frame.</param>
        /// <param name="current">Output parameter for the current frame.</param>
        /// <param name="next">Output parameter for the next frame.</param>
        /// <returns>True if both frames were successfully retrieved, false otherwise.</returns>
        /// <remarks>
        /// This method is typically used for time-based playback interpolation where
        /// hand data needs to be blended between two consecutive frames.
        /// </remarks>
        public bool TryGetInterpolationFrames(
            int frameIndex,
            out XRHandCaptureFrame current,
            out XRHandCaptureFrame next)
        {
            current = default;
            next = default;

            if (!TryGetCurrentFrame(frameIndex, out current))
                return false;

            if (!TryGetNextFrame(frameIndex, out next))
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to extract hand data from a frame for the configured handedness.
        /// </summary>
        /// <param name="frame">The frame to extract hand data from.</param>
        /// <param name="updateType">The update type to use for extracting hand data (Dynamic or BeforeRender).</param>
        /// <param name="hand">Output parameter for the hand data if extraction succeeds.</param>
        /// <returns>True if hand data was successfully extracted and the hand is tracked, false otherwise.</returns>
        /// <remarks>
        /// The hand data returned depends on both the handedness configured in the constructor
        /// and the update type parameter. BeforeRender and Dynamic update types may contain
        /// different hand poses for the same frame.
        /// </remarks>
        public bool TryGetHandFromFrame(
            in XRHandCaptureFrame frame,
            XRHandSubsystem.UpdateType updateType,
            out XRHand hand)
        {
            hand = default;

            if (!frame.TryGetHandSnapshot(m_Handedness, out var handCaptureSnapshot))
                return false;

            if (!handCaptureSnapshot.TryGetHand(updateType, Allocator.Temp, out hand))
                return false;

            return hand.isTracked;
        }

        /// <summary>
        /// Checks whether a frame index is valid for the current sequence.
        /// </summary>
        /// <param name="frameIndex">The frame index to validate.</param>
        /// <returns>True if the index is within valid bounds, false otherwise.</returns>
        public bool IsFrameIndexValid(int frameIndex)
        {
            return frameIndex >= 0 && frameIndex < frameCount;
        }

        /// <summary>
        /// Checks whether interpolation is possible starting at the specified frame index.
        /// </summary>
        /// <param name="frameIndex">The starting frame index for interpolation.</param>
        /// <returns>True if both the current frame and next frame exist, false otherwise.</returns>
        /// <remarks>
        /// This method returns false if frameIndex is the last frame in the sequence,
        /// since there is no next frame to interpolate to.
        /// </remarks>
        public bool CanInterpolate(int frameIndex)
        {
            return IsFrameIndexValid(frameIndex) &&
                   IsFrameIndexValid(frameIndex + 1);
        }
    }
}
