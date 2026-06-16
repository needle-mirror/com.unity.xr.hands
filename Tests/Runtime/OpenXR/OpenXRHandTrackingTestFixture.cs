#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    /// <summary>
    /// Base test fixture that saves and restores XRGeneralSettings
    /// InitManagerOnStart so tests can control XR loader initialization.
    /// </summary>
    public abstract class OpenXRHandTrackingTestFixture : IPrebuildSetup, IPostBuildCleanup
    {
#if UNITY_EDITOR
        bool m_PreviousInitManagerOnStart;
#endif

        public void Setup()
        {
#if UNITY_EDITOR
            if (XRGeneralSettings.Instance != null)
            {
                m_PreviousInitManagerOnStart = XRGeneralSettings.Instance.InitManagerOnStart;
                XRGeneralSettings.Instance.InitManagerOnStart = false;
            }
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            if (XRGeneralSettings.Instance != null)
            {
                XRGeneralSettings.Instance.InitManagerOnStart = m_PreviousInitManagerOnStart;
            }
#endif
        }
    }
}
#endif
