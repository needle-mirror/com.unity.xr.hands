#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Specifies the motion range constraint applied to hand joint poses.
    /// Values correspond to the
    /// <c>XrHandJointsMotionRangeEXT</c> enumeration defined by
    /// <a href="https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_EXT_hand_joints_motion_range">
    /// XR_EXT_hand_joints_motion_range</a>.
    /// </summary>
    public enum HandJointsMotionRange
    {
        /// <summary>
        /// Joint poses are not constrained by a held controller and reflect
        /// the full natural range of hand motion.
        /// Maps to <c>XR_HAND_JOINTS_MOTION_RANGE_UNOBSTRUCTED_EXT</c>.
        /// </summary>
        Unobstructed = 1,

        /// <summary>
        /// Joint poses are constrained to conform to the shape of a held
        /// controller, reflecting how the hand wraps around the device.
        /// Maps to <c>XR_HAND_JOINTS_MOTION_RANGE_CONFORMING_TO_CONTROLLER_EXT</c>.
        /// </summary>
        ConformingToController = 2,
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION
