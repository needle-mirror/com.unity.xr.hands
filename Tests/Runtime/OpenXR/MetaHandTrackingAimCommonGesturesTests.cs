#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING && ENABLE_INPUT_SYSTEM
using System.Collections;
using System.Runtime.InteropServices;
using NUnit.Framework;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.Hands.Configuration;
using UnityEngine.XR.Hands.OpenXR.MockHandsRuntime.Tests;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;

namespace UnityEngine.XR.Hands.OpenXR.Tests
{
    static class MetaAimMock
    {
        /// <summary>
        /// Mirrors <c>XrHandTrackingAimStateFB</c> from <c>openxr.h</c>, which has no managed
        /// counterpart in the OpenXR package (i.e. in the UnityEngine.XR.OpenXR.NativeTypes namespace).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        unsafe struct XrHandTrackingAimStateFB
        {
            public XrStructureType type;
            public void* next;
            public MetaAimFlags status;
            public XrPosef aimPose;
            public float pinchStrengthIndex;
            public float pinchStrengthMiddle;
            public float pinchStrengthRing;
            public float pinchStrengthLittle;
        }

        // Joint data has to be written for the hand to be reported as tracked, which is what lets the
        // Meta aim state through. Kept well away from the aim poses so the two can't be confused.
        const float k_Marker = 0.1024f;

        static readonly Pose s_LeftAimPose =
            new Pose(new Vector3(-0.21f, 1.32f, 0.43f), Quaternion.Euler(12f, -24f, 6f));

        static readonly Pose s_RightAimPose =
            new Pose(new Vector3(0.27f, 1.18f, 0.51f), Quaternion.Euler(-8f, 31f, -4f));

        public static int aimStatesWritten { get; private set; }

        public static void Reset() => aimStatesWritten = 0;

        public static Pose AimPoseFor(Handedness handedness)
            => handedness == Handedness.Left ? s_LeftAimPose : s_RightAimPose;

        /// <summary>
        /// Answers a locate call with a tracked hand and, when the native plugin chained one on,
        /// fills out the <see cref="XrHandTrackingAimStateFB"/> with a per-hand aim pose.
        /// </summary>
        /// <param name="tracker">The hand tracker handle.</param>
        /// <param name="locateInfo">The locate info the runtime was handed.</param>
        /// <param name="locations">The locations to fill out.</param>
        /// <returns>Returns <c>XrResult.Success</c>.</returns>
        public static unsafe XrResult Locate(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            var handedness = tracker == MockHandsEnvironment.k_LeftTrackerHandle
                ? Handedness.Left
                : tracker == MockHandsEnvironment.k_RightTrackerHandle
                    ? Handedness.Right
                    : Handedness.Invalid;

            MockHandJoints.WriteTrackedHand(locations.jointLocations, k_Marker);

            var current = (XrBaseInStructure*)locations.next;
            while (current != null)
            {
                if (current->type == XrStructureType.HandTrackingAimStateFB)
                {
                    var aimState = (XrHandTrackingAimStateFB*)current;
                    aimState->status = MetaAimFlags.Computed | MetaAimFlags.Valid;

                    // XrPosef constructor converts into OpenXR space, so the native plugin
                    // receives runtime-space data as it would from a real runtime.
                    aimState->aimPose = new XrPosef(AimPoseFor(handedness));
                    ++aimStatesWritten;
                    break;
                }

                current = (XrBaseInStructure*)current->next;
            }

            locations = new XrHandJointLocationsEXT(
                locations.next, true, MockHandJoints.k_JointCount, locations.jointLocations);
            return XrResult.Success;
        }
    }

