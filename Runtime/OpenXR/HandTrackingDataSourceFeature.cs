#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;
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
    /// <a href="https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_EXT_hand_tracking_data_source">
    /// XR_EXT_hand_tracking_data_source</a>, allowing the application to
    /// specify preferred hand tracking data sources (e.g., optical tracking or
    /// controller-derived poses) at hand tracker creation time and to query the
    /// active data source per hand per frame.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Hand Tracking Data Source",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows the application to specify preferred hand tracking data sources and query which source is active per hand per frame.",
        DocumentationLink = XRHelpURLConstants.k_OpenXRFeaturesDocsBaseUrl + "handtrackingdatasource.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId)]
#endif
    internal unsafe class HandTrackingDataSourceFeature
        : OpenXRHandTrackingFeature,
          IXRHandExtendedDataReadHandler<HandTrackingDataSource>,
          IXRHandConfigurationHandler<HandTrackingDataSourceConfig>
    {
        /// <summary>
        /// The feature ID string. This is used to give the feature a well known
        /// ID for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.handtrackingdatasource";

        /// <summary>
        /// The OpenXR Extension string. OpenXR uses this to check if this
        /// extension is available or enabled.
        /// </summary>
        public const string extensionString = "XR_EXT_hand_tracking_data_source";

        [SerializeField]
        [Tooltip("Specifies hand tracking data sources the runtime should use for the left hand.")]
        internal DataSourcePreference m_LeftHandPreference = DataSourcePreference.Both;

        [SerializeField]
        [Tooltip("Specifies hand tracking data sources the runtime should use for the right hand.")]
        internal DataSourcePreference m_RightHandPreference = DataSourcePreference.Both;

        // Per-hand preferred sources, staged by TryUpdateConfiguration and
        // consumed by OnHandTrackingCreateRequest at tracker creation time.
        List<HandTrackingDataSource> m_LeftPreferredSources;
        List<HandTrackingDataSource> m_RightPreferredSources;

        // Cached array snapshots of the preferred lists, rebuilt only when
        // TryUpdateConfiguration or OnInstanceCreate modifies the lists.
        HandTrackingDataSource[] m_LeftPreferredSourcesCache;
        HandTrackingDataSource[] m_RightPreferredSourcesCache;

        // Per-hand NativeArrays rebuilt from the preferred lists at tracker
        // creation time. Must stay alive while the tracker exists because the
        // runtime holds a pointer into them.
        NativeArray<XrHandTrackingDataSourceEXT> m_LeftRequestedSources;
        NativeArray<XrHandTrackingDataSourceEXT> m_RightRequestedSources;

        // Per-hand active source reported by the runtime each frame.
        HandTrackingDataSource m_LeftActiveSource;
        HandTrackingDataSource m_RightActiveSource;

        // Whether the runtime reported a valid active source this frame.
        // When false, the corresponding activeSource value is stale.
        bool m_LeftActive;
        bool m_RightActive;

        // Subsystem reference for handler registration.
        XRHandSubsystem m_Subsystem;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            if (!base.OnInstanceCreate(xrInstance))
                return false;

            // Reset preferred sources from serialized preferences.
            m_LeftPreferredSources = PreferenceToSources(m_LeftHandPreference);
            m_RightPreferredSources = PreferenceToSources(m_RightHandPreference);
            RebuildCaches();

            return true;
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);

            var outputSlot = XrHandTrackingDataSourceStateEXT.defaultValue;
            GetLocateOutputChain(XrHandEXT.Left)?.TryAddNode(outputSlot);
            GetLocateOutputChain(XrHandEXT.Right)?.TryAddNode(outputSlot);
        }

        /// <inheritdoc/>
        protected override void OnHandSubsystemCreated(XRHandSubsystem subsystem)
        {
            base.OnHandSubsystemCreated(subsystem);
            m_Subsystem = subsystem;
            subsystem.RegisterHandExtendedDataHandler(this);
            subsystem.RegisterConfigurationHandler(this);
        }

        /// <inheritdoc/>
        protected override void OnHandSubsystemDestroyed(XRHandSubsystem subsystem)
        {
            if (m_Subsystem != null && m_Subsystem == subsystem)
            {
                m_Subsystem.UnregisterHandExtendedDataHandler<HandTrackingDataSource>();
                m_Subsystem.UnregisterConfigurationHandler<HandTrackingDataSourceConfig>();
                m_Subsystem = null;
            }

            base.OnHandSubsystemDestroyed(subsystem);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            // Safety net: dispose native arrays in case OnHandTrackerDestroyed
            // was not called (e.g., abrupt Play Mode exit).
            if (m_LeftRequestedSources.IsCreated)
                m_LeftRequestedSources.Dispose();
            if (m_RightRequestedSources.IsCreated)
                m_RightRequestedSources.Dispose();

            base.OnInstanceDestroy(xrInstance);
        }

        /// <inheritdoc/>
        protected override void OnHandTrackingCreateRequest(
            XrHandEXT hand,
            XrStructureChain extensionChain)
        {
            EnsurePreferredSources();

            ref var nativeSources = ref (hand == XrHandEXT.Left
                ? ref m_LeftRequestedSources
                : ref m_RightRequestedSources);
            var preferredSources = hand == XrHandEXT.Left
                ? m_LeftPreferredSources
                : m_RightPreferredSources;

            if (nativeSources.IsCreated)
                nativeSources.Dispose();

            nativeSources = new NativeArray<XrHandTrackingDataSourceEXT>(
                preferredSources.Count, Allocator.Persistent);
            for (int i = 0; i < preferredSources.Count; i++)
                nativeSources[i] = (XrHandTrackingDataSourceEXT)preferredSources[i];

            if (!extensionChain.TryAddNode(new XrHandTrackingDataSourceInfoEXT(nativeSources)))
                Debug.LogWarning("HandTrackingDataSourceFeature: Failed to add data source info to the extension chain.");
        }

        /// <inheritdoc/>
        protected override void OnHandTrackerDestroyed(XrHandEXT hand, XrResult destroyResult)
        {
            ref var nativeSources = ref (hand == XrHandEXT.Left
                ? ref m_LeftRequestedSources
                : ref m_RightRequestedSources);
            if (nativeSources.IsCreated)
                nativeSources.Dispose();

            if (hand == XrHandEXT.Left)
                m_LeftActive = false;
            else
                m_RightActive = false;
        }

        /// <inheritdoc/>
        protected override void OnLocateHandJointsResult(
            XrHandEXT hand,
            XrStructureChain outputChain,
            XrResult locateHandJointsResult,
            bool isActive)
        {
            ref var activeSource = ref (hand == XrHandEXT.Left
                ? ref m_LeftActiveSource
                : ref m_RightActiveSource);
            ref var active = ref (hand == XrHandEXT.Left
                ? ref m_LeftActive
                : ref m_RightActive);

            if (locateHandJointsResult != XrResult.Success || !isActive)
            {
                active = false;
                return;
            }

            if (outputChain.TryGetNode<XrHandTrackingDataSourceStateEXT>(
                XrStructureType.HandTrackingDataSourceStateEXT,
                out var state))
            {
                active = true;
                activeSource = (HandTrackingDataSource)state.dataSource;
            }
        }

        /// <inheritdoc/>
        public bool TryGetData(Handedness handedness, out HandTrackingDataSource data)
        {
            bool active = handedness == Handedness.Left
                ? m_LeftActive
                : m_RightActive;

            if (!active)
            {
                data = default;
                return false;
            }

            data = handedness == Handedness.Left
                ? m_LeftActiveSource
                : m_RightActiveSource;

            return true;
        }

        /// <inheritdoc/>
        public bool TryGetConfiguration(out HandTrackingDataSourceConfig config)
        {
            EnsurePreferredSources();

            config = new HandTrackingDataSourceConfig
            {
                leftPreferredSources = m_LeftPreferredSourcesCache,
                rightPreferredSources = m_RightPreferredSourcesCache,
            };
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Calling this method triggers a hand tracker restart on the next Dynamic update.
        /// See <see cref="RequestHandTrackerRestart"/> for restart semantics and timing.
        /// </remarks>
        public bool TryUpdateConfiguration(HandTrackingDataSourceConfig config)
        {
            EnsurePreferredSources();

            if (config.leftPreferredSources != null)
            {
                m_LeftPreferredSources.Clear();
                m_LeftPreferredSources.AddRange(config.leftPreferredSources);
            }

            if (config.rightPreferredSources != null)
            {
                m_RightPreferredSources.Clear();
                m_RightPreferredSources.AddRange(config.rightPreferredSources);
            }

            RebuildCaches();
            RequestHandTrackerRestart();
            return true;
        }

        void EnsurePreferredSources()
        {
            if (m_LeftPreferredSources != null && m_RightPreferredSources != null)
                return;

            m_LeftPreferredSources ??= PreferenceToSources(m_LeftHandPreference);
            m_RightPreferredSources ??= PreferenceToSources(m_RightHandPreference);
            RebuildCaches();
        }

        void RebuildCaches()
        {
            m_LeftPreferredSourcesCache = m_LeftPreferredSources?.ToArray();
            m_RightPreferredSourcesCache = m_RightPreferredSources?.ToArray();
        }

        static List<HandTrackingDataSource> PreferenceToSources(DataSourcePreference preference)
        {
            return preference switch
            {
                DataSourcePreference.TrackedHand => new List<HandTrackingDataSource> { HandTrackingDataSource.Unobstructed },
                DataSourcePreference.ControllerDriven => new List<HandTrackingDataSource> { HandTrackingDataSource.Controller },
                DataSourcePreference.Both => new List<HandTrackingDataSource> { HandTrackingDataSource.Unobstructed, HandTrackingDataSource.Controller },
                _ => new List<HandTrackingDataSource> { HandTrackingDataSource.Unobstructed, HandTrackingDataSource.Controller },
            };
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(this)
            {
                message = "Hand Tracking Data Source requires the Hand Tracking Subsystem feature to be enabled.",
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

        /// <summary>
        /// Simplified preference for per-hand data source configuration.
        /// Maps to arrays of <see cref="HandTrackingDataSource"/> values
        /// when handed to the extensibility framework.
        /// </summary>
        internal enum DataSourcePreference
        {
            /// <summary>
            /// Optical (camera-based) hand tracking.
            /// </summary>
            TrackedHand,

            /// <summary>
            /// Controller-driven hand poses.
            /// </summary>
            ControllerDriven,

            /// <summary>
            /// Accept both optical and controller-derived sources. The runtime
            /// chooses the active source based on current conditions.
            /// </summary>
            Both,
        }
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
