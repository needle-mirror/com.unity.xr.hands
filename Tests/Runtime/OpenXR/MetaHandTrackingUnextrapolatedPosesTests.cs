#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
#if OPENXR_1_19_0_PRE_2_OR_NEWER
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR.MockHandsRuntime.Tests;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using XrHandTrackerEXT = System.UInt64;

namespace UnityEngine.XR.Hands.OpenXR.Tests
{
    static class UnextrapolatedPosesMock
    {
        public const long k_CaptureTime = 918273645L;

        // Different locates answer in a single frame: the primary one, and one per hand carrying the
        // unextrapolated poses request. Each writes its own marker into every joint's position.x,
        // so a test can tell which locate produced a hand's data.
        public const float k_ExtrapolatedMarker = 0.125f;
        public const float k_UnextrapolatedLeftMarker = 0.625f;
        public const float k_UnextrapolatedRightMarker = 0.875f;

        public static int locateCallCount;
        public static int locatesCarryingRequest;
        public static long captureTimeToReport = k_CaptureTime;
        public static bool leftSecondaryLocateIsActive = true;

        public static void Reset()
        {
            locateCallCount = 0;
            locatesCarryingRequest = 0;
            captureTimeToReport = k_CaptureTime;
            leftSecondaryLocateIsActive = true;
        }

        /// <summary>
        /// Walks a locate call's input chain looking for the unextrapolated poses request.
        /// </summary>
        /// <param name="locateInfo">The locate info the runtime was handed.</param>
        /// <returns>Whether the request node rides this locate.</returns>
        public static unsafe bool CarriesUnextrapolatedRequest(in XrHandJointsLocateInfoEXT locateInfo)
        {
            var current = (XrBaseInStructure*)locateInfo.next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandTrackingUnextrapolatedPosesRequestMETA)
                    return true;

                current = (XrBaseInStructure*)current->next;
            }

