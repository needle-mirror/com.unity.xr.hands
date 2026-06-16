#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION

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
    /// <see href="https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_EXT_hand_joints_motion_range">
    /// XR_EXT_hand_joints_motion_range</see>, allowing the application to
    /// constrain hand joint poses to either natural (unobstructed) motion or
    /// motion conforming to a held controller.
    /// </summary>
    /// <remarks>
    /// The motion range is injected into the per-frame
    /// <c>xrLocateHandJointsEXT</c> input chain. Updates made through
    /// <see cref="IXRHandConfigurationHandler{TConfig}.TryUpdateConfiguration"/>
    /// take effect on the next <c>xrLocateHandJointsEXT</c> call without
    /// requiring a hand tracker recreation.
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Hand Joints Motion Range",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows the application to constrain hand joint poses to natural movement or controller-conforming movement.",
        DocumentationLink = XRHelpURLConstants.k_OpenXRFeaturesDocsBaseUrl + "handjointsmotionrange.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId)]
#endif
    internal class HandJointsMotionRangeFeature
        : OpenXRHandTrackingFeature,
          IXRHandConfigurationHandler<HandJointsMotionRangeConfig>
    {
        /// <summary>
        /// The feature ID string. This is used to give the feature a well known
        /// ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.handjointsmotionrange";

        /// <summary>
        /// The OpenXR Extension string. OpenXR uses this to check if this
        /// extension is available or enabled.
        /// </summary>
        public const string extensionString = "XR_EXT_hand_joints_motion_range";

        [SerializeField]
        [Tooltip("Specifies the motion range constraint for the left hand.")]
        internal HandJointsMotionRange m_LeftMotionRange = HandJointsMotionRange.Unobstructed;

        [SerializeField]
        [Tooltip("Specifies the motion range constraint for the right hand.")]
        internal HandJointsMotionRange m_RightMotionRange = HandJointsMotionRange.Unobstructed;

        XRHandSubsystem m_Subsystem;
        bool m_SessionActive;

        /// <inheritdoc/>
        public bool TryGetConfiguration(out HandJointsMotionRangeConfig config)
        {
            config = new HandJointsMotionRangeConfig
            {
                leftMotionRange = m_LeftMotionRange,
                rightMotionRange = m_RightMotionRange,
            };
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Returns <c>false</c> without modifying state if either value in
        /// <paramref name="config"/> is not a defined <see cref="HandJointsMotionRange"/>
        /// member. State is mutated atomically; on any other failure the previous
        /// configuration is retained.
        /// </remarks>
        public bool TryUpdateConfiguration(HandJointsMotionRangeConfig config)
        {
            if (!IsValid(config.leftMotionRange) || !IsValid(config.rightMotionRange))
                return false;

            if (m_SessionActive && !(IsStructureChainValid(XrHandEXT.Left) && IsStructureChainValid(XrHandEXT.Right)))
                return false;

            if (m_SessionActive)
            {
                if (config.leftMotionRange != m_LeftMotionRange)
                    UpdateMotionRangeNode(XrHandEXT.Left, config.leftMotionRange);
                if (config.rightMotionRange != m_RightMotionRange)
                    UpdateMotionRangeNode(XrHandEXT.Right, config.rightMotionRange);
            }

            m_LeftMotionRange = config.leftMotionRange;
            m_RightMotionRange = config.rightMotionRange;
            return true;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            if (!base.OnInstanceCreate(xrInstance))
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Stages the initial per-hand <c>XrHandJointsMotionRangeInfoEXT</c> nodes
        /// on the locate input chain. The nodes persist for the session lifetime
        /// and are mutated in place by <see cref="TryUpdateConfiguration"/>.
        /// </remarks>
        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);

            GetLocateInputChain(XrHandEXT.Left)?.TryAddNode(
                new XrHandJointsMotionRangeInfoEXT((XrHandJointsMotionRangeEXT)m_LeftMotionRange));
            GetLocateInputChain(XrHandEXT.Right)?.TryAddNode(
                new XrHandJointsMotionRangeInfoEXT((XrHandJointsMotionRangeEXT)m_RightMotionRange));

            m_SessionActive = true;
        }

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            m_SessionActive = false;
            base.OnSessionDestroy(xrSession);
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
                m_Subsystem.UnregisterConfigurationHandler<HandJointsMotionRangeConfig>();
                m_Subsystem = null;
            }

            base.OnHandSubsystemDestroyed(subsystem);
        }

        bool UpdateMotionRangeNode(XrHandEXT hand, HandJointsMotionRange motionRange)
        {
            var chain = GetLocateInputChain(hand);
            return chain != null && chain.TryUpdateNode(
                new XrHandJointsMotionRangeInfoEXT((XrHandJointsMotionRangeEXT)motionRange));
        }

        bool IsStructureChainValid(XrHandEXT hand)
        {
            var chain = GetLocateInputChain(hand);
            return chain != null && chain.ContainsNode(XrStructureType.HandJointsMotionRangeInfoEXT);
        }

        static bool IsValid(HandJointsMotionRange value)
            => value == HandJointsMotionRange.Unobstructed
                || value == HandJointsMotionRange.ConformingToController;

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(this)
            {
                message = "Hand Joints Motion Range requires the Hand Tracking Subsystem feature to be enabled.",
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

#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
            results.Add(new ValidationRule(this)
            {
                message = "'Controller-locked' motion range requires the Hand Tracking Data Source feature to include 'Controller-driven' as a preferred source.",
                checkPredicate = () =>
                {
                    if (m_LeftMotionRange != HandJointsMotionRange.ConformingToController
                        && m_RightMotionRange != HandJointsMotionRange.ConformingToController)
                        return true;

                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return true;

                    var dataSourceFeature = settings.GetFeature<HandTrackingDataSourceFeature>();
                    if (dataSourceFeature == null || !dataSourceFeature.enabled)
                        return true;

                    bool leftValid = m_LeftMotionRange != HandJointsMotionRange.ConformingToController
                        || dataSourceFeature.m_LeftHandPreference != HandTrackingDataSourceFeature.DataSourcePreference.TrackedHand;

                    bool rightValid = m_RightMotionRange != HandJointsMotionRange.ConformingToController
                        || dataSourceFeature.m_RightHandPreference != HandTrackingDataSourceFeature.DataSourcePreference.TrackedHand;

                    return leftValid && rightValid;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                        return;

                    var dataSourceFeature = settings.GetFeature<HandTrackingDataSourceFeature>();
                    if (dataSourceFeature == null)
                        return;

                    dataSourceFeature.enabled = true;

                    if (m_LeftMotionRange == HandJointsMotionRange.ConformingToController
                        && dataSourceFeature.m_LeftHandPreference == HandTrackingDataSourceFeature.DataSourcePreference.TrackedHand)
                        dataSourceFeature.m_LeftHandPreference = HandTrackingDataSourceFeature.DataSourcePreference.Both;

                    if (m_RightMotionRange == HandJointsMotionRange.ConformingToController
                        && dataSourceFeature.m_RightHandPreference == HandTrackingDataSourceFeature.DataSourcePreference.TrackedHand)
                        dataSourceFeature.m_RightHandPreference = HandTrackingDataSourceFeature.DataSourcePreference.Both;

                    EditorUtility.SetDirty(dataSourceFeature);
                },
                fixItAutomatic = true,
                error = true,
            });
#endif
        }
#endif
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING || PACKAGE_DOCS_GENERATION
