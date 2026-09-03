#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
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
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class HandJointsMotionRangeFeatureTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;
        HandJointsMotionRangeFeature m_Feature;
        TestHandTrackingExtensionFeature m_TestFeature;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.Environment.AddSupportedExtension("XR_EXT_hand_joints_motion_range", 1);
            m_MockEnvironment.Environment.Settings.EnableFeature<HandJointsMotionRangeFeature>(true);
            m_Feature = m_MockEnvironment.Environment.Settings.GetFeature<HandJointsMotionRangeFeature>();
            m_MockEnvironment.Environment.Settings.EnableFeature<TestHandTrackingExtensionFeature>(true);
            m_TestFeature = m_MockEnvironment.Environment.Settings.GetFeature<TestHandTrackingExtensionFeature>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
        }

        [SetUp]
        public void SetUpPerTest()
        {
            // Feature instance is shared across tests via [OneTimeSetUp].
            // Reset managed motion-range state so tests don't see prior mutations.
            // No session is live at this point — TryUpdateConfiguration takes the
            // staging path (!m_SessionActive), updating only the managed fields.
            m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
            {
                leftMotionRange = HandJointsMotionRange.Unobstructed,
                rightMotionRange = HandJointsMotionRange.Unobstructed,
            });
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
            m_TestFeature?.ResetCallbacks();
        }

        [Test]
        public void TryGetConfiguration_Default_ReturnsUnobstructedForBothHands()
        {
            Assert.IsTrue(m_Feature.TryGetConfiguration(out var config));
            Assert.AreEqual(HandJointsMotionRange.Unobstructed, config.leftMotionRange);
            Assert.AreEqual(HandJointsMotionRange.Unobstructed, config.rightMotionRange);
        }

        [UnityTest]
        public IEnumerator TryUpdateConfiguration_RoundTripsViaSubsystem()
        {
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsActive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            try
            {
                var newConfig = new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                };

                // TryUpdateConfiguration returns false if no handler is registered, so a
                // successful call is itself proof that OnHandSubsystemCreated wired us in.
                Assert.IsTrue(HandTracking.subsystem.TryUpdateConfiguration(newConfig));
                Assert.IsTrue(HandTracking.subsystem.TryGetConfiguration<HandJointsMotionRangeConfig>(out var readBack));
                Assert.AreEqual(HandJointsMotionRange.ConformingToController, readBack.leftMotionRange);
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, readBack.rightMotionRange);
            }
            finally
            {
                // Restore defaults.
                HandTracking.subsystem.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.Unobstructed,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });
            }
        }

        [UnityTest]
        public IEnumerator OnLocate_InjectsMotionRangeInfoIntoLocateInputChain()
        {
            s_LeftObservedMotionRange = default;
            s_RightObservedMotionRange = default;
            s_FoundMotionRangeLeft = false;
            s_FoundMotionRangeRight = false;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithMotionRangeCheck;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.IsTrue(s_FoundMotionRangeLeft,
                "XrHandJointsMotionRangeInfoEXT should be in the left hand locate input chain.");
            Assert.IsTrue(s_FoundMotionRangeRight,
                "XrHandJointsMotionRangeInfoEXT should be in the right hand locate input chain.");
            Assert.AreEqual(XrHandJointsMotionRangeEXT.Unobstructed, s_LeftObservedMotionRange);
            Assert.AreEqual(XrHandJointsMotionRangeEXT.Unobstructed, s_RightObservedMotionRange);
        }

        [UnityTest]
        public IEnumerator TryUpdateConfiguration_PropagatesToNextLocateFrame()
        {
            s_LeftObservedMotionRange = default;
            s_RightObservedMotionRange = default;
            s_FoundMotionRangeLeft = false;
            s_FoundMotionRangeRight = false;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithMotionRangeCheck;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            try
            {
                // Mutate and drive another frame.
                m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });

                s_LeftObservedMotionRange = default;
                s_RightObservedMotionRange = default;
                s_FoundMotionRangeLeft = false;
                s_FoundMotionRangeRight = false;

                yield return new WaitForXrFrame();

                Assert.IsTrue(s_FoundMotionRangeLeft);
                Assert.IsTrue(s_FoundMotionRangeRight);
                Assert.AreEqual(XrHandJointsMotionRangeEXT.ConformingToController, s_LeftObservedMotionRange,
                    "Left hand should reflect ConformingToController after TryUpdateConfiguration.");
                Assert.AreEqual(XrHandJointsMotionRangeEXT.Unobstructed, s_RightObservedMotionRange,
                    "Right hand should remain Unobstructed.");
            }
            finally
            {
                // Restore defaults.
                m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.Unobstructed,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });
            }
        }

        [Test]
        public void TryUpdateConfiguration_BeforeSessionStart_ReturnsTrue()
        {
            try
            {
                // Staging without a live session — chain is null, should still succeed.
                bool stagingResult = m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });

                Assert.IsTrue(stagingResult, "TryUpdateConfiguration should return true when no session is active (staging-only).");
            }
            finally
            {
                m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.Unobstructed,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });
            }
        }

        [TestCase((HandJointsMotionRange)0, HandJointsMotionRange.Unobstructed, TestName = "InvalidLeft")]
        [TestCase(HandJointsMotionRange.Unobstructed, (HandJointsMotionRange)0, TestName = "InvalidRight")]
        [TestCase((HandJointsMotionRange)0, (HandJointsMotionRange)0, TestName = "BothDefault")]
        [TestCase((HandJointsMotionRange)999, HandJointsMotionRange.Unobstructed, TestName = "OutOfRange")]
        public void TryUpdateConfiguration_InvalidValue_ReturnsFalseAndRetainsState(
            HandJointsMotionRange left, HandJointsMotionRange right)
        {
            bool result = m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
            {
                leftMotionRange = left,
                rightMotionRange = right,
            });

            Assert.That(result, Is.False);
            Assert.That(m_Feature.TryGetConfiguration(out var config), Is.True);
            Assert.That(config.leftMotionRange, Is.EqualTo(HandJointsMotionRange.Unobstructed));
            Assert.That(config.rightMotionRange, Is.EqualTo(HandJointsMotionRange.Unobstructed));
        }

        [UnityTest]
        public IEnumerator TryUpdateConfiguration_DuringInstanceCreate_StagesValueToSessionStart()
        {
            s_LeftObservedMotionRange = default;
            s_RightObservedMotionRange = default;
            s_FoundMotionRangeLeft = false;
            s_FoundMotionRangeRight = false;

            bool? stagingResult = null;

            // Fires after HandJointsMotionRangeFeature.OnInstanceCreate (priority 0) but before
            // any OnSessionCreate — the window where the chain is non-null but empty.
            m_TestFeature.instanceCreate = _ =>
            {
                stagingResult = m_Feature.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.ConformingToController,
                });
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithMotionRangeCheck;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.That(stagingResult, Is.True,
                "TryUpdateConfiguration called between OnInstanceCreate and OnSessionCreate must return true (treat as staging).");
            Assert.That(s_FoundMotionRangeLeft, Is.True);
            Assert.That(s_FoundMotionRangeRight, Is.True);
            Assert.That(s_LeftObservedMotionRange, Is.EqualTo(XrHandJointsMotionRangeEXT.ConformingToController),
                "Left hand staged value must be honored once OnSessionCreate adds the node.");
            Assert.That(s_RightObservedMotionRange, Is.EqualTo(XrHandJointsMotionRangeEXT.ConformingToController));
        }

        [UnityTest]
        public IEnumerator SubsystemDestroyAndRestart_HandlerReregistersOnNewSubsystem()
        {
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsActive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            m_MockEnvironment.Stop();

            // Re-arm mocks before restarting.
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsActive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            try
            {
                // Handler should be registered on the new subsystem; TryUpdateConfiguration would
                // return false if UnregisterConfigurationHandler leaked on the destroyed subsystem
                // and RegisterConfigurationHandler was not called on the new one.
                Assert.IsTrue(HandTracking.subsystem.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                }));
                Assert.IsTrue(HandTracking.subsystem.TryGetConfiguration<HandJointsMotionRangeConfig>(out var readBack));
                Assert.AreEqual(HandJointsMotionRange.ConformingToController, readBack.leftMotionRange);
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, readBack.rightMotionRange);
            }
            finally
            {
                HandTracking.subsystem.TryUpdateConfiguration(new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.Unobstructed,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                });
            }
        }

        static bool s_FoundMotionRangeLeft;
        static bool s_FoundMotionRangeRight;
        static XrHandJointsMotionRangeEXT s_LeftObservedMotionRange;
        static XrHandJointsMotionRangeEXT s_RightObservedMotionRange;

        static unsafe XrResult MockLocateHandJointsActive(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);
            return XrResult.Success;
        }

        static unsafe XrResult MockLocateWithMotionRangeCheck(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);

            bool isLeft = tracker == MockHandsEnvironment.k_LeftTrackerHandle;
            FindMotionRangeNode(locateInfo.next, isLeft);

            return XrResult.Success;
        }

        static unsafe void FindMotionRangeNode(void* next, bool isLeft)
        {
            var current = (XrBaseInStructure*)next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandJointsMotionRangeInfoEXT)
                {
                    var info = *(XrHandJointsMotionRangeInfoEXT*)current;
                    if (isLeft)
                    {
                        s_FoundMotionRangeLeft = true;
                        s_LeftObservedMotionRange = info.handJointsMotionRange;
                    }
                    else
                    {
                        s_FoundMotionRangeRight = true;
                        s_RightObservedMotionRange = info.handJointsMotionRange;
                    }
                    return;
                }
                current = (XrBaseInStructure*)current->next;
            }
        }
    }
}
#endif
