#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime;
using UnityEngine.XR.OpenXR.NativeTypes;
using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    /// <summary>
    /// Basic integration test validating OpenXRHandProvider initialization
    /// using MockHandsEnvironment.
    /// </summary>
    public class BasicOpenXRHandTrackingExtensionIntegrationTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;

        // 2 trackers * 2 updates per frame (dynamic, and beforerender)
        const uint kExpectedTrackerCallsPerFrame = 4;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            if (m_MockEnvironment != null)
            {
                m_MockEnvironment.createHandTracker.mock = null;
                m_MockEnvironment.locateHandJoints.mock = null;
                m_MockEnvironment.destroyHandTracker.mock = null;
            }
        }

        [UnityTest]
        public IEnumerator OpenXRHandProvider_Lifecycle_CreatesUpdatesAndDestroys()
        {
            var handTrackingFeature = m_MockEnvironment.Environment.Settings.GetFeature<HandTracking>();
            Assert.IsNotNull(handTrackingFeature, "HandTracking feature should be available");
            Assert.IsTrue(handTrackingFeature.enabled, "HandTracking feature should be enabled");

            const XrHandTrackerEXT kExpectedTrackerLeft = 0x1234;
            const XrHandTrackerEXT kExpectedTrackerRight = 0xABCD;

            var createSessions = new List<XrSession>();
            var createHands = new List<XrHandEXT>();
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    createSessions.Add(session);
                    createHands.Add(info.hand);
                    tracker = info.hand == XrHandEXT.Left ? kExpectedTrackerLeft : kExpectedTrackerRight;
                    return XrResult.Success;
                };

            var locateTrackers = new List<XrHandTrackerEXT>();
            m_MockEnvironment.locateHandJoints.mock = (
                XrHandTrackerEXT tracker,
                in XrHandJointsLocateInfoEXT info,
                ref XrHandJointLocationsEXT locations) =>
            {
                locateTrackers.Add(tracker);
                return XrResult.Success;
            };

            var destroyTrackers = new List<XrHandTrackerEXT>();
            m_MockEnvironment.destroyHandTracker.mock =
                tracker =>
                {
                    destroyTrackers.Add(tracker);
                    return XrResult.Success;
                };

            m_MockEnvironment.Start();

            // Wait for hand trackers to be created by the loader-driven subsystem
            yield return new WaitForXrFrame();

            Assert.That(createSessions, Has.Count.EqualTo(2));
            Assert.That(createSessions, Has.All.Not.EqualTo((XrSession)0));
            Assert.That(createHands, Is.EquivalentTo(new[] { XrHandEXT.Left, XrHandEXT.Right }));

            // Clear accumulated locate calls from the creation phase, then
            // wait one more frame to observe a clean update cycle.
            locateTrackers.Clear();
            yield return new WaitForXrFrame();

            Assert.That(locateTrackers, Has.Count.EqualTo(kExpectedTrackerCallsPerFrame));
            Assert.That(locateTrackers, Has.All.Matches<XrHandTrackerEXT>(t => t == kExpectedTrackerLeft || t == kExpectedTrackerRight));

            m_MockEnvironment.Stop();
            yield return null;

            Assert.That(destroyTrackers, Has.Count.EqualTo(2));
            Assert.That(destroyTrackers, Is.EquivalentTo(new[] { kExpectedTrackerLeft, kExpectedTrackerRight }));
        }

        /// <summary>
        /// Validate that returning a failure from xrCreateHandTrackerEXT does not bring down the process.
        /// </summary>
        [UnityTest]
        public IEnumerator OpenXRHandProvider_Lifecycle_FailsTrackerCreation()
        {
            var handTrackingFeature = m_MockEnvironment.Environment.Settings.GetFeature<HandTracking>();
            Assert.IsNotNull(handTrackingFeature, "HandTracking feature should be available");
            Assert.IsTrue(handTrackingFeature.enabled, "HandTracking feature should be enabled");

            bool createTrackerCalled = false;
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    createTrackerCalled = true;
                    tracker = 0;
                    return XrResult.RuntimeFailure;
                };

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(frames: 3);
            Assert.That(createTrackerCalled, Is.True);

            m_MockEnvironment.Stop();
            yield return null;
        }
    }
}
#endif
