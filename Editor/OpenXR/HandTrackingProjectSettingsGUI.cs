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
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
