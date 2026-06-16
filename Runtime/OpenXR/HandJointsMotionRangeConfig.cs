#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Configuration for per-hand motion range constraints.
    /// Used with
    /// <see cref="IXRHandConfigurationHandler{TConfig}"/>
    /// to stage and query the active motion range on the
    /// the hand joints motion range feature.
    /// </summary>
    public struct HandJointsMotionRangeConfig
    {
        /// <summary>
        /// The motion range constraint for the left hand.
        /// </summary>
        public HandJointsMotionRange leftMotionRange;

        /// <summary>
        /// The motion range constraint for the right hand.
        /// </summary>
        public HandJointsMotionRange rightMotionRange;
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION
