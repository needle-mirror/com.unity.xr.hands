namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Interface for coordinate transformation strategies used during playback.
    /// Transforms captured hand poses from capture space to world space based on
    /// the configured anchor and transformation mode.
    /// </summary>
    interface IPlaybackCoordinateTransform
    {
        /// <summary>
        /// Gets the current anchor/origin pose used for coordinate transformation.
        /// </summary>
        Pose CurrentAnchor { get; }

        /// <summary>
        /// Updates the coordinate transform with the anchor pose and frame data.
        /// </summary>
        /// <param name="anchor">The anchor pose that defines the reference frame for transformation.</param>
        /// <param name="frame">The frame of the capture sequence, used for pose calculations.</param>
        /// <param name="handedness">The handedness of the hand being transformed.</param>
        /// <returns>Returns true of root pose was successfully updated</returns>
        bool UpdateRootPose(Pose anchor, XRHandCaptureFrame frame, Handedness handedness);

        /// <summary>
        /// Transforms a captured pose from capture space to world space.
        /// </summary>
        /// <param name="capturedPose">The pose as it was captured in the original coordinate system.</param>
        /// <returns>The transformed pose in world space.</returns>
        Pose TransformPose(in Pose capturedPose);

        /// <summary>
        /// Updates the anchor pose used for coordinate transformation.
        /// This allows the anchor to be changed during playback without full reinitialization.
        /// </summary>
        /// <param name="newAnchor">The new anchor pose to use for subsequent transformations.</param>
        void UpdateAnchor(Pose newAnchor);
    }
}
