#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.OpenXR;

[TestFixture]
class OpenXRHandSubsystemManagerTests
{
    XRHandSubsystem m_MockSubsystem;
    HandTracking m_HandTrackingInstance;
    OpenXRHandSubsystemManager m_Manager;
    GameObject m_TestGameObject;

    // To store the previous state of the static fields of HandTracking.
    XRHandSubsystem m_PreviousSubsystem;
    HandTracking m_PreviousHandTracking;

    // At the moment, we use FieldInfo to inject test instances into the private static fields of HandTracking,
    // since the real OpenXR loader lifecycle (which creates HandTracking) can't be triggered or mocked cleanly in tests.
    // This is a temporary workaround until a proper mock runtime setup is available.
    static readonly FieldInfo k_SubsystemField = typeof(HandTracking).GetField("s_Subsystem", BindingFlags.Static | BindingFlags.NonPublic);
    static readonly FieldInfo k_HandTrackingField = typeof(HandTracking).GetField("s_This", BindingFlags.Static | BindingFlags.NonPublic);

    [SetUp]
    public void SetUp()
    {
        Assert.IsNotNull(k_SubsystemField, "Failed to find HandTracking.s_Subsystem field.");
        Assert.IsNotNull(k_HandTrackingField, "Failed to find HandTracking.s_This field.");

        m_PreviousSubsystem = (XRHandSubsystem)k_SubsystemField.GetValue(null);
        m_PreviousHandTracking = (HandTracking)k_HandTrackingField.GetValue(null);

        // Create a mock subsystem
        m_MockSubsystem = TestHandUtils.CreateTestSubsystem();
        Assert.IsNotNull(m_MockSubsystem, "Failed to create mock hand subsystem.");

        // Create HandTracking instance
        m_HandTrackingInstance = ScriptableObject.CreateInstance<HandTracking>();
        k_HandTrackingField.SetValue(null, m_HandTrackingInstance);
        Assert.IsNotNull(m_HandTrackingInstance, "Failed to create hand tracking instance.");
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            // Stop the subsystem before destroying if it's running
            if (m_MockSubsystem != null && m_MockSubsystem.running)
                m_MockSubsystem.Stop();
        }
        finally
        {
            // Destroy HandTracking instance
            if (m_HandTrackingInstance != null)
            {
                Object.DestroyImmediate(m_HandTrackingInstance);
                m_HandTrackingInstance = null;
            }

            // Destroy GameObject
            if (m_TestGameObject != null)
            {
                Object.DestroyImmediate(m_TestGameObject);
                m_TestGameObject = null;
            }

            m_Manager = null;

            // Restore previous static state
            k_SubsystemField?.SetValue(null, m_PreviousSubsystem);
            k_HandTrackingField?.SetValue(null, m_PreviousHandTracking);
            m_PreviousSubsystem = null;
            m_PreviousHandTracking = null;

            // Destroy the mock subsystem
            m_MockSubsystem?.Destroy();
            m_MockSubsystem = null;
        }
    }

    /// <summary>
    /// Injects the mock subsystem into HandTracking and starts it.
    /// </summary>
    void InjectSubsystemAndStart()
    {
        InjectSubsystem();

        HandTracking.subsystem.Start();
        Assert.IsTrue(HandTracking.subsystem.running, "Failed to start mock subsystem.");
    }

    /// <summary>
    /// Injects the mock subsystem into HandTracking without starting it.
    /// </summary>
    void InjectSubsystem()
    {
        k_SubsystemField?.SetValue(null, m_MockSubsystem);
        Assert.IsNotNull(HandTracking.subsystem, "HandTracking.subsystem was not injected.");
    }

    /// <summary>
    /// Creates a manager component on a new GameObject and activates it, triggering Awake() and OnEnable().
    /// </summary>
    void CreateActiveManager()
    {
        m_TestGameObject = new GameObject("TestManager");
        m_TestGameObject.SetActive(false);
        m_Manager = m_TestGameObject.AddComponent<OpenXRHandSubsystemManager>();
        m_TestGameObject.SetActive(true);
    }

    [UnityTest]
    public IEnumerator Awake_SubsystemExists_StaysEnabled()
    {
        InjectSubsystemAndStart();
        CreateActiveManager();

        yield return null;

        Assert.IsTrue(m_Manager.enabled, "Manager should remain enabled when subsystem exists");
        Assert.IsTrue(m_MockSubsystem.running, "Subsystem should remain running");
    }

    [UnityTest]
    public IEnumerator Awake_SubsystemNull_DisablesSelf()
    {
        CreateActiveManager();

        yield return null;

        Assert.IsFalse(m_Manager.enabled, "Manager should disable itself when subsystem is null");
    }

    [UnityTest]
    public IEnumerator OnEnable_StartsStoppedSubsystem()
    {
        InjectSubsystemAndStart();
        m_MockSubsystem.Stop();
        Assert.IsFalse(m_MockSubsystem.running, "Subsystem should be stopped before test");

        CreateActiveManager();

        yield return null;

        Assert.IsTrue(m_Manager.enabled, "Manager should be enabled when subsystem exists");
        Assert.IsTrue(m_MockSubsystem.running, "Subsystem should be started by OnEnable");
    }

    [UnityTest]
    public IEnumerator ReEnable_AfterSubsystemAvailable_StartsSubsystem()
    {
        CreateActiveManager();
        yield return null;
        Assert.IsFalse(m_Manager.enabled, "Manager should be disabled when subsystem is null");

        InjectSubsystem();

        m_Manager.enabled = true;
        yield return null;

        Assert.IsTrue(m_Manager.enabled, "Manager should be enabled after re-enabling with subsystem available");
        Assert.IsTrue(m_MockSubsystem.running, "Subsystem should be running after manager re-enabled");
    }

    [UnityTest]
    public IEnumerator Toggle_EnableDisable_StartsAndStopsSubsystem()
    {
        InjectSubsystemAndStart();
        CreateActiveManager();
        yield return null;
        Assert.IsTrue(m_MockSubsystem.running, "Subsystem should be running initially");

        m_Manager.enabled = false;
        yield return null;
        Assert.IsFalse(m_MockSubsystem.running, "Subsystem should stop when manager disabled");

        m_Manager.enabled = true;
        yield return null;
        Assert.IsTrue(m_MockSubsystem.running, "Subsystem should restart when manager re-enabled");
    }
}

#endif
