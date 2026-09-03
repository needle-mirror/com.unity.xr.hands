#if OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Configuration for the requested hand tracking update frequency.
    /// Used with
    /// <see cref="IXRHandConfigurationHandler{TConfig}"/>
    /// to stage and query the requested frequency on the
    /// Meta hand tracking frequency hint feature.
    /// </summary>
    /// <remarks>
    /// The frequency applies per-session, not per-hand. Both hands share the
    /// same tracking frequency.
    /// </remarks>
    public struct MetaHandTrackingFrequencyHintConfig
    {
        /// <summary>
        /// The requested hand tracking update frequency.
        /// </summary>
        public MetaHandTrackingFrequencyHint frequencyHint;
    }
}

#endif // OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION
