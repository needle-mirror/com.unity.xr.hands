#if OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Enumerates the frequency hints that applications can provide to suggest the
    /// desired hand tracking update frequency to the runtime.
    /// </summary>
    public enum MetaHandTrackingFrequencyHint
    {
        /// <summary>
        /// Suggests the runtime use its default hand tracking frequency. This is
        /// typically the most power-efficient frequency that provides adequate
        /// tracking quality for general use cases.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>XR_HAND_TRACKING_FREQUENCY_HINT_DEFAULT_META</c> in the OpenXR header.
        /// </remarks>
        Default = 1,

        /// <summary>
        /// Suggests the runtime use a higher hand tracking frequency when possible.
        /// This may provide more responsive tracking for performance-critical
        /// applications, but at higher frame rates the effectiveness of temporal
        /// smoothing algorithms is reduced, which can result in increased jitter
        /// and less visually smooth hand tracking.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>XR_HAND_TRACKING_FREQUENCY_HINT_HIGH_META</c> in the OpenXR header.
        /// </remarks>
        High = 2,
    }
}

#endif // OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION
