#if UNITY_OPENXR_PACKAGE

using UnityEngine.XR.Hands.OpenXR;

namespace UnityEditor.XR.Hands.OpenXR
{
    [CustomEditor(typeof(OpenXRHandSubsystemManager))]
    class OpenXRHandSubsystemManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
            EditorGUILayout.Space();
            HandTrackingProjectSettingsGUI.Draw();
#endif
        }
    }
}

#endif // UNITY_OPENXR_PACKAGE
