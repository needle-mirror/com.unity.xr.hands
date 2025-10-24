#if UNITY_EDITOR && (ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER)
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Analytics;

namespace Unity.XR.Hands.Runtime.Tests
{
    public class XRHandsAnalyticsTests
    {
        [Test]
        public void TestXRHandsAnalyticsFlagsFalse()
        {
            Assert.That(XRHandFeatureUsageData.xrHandSubsystemRuntimeUsed, Is.False);
            Assert.That(XRHandFeatureUsageData.xrHandCustomGestureUsed, Is.False);
            Assert.That(XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed, Is.False);
        }

        [UnityTest]
        public IEnumerator TestXRHandsAnalyticsSubsystemFlagTrue()
        {
            var subsystem = TestHandUtils.CreateTestSubsystem();
            subsystem.Start();
            yield return new WaitForSeconds(0.1f);
            subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
            Assert.That(XRHandFeatureUsageData.xrHandSubsystemRuntimeUsed, Is.True);
            Assert.That(XRHandFeatureUsageData.xrHandCustomGestureUsed, Is.False);
            Assert.That(XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed, Is.False);
        }

        [SetUp]
        public void Setup()
        {
            XRHandFeatureUsageData.ResetData();
        }
    }
}
#endif // UNITY_EDITOR && (ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER)
