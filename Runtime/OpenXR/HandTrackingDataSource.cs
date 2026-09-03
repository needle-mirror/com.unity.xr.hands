#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Identifies the source of hand tracking data reported by the runtime.
    /// Values correspond to the
    /// <c>XrHandTrackingDataSourceEXT</c> enumeration defined by
    /// <a href="https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_EXT_hand_tracking_data_source">
    /// XR_EXT_hand_tracking_data_source</a>.
    /// </summary>
    public enum HandTrackingDataSource
    {
        /// <summary>
        /// Hand tracking data is derived from optical (camera-based) hand tracking
        /// with no controller present.
        /// Maps to <c>XR_HAND_TRACKING_DATA_SOURCE_UNOBSTRUCTED_EXT</c>.
        /// </summary>
        Unobstructed = 1,

        /// <summary>
        /// Hand tracking data is derived from a held controller's sensors
        /// (e.g., capacitive touch, IMU).
        /// Maps to <c>XR_HAND_TRACKING_DATA_SOURCE_CONTROLLER_EXT</c>.
        /// </summary>
        Controller = 2,

#if OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION
        /// <summary>
        /// Hand tracking data is derived from inference algorithms that estimate
        /// hand poses when hands are outside the normal camera tracking volume.
        /// Maps to <c>XR_HAND_TRACKING_DATA_SOURCE_UNOBSTRUCTED_WIDE_MOTION_META</c>.
        /// </summary>
        UnobstructedWideMotion = 1000695000,
#endif
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