    /// <summary>
    /// Covers surfacing the aim pose through <see cref="XRCommonHandGestures"/> and
    /// <see cref="XRHandDevice"/> when <see cref="MetaHandTrackingAim"/> is enabled but the
    /// Hand Interaction Profile is not.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class MetaHandTrackingAimCommonGesturesTests : OpenXRHandTrackingTestFixture
    {
        // Current version of XR_FB_hand_tracking_aim in the OpenXR spec.
        const uint k_MetaHandTrackingAimExtensionVersion = 2;

        static readonly XRHandDevicePoseSource[] s_PoseSourceOptions =
        {
            XRHandDevicePoseSource.LegacyJointRecognition,
            XRHandDevicePoseSource.CommonGestures,
        };

        MockHandsEnvironment m_MockEnvironment;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.Environment.AddSupportedExtension(
                MetaHandTrackingAim.extensionString, k_MetaHandTrackingAimExtensionVersion);

            // Explicitly off, so the aim pose can only come from the Meta aim fallback.
            m_MockEnvironment.Environment.Settings.EnableFeature<HandInteractionProfile>(false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            MetaAimMock.Reset();
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingAim>(true);
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();

            MetaAimMock.Reset();
        }

        [UnityTest]
        public IEnumerator AimPoseIsFilledFromMetaAimWithoutHandInteractionProfile([ValueSource(nameof(s_PoseSourceOptions))] XRHandDevicePoseSource poseSource)
        {
            m_MockEnvironment.InstallMocks(MetaAimMock.Locate);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(OpenXRRuntime.IsExtensionEnabled(MetaHandTrackingAim.extensionString), Is.True,
                "The Meta Hand Tracking Aim extension should be enabled for this test.");
            Assert.That(OpenXRRuntime.IsExtensionEnabled(HandInteractionProfile.extensionString), Is.False,
                "The Hand Interaction Profile extension must be disabled for this test.");

            var subsystem = HandTracking.subsystem;
            Assert.That(subsystem, Is.Not.Null);

            UpdateHandsConfiguration(subsystem, poseSource);

            yield return new WaitForXrFrame(2);

            Assert.That(MetaAimMock.aimStatesWritten, Is.GreaterThan(0),
                "The native plugin should chain XrHandTrackingAimStateFB onto the locate call.");

            foreach (var handedness in TestHandUtils.handednessOptions)
            {
                var expectedAimPose = MetaAimMock.AimPoseFor(handedness);

                var commonGestures = subsystem.GetCommonGestures(handedness);
                Assert.That(commonGestures.TryGetAimPose(out var commonGesturesAimPose), Is.True,
                    $"{handedness}: The aim pose should be valid from the Meta aim fallback.");
                AssertPoseEqual(expectedAimPose, commonGesturesAimPose,
                    $"{handedness}: The common gestures aim pose should be the Meta aim pose.");
                Assert.That(commonGestures.GetAimPoseIsTracked(), Is.True);
                Assert.That(commonGestures.TryGetGripPose(out _), Is.False,
                    $"{handedness}: Only the aim pose is backed by Meta aim; the grip pose still needs the Hand Interaction Profile.");

                var handDevice = GetHandDevice(handedness);
                Assert.That(handDevice, Is.Not.Null, $"The {handedness} XRHandDevice should have been created.");
                Assert.That((InputTrackingState)handDevice.aimTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
                Assert.That(handDevice.aimIsTracked.IsPressed(), Is.True);
                AssertPoseEqual(expectedAimPose,
                    new Pose(handDevice.aimPosition.ReadValue(), handDevice.aimRotation.ReadValue()),
                    $"{handedness}: The XRHandDevice aim pose should be the Meta aim pose.");
            }
        }

        [UnityTest]
        public IEnumerator AimPoseIsUnavailableWithoutMetaAimOrHandInteractionProfile([ValueSource(nameof(s_PoseSourceOptions))] XRHandDevicePoseSource poseSource)
        {
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingAim>(false);

            m_MockEnvironment.InstallMocks(MetaAimMock.Locate);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            var subsystem = HandTracking.subsystem;
            Assert.That(subsystem, Is.Not.Null);

            UpdateHandsConfiguration(subsystem, poseSource);

            yield return new WaitForXrFrame(2);

            Assert.That(MetaAimMock.aimStatesWritten, Is.EqualTo(0),
                "The native plugin should not request the Meta aim state when the feature is disabled.");

            foreach (var handedness in TestHandUtils.handednessOptions)
            {
                Assert.That(subsystem.GetCommonGestures(handedness).TryGetAimPose(out _), Is.False,
                    $"{handedness}: no feature provides an aim pose.");

                var handDevice = GetHandDevice(handedness);
                Assert.That(handDevice, Is.Not.Null, $"The {handedness} XRHandDevice should have been created.");
                Assert.That((InputTrackingState)handDevice.aimTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None),
                    $"{handedness}: the XRHandDevice aim pose should not be valid.");
            }
        }

        static void UpdateHandsConfiguration(XRHandSubsystem handSubsystem, XRHandDevicePoseSource poseSource)
        {
            var config = handSubsystem.handSubsystemConfiguration;
            config.xrHandDevicePoseSource = poseSource;

            handSubsystem.UpdateHandsConfiguration(config);
        }

        static XRHandDevice GetHandDevice(Handedness handedness)
            => handedness == Handedness.Left ? XRHandDevice.leftHand : XRHandDevice.rightHand;

        static void AssertPoseEqual(Pose expected, Pose actual, string message)
        {
            Assert.That(actual.position, Is.EqualTo(expected.position).Using(Vector3ComparerWithEqualsOperator.Instance),
                $"{message} Position: expected {expected.position}, was {actual.position}.");
            Assert.That(actual.rotation, Is.EqualTo(expected.rotation).Using(QuaternionEqualityComparer.Instance),
                $"{message} Rotation: expected {expected.rotation}, was {actual.rotation}.");
        }
    }
}
#endif
