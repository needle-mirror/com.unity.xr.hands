using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.XR.Hands.Capture;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;

namespace UnityEditor.XR.Hands.Tests
{
    /// <summary>
    /// Regression tests for the XR Hand Capture window playback hands (UUM-151474).
    /// </summary>
    class XRHandCapturePlaybackTests
    {
        // Left and right hand roots plus their joints parents.
        const int k_PlaybackObjectsPerRecording = 4;

        readonly List<Object> m_CreatedObjects = new List<Object>();

        XRHandCapturePlayback m_Playback;
        XRHandCaptureEditor m_Editor;
        GameObject m_HandTemplate;

        // Playback is a singleton shared with an open Capture window, so restore what the test changes.
        GameObject m_OriginalLeftHandPrefab;
        GameObject m_OriginalRightHandPrefab;
        GameObject m_OriginalJointPrefab;
        Material m_OriginalHandMeshMaterial;
        XRHandCaptureSequence m_OriginalRecording;
        int m_OriginalFrameID;

        [SetUp]
        public void SetUp()
        {
            // An open Capture window shares the playback and would lose its hand-shape inspector state.
            if (EditorWindow.HasOpenInstances<XRHandCaptureEditorWindow>())
                Assert.Ignore("Close the XR Hand Capture window to run these tests.");

            m_Playback = XRHandCapturePlayback.GetInstance();
            m_OriginalLeftHandPrefab = m_Playback.leftHandPrefab;
            m_OriginalRightHandPrefab = m_Playback.rightHandPrefab;
            m_OriginalJointPrefab = m_Playback.jointPrefab;
            m_OriginalHandMeshMaterial = m_Playback.handMeshMaterial;
            m_OriginalRecording = m_Playback.selectedRecording;
            m_OriginalFrameID = m_Playback.selectedFrameID;

            m_Playback.SelectRecording(null);
            m_Playback.DestroyHandGameObjects();

            m_HandTemplate = CreateHandTemplate("UserHand");
            m_Playback.leftHandPrefab = m_HandTemplate;
            m_Playback.rightHandPrefab = m_HandTemplate;
            m_Playback.jointPrefab = null;
            m_Playback.handMeshMaterial = null;
        }

        [TearDown]
        public void TearDown()
        {
            // Unity still runs TearDown after Assert.Ignore in SetUp.
            if (m_Playback == null)
                return;

            if (m_Editor != null)
                Object.DestroyImmediate(m_Editor);
            m_Editor = null;

            m_Playback.DestroyHandGameObjects();
            // Restore prefabs before re-selecting so a live Capture window re-renders with its own prefabs.
            m_Playback.leftHandPrefab = m_OriginalLeftHandPrefab;
            m_Playback.rightHandPrefab = m_OriginalRightHandPrefab;
            m_Playback.jointPrefab = m_OriginalJointPrefab;
            m_Playback.handMeshMaterial = m_OriginalHandMeshMaterial;
            m_Playback.SelectRecording(m_OriginalRecording);
            m_Playback.selectedFrameID = m_OriginalFrameID;

            foreach (var createdObject in m_CreatedObjects)
            {
                if (createdObject != null)
                    Object.DestroyImmediate(createdObject);
            }
            m_CreatedObjects.Clear();
            m_Playback = null;
        }

        [Test]
        public void ClearingRecording_DestroysPlaybackHands()
        {
            var baseline = CountPlaybackObjects();
            var recording = CreateRecording(3);
            CreateCaptureEditor();

            m_Playback.SelectRecording(recording);
            Assert.AreEqual(baseline + k_PlaybackObjectsPerRecording, CountPlaybackObjects(), "Selecting a recording should spawn a left and right playback hand and their joints parents.");

            m_Playback.SelectRecording(null);

            Assert.IsNull(m_Playback.selectedRecording);
            Assert.AreEqual(baseline, CountPlaybackObjects(), "Clearing the recording should destroy the playback hands.");
        }

        [Test]
        public void SceneObjectAsHandPrefab_IsCopiedNotConsumed()
        {
            var handGameObjects = new HandGameObjects(Handedness.Left, null, m_HandTemplate, null, null);
            try
            {
                Assert.AreEqual("UserHand", m_HandTemplate.name, "The user's object should keep its name.");
                Assert.AreEqual(HideFlags.None, m_HandTemplate.hideFlags, "The user's object should not be hidden or excluded from the saved scene.");
                Assert.AreEqual(1, FindPlaybackObjects().Count(go => go.name == $"Left {HandGameObjects.k_HandRootNameSuffix}"), "Playback should use its own copy of the prefab.");
                Assert.AreEqual(1, FindPlaybackObjects().Count(go => go.name == $"Left {HandGameObjects.k_DrawJointsParentNameSuffix}"), "Playback should create its own joints parent.");
            }
            finally
            {
                handGameObjects.OnDestroy();
            }

            // Unity null check on purpose: Assert.IsNotNull uses .NET null and would miss a destroyed object.
            Assert.IsTrue(m_HandTemplate != null, "Destroying the playback hand should not destroy the user's object.");
            Assert.AreEqual(0, FindPlaybackObjects().Count(go => go.name.StartsWith("Left ")));
        }

