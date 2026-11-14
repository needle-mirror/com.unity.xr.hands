#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
#if OPENXR_7_0_0_PRE_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR.MockHandsRuntime.Tests;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.ConformanceAutomation;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.NativeTypes;
using XrHandTrackerEXT = System.UInt64;

namespace UnityEngine.XR.Hands.OpenXR.Tests
{
    static class MicrogesturesMock
    {
        public static MockHandsEnvironment CreateEnvironment(bool microgesturesEnabled)
        {
            var mockEnvironment = new MockHandsEnvironment();
            mockEnvironment.SetUpDefaultHandTrackingEnvironment();

            var environment = mockEnvironment.Environment;

            // The mock always advertises XR_ANDROID_enumerate_system_extension_properties with an empty list, and
            // the Hand Interaction Profile gates on IsSystemExtensionEnabled. Without this, the base profile never
            // registers the action map the microgesture actions splice into.
            environment.SetSystemExtensionEnabled(HandInteractionProfile.extensionString, true);

            if (microgesturesEnabled)
                environment.AddSupportedExtension(HandInteractionProfileMicrogestures.extensionString, 1);

            // Set false explicitly rather than omitting it: the mock's system-extension list persists across
            // instances in the editor process, so a prior enabled fixture would otherwise leak a true.
            environment.SetSystemExtensionEnabled(HandInteractionProfileMicrogestures.extensionString, microgesturesEnabled);

            Assert.That(environment.Settings.EnableFeature<HandInteractionProfile>(true), Is.True);
            Assert.That(environment.Settings.EnableFeature<HandInteractionProfileMicrogestures>(microgesturesEnabled), Is.True);
            Assert.That(environment.Settings.EnableFeature<ConformanceAutomationFeature>(true), Is.True);

            return mockEnvironment;
        }

        public static XrResult LocateNoJoints(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations) => XrResult.Success;

        public static XRHandSubsystem GetRunningSubsystem()
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);

            for (var subsystemIdx = 0; subsystemIdx < subsystems.Count; subsystemIdx++)
            {
                if (subsystems[subsystemIdx].subsystemDescriptor.id == OpenXRHandProvider.id)
                    return subsystems[subsystemIdx];
            }

