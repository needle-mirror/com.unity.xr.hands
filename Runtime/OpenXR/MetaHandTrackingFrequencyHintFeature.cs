#if OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRFeature"/> enables the <c>XR_META_hand_tracking_frequency_hint</c>
    /// OpenXR extension, which augments the <c>XR_EXT_hand_tracking</c> extension.
    /// It allows applications to provide a frequency hint to the runtime to indicate the
    /// desired hand tracking update frequency. Applications can suggest that the runtime
    /// use a higher tracking frequency for low-latency scenarios, or use the default
    /// frequency for normal operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This feature requires the <see cref="HandTracking"/> feature to also be enabled.
    /// </para>
    /// <para>
    /// The frequency hint is applied to the session rather than injected into a hand
    /// tracker chain, so updates made through
    /// <see cref="IXRHandConfigurationHandler{TConfig}.TryUpdateConfiguration"/>
    /// take effect immediately while a session is running. A hint staged before the
    /// session exists is deferred until session creation.
    /// </para>
    /// <para>
    /// The frequency hint is a suggestion only. The runtime may choose to ignore the hint
    /// based on user preferences, system constraints, power management policies, or other
    /// considerations. Applications should not rely on the runtime honoring the hint and
    /// should be prepared to handle hand tracking data at any supported frequency.
    /// </para>
    /// <para>
    /// For this extension to be available, you must install the
    /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@latest/manual/index.html">
    /// XR Hands package</see>.
    /// </para>
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "Meta Hand Tracking Frequency Hint",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows requesting higher frequency hand tracking updates for improved responsiveness on Meta Quest devices.",
        DocumentationLink = XRHelpURLConstants.k_OpenXRFeaturesDocsBaseUrl + "metahandtrackingfrequencyhint.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId,
        // Must run after HandTracking so that the xr session handle is available.
        Priority = HandTracking.k_Priority - 1)]
#endif
    internal class MetaHandTrackingFrequencyHintFeature
        : OpenXRHandTrackingFeature,
          IXRHandConfigurationHandler<MetaHandTrackingFrequencyHintConfig>
    {
        /// <summary>
        /// The feature ID string. This is used to give the feature a well-known ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.metahandtrackingfrequencyhint";

        /// <summary>
        /// The OpenXR extension string. OpenXR uses this to check if this
        /// extension is available or enabled.
        /// </summary>
        public const string extensionString = "XR_META_hand_tracking_frequency_hint";

        [SerializeField]
        [Tooltip("The frequency hint to apply when the OpenXR session starts. " +
            "Higher frequencies improve responsiveness but may increase power consumption. " +
            "The runtime may choose to ignore this hint based on system constraints or other considerations.")]
        internal MetaHandTrackingFrequencyHint m_FrequencyHint = MetaHandTrackingFrequencyHint.Default;

        XRHandSubsystem m_Subsystem;
        bool m_SessionRunning;

        /// <inheritdoc/>
        /// <remarks>
        /// Gets the frequency hint that the application last successfully requested.
        /// This does not indicate the actual frequency the runtime is delivering,
        /// only what the application asked for. The runtime may choose to ignore the
        /// hint based on system constraints. This implementation always returns <c>true</c>.
        /// </remarks>
        public bool TryGetConfiguration(out MetaHandTrackingFrequencyHintConfig config)
        {
            config = new MetaHandTrackingFrequencyHintConfig
            {
                frequencyHint = m_FrequencyHint,
            };
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If an OpenXR session is running, the hint is applied immediately. The stored
        /// value is only updated when the native call succeeds or when no session is
        /// running yet (in which case the hint is deferred until session creation).
        /// Returns <c>false</c> if the native call to set the frequency hint failed.
        /// </remarks>
        public bool TryUpdateConfiguration(MetaHandTrackingFrequencyHintConfig config)
        {
            if (m_FrequencyHint == config.frequencyHint)
                return true;

            if (m_SessionRunning)
            {
                if (!ApplyFrequencyHint(config.frequencyHint))
                    return false;
            }

            m_FrequencyHint = config.frequencyHint;
            return true;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(xrInstance);
        }

        /// <summary>
        /// Called after <c>xrCreateSession</c>. Applies the configured frequency hint
        /// to the newly created session.
        /// </summary>
        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);
            m_SessionRunning = true;

            if (!ApplyFrequencyHint(m_FrequencyHint))
                m_FrequencyHint = MetaHandTrackingFrequencyHint.Default;
        }

        /// <summary>
        /// Called before xrDestroySession.
        /// </summary>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            base.OnSessionDestroy(xrSession);
            m_SessionRunning = false;
        }

        /// <inheritdoc/>
        protected override void OnHandSubsystemCreated(XRHandSubsystem subsystem)
        {
            base.OnHandSubsystemCreated(subsystem);
            m_Subsystem = subsystem;
            subsystem.RegisterConfigurationHandler(this);
        }

        /// <inheritdoc/>
        protected override void OnHandSubsystemDestroyed(XRHandSubsystem subsystem)
        {
            if (m_Subsystem != null && m_Subsystem == subsystem)
            {
                m_Subsystem.UnregisterConfigurationHandler<MetaHandTrackingFrequencyHintConfig>();
                m_Subsystem = null;
            }

            base.OnHandSubsystemDestroyed(subsystem);
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(this)
            {
                message = "Meta Hand Tracking Frequency Hint requires the Hand Tracking Subsystem feature to be enabled.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return false;

                    var handTracking = settings.GetFeature<HandTracking>();
                    return handTracking != null && handTracking.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return;

                    var handTracking = settings.GetFeature<HandTracking>();
                    if (handTracking != null)
                    {
                        handTracking.enabled = true;
                        EditorUtility.SetDirty(handTracking);
                    }
                },
                fixItAutomatic = true,
                error = true,
            });
        }
#endif

        OpenXRResultStatus ApplyFrequencyHint(MetaHandTrackingFrequencyHint hint)
        {
            var xrResult = NativeApi.SetFrequencyHint((int)hint);

            if (xrResult.IsError())
            {
                Debug.LogWarning(
                    $"[MetaHandTrackingFrequencyHintFeature] Failed to set hand tracking frequency hint to {hint.ToString()}. " +
                    $"Native result: {xrResult}. " +
                    $"This extension ({extensionString}) may not be supported by your device or runtime. " +
                    "The extension is optional and hand tracking will continue to work normally.");
            }
            var result = new OpenXRResultStatus(xrResult);

            return result;
        }

        static class NativeApi
        {
            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_SetHandTrackingFrequencyHint")]
            internal static extern XrResult SetFrequencyHint(int hint);
        }
    }
}

#endif // OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION
