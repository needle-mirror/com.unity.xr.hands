#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.Hands.OpenXR
{
    /// <summary>
    /// Draws a read-only mirror of the hand tracking extension project settings.
    /// Intended to be embedded in any inspector that needs to surface the
    /// effective project-level configuration without allowing edits.
    /// </summary>
    static class HandTrackingProjectSettingsGUI
    {
        const string k_OpenXRSettingsPath = "Project/XR Plug-in Management/OpenXR";

        const string k_ParentFoldKey = "XRHands.ProjectSettingsGUI.ParentExpanded";
        const string k_DataSourceFoldKey = "XRHands.ProjectSettingsGUI.DataSourceExpanded";
        const string k_MotionRangeFoldKey = "XRHands.ProjectSettingsGUI.MotionRangeExpanded";
#if OPENXR_1_19_OR_NEWER
        const string k_FrequencyHintFoldKey = "XRHands.ProjectSettingsGUI.FrequencyHintExpanded";
#endif

        static readonly GUIContent k_Header =
            EditorGUIUtility.TrTextContent("OpenXR Hand Tracking Project Settings");
        static readonly GUIContent k_HeaderDescription =
            EditorGUIUtility.TrTextContent(
                "The settings below are configured in XR Plug-in Management and apply to all scenes in the project.");
        static readonly GUIContent k_ChangeInProjectSettings =
            EditorGUIUtility.TrTextContent("Change in Project Settings");

        static readonly GUIContent k_DataSourceSection =
            EditorGUIUtility.TrTextContent("Hand Tracking Data Source");
        static readonly GUIContent k_DataSourceDescription =
            EditorGUIUtility.TrTextContent(
                "Specifies the runtime hand tracking data source: tracked hand, controller, or both.");

        static readonly GUIContent k_MotionRangeSection =
            EditorGUIUtility.TrTextContent("Hand Joints Motion Range");
        static readonly GUIContent k_MotionRangeDescription =
            EditorGUIUtility.TrTextContent(
                "Controls hand joint motion range: full natural range or constrained to mimic gripping a held controller.");

#if OPENXR_1_19_OR_NEWER
        static readonly GUIContent k_FrequencyHintSection =
            EditorGUIUtility.TrTextContent("Requested Hand Tracking Update Frequency");
        static readonly GUIContent k_FrequencyHintDescription =
            EditorGUIUtility.TrTextContent(
                "Controls the hand tracking update frequency. Setting below is passed to the " +
                "runtime as a suggestion. The runtime may choose to ignore it.");
        const string k_FrequencyHintHelp =
            "Higher frequency improves responsiveness but may increase power consumption.";
        static readonly GUIContent k_Frequency =
            EditorGUIUtility.TrTextContent("Frequency");
#endif

        static readonly GUIContent k_LeftHand =
            EditorGUIUtility.TrTextContent("Left Hand");
        static readonly GUIContent k_RightHand =
            EditorGUIUtility.TrTextContent("Right Hand");

        /// <summary>
        /// Draws the full read-only project settings view: header foldout,
        /// navigation button, and all extension sections as disabled controls.
        /// </summary>
        internal static void Draw()
        {
            bool parentExpanded = SessionState.GetBool(k_ParentFoldKey, true);
            parentExpanded = EditorGUILayout.Foldout(parentExpanded, k_Header, true, EditorStyles.foldoutHeader);
            SessionState.SetBool(k_ParentFoldKey, parentExpanded);

            if (!parentExpanded)
                return;

            EditorGUILayout.LabelField(k_HeaderDescription, EditorStyles.wordWrappedMiniLabel);

            if (GUILayout.Button(k_ChangeInProjectSettings))
                SettingsService.OpenProjectSettings(k_OpenXRSettingsPath);

            EditorGUILayout.Space();

            var settings = OpenXRSettings.ActiveBuildTargetInstance;

            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    DrawDataSourceSection(settings);
                    EditorGUILayout.Space();
                    DrawMotionRangeSection(settings);
#if OPENXR_1_19_OR_NEWER
                    EditorGUILayout.Space();
                    DrawFrequencyHintSection(settings);
#endif
                }
            }
        }

        static void DrawDataSourceSection(OpenXRSettings settings)
        {
            var feature = settings != null ? settings.GetFeature<HandTrackingDataSourceFeature>() : null;
            bool featureEnabled = feature != null && feature.enabled;

            bool stored = SessionState.GetBool(k_DataSourceFoldKey, true);
            bool expanded = EditorGUILayout.Foldout(featureEnabled && stored, k_DataSourceSection, true);

            if (featureEnabled)
                SessionState.SetBool(k_DataSourceFoldKey, expanded);

            if (!expanded || !featureEnabled)
                return;

            EditorGUILayout.LabelField(k_DataSourceDescription, EditorStyles.wordWrappedMiniLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                int leftValue = (int)feature.m_LeftHandPreference;
                int rightValue = (int)feature.m_RightHandPreference;

                EditorGUILayout.IntPopup(
                    k_LeftHand, leftValue,
                    HandTrackingDataSourceFeatureDrawer.s_Options,
                    HandTrackingDataSourceFeatureDrawer.s_Values);

                EditorGUILayout.IntPopup(
                    k_RightHand, rightValue,
                    HandTrackingDataSourceFeatureDrawer.s_Options,
                    HandTrackingDataSourceFeatureDrawer.s_Values);
            }
        }

        static void DrawMotionRangeSection(OpenXRSettings settings)
        {
            var feature = settings != null ? settings.GetFeature<HandJointsMotionRangeFeature>() : null;
            bool featureEnabled = feature != null && feature.enabled;

            bool stored = SessionState.GetBool(k_MotionRangeFoldKey, true);
            bool expanded = EditorGUILayout.Foldout(featureEnabled && stored, k_MotionRangeSection, true);

            if (featureEnabled)
                SessionState.SetBool(k_MotionRangeFoldKey, expanded);

            if (!expanded || !featureEnabled)
                return;

            EditorGUILayout.LabelField(k_MotionRangeDescription, EditorStyles.wordWrappedMiniLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                int leftValue = (int)feature.m_LeftMotionRange;
                int rightValue = (int)feature.m_RightMotionRange;

                EditorGUILayout.IntPopup(
                    k_LeftHand, leftValue,
                    HandJointsMotionRangeFeatureDrawer.s_Options,
                    HandJointsMotionRangeFeatureDrawer.s_Values);

                EditorGUILayout.IntPopup(
                    k_RightHand, rightValue,
                    HandJointsMotionRangeFeatureDrawer.s_Options,
                    HandJointsMotionRangeFeatureDrawer.s_Values);
            }
        }

#if OPENXR_1_19_OR_NEWER
        static void DrawFrequencyHintSection(OpenXRSettings settings)
        {
            var feature = settings != null ? settings.GetFeature<MetaHandTrackingFrequencyHintFeature>() : null;
            bool featureEnabled = feature != null && feature.enabled;

            bool stored = SessionState.GetBool(k_FrequencyHintFoldKey, true);
            bool expanded = EditorGUILayout.Foldout(featureEnabled && stored, k_FrequencyHintSection, true);

            if (featureEnabled)
                SessionState.SetBool(k_FrequencyHintFoldKey, expanded);

            if (!expanded || !featureEnabled)
                return;

            EditorGUILayout.LabelField(k_FrequencyHintDescription, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.HelpBox(k_FrequencyHintHelp, MessageType.Info);

            using (new EditorGUI.IndentLevelScope())
            {
                int frequencyValue = (int)feature.m_FrequencyHint;

                EditorGUILayout.IntPopup(
                    k_Frequency, frequencyValue,
                    MetaHandTrackingFrequencyHintFeatureDrawer.s_Options,
                    MetaHandTrackingFrequencyHintFeatureDrawer.s_Values);
            }
        }
#endif
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
