using System;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace UnityEditor.XR.Hands
{
    /// <summary>
    /// Custom editor for an <see cref="XRHandTrackingEvents"/>.
    /// </summary>
    [CustomEditor(typeof(XRHandTrackingEvents), true), CanEditMultipleObjects]
    public class XRHandTrackingEventsEditor : BaseXRHandsEditor
    {
        const string k_HandTrackingEventsExpandedKey = "XRHands." + nameof(XRHandTrackingEventsEditor) + ".EventsExpanded";

        SerializedProperty m_Handedness;
        SerializedProperty m_UpdateType;
        SerializedProperty m_PoseUpdated;
        SerializedProperty m_JointsUpdated;
        SerializedProperty m_TrackingAcquired;
        SerializedProperty m_TrackingLost;
        SerializedProperty m_TrackingChanged;

        bool m_EventsExpanded;
        bool m_UpdateTypeExpanded;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        static class Contents
        {
            public static readonly GUIContent events = EditorGUIUtility.TrTextContent("Hand Tracking Events", "Add events listeners to react to hand tracking events.");
            public static readonly GUIContent updateTypeFoldout = EditorGUIUtility.TrTextContent("Advanced", "More settings related to hand tracking events.");
            public static readonly GUIContent updateTypeInfo = EditorGUIUtility.TrTextContent("Dynamic update is similar timing to MonoBehaviour Update. \n\n" +
                "BeforeRender provides the lowest latency between hand motion and rendering, but it occurs too late " +
                "to affect physics and complex work in callbacks can negatively impact framerate.");
        }

        void OnEnable()
        {
            m_Handedness = serializedObject.FindProperty("m_Handedness");
            m_UpdateType = serializedObject.FindProperty("m_UpdateType");
            m_PoseUpdated = serializedObject.FindProperty("m_PoseUpdated");
            m_JointsUpdated = serializedObject.FindProperty("m_JointsUpdated");
            m_TrackingChanged = serializedObject.FindProperty("m_TrackingChanged");
            m_TrackingAcquired = serializedObject.FindProperty("m_TrackingAcquired");
            m_TrackingLost = serializedObject.FindProperty("m_TrackingLost");
            m_EventsExpanded = SessionState.GetBool(k_HandTrackingEventsExpandedKey, false);
        }

        void OnDisable()
        {
            SessionState.SetBool(k_HandTrackingEventsExpandedKey, m_EventsExpanded);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawInspector();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawInspector()
        {
            DrawScript();

            EditorGUILayout.PropertyField(m_Handedness);
            DrawEventFieldsFoldout();
            DrawUpdateTypeFoldout();

            EditorGUILayout.Space();
        }

        void DrawUpdateTypeFoldout()
        {
            m_UpdateTypeExpanded = EditorGUILayout.Foldout(m_UpdateTypeExpanded, Contents.updateTypeFoldout, true);
            if (!m_UpdateTypeExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_UpdateType);
                EditorGUILayout.HelpBox(Contents.updateTypeInfo.text, MessageType.None);
            }
        }

        void DrawEventFieldsFoldout()
        {
            // Draw foldout
            m_EventsExpanded = EditorGUILayout.Foldout(m_EventsExpanded, Contents.events, true);
            if (!m_EventsExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_PoseUpdated);
                EditorGUILayout.PropertyField(m_JointsUpdated);
                EditorGUILayout.PropertyField(m_TrackingChanged);
                EditorGUILayout.PropertyField(m_TrackingAcquired);
                EditorGUILayout.PropertyField(m_TrackingLost);
            }
        }
    }
}