            return false;
        }

        /// <summary>
        /// Writes the capture time into the output chain's unextrapolated poses node, if present.
        /// </summary>
        /// <param name="next">The head of the output chain.</param>
        /// <param name="captureTime">The capture time to report.</param>
        /// <returns>Whether the node was found.</returns>
        public static unsafe bool WriteCaptureTime(void* next, long captureTime)
        {
            var current = (XrBaseInStructure*)next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandTrackingUnextrapolatedPosesMETA)
                {
                    // The struct is readonly, so the whole value has to be replaced through the
                    // pointer; assigning the property directly does not compile.
                    var node = (XrHandTrackingUnextrapolatedPosesMETA*)current;
                    *node = new XrHandTrackingUnextrapolatedPosesMETA(node->next, captureTime);
                    return true;
                }

                current = (XrBaseInStructure*)current->next;
            }

            return false;
        }

        /// <summary>
        /// Answers a locate call with data that identifies which locate it was, and which hand.
        /// </summary>
        /// <param name="tracker">The hand tracker handle.</param>
        /// <param name="locateInfo">The locate info the runtime was handed.</param>
        /// <param name="locations">The locations to fill out.</param>
        /// <returns><c>XrResult.Success</c>.</returns>
        public static unsafe XrResult Locate(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            ++locateCallCount;

            var carriesRequest = CarriesUnextrapolatedRequest(in locateInfo);
            var isLeftHand = tracker == MockHandsEnvironment.k_LeftTrackerHandle;
            var isActive = true;

            if (carriesRequest)
            {
                ++locatesCarryingRequest;
                isActive = !isLeftHand || leftSecondaryLocateIsActive;

                if (isActive)
                {
                    MockHandJoints.WriteTrackedHand(
                        locations.jointLocations,
                        isLeftHand ? k_UnextrapolatedLeftMarker : k_UnextrapolatedRightMarker);
                    WriteCaptureTime(locations.next, captureTimeToReport);
                }
            }
            else
            {
                MockHandJoints.WriteTrackedHand(locations.jointLocations, k_ExtrapolatedMarker);
            }

            locations = new XrHandJointLocationsEXT(
                locations.next, isActive, MockHandJoints.k_JointCount, locations.jointLocations);
            return XrResult.Success;
        }
    }

    /// <summary>
    /// Covers the wiring between <see cref="MetaHandTrackingUnextrapolatedPoses"/>, the secondary
    /// locate chains, and the unextrapolated hand the subsystem surfaces.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class MetaHandTrackingUnextrapolatedPosesTests : OpenXRHandTrackingTestFixture
    {
        // Per Unity frame the subsystem locates each hand once per update type, and the feature adds
        // one secondary locate per hand on the Dynamic update only.
        const int k_PrimaryLocatesPerFrame = 4;
        const int k_SecondaryLocatesPerFrame = 2;

        MockHandsEnvironment m_MockEnvironment;
        OpenXRSettings.LatencyOptimization m_PreviousLatencyOptimization;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();

            // SetUpDefaultHandTrackingEnvironment only adds XR_EXT_hand_tracking, so without this the
            // feature's OnInstanceCreate returns false and it never registers anything.
            m_MockEnvironment.Environment.AddSupportedExtension(
                MetaHandTrackingUnextrapolatedPoses.extensionString, 1);
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingUnextrapolatedPoses>(true);

            m_PreviousLatencyOptimization = OpenXRSettings.Instance.latencyOptimization;
            OpenXRSettings.Instance.latencyOptimization =
                OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            OpenXRSettings.Instance.latencyOptimization = m_PreviousLatencyOptimization;
            m_MockEnvironment?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            UnextrapolatedPosesMock.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();

            UnextrapolatedPosesMock.Reset();
        }

        static float ExpectedUnextrapolatedMarker(Handedness handedness)
            => handedness == Handedness.Left
                ? UnextrapolatedPosesMock.k_UnextrapolatedLeftMarker
                : UnextrapolatedPosesMock.k_UnextrapolatedRightMarker;

        static float GetLocateMarker(XRHand hand)
        {
            Assert.IsTrue(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var pose),
                $"{hand} should have a wrist pose.");
            return pose.position.x;
        }

        [UnityTest]
        public IEnumerator RequestRidesOnlyTheSecondaryLocate()
        {
            m_MockEnvironment.InstallMocks(UnextrapolatedPosesMock.Locate);
            m_MockEnvironment.Start();

            // The first wait of a session spans session bootstrap, so counts are only meaningful
            // across a warm wait.
            yield return new WaitForXrFrame();

            UnextrapolatedPosesMock.locateCallCount = 0;
            UnextrapolatedPosesMock.locatesCarryingRequest = 0;

            yield return new WaitForXrFrame();

            Assert.AreEqual(
                k_PrimaryLocatesPerFrame + k_SecondaryLocatesPerFrame,
                UnextrapolatedPosesMock.locateCallCount,
                "The feature should add exactly one secondary locate per hand per frame.");
            Assert.AreEqual(
                k_SecondaryLocatesPerFrame,
                UnextrapolatedPosesMock.locatesCarryingRequest,
                "Only the secondary locates may carry the unextrapolated poses request. Carrying it on " +
                "the primary locate would hand every existing consumer unextrapolated joints.");
        }

        [UnityTest]
        public IEnumerator BothPoseSetsSurfaceInTheSameFrame(
            [ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            m_MockEnvironment.InstallMocks(UnextrapolatedPosesMock.Locate);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            var subsystem = HandTracking.subsystem;
            Assert.IsNotNull(subsystem);

            var extrapolatedHand = subsystem.GetHand(handedness, XRHandExtrapolation.Extrapolated);
            Assert.That(GetLocateMarker(extrapolatedHand),
                Is.EqualTo(UnextrapolatedPosesMock.k_ExtrapolatedMarker).Within(1e-4f),
                "The extrapolated hand should hold the data from the locate without the request.");
            Assert.IsFalse(extrapolatedHand.TryGetTimestamp(out _),
                "Only the unextrapolated hand reports a capture time.");

            var unextrapolatedHand = subsystem.GetHand(handedness, XRHandExtrapolation.Unextrapolated);
            Assert.That(GetLocateMarker(unextrapolatedHand),
                Is.EqualTo(ExpectedUnextrapolatedMarker(handedness)).Within(1e-4f),
                "The unextrapolated hand should hold the data from the request-carrying locate.");
            Assert.IsTrue(unextrapolatedHand.TryGetTimestamp(out var captureTime));
            Assert.AreEqual(UnextrapolatedPosesMock.k_CaptureTime, captureTime);

            var otherHandedness = handedness == Handedness.Left ? Handedness.Right : Handedness.Left;
            var otherUnextrapolatedHand =
                subsystem.GetHand(otherHandedness, XRHandExtrapolation.Unextrapolated);
            Assert.That(GetLocateMarker(otherUnextrapolatedHand),
                Is.Not.EqualTo(GetLocateMarker(unextrapolatedHand)).Within(1e-4f),
                "Each hand should receive its own secondary locate results.");

            const long secondCaptureTime = UnextrapolatedPosesMock.k_CaptureTime + 4242L;
            UnextrapolatedPosesMock.captureTimeToReport = secondCaptureTime;

            yield return new WaitForXrFrame(2);

            Assert.IsTrue(subsystem
                .GetHand(handedness, XRHandExtrapolation.Unextrapolated)
                .TryGetTimestamp(out var updatedCaptureTime));
            Assert.AreEqual(secondCaptureTime, updatedCaptureTime,
                "The capture time should be read every frame, not cached.");
        }

        [UnityTest]
        public IEnumerator TrackingAndCaptureTimeSurviveAFailedSecondaryLocate()
        {
            m_MockEnvironment.InstallMocks(UnextrapolatedPosesMock.Locate);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            var subsystem = HandTracking.subsystem;
            Assert.IsNotNull(subsystem);

            var leftBefore = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
            Assert.IsTrue(leftBefore.isTracked);
            Assert.IsTrue(leftBefore.TryGetTimestamp(out var captureTimeBefore));
            Assert.AreEqual(UnextrapolatedPosesMock.k_CaptureTime, captureTimeBefore);

            UnextrapolatedPosesMock.leftSecondaryLocateIsActive = false;
            UnextrapolatedPosesMock.captureTimeToReport = UnextrapolatedPosesMock.k_CaptureTime + 1111L;

            yield return new WaitForXrFrame(2);

            var leftUnextrapolated = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
            Assert.IsFalse(leftUnextrapolated.isTracked,
                "A failed secondary locate should leave the left unextrapolated hand untracked.");
            Assert.IsTrue(leftUnextrapolated.TryGetTimestamp(out var retainedCaptureTime),
                "An untracked hand keeps the capture time of its last good poses.");
            Assert.AreEqual(UnextrapolatedPosesMock.k_CaptureTime, retainedCaptureTime,
                "The retained capture time should be the last tracked one.");

            var rightUnextrapolated = subsystem.GetHand(Handedness.Right, XRHandExtrapolation.Unextrapolated);
            Assert.IsTrue(rightUnextrapolated.isTracked,
                "The right hand's secondary locate is unaffected by the left hand's failure.");
            Assert.IsTrue(rightUnextrapolated.TryGetTimestamp(out var rightCaptureTime));
            Assert.AreEqual(UnextrapolatedPosesMock.captureTimeToReport, rightCaptureTime,
                "The right hand should report its own, newer capture time.");

            foreach (var handedness in TestHandUtils.handednessOptions)
            {
                var extrapolatedHand = subsystem.GetHand(handedness, XRHandExtrapolation.Extrapolated);
                Assert.IsTrue(extrapolatedHand.isTracked,
                    $"{handedness}: the primary locate is unaffected by a secondary failure.");
                Assert.That(GetLocateMarker(extrapolatedHand),
                    Is.EqualTo(UnextrapolatedPosesMock.k_ExtrapolatedMarker).Within(1e-4f),
                    $"{handedness}: the extrapolated hand should still hold primary-locate data.");
            }
        }

        [UnityTest]
        public IEnumerator RequestIsReregisteredAfterASessionRestart()
        {
            m_MockEnvironment.InstallMocks(UnextrapolatedPosesMock.Locate);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.Greater(UnextrapolatedPosesMock.locatesCarryingRequest, 0,
                "The request should ride the secondary locate in the first session.");

            m_MockEnvironment.Stop();
            yield return null;

            UnextrapolatedPosesMock.Reset();
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.Greater(UnextrapolatedPosesMock.locatesCarryingRequest, 0,
                "HandExtensibilityManager.OnSessionEnd clears every chain, so the feature has to " +
                "re-add its nodes on each session create.");
        }
    }

    /// <summary>
    /// The same feature in an environment whose runtime does not support the extension.
    /// </summary>
    /// <remarks>
    /// A separate fixture because the supported-extension set is per-environment, and its siblings
    /// build theirs in <c>OneTimeSetUp</c>.
    /// </remarks>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class MetaHandTrackingUnextrapolatedPosesUnsupportedTests : OpenXRHandTrackingTestFixture
    {
        const int k_LocateHandJointsCallsPerFrame = 4;

        MockHandsEnvironment m_MockEnvironment;
        MetaHandTrackingUnextrapolatedPoses m_Feature;
        OpenXRSettings.LatencyOptimization m_PreviousLatencyOptimization;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();

            // Deliberately no AddSupportedExtension for the unextrapolated poses extension.
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingUnextrapolatedPoses>(true);
            m_Feature = m_MockEnvironment.Environment.Settings
                .GetFeature<MetaHandTrackingUnextrapolatedPoses>();

            m_PreviousLatencyOptimization = OpenXRSettings.Instance.latencyOptimization;
            OpenXRSettings.Instance.latencyOptimization =
                OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            OpenXRSettings.Instance.latencyOptimization = m_PreviousLatencyOptimization;
            m_MockEnvironment?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            UnextrapolatedPosesMock.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();

            UnextrapolatedPosesMock.Reset();
        }

        [UnityTest]
        public IEnumerator FeatureIsInertWhenTheRuntimeLacksTheExtension()
        {
            m_MockEnvironment.InstallMocks(UnextrapolatedPosesMock.Locate);

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            // enabled folds in failedInitialization only while a loader exists, so this has to be
            // asserted inside the session.
            Assert.IsFalse(m_Feature.enabled,
                "OnInstanceCreate should fail the feature when the extension is absent.");

            UnextrapolatedPosesMock.locateCallCount = 0;
            UnextrapolatedPosesMock.locatesCarryingRequest = 0;

            yield return new WaitForXrFrame();

            Assert.AreEqual(k_LocateHandJointsCallsPerFrame, UnextrapolatedPosesMock.locateCallCount,
                "No secondary locate should run at all. Running one with an empty input chain would " +
                "return extrapolated poses into the unextrapolated hand, with no capture time.");
            Assert.AreEqual(0, UnextrapolatedPosesMock.locatesCarryingRequest);

            var subsystem = HandTracking.subsystem;
            Assert.IsNotNull(subsystem);

            foreach (var handedness in new[] { Handedness.Left, Handedness.Right })
            {
                Assert.IsFalse(
                    subsystem.TryGetHand(handedness, XRHandExtrapolation.Unextrapolated, out _),
                    $"{handedness}: unextrapolated poses should be unavailable.");
                Assert.Throws<System.InvalidOperationException>(
                    () => subsystem.GetHand(handedness, XRHandExtrapolation.Unextrapolated));
            }
        }
    }
}
#endif
