#if OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION

using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRFeature"/> enables the
    /// <c>XR_META_hand_tracking_wide_motion_mode2</c> OpenXR extension, which
    /// adds a wide-motion data source to <c>XR_EXT_hand_tracking_data_source</c>.
    /// When enabled, the runtime uses inference algorithms to estimate hand poses
    /// even when hands are outside the normal camera tracking volume.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This feature requires both the <see cref="HandTracking"/> feature and the
    /// <see cref="HandTrackingDataSourceFeature"/> to be enabled. It configures
    /// the data source feature to request both <see cref="HandTrackingDataSource.Unobstructed"/>
    /// and <see cref="HandTrackingDataSource.UnobstructedWideMotion"/> so that the
    /// runtime uses direct tracking when hands are visible and falls back
    /// to wide-motion inference when they leave the camera field of view.
    /// </para>
    /// <para>
    /// Requesting <see cref="HandTrackingDataSource.UnobstructedWideMotion"/> alone
    /// disables optical tracking entirely. This feature always requests both sources
    /// to ensure continuous hand tracking.
    /// </para>
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "Meta Hand Tracking Wide Motion Mode",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Enables wide-motion hand tracking that uses inference to estimate hand poses when hands are outside the camera tracking volume.",
        DocumentationLink = XRHelpURLConstants.k_OpenXRFeaturesDocsBaseUrl + "metahandtrackingwidemotionmode.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId,
        // Must run after HandTracking so that HandTrackingDataSourceFeature is initialized.
        Priority = HandTracking.k_Priority - 1)]
#endif
    public class MetaHandTrackingWideMotionMode : OpenXRFeature
    {
        /// <summary>
        /// The feature ID string. This is used to give the feature a well-known
        /// ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.metahandtrackingwidemotionmode";

        /// <summary>
        /// The OpenXR Extension string. OpenXR uses this to check if this
        /// extension is available or enabled.
        /// </summary>
        public const string extensionString = "XR_META_hand_tracking_wide_motion_mode2";

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            var settings = OpenXRSettings.Instance;
            if (settings == null)
                return false;

            var dataSourceFeature = settings.GetFeature<HandTrackingDataSourceFeature>();
            if (dataSourceFeature == null || !dataSourceFeature.enabled)
            {
                Debug.LogWarning(
                    $"[MetaHandTrackingWideMotionMode] {nameof(HandTrackingDataSourceFeature)} " +
                    "is not enabled. Wide motion mode requires the Hand Tracking Data Source feature.");
                return false;
            }

            var preferredSources = new[]
            {
                HandTrackingDataSource.Unobstructed,
                HandTrackingDataSource.UnobstructedWideMotion,
            };

            dataSourceFeature.TryUpdateConfiguration(new HandTrackingDataSourceConfig
            {
                leftPreferredSources = preferredSources,
                rightPreferredSources = preferredSources,
            });

            return base.OnInstanceCreate(xrInstance);
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(this)
            {
                message = "Meta Hand Tracking Wide Motion Mode requires the Hand Tracking Subsystem feature to be enabled.",
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

            results.Add(new ValidationRule(this)
            {
                message = "Meta Hand Tracking Wide Motion Mode requires the Hand Tracking Data Source feature to be enabled.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return false;

                    var dataSource = settings.GetFeature<HandTrackingDataSourceFeature>();
                    return dataSource != null && dataSource.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return;

                    var dataSource = settings.GetFeature<HandTrackingDataSourceFeature>();
                    if (dataSource != null)
                    {
                        dataSource.enabled = true;
                        EditorUtility.SetDirty(dataSource);
                    }
                },
                fixItAutomatic = true,
                error = true,
            });
        }
#endif
    }
}

#endif // OPENXR_1_19_OR_NEWER || PACKAGE_DOCS_GENERATION
