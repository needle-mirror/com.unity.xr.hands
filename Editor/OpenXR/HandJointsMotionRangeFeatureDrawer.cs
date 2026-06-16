#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;

namespace UnityEditor.XR.Hands.OpenXR
{
    [CustomPropertyDrawer(typeof(HandJointsMotionRange))]
    class HandJointsMotionRangeFeatureDrawer : PropertyDrawer
    {
        // Source of truth for the option labels and tooltips. Consumed both by
        // this drawer (Project Settings inspector) and by HandTrackingProjectSettingsGUI's
        // read-only mirror view, so any change here propagates to both surfaces automatically.
        internal static readonly GUIContent[] s_Options =
        {
            new GUIContent("Natural Movement",
                "Joint poses reflect the full natural range of hand motion " +
                "(XR_HAND_JOINTS_MOTION_RANGE_UNOBSTRUCTED_EXT)."),
            new GUIContent("Controller-locked",
                "Joint poses are constrained as if gripping a held controller " +
                "(XR_HAND_JOINTS_MOTION_RANGE_CONFORMING_TO_CONTROLLER_EXT).\n" +
                "Requires the Hand Tracking Data Source feature to include 'Controller' as a preferred source."),
        };

        internal static readonly int[] s_Values =
        {
            (int)HandJointsMotionRange.Unobstructed,
            (int)HandJointsMotionRange.ConformingToController,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.IntPopup(position, property, s_Options, s_Values, label);
        }
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