            return null;
        }

        public static XRHandSubsystemDescriptor GetRunningDescriptor() => GetRunningSubsystem()?.subsystemDescriptor;

        public static string UserPathFor(Handedness handedness)
            => handedness == Handedness.Left
                ? OpenXRInteractionFeature.UserPaths.leftHand
                : OpenXRInteractionFeature.UserPaths.rightHand;

        public static Handedness OtherHand(Handedness handedness)
            => handedness == Handedness.Left ? Handedness.Right : Handedness.Left;
    }

    /// <summary>
    /// Covers <see cref="OpenXRHandProvider"/> surfacing the microgesture actions that the
    /// Hand Interaction Profile Microgestures feature splices into the Hand Interaction Profile device.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    [Category("Level.Integration")]
    [NUnit.Framework.Property("Covers", "Interactions")]
    [NUnit.Framework.Property("Covers", "Input")]
    public class OpenXRHandProviderMicrogesturesTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = MicrogesturesMock.CreateEnvironment(true);

            // The mock environment registers the provider only once per process, so re-register for these settings.
            OpenXRHandProvider.Register();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
            OpenXRHandProvider.Register();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();
        }

        internal static readonly (string interactionPath, XRHandMicrogesture microgesture)[] k_Gestures =
        {
            (HandInteractionProfileMicrogestures.swipeLeft, XRHandMicrogesture.SwipeLeft),
            (HandInteractionProfileMicrogestures.swipeRight, XRHandMicrogesture.SwipeRight),
            (HandInteractionProfileMicrogestures.swipeForward, XRHandMicrogesture.SwipeForward),
            (HandInteractionProfileMicrogestures.swipeBackward, XRHandMicrogesture.SwipeBackward),
            (HandInteractionProfileMicrogestures.tapThumb, XRHandMicrogesture.TapThumb),
        };

        [UnityTest]
        [Description("With the Hand Interaction Profile and its Microgestures feature enabled, the running " +
                     "OpenXR hand subsystem descriptor reports supportsMicrogestures.")]
        public IEnumerator DescriptorSupportsMicrogestures()
        {
            m_MockEnvironment.InstallMocks(MicrogesturesMock.LocateNoJoints);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(2);

            var descriptor = MicrogesturesMock.GetRunningDescriptor();

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.supportsMicrogestures, Is.True);
        }

        // Loops over the gestures in one session per hand rather than parameterizing per gesture, which would
        // restart the mock runtime for each of the 10 cases.
        [UnityTest]
        [Description("Driving each microgesture action on one hand makes IsMicrogestureActive report only " +
                     "that gesture as active on that hand and no gesture as active on the other hand.")]
        public IEnumerator EachGestureActivatesOnlyItselfOnTheDrivenHand(
            [ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            m_MockEnvironment.InstallMocks(MicrogesturesMock.LocateNoJoints);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(2);

            var userPath = MicrogesturesMock.UserPathFor(handedness);
            var otherHandedness = MicrogesturesMock.OtherHand(handedness);
            var provider = new OpenXRHandProvider();

            for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
            {
                var (interactionPath, microgesture) = k_Gestures[gestureIdx];
                var fullPath = userPath + interactionPath;

                Assert.That(ConformanceAutomationFeature.ConformanceAutomationSetBool(userPath, fullPath, true), Is.True);

                yield return new WaitForXrFrame(2);

                for (var candidateIdx = 0; candidateIdx < k_Gestures.Length; candidateIdx++)
                {
                    var candidate = k_Gestures[candidateIdx].microgesture;
                    Assert.That(provider.IsMicrogestureActive(handedness, candidate), Is.EqualTo(candidate == microgesture));
                    Assert.That(provider.IsMicrogestureActive(otherHandedness, candidate), Is.False);
                }

                Assert.That(ConformanceAutomationFeature.ConformanceAutomationSetBool(userPath, fullPath, false), Is.True);

                yield return new WaitForXrFrame(2);
            }
        }

        [UnityTest]
        [Description("Driving each microgesture action on one hand makes that hand's common gestures report only " +
                     "that gesture as active, while the other hand's common gestures report no active microgesture.")]
        public IEnumerator DrivenGesturesSurfaceThroughCommonGestures(
            [ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            m_MockEnvironment.InstallMocks(MicrogesturesMock.LocateNoJoints);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(2);

            var subsystem = MicrogesturesMock.GetRunningSubsystem();
            Assert.That(subsystem, Is.Not.Null);

            var userPath = MicrogesturesMock.UserPathFor(handedness);
            var drivenGestures = subsystem.GetCommonGestures(handedness);
            var otherGestures = subsystem.GetCommonGestures(MicrogesturesMock.OtherHand(handedness));

            for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
            {
                var (interactionPath, microgesture) = k_Gestures[gestureIdx];
                var fullPath = userPath + interactionPath;

                Assert.That(ConformanceAutomationFeature.ConformanceAutomationSetBool(userPath, fullPath, true), Is.True);

                yield return new WaitForXrFrame(2);

                for (var candidateIdx = 0; candidateIdx < k_Gestures.Length; candidateIdx++)
                {
                    var candidate = k_Gestures[candidateIdx].microgesture;

                    Assert.That(drivenGestures.IsMicrogestureActive(candidate), Is.EqualTo(candidate == microgesture), $"{microgesture} driven, checking {candidate}");
                    Assert.That(otherGestures.IsMicrogestureActive(candidate), Is.False, $"{microgesture} driven, checking {candidate} on the other hand");
                }

                Assert.That(ConformanceAutomationFeature.ConformanceAutomationSetBool(userPath, fullPath, false), Is.True);

                yield return new WaitForXrFrame(2);
            }
        }
    }

    /// <summary>
    /// The same provider with the Hand Interaction Profile Microgestures feature disabled.
    /// </summary>
    /// <remarks>
    /// A separate fixture because the supported-extension set is per-environment, and its sibling
    /// builds its own in <c>OneTimeSetUp</c>.
    /// </remarks>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    [Category("Level.Integration")]
    [NUnit.Framework.Property("Covers", "Interactions")]
    [NUnit.Framework.Property("Covers", "Input")]
    public class OpenXRHandProviderMicrogesturesDisabledTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = MicrogesturesMock.CreateEnvironment(false);

            // The mock environment registers the provider only once per process, so re-register for these settings.
            OpenXRHandProvider.Register();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
            OpenXRHandProvider.Register();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            m_MockEnvironment?.ClearMocks();
        }

        [UnityTest]
        [Description("With the Hand Interaction Profile enabled but its Microgestures feature disabled, the " +
                     "running OpenXR hand subsystem descriptor does not report supportsMicrogestures.")]
        public IEnumerator DescriptorDoesNotSupportMicrogestures()
        {
            m_MockEnvironment.InstallMocks(MicrogesturesMock.LocateNoJoints);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(2);

            var descriptor = MicrogesturesMock.GetRunningDescriptor();

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.supportsMicrogestures, Is.False);
        }

        [UnityTest]
        [Description("With the Microgestures feature disabled, IsMicrogestureActive returns false for every " +
                     "microgesture.")]
        public IEnumerator IsMicrogestureActiveIsFalseWithoutTheFeature(
            [ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            m_MockEnvironment.InstallMocks(MicrogesturesMock.LocateNoJoints);
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame(2);

            var provider = new OpenXRHandProvider();

            for (var gestureIdx = 0; gestureIdx < OpenXRHandProviderMicrogesturesTests.k_Gestures.Length; gestureIdx++)
            {
                Assert.That(provider.IsMicrogestureActive(handedness, OpenXRHandProviderMicrogesturesTests.k_Gestures[gestureIdx].microgesture), Is.False);
            }
        }
    }
}
#endif
