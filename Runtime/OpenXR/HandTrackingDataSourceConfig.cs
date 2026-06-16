#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Configuration for per-hand preferred hand tracking data sources.
    /// Used with
    /// <see cref="IXRHandConfigurationHandler{TConfig}"/>
    /// to stage and query preferred sources on the
    /// <see cref="HandTrackingDataSourceFeature"/>.
    /// </summary>
    /// <remarks>
    /// Passing this struct to
    /// <see cref="UnityEngine.XR.Hands.XRHandSubsystem.TryUpdateConfiguration{TConfig}"/> triggers a
    /// hand tracker restart on the next Dynamic update so the new preferred
    /// sources take effect at tracker creation time.
    /// </remarks>
    public struct HandTrackingDataSourceConfig
    {
        /// <summary>
        /// The preferred data sources for the left hand.
        /// </summary>
        public HandTrackingDataSource[] leftPreferredSources;

        /// <summary>
        /// The preferred data sources for the right hand.
        /// </summary>
        public HandTrackingDataSource[] rightPreferredSources;
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
