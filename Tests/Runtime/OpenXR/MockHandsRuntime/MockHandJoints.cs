#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.OpenXR.MockHandsRuntime.Tests
{
    /// <summary>
    /// Fills the joint array a mocked <c>xrLocateHandJointsEXT</c> is handed, and computes the
    /// Unity-space poses the subsystem should surface in return.
    /// </summary>
    /// <remarks>
    /// The native plugin declares its <c>XrHandJointLocationEXT</c> array on the stack without
    /// initializing it, and reports a hand as tracked only once a joint carries
    /// <c>XR_SPACE_LOCATION_POSITION_VALID_BIT</c>. A mock that sets only <c>isActive</c> therefore
    /// leaves tracking state riding on whatever was on the stack, so any test that asserts tracking
    /// state, joint data or a capture time has to write every joint.
    /// </remarks>
    static class MockHandJoints
    {
        /// <summary>
        /// The number of joints <c>XR_EXT_hand_tracking</c> defines.
        /// </summary>
        public const int k_JointCount = 26;

        /// <summary>
        /// Everything valid and tracked, which is what the native plugin requires before it reports a
        /// joint's pose as high fidelity.
        /// </summary>
        public const XrSpaceLocationFlags k_HighFidelityFlags =
            XrSpaceLocationFlags.PositionValid | XrSpaceLocationFlags.OrientationValid |
            XrSpaceLocationFlags.PositionTracked | XrSpaceLocationFlags.OrientationTracked;

        /// <summary>
        /// Valid but not tracked, which yields a pose without high fidelity.
        /// </summary>
        public const XrSpaceLocationFlags k_LowFidelityFlags =
            XrSpaceLocationFlags.PositionValid | XrSpaceLocationFlags.OrientationValid;

        public const float k_Radius = 0.0123f;

        /// <summary>
        /// The pose the subsystem should surface for the joint the runtime reported at
        /// <paramref name="openXRJointIndex"/>.
        /// </summary>
        /// <param name="openXRJointIndex">An <c>XrHandJointEXT</c> index.</param>
        /// <param name="marker">
        /// Written to the pose's x coordinate so a test can tell which locate call, and which hand,
        /// produced the data it is looking at.
        /// </param>
        /// <returns>The expected Unity-space pose.</returns>
        public static Pose UnityPoseFor(int openXRJointIndex, float marker)
            => new Pose(
                new Vector3(marker, openXRJointIndex * 0.01f, openXRJointIndex * 0.02f),
                Quaternion.Euler(11f, openXRJointIndex * 3f, 7f));

        /// <summary>
        /// Maps a Unity joint index to the <c>XrHandJointEXT</c> index the native plugin reads it from.
        /// Mirrors <c>k_JointIndexToOpenXRType</c>: Unity leads with wrist then palm, OpenXR with palm
        /// then wrist, and the two agree from the thumb onward.
        /// </summary>
        /// <param name="unityJointIndex">An <see cref="XRHandJointID"/> index.</param>
        /// <returns>The corresponding <c>XrHandJointEXT</c> index.</returns>
        public static int ToOpenXRJointIndex(int unityJointIndex)
        {
            if (unityJointIndex == XRHandJointID.Wrist.ToIndex())
                return 1;

            if (unityJointIndex == XRHandJointID.Palm.ToIndex())
                return 0;

            return unityJointIndex;
        }

        /// <summary>
        /// Writes every joint with <paramref name="marker"/> baked into its pose.
        /// </summary>
        /// <param name="joints">The array the runtime was handed.</param>
        /// <param name="marker">Distinguishing value, see <see cref="UnityPoseFor"/>.</param>
        /// <param name="lowFidelityOpenXRJointIndex">
        /// An <c>XrHandJointEXT</c> index to report as valid but untracked, or <c>-1</c> for none.
        /// </param>
        public static unsafe void WriteTrackedHand(
            XrHandJointLocationEXT* joints, float marker, int lowFidelityOpenXRJointIndex = -1)
        {
            if (joints == null)
                return;

            for (var jointIndex = 0; jointIndex < k_JointCount; ++jointIndex)
            {
                var flags = jointIndex == lowFidelityOpenXRJointIndex
                    ? k_LowFidelityFlags
                    : k_HighFidelityFlags;

                // XrPosef's constructor converts into OpenXR space, so what the plugin receives is
                // runtime-space data, as it would be from a real runtime.
                joints[jointIndex] = new XrHandJointLocationEXT(
                    flags, new XrPosef(UnityPoseFor(jointIndex, marker)), k_Radius);
            }
        }
    }
}
#endif
