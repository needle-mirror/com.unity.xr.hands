#if OPENXR_1_19_OR_NEWER

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;

namespace UnityEditor.XR.Hands.OpenXR
{
    [CustomPropertyDrawer(typeof(MetaHandTrackingFrequencyHint))]
    class MetaHandTrackingFrequencyHintFeatureDrawer : PropertyDrawer
    {
        internal static readonly GUIContent[] s_Options =
        {
            EditorGUIUtility.TrTextContent("Default",
                "Suggests the runtime use its default hand tracking frequency. This is " +
                "typically the most power-efficient option that provides adequate tracking " +
                "quality for general use cases."),
            EditorGUIUtility.TrTextContent("High",
                "Suggests the runtime use a higher hand tracking frequency for more " +
                "responsive tracking. May increase power consumption and reduce the " +
                "effectiveness of temporal smoothing, which can result in increased jitter."),
        };

        internal static readonly int[] s_Values =
        {
            (int)MetaHandTrackingFrequencyHint.Default,
            (int)MetaHandTrackingFrequencyHint.High,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.IntPopup(position, property, s_Options, s_Values, label);
        }
    }
}

#endif // OPENXR_1_19_OR_NEWER