        [Test]
        public void SelectingAnotherRecording_ResetsFrameToZeroAndSyncsSlider()
        {
            var recordingA = CreateRecording(3);
            var recordingB = CreateRecording(3);
            var root = CreateCaptureEditor();
            var slider = root.Q<SliderInt>();
            var frameField = root.Q<IntegerField>();

            m_Playback.SelectRecording(recordingA);
            m_Playback.selectedFrameID = 2;
            Assert.AreEqual(2, slider.value);
            Assert.AreEqual(2, frameField.value);

            m_Playback.SelectRecording(recordingB);

            Assert.AreEqual(0, m_Playback.selectedFrameID);
            Assert.AreEqual(0, slider.value, "The frame slider should follow the reset to frame 0.");
            Assert.AreEqual(0, frameField.value, "Only OnFrameChanged writes the Frame # field; a stale value means the first frame of the new recording was not rendered.");
        }

        [Test]
        public void ReselectingAfterClear_RespawnsPlaybackHands()
        {
            var baseline = CountPlaybackObjects();
            var recording = CreateRecording(3);
            CreateCaptureEditor();

            m_Playback.SelectRecording(recording);
            m_Playback.SelectRecording(null);
            Assert.AreEqual(baseline, CountPlaybackObjects(), "Precondition: clearing removed the playback objects.");

            m_Playback.SelectRecording(recording);

            Assert.AreEqual(baseline + k_PlaybackObjectsPerRecording, CountPlaybackObjects(),
                "Selecting again should spawn fresh hands and joints parents; a stale HandGameObjects reference would skip creation.");
        }

        [Test]
        public void SelectingAnotherRecording_KeepsOnePairOfPlaybackHands()
        {
            var baseline = CountPlaybackObjects();
            var recordingA = CreateRecording(3);
            var recordingB = CreateRecording(3);
            CreateCaptureEditor();

            m_Playback.SelectRecording(recordingA);
            Assert.AreEqual(baseline + k_PlaybackObjectsPerRecording, CountPlaybackObjects(), "Precondition: selecting a recording spawned the playback hands.");

            m_Playback.SelectRecording(recordingB);

            Assert.AreEqual(baseline + k_PlaybackObjectsPerRecording, CountPlaybackObjects(),
                "Switching between valid recordings should keep exactly one left and right playback hand and their joints parents.");
        }

        VisualElement CreateCaptureEditor()
        {
            m_Editor = ScriptableObject.CreateInstance<XRHandCaptureEditor>();
            return m_Editor.CreateInspectorGUI();
        }

        // The child name must end in Wrist so XRHandSkeletonDriverUtility.FindJointsFromRoot has a root, and the
        // SkinnedMeshRenderer is required so XRHandMeshController.handMeshRenderer is non-null when ToggleDrawMesh runs.
        GameObject CreateHandTemplate(string name)
        {
            var hand = new GameObject(name);
            var wrist = new GameObject($"L_{XRHandJointID.Wrist}");
            wrist.transform.SetParent(hand.transform, false);
            wrist.AddComponent<SkinnedMeshRenderer>();
            m_CreatedObjects.Add(hand);
            return hand;
        }

        XRHandCaptureSequence CreateRecording(int frameCount)
        {
            var sequence = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
            m_CreatedObjects.Add(sequence);

            var frames = new List<XRHandCaptureFrame>();
            for (var i = 0; i < frameCount; i++)
                frames.Add(new XRHandCaptureFrame(sequence, default, i) { timestamp = i });

            const BindingFlags k_Flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var framesField = typeof(XRHandCaptureSequence).GetField("m_Frames", k_Flags);
            Assert.IsNotNull(framesField, "m_Frames field not found via reflection - internal field name may have changed");
            framesField.SetValue(sequence, frames);

            return sequence;
        }

        static IEnumerable<GameObject> FindPlaybackObjects()
        {
            // Playback objects are HideInHierarchy, so search all loaded objects rather than the scene.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => !EditorUtility.IsPersistent(go)
                    && (go.name.EndsWith(HandGameObjects.k_HandRootNameSuffix) || go.name.EndsWith(HandGameObjects.k_DrawJointsParentNameSuffix)));
        }

        static int CountPlaybackObjects() => FindPlaybackObjects().Count();
    }
}
