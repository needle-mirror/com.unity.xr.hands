using Unity.Collections;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Coordinate transformation that anchors playback relative to the initial root pose of the hand.
    /// This transformation mode calculates the position and rotation of the hand relative to its
    /// initial pose in the first frame, then applies that relative transformation to the anchor pose.
    /// This is used when <see cref="XRHandPlaybackOptions.RootPoseLockedToAnchor"/> is enabled.
    /// </summary>
    class RelativeToInitialRootTransform : IPlaybackCoordinateTransform
    {
        Pose m_OriginPose;
        Quaternion m_RotateIntoInitialRootPoseSpace;
        Vector3 m_InitialRootPosition;

        /// <summary>
        /// Gets the current anchor/origin pose used for coordinate transformation.
        /// </summary>
        public Pose CurrentAnchor => m_OriginPose;

        /// <summary>
        /// Updates the transform by extracting the root pose from the frame.
        /// </summary>
        /// <param name="anchor">The anchor pose that defines the world-space reference.</param>
        /// <param name="frame">The frame of capture used to determine the initial hand root pose.</param>
        /// <param name="handedness">Which hand to extract the root pose from.</param>
        public bool UpdateRootPose(Pose anchor, XRHandCaptureFrame frame, Handedness handedness)
        {
            m_OriginPose = anchor;

            // Try to get the hand from the frame to establish the root pose
            if (TryGetHandFromFrame(frame, handedness, out var hand))
            {
                // Try to get the wrist pose, fall back to root pose if not available
                if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var initialRootPose))
                    initialRootPose = hand.rootPose;

                // Calculate the rotation that brings us into the root pose space
                // We only use the Y rotation to keep the hand upright
                // Note: eulerAngles returns degrees, Quaternion.Euler expects degrees
                m_RotateIntoInitialRootPoseSpace = Quaternion.Inverse(
                    Quaternion.Euler(0f, initialRootPose.rotation.eulerAngles.y, 0f));
                m_InitialRootPosition = initialRootPose.position;

                hand.Dispose();
                return true;
            }
            else
            {
                // If we can't get the hand, use identity transforms.
                // Hand is likely untracked and will not render.
                m_RotateIntoInitialRootPoseSpace = Quaternion.identity;
                m_InitialRootPosition = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Transforms a captured pose to world space relative to the initial root pose.
        /// The transformation applies the relative position and rotation changes from the
        /// initial pose, then transforms the result by the anchor/origin pose.
        /// </summary>
        /// <param name="capturedPose">The pose as captured in the original coordinate system.</param>
        /// <returns>The pose transformed to world space relative to initial root.</returns>
        public Pose TransformPose(in Pose capturedPose)
        {
            // Calculate the position relative to the initial root position, rotated into the initial root pose space,
            // then transformed by the origin pose
            Vector3 relativePosition = capturedPose.position - m_InitialRootPosition;
            Vector3 rotatedRelativePosition = m_RotateIntoInitialRootPoseSpace * relativePosition;
            Vector3 worldPosition = m_OriginPose.position + (m_OriginPose.rotation * rotatedRelativePosition);

            // Calculate the rotation by combining the rotations
            Quaternion worldRotation = m_OriginPose.rotation * m_RotateIntoInitialRootPoseSpace * capturedPose.rotation;

            return new Pose(worldPosition, worldRotation);
        }

        /// <summary>
        /// Updates the anchor/origin pose used for transformation.
        /// Note: This does not recalculate the initial root pose - that remains fixed from <see cref="UpdateRootPose"/>.
        /// </summary>
        /// <param name="newAnchor">The new anchor pose.</param>
        public void UpdateAnchor(Pose newAnchor)
        {
            m_OriginPose = newAnchor;
        }

        /// <summary>
        /// Helper method to extract a hand from a capture frame.
        /// Tries both Dynamic and BeforeRender update types to find valid hand data.
        /// </summary>
        bool TryGetHandFromFrame(in XRHandCaptureFrame frame, Handedness handedness, out XRHand hand)
        {
            hand = default;

            if (!frame.TryGetHandSnapshot(handedness, out var handCaptureSnapshot))
                return false;

            // Try Dynamic first, then BeforeRender
            return handCaptureSnapshot.TryGetHand(XRHandSubsystem.UpdateType.Dynamic, Allocator.Temp, out hand)
                || handCaptureSnapshot.TryGetHand(XRHandSubsystem.UpdateType.BeforeRender, Allocator.Temp, out hand);
        }
    }
}
