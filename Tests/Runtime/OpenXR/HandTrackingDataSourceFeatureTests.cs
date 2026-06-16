#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    public class HandTrackingDataSourceFeatureTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;
        HandTrackingDataSourceFeature m_Feature;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.Environment.AddSupportedExtension("XR_EXT_hand_tracking_data_source", 1);
            m_MockEnvironment.Environment.Settings.EnableFeature<HandTrackingDataSourceFeature>(true);
            m_Feature = m_MockEnvironment.Environment.Settings.GetFeature<HandTrackingDataSourceFeature>();
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

            // The feature is created once per fixture; reset preferences so
            // tests that mutate them via TryUpdateConfiguration don't leak
            // into other tests.
            m_Feature?.TryUpdateConfiguration(new HandTrackingDataSourceConfig
            {
                leftPreferredSources = new[] { HandTrackingDataSource.Unobstructed, HandTrackingDataSource.Controller },
                rightPreferredSources = new[] { HandTrackingDataSource.Unobstructed, HandTrackingDataSource.Controller },
            });
        }

        [Test]
        public void TryGetConfiguration_Default_ReturnsUnobstructedAndController()
        {
            Assert.IsTrue(m_Feature.TryGetConfiguration(out var config));
            Assert.IsNotNull(config.leftPreferredSources);
            Assert.IsNotNull(config.rightPreferredSources);

            Assert.Contains(HandTrackingDataSource.Unobstructed, config.leftPreferredSources);
            Assert.Contains(HandTrackingDataSource.Controller, config.leftPreferredSources);
            Assert.AreEqual(2, config.leftPreferredSources.Length);

            Assert.Contains(HandTrackingDataSource.Unobstructed, config.rightPreferredSources);
            Assert.Contains(HandTrackingDataSource.Controller, config.rightPreferredSources);
            Assert.AreEqual(2, config.rightPreferredSources.Length);
        }

        [Test]
        public void TryUpdateConfiguration_StagesNewSources()
        {
            var newConfig = new HandTrackingDataSourceConfig
            {
                leftPreferredSources = new[] { HandTrackingDataSource.Controller },
                rightPreferredSources = new[] { HandTrackingDataSource.Unobstructed },
            };

            Assert.IsTrue(m_Feature.TryUpdateConfiguration(newConfig));
            Assert.IsTrue(m_Feature.TryGetConfiguration(out var readBack));

            Assert.AreEqual(1, readBack.leftPreferredSources.Length);
            Assert.AreEqual(HandTrackingDataSource.Controller, readBack.leftPreferredSources[0]);

            Assert.AreEqual(1, readBack.rightPreferredSources.Length);
            Assert.AreEqual(HandTrackingDataSource.Unobstructed, readBack.rightPreferredSources[0]);
        }

        [UnityTest]
        public IEnumerator TryGetData_BeforeLocate_ReturnsFalse()
        {
            // Start with a locate mock that reports inactive hands so
            // any prior active state from other tests is cleared.
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsInactive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.IsFalse(m_Feature.TryGetData(Handedness.Left, out _));
            Assert.IsFalse(m_Feature.TryGetData(Handedness.Right, out _));
        }

        [UnityTest]
        public IEnumerator OnHandTrackingCreateRequest_InjectsInfoIntoCreateChain()
        {
            s_FoundDataSourceInfoLeft = false;
            s_FoundDataSourceInfoRight = false;

            m_MockEnvironment.createHandTracker.mock = CreateHandTrackerWithChainCheck;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsActive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.IsTrue(s_FoundDataSourceInfoLeft,
                "XrHandTrackingDataSourceInfoEXT should be in the left hand create chain.");
            Assert.IsTrue(s_FoundDataSourceInfoRight,
                "XrHandTrackingDataSourceInfoEXT should be in the right hand create chain.");
        }

        [UnityTest]
        public IEnumerator OnLocateHandJointsResult_ReadsActiveSource()
        {
            s_MockDataSource = XrHandTrackingDataSourceEXT.Controller;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithDataSourceWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.IsTrue(m_Feature.TryGetData(Handedness.Left, out var leftSource));
            Assert.AreEqual(HandTrackingDataSource.Controller, leftSource);

            Assert.IsTrue(m_Feature.TryGetData(Handedness.Right, out var rightSource));
            Assert.AreEqual(HandTrackingDataSource.Controller, rightSource);
        }

        [UnityTest]
        public IEnumerator StartStopCycle_CompletesWithoutErrors()
        {
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithDataSourceWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();
            m_MockEnvironment.Stop();
            yield return null;

            // Second cycle — verifies NativeArray disposal and recreation works.
            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();
        }

        [UnityTest]
        public IEnumerator TryUpdateConfiguration_TriggersRestart_NewSourceActiveOnNextDynamic()
        {
            s_MockDataSource = XrHandTrackingDataSourceEXT.Unobstructed;

            int createCount = 0;
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = info.hand == XrHandEXT.Left ? MockHandsEnvironment.k_LeftTrackerHandle : MockHandsEnvironment.k_RightTrackerHandle;
                    createCount++;
                    return XrResult.Success;
                };
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithDataSourceWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out var initialLeft), Is.True);
            Assert.That(initialLeft, Is.EqualTo(HandTrackingDataSource.Unobstructed));
            Assert.That(m_Feature.TryGetData(Handedness.Right, out var initialRight), Is.True);
            Assert.That(initialRight, Is.EqualTo(HandTrackingDataSource.Unobstructed));

            int createCountBeforeRestart = createCount;
            s_MockDataSource = XrHandTrackingDataSourceEXT.Controller;
            m_Feature.TryUpdateConfiguration(new HandTrackingDataSourceConfig
            {
                leftPreferredSources = new[] { HandTrackingDataSource.Controller },
                rightPreferredSources = new[] { HandTrackingDataSource.Controller },
            });

            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out var updatedLeft), Is.True);
            Assert.That(updatedLeft, Is.EqualTo(HandTrackingDataSource.Controller));
            Assert.That(m_Feature.TryGetData(Handedness.Right, out var updatedRight), Is.True);
            Assert.That(updatedRight, Is.EqualTo(HandTrackingDataSource.Controller));
            Assert.That(createCount - createCountBeforeRestart, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator OnHandTrackerDestroyed_ResetsActiveFlag_TryGetDataReturnsFalse()
        {
            s_MockDataSource = XrHandTrackingDataSourceEXT.Unobstructed;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithDataSourceWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out _), Is.True);
            Assert.That(m_Feature.TryGetData(Handedness.Right, out _), Is.True);

            // Switch create to RuntimeFailure so the restart leaves trackers destroyed.
            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = 0;
                    return XrResult.RuntimeFailure;
                };
            m_Feature.TryUpdateConfiguration(new HandTrackingDataSourceConfig
            {
                leftPreferredSources = new[] { HandTrackingDataSource.Unobstructed },
                rightPreferredSources = new[] { HandTrackingDataSource.Unobstructed },
            });

            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out _), Is.False);
            Assert.That(m_Feature.TryGetData(Handedness.Right, out _), Is.False);
        }

        [UnityTest]
        public IEnumerator TryUpdateConfiguration_FailedRecreate_StaysInactive()
        {
            s_MockDataSource = XrHandTrackingDataSourceEXT.Unobstructed;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithDataSourceWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out _), Is.True);
            Assert.That(m_Feature.TryGetData(Handedness.Right, out _), Is.True);

            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = 0;
                    return XrResult.RuntimeFailure;
                };
            m_Feature.TryUpdateConfiguration(new HandTrackingDataSourceConfig
            {
                leftPreferredSources = new[] { HandTrackingDataSource.Controller },
                rightPreferredSources = new[] { HandTrackingDataSource.Controller },
            });

            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out _), Is.False);
            Assert.That(m_Feature.TryGetData(Handedness.Right, out _), Is.False);

            // A second frame should still be inactive — the recovery loop retries
            // but RuntimeFailure keeps the trackers from becoming active.
            yield return new WaitForXrFrame();

            Assert.That(m_Feature.TryGetData(Handedness.Left, out _), Is.False);
            Assert.That(m_Feature.TryGetData(Handedness.Right, out _), Is.False);
        }

        // --- Mock helpers ---

        static bool s_FoundDataSourceInfoLeft;
        static bool s_FoundDataSourceInfoRight;

        static unsafe XrResult CreateHandTrackerWithChainCheck(
            XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker)
        {
            tracker = info.hand == XrHandEXT.Left ? MockHandsEnvironment.k_LeftTrackerHandle : MockHandsEnvironment.k_RightTrackerHandle;

            bool found = false;
            var current = (XrBaseInStructure*)info.next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandTrackingDataSourceInfoEXT)
                {
                    found = true;
                    break;
                }
                current = (XrBaseInStructure*)current->next;
            }

            if (info.hand == XrHandEXT.Left)
                s_FoundDataSourceInfoLeft = found;
            else
                s_FoundDataSourceInfoRight = found;

            return XrResult.Success;
        }

        static XrHandTrackingDataSourceEXT s_MockDataSource;

        static unsafe XrResult MockLocateWithDataSourceWrite(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);

            var current = (XrBaseInStructure*)locations.next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandTrackingDataSourceStateEXT)
                {
                    var filled = new XrHandTrackingDataSourceStateEXT(current->next, true, s_MockDataSource);
                    UnsafeUtility.CopyStructureToPtr(ref filled, current);
                    break;
                }
                current = (XrBaseInStructure*)current->next;
            }

            return XrResult.Success;
        }

        static unsafe XrResult MockLocateHandJointsInactive(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, false, 26, locations.jointLocations);
            return XrResult.Success;
        }

        static unsafe XrResult MockLocateHandJointsActive(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);
            return XrResult.Success;
        }
    }
}
#endif
