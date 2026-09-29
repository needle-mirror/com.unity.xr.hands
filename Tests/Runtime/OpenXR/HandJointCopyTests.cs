#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR.MockHandsRuntime.Tests;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using Is = Unity.XR.Hands.Tests.NUnitExtensions.Is;
using XrHandTrackerEXT = System.UInt64;

namespace UnityEngine.XR.Hands.OpenXR.Tests
{
    /// <summary>
    /// Covers what the primary <c>xrLocateHandJointsEXT</c> call writes into a hand: the joint index
    /// mapping, the coordinate conversion, radius, and high-fidelity state.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class HandJointCopyTests : OpenXRHandTrackingTestFixture
    {
        const float k_Marker = 0.1024f;

        // Valid but untracked, so the plugin should surface a pose without high fidelity.
        static readonly int k_LowFidelityOpenXRJointIndex =
            MockHandJoints.ToOpenXRJointIndex(XRHandJointID.MiddleTip.ToIndex());

        MockHandsEnvironment m_MockEnvironment;
        OpenXRSettings.LatencyOptimization m_PreviousLatencyOptimization;

        static bool s_MockLocateIsActive = true;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();

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
            s_MockLocateIsActive = true;
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();

            s_MockLocateIsActive = true;
        }

        static unsafe XrResult MockLocateWithJointData(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            if (s_MockLocateIsActive)
            {
                MockHandJoints.WriteTrackedHand(
                    locations.jointLocations, k_Marker, k_LowFidelityOpenXRJointIndex);
            }

            locations = new XrHandJointLocationsEXT(
                locations.next, s_MockLocateIsActive, MockHandJoints.k_JointCount, locations.jointLocations);
            return XrResult.Success;
        }

        [UnityTest]
        public IEnumerator PrimaryLocateCopiesJointData()
        {
            m_MockEnvironment.InstallMocks(MockLocateWithJointData);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            var subsystem = HandTracking.subsystem;
            Assert.IsNotNull(subsystem, "The hand subsystem should exist once the session is running.");

            var hand = subsystem.GetHand(Handedness.Left);
            Assert.IsTrue(hand.isTracked, "A locate that reports active joints should track the hand.");

            for (var jointIndex = XRHandJointID.BeginMarker.ToIndex();
                 jointIndex < XRHandJointID.EndMarker.ToIndex();
                 ++jointIndex)
            {
                var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
                var joint = hand.GetJoint(jointId);
                var openXRJointIndex = MockHandJoints.ToOpenXRJointIndex(jointIndex);
                var expectedPose = MockHandJoints.UnityPoseFor(openXRJointIndex, k_Marker);

                Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(joint.TryGetPose),
                    Is.EqualTo((true, expectedPose)),
                    $"{jointId}: the pose should survive the round trip through OpenXR space.");

                Assert.That(TestHandUtils.InvokeTryGetFunc<float>(joint.TryGetRadius),
                    Is.EqualTo((true, MockHandJoints.k_Radius)), $"{jointId}: radius.");

                var isHighFidelity =
                    (joint.trackingState & XRHandJointTrackingState.HighFidelityPose) != 0;
                Assert.AreEqual(
                    openXRJointIndex != k_LowFidelityOpenXRJointIndex,
                    isHighFidelity,
                    $"{jointId}: high fidelity requires all four valid and tracked bits.");
            }

            var wristPose = MockHandJoints.UnityPoseFor(
                MockHandJoints.ToOpenXRJointIndex(XRHandJointID.Wrist.ToIndex()), k_Marker);
            TestHandUtils.AssertAreApproximatelyEqual(wristPose.position, hand.rootPose.position,
                "The root pose should come from the wrist joint.");
        }

        [UnityTest]
        public IEnumerator InactiveLocateLeavesTheHandUntracked()
        {
            m_MockEnvironment.InstallMocks(MockLocateWithJointData);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            var subsystem = HandTracking.subsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.GetHand(Handedness.Left).isTracked);

            s_MockLocateIsActive = false;

            yield return new WaitForXrFrame(2);

            var hand = subsystem.GetHand(Handedness.Left);
            Assert.IsFalse(hand.isTracked, "An inactive locate should leave the hand untracked.");

            var wristJoint = hand.GetJoint(XRHandJointID.Wrist);
            Assert.IsTrue(wristJoint.TryGetPose(out var retainedPose),
                "The joints from the last good locate should survive an inactive one.");
            var expectedPose = MockHandJoints.UnityPoseFor(
                MockHandJoints.ToOpenXRJointIndex(XRHandJointID.Wrist.ToIndex()), k_Marker);
            Assert.That(retainedPose.position.y, Is.EqualTo(expectedPose.position.y).Within(1e-4f),
                "The retained pose should still be the last good one.");
        }
    }
}
#endif
