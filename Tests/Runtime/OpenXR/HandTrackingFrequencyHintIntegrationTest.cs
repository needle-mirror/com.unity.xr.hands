#if OPENXR_1_19_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    /// <summary>
    /// Integration tests for the <see cref="MetaHandTrackingFrequencyHintFeature"/>
    /// OpenXR feature, validating the managed-to-native round trip through
    /// the mock OpenXR runtime.
    /// </summary>
    public class HandTrackingFrequencyHintIntegrationTest
    {
        MockHandsEnvironment m_MockEnvironment;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.SetUpHandTrackingFrequencyHintExtension();
        }

        [SetUp]
        public void SetUp()
        {
            m_MockEnvironment.Environment.Settings.EnableFeature<HandTracking>(true);
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingFrequencyHintFeature>(true);

            // Reset stored hint so tests don't depend on execution order.
            // m_SessionRunning is false after the previous TearDown's Stop(),
            // so this just writes to m_FrequencyHint without a native call.
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature, "MetaHandTrackingFrequencyHintFeature should be available after enabling");
            feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.Default });
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
                m_MockEnvironment.setHandTrackingFrequencyHint.mock = null;
            }
        }

        void SetUpMinimalHandTrackingMocks()
        {
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = info.hand == XrHandEXT.Left ? 0x1234UL : 0xABCDUL;
                    return XrResult.Success;
                };

            m_MockEnvironment.locateHandJoints.mock =
                (XrHandTrackerEXT tracker, in XrHandJointsLocateInfoEXT info, ref XrHandJointLocationsEXT locations) =>
                    XrResult.Success;

            m_MockEnvironment.destroyHandTracker.mock =
                tracker => XrResult.Success;
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_AppliesDefaultHintOnSessionCreate()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature, "MetaHandTrackingFrequencyHintFeature should be available");
            Assert.IsTrue(feature.enabled, "MetaHandTrackingFrequencyHintFeature should be enabled");

            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<(XrSession session, int hint)>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add((session, hint));
                return XrResult.Success;
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(hintCalls, Has.Count.GreaterThanOrEqualTo(1),
                "xrSetHandTrackingFrequencyHintMETA should be called on session create");
            Assert.That(hintCalls[0].session, Is.Not.EqualTo((XrSession)0),
                "Session handle should be valid");
            Assert.That(hintCalls[0].hint, Is.EqualTo((int)MetaHandTrackingFrequencyHint.Default),
                "Initial hint should be Default");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_DuringSession_Succeeds()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<(XrSession session, int hint)>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add((session, hint));
                return XrResult.Success;
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            int callCountBeforeUpdate = hintCalls.Count;
            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });

            Assert.IsTrue(result, "TryUpdateConfiguration should return true on success");
            Assert.That(hintCalls, Has.Count.GreaterThan(callCountBeforeUpdate),
                "A new native call should have been made");
            Assert.That(hintCalls[hintCalls.Count - 1].hint,
                Is.EqualTo((int)MetaHandTrackingFrequencyHint.High),
                "Native call should receive High hint");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_RoundTripsViaSubsystem()
        {
            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<int>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add(hint);
                return XrResult.Success;
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            int callCountAfterCreate = hintCalls.Count;

            // TryUpdateConfiguration returns false if no handler is registered, so a
            // successful call is itself proof that OnHandSubsystemCreated wired us in.
            Assert.IsTrue(HandTracking.subsystem.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High }));
            Assert.That(hintCalls, Has.Count.EqualTo(callCountAfterCreate + 1),
                "Updating through the subsystem should reach the native call");

            Assert.IsTrue(HandTracking.subsystem.TryGetConfiguration<MetaHandTrackingFrequencyHintConfig>(out var readBack));
            Assert.That(readBack.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.High));
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_NativeFailure_ReturnsFalseAndPreservesHint()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) => XrResult.Success;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) => XrResult.RuntimeFailure;

            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });

            Assert.IsFalse(result, "TryUpdateConfiguration should return false when native call fails");

            feature.TryGetConfiguration(out var currentConfig);
            Assert.That(currentConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.Default),
                "Stored hint should remain Default after failed update");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_SameValue_NoNativeCall()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<int>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add(hint);
                return XrResult.Success;
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            int callCountAfterSessionCreate = hintCalls.Count;

            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.Default });

            Assert.IsTrue(result, "TryUpdateConfiguration should return true for same value");
            Assert.That(hintCalls, Has.Count.EqualTo(callCountAfterSessionCreate),
                "No additional native call should be made for the same hint value");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_BeforeSession_DefersUntilSessionCreate()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<int>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add(hint);
                return XrResult.Success;
            };

            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });
            Assert.IsTrue(result, "TryUpdateConfiguration should return true before session (deferred)");
            Assert.That(hintCalls, Has.Count.EqualTo(0),
                "No native call should be made before session starts");

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(hintCalls, Has.Count.GreaterThanOrEqualTo(1),
                "xrSetHandTrackingFrequencyHintMETA should be called after session creates");
            Assert.That(hintCalls[0], Is.EqualTo((int)MetaHandTrackingFrequencyHint.High),
                "Deferred hint should be High, not Default");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_DeferredHintFailsOnSessionCreate_ResetsToDefault()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            // Defer High before session starts.
            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });
            Assert.IsTrue(result, "TryUpdateConfiguration should return true before session (deferred)");

            feature.TryGetConfiguration(out var configBeforeSession);
            Assert.That(configBeforeSession.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.High),
                "Stored hint should be High before session starts");

            // Reject all frequency hint calls so OnSessionCreate's ApplyFrequencyHint fails.
            m_MockEnvironment.setHandTrackingFrequencyHint.mock =
                (XrSession session, int hint) => XrResult.RuntimeFailure;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            // OnSessionCreate tried to apply High but the runtime rejected it, so the
            // stored hint resets to Default — consistent with TryUpdateConfiguration,
            // which also does not update the stored value on failure.
            feature.TryGetConfiguration(out var configAfterSession);
            Assert.That(configAfterSession.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.Default),
                "Stored hint should reset to Default when the runtime rejects it on session create");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryGetConfiguration_ReturnsCurrentHint()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) => XrResult.Success;

            feature.TryGetConfiguration(out var initialConfig);
            Assert.That(initialConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.Default),
                "Initial hint should be Default");

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });

            feature.TryGetConfiguration(out var updatedConfig);
            Assert.That(updatedConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.High),
                "Hint should reflect the last successful update");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_InvalidEnum_ReturnsFalseWithoutCrash()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
                hint == (int)MetaHandTrackingFrequencyHint.Default ||
                hint == (int)MetaHandTrackingFrequencyHint.High
                    ? XrResult.Success
                    : XrResult.ValidationFailure;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            var invalidHint = (MetaHandTrackingFrequencyHint)999;
            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = invalidHint });

            Assert.IsFalse(result, "Invalid enum value should cause the runtime to reject the call");

            feature.TryGetConfiguration(out var currentConfig);
            Assert.That(currentConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.Default),
                "Stored hint should remain Default after rejected invalid value");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_RuntimeRejectsHint_HandTrackingStillWorks()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            var createHands = new List<XrHandEXT>();
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    createHands.Add(info.hand);
                    tracker = info.hand == XrHandEXT.Left ? 0x1234UL : 0xABCDUL;
                    return XrResult.Success;
                };

            var locateCount = 0;
            m_MockEnvironment.locateHandJoints.mock =
                (XrHandTrackerEXT tracker, in XrHandJointsLocateInfoEXT info, ref XrHandJointLocationsEXT locations) =>
                {
                    locateCount++;
                    return XrResult.Success;
                };

            m_MockEnvironment.destroyHandTracker.mock = tracker => XrResult.Success;

            m_MockEnvironment.setHandTrackingFrequencyHint.mock =
                (XrSession session, int hint) => XrResult.FunctionUnsupported;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(createHands, Is.EquivalentTo(new[] { XrHandEXT.Left, XrHandEXT.Right }),
                "Both hand trackers should be created despite frequency hint being unsupported");
            Assert.That(locateCount, Is.GreaterThan(0),
                "Hand joints should still be located when frequency hint is unsupported");

            bool result = feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High });
            Assert.IsFalse(result,
                "TryUpdateConfiguration should return false when runtime does not support the extension");

            feature.TryGetConfiguration(out var currentConfig);
            Assert.That(currentConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.Default),
                "Stored hint should remain Default after unsupported call");
        }

        [UnityTest]
        public IEnumerator HandTrackingFrequencyHint_TryUpdateConfiguration_SequentialChanges_AppliesCorrectly()
        {
            var feature = m_MockEnvironment.Environment.Settings.GetFeature<MetaHandTrackingFrequencyHintFeature>();
            Assert.IsNotNull(feature);

            SetUpMinimalHandTrackingMocks();

            var hintCalls = new List<int>();
            m_MockEnvironment.setHandTrackingFrequencyHint.mock = (XrSession session, int hint) =>
            {
                hintCalls.Add(hint);
                return XrResult.Success;
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            int callCountAfterCreate = hintCalls.Count;

            Assert.IsTrue(feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High }));
            Assert.IsTrue(feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.Default }));
            Assert.IsTrue(feature.TryUpdateConfiguration(
                new MetaHandTrackingFrequencyHintConfig { frequencyHint = MetaHandTrackingFrequencyHint.High }));

            Assert.That(hintCalls, Has.Count.EqualTo(callCountAfterCreate + 3),
                "Each distinct configuration change should produce a native call");
            Assert.That(hintCalls[callCountAfterCreate], Is.EqualTo((int)MetaHandTrackingFrequencyHint.High));
            Assert.That(hintCalls[callCountAfterCreate + 1], Is.EqualTo((int)MetaHandTrackingFrequencyHint.Default));
            Assert.That(hintCalls[callCountAfterCreate + 2], Is.EqualTo((int)MetaHandTrackingFrequencyHint.High));

            feature.TryGetConfiguration(out var finalConfig);
            Assert.That(finalConfig.frequencyHint, Is.EqualTo(MetaHandTrackingFrequencyHint.High));
        }
    }
}
#endif
