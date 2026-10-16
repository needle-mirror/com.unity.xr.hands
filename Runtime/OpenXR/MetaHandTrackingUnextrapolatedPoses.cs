#if OPENXR_1_19_0_PRE_2_OR_NEWER || PACKAGE_DOCS_GENERATION

using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRFeature"/> enables
    /// <a href="https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_META_hand_tracking_unextrapolated_poses">
    /// XR_META_hand_tracking_unextrapolated_poses</a>, which surfaces hand joint poses that the
    /// runtime has not extrapolated, along with the time at which they were captured.
    /// </summary>
    /// <remarks>
    /// Retrieve the poses with <see cref="XRHandSubsystem.GetHand(Handedness, XRHandExtrapolation)"/>
    /// using <see cref="XRHandExtrapolation.Unextrapolated"/>. The extrapolated poses are unaffected.
    ///
    /// <see cref="XRHand.TryGetTimestamp"/> reports the capture time as a raw <c>XrTime</c> on the
    /// runtime's clock.
    ///
    /// > [!WARNING]
    /// > The runtime currently reports incorrect joint velocities for unextrapolated poses. It's a known issue.
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Meta Hand Tracking Unextrapolated Poses",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Surfaces hand joint poses without runtime extrapolation, along with the time at which they were captured.",
        DocumentationLink = XRHelpURLConstants.k_OpenXRFeaturesDocsBaseUrl + "metahandtrackingunextrapolatedposes.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId,
        Priority = HandTracking.k_ExtensionPriority)]
#endif
    public class MetaHandTrackingUnextrapolatedPoses : OpenXRHandTrackingFeature
    {
        /// <summary>
        /// The feature ID string. This is used to give the feature a well known ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.metahandtrackingunextrapolatedposes";

        /// <summary>
        /// The OpenXR Extension string. OpenXR uses this to check if this extension is available
        /// or enabled.
        /// </summary>
        public const string extensionString = "XR_META_hand_tracking_unextrapolated_poses";

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(xrInstance);
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);

            AddSecondaryLocateNodes(XrHandEXT.Left);
            AddSecondaryLocateNodes(XrHandEXT.Right);
        }

        void AddSecondaryLocateNodes(XrHandEXT hand)
        {
            GetSecondaryLocateInputChain(hand)?.TryAddNode(
                XrHandTrackingUnextrapolatedPosesRequestMETA.defaultValue);

            GetSecondaryLocateOutputChain(hand)?.TryAddNode(
                XrHandTrackingUnextrapolatedPosesMETA.defaultValue);
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(this)
            {
                message = "Meta Hand Tracking Unextrapolated Poses requires the Hand Tracking Subsystem feature to be enabled.",
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
    }
}

#endif // OPENXR_1_19_0_PRE_2_OR_NEWER || PACKAGE_DOCS_GENERATION
