namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Simple world-space coordinate transformation.
    /// Treats the anchor pose as the origin of the capture space, applying a straightforward
    /// transformation to convert captured poses to world space.
    /// This is the default transformation mode when <see cref="XRHandPlaybackOptions.RootPoseLockedToAnchor"/>
    /// is not enabled.
    /// </summary>
    class WorldSpaceCoordinateTransform : IPlaybackCoordinateTransform
    {
        Pose m_OriginPose;

        /// <summary>
        /// Gets the current anchor/origin pose used for coordinate transformation.
        /// </summary>
        public Pose CurrentAnchor => m_OriginPose;

        /// <summary>
        /// Updates the transform with the anchor pose.
        /// </summary>
        /// <param name="anchor">The anchor pose that defines where the capture origin is in world space.</param>
        /// <param name="frame">Not used in this transformation mode.</param>
        /// <param name="handedness">Not used in this transformation mode.</param>
        public bool UpdateRootPose(Pose anchor, XRHandCaptureFrame frame, Handedness handedness)
        {
            m_OriginPose = anchor;
            return true;
        }

        /// <summary>
        /// Transforms a captured pose to world space by applying the origin pose transformation.
        /// </summary>
        /// <param name="capturedPose">The pose as captured in the original coordinate system.</param>
        /// <returns>The pose transformed to world space.</returns>
        public Pose TransformPose(in Pose capturedPose)
        {
            return capturedPose.GetTransformedBy(m_OriginPose);
        }

        /// <summary>
        /// Updates the anchor/origin pose used for transformation.
        /// </summary>
        /// <param name="newAnchor">The new anchor pose.</param>
        public void UpdateAnchor(Pose newAnchor)
        {
            m_OriginPose = newAnchor;
        }
    }
}
