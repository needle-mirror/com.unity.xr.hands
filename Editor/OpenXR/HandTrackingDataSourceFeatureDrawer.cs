#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;

namespace UnityEditor.XR.Hands.OpenXR
{
    [CustomPropertyDrawer(typeof(HandTrackingDataSourceFeature.DataSourcePreference))]
    class HandTrackingDataSourceFeatureDrawer : PropertyDrawer
    {
        // Source of truth for the option labels and tooltips. Consumed both by
        // this drawer (Project Settings inspector) and by HandTrackingProjectSettingsGUI's
        // read-only mirror view, so any change here propagates to both surfaces automatically.
        internal static readonly GUIContent[] s_Options =
        {
            new GUIContent("Tracked Hand",
                "Hand poses from optical (camera-based) hand tracking " +
                "(XR_HAND_TRACKING_DATA_SOURCE_UNOBSTRUCTED_EXT)."),
            new GUIContent("Controller",
                "Hand poses derived from a held controller " +
                "(XR_HAND_TRACKING_DATA_SOURCE_CONTROLLER_EXT)."),
            new GUIContent("Both (Tracked Hand and Controller)",
                "Runtime selects between optical tracking and controller-derived " +
                "poses based on current conditions."),
        };

        internal static readonly int[] s_Values =
        {
            (int)HandTrackingDataSourceFeature.DataSourcePreference.TrackedHand,
            (int)HandTrackingDataSourceFeature.DataSourcePreference.ControllerDriven,
            (int)HandTrackingDataSourceFeature.DataSourcePreference.Both,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.IntPopup(position, property, s_Options, s_Values, label);
        }
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
