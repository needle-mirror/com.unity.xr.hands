using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Configuration;

namespace Unity.XR.Hands.Runtime.Tests
{
    [TestFixture]
    class XRHandDeviceMicrogestureControlTests
    {
        const float k_Diagonal = 0.70710678f;
        const float k_Tolerance = 1e-5f;
        const string k_MicrogestureSubsystemId = "TestHandProviderMicrogestures";
        const XRHandMicrogestureFlags k_ActiveMicrogestures =
            XRHandMicrogestureFlags.SwipeLeft | XRHandMicrogestureFlags.SwipeForward | XRHandMicrogestureFlags.TapThumb;

        static readonly XRHandMicrogesture[] k_Gestures =
        {
            XRHandMicrogesture.SwipeLeft,
            XRHandMicrogesture.SwipeRight,
            XRHandMicrogesture.SwipeForward,
            XRHandMicrogesture.SwipeBackward,
            XRHandMicrogesture.TapThumb,
        };

        static readonly XRHandDevicePoseSource[] k_PoseSources =
        {
            XRHandDevicePoseSource.LegacyJointRecognition,
            XRHandDevicePoseSource.CommonGestures,
        };

        readonly struct MicrogestureCase
        {
            public readonly string name;
            public readonly byte microgestures;
            public readonly float expectedX;
            public readonly float expectedY;
            public readonly string[] expectedPressedChildren;
            public readonly bool expectedTapPressed;

            public MicrogestureCase(string name, byte microgestures, float expectedX, float expectedY, string[] expectedPressedChildren, bool expectedTapPressed)
            {
                this.name = name;
                this.microgestures = microgestures;
                this.expectedX = expectedX;
                this.expectedY = expectedY;
                this.expectedPressedChildren = expectedPressedChildren;
                this.expectedTapPressed = expectedTapPressed;
            }
        }

        static readonly MicrogestureCase[] k_Cases =
        {
            new MicrogestureCase("SwipeLeft", (byte)XRHandMicrogestureFlags.SwipeLeft, -1f, 0f, new[] { "swipeLeft" }, false),
            new MicrogestureCase("SwipeRight", (byte)XRHandMicrogestureFlags.SwipeRight, 1f, 0f, new[] { "swipeRight" }, false),
            new MicrogestureCase("SwipeForward", (byte)XRHandMicrogestureFlags.SwipeForward, 0f, 1f, new[] { "swipeForward" }, false),
            new MicrogestureCase("SwipeBackward", (byte)XRHandMicrogestureFlags.SwipeBackward, 0f, -1f, new[] { "swipeBackward" }, false),
            new MicrogestureCase("None after SwipeBackward", (byte)XRHandMicrogestureFlags.None, 0f, 0f, System.Array.Empty<string>(), false),
            new MicrogestureCase("SwipeLeft|SwipeForward", (byte)(XRHandMicrogestureFlags.SwipeLeft | XRHandMicrogestureFlags.SwipeForward), -k_Diagonal, k_Diagonal, new[] { "swipeLeft", "swipeForward" }, false),
            new MicrogestureCase("TapThumb", (byte)XRHandMicrogestureFlags.TapThumb, 0f, 0f, System.Array.Empty<string>(), true),
            new MicrogestureCase("SwipeLeft|TapThumb", (byte)(XRHandMicrogestureFlags.SwipeLeft | XRHandMicrogestureFlags.TapThumb), -1f, 0f, new[] { "swipeLeft" }, true),
            new MicrogestureCase("None after SwipeLeft|TapThumb", (byte)XRHandMicrogestureFlags.None, 0f, 0f, System.Array.Empty<string>(), false),
        };

        static readonly Vector2[] k_SwipeValues =
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down,
            Vector2.zero,
        };

        static readonly string[] k_SwipeValuePressedChildren =
        {
            "swipeLeft",
            "swipeRight",
            "swipeForward",
            "swipeBackward",
            null,
        };

        static readonly MicrogestureCase k_ActiveCase =
            new MicrogestureCase("Active", (byte)k_ActiveMicrogestures, -k_Diagonal, k_Diagonal, new[] { "swipeLeft", "swipeForward" }, true);

        static readonly MicrogestureCase k_ReleasedCase =
            new MicrogestureCase("None", (byte)XRHandMicrogestureFlags.None, 0f, 0f, System.Array.Empty<string>(), false);

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void MicrogestureBitsReadAsExpected()
        {
            var device = InputSystem.AddDevice<XRHandDevice>();
            try
            {
                var swipeChildren = GetSwipeChildren(device);

                for (var caseIdx = 0; caseIdx < k_Cases.Length; caseIdx++)
                {
                    var testCase = k_Cases[caseIdx];
                    InputSystem.QueueStateEvent(device, new XRHandDeviceState { microgestures = testCase.microgestures });
                    InputSystem.Update();

                    AssertMicrogestureControls(device, swipeChildren, testCase, testCase.name);
                }
            }
            finally
            {
                InputSystem.RemoveDevice(device);
            }
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void SwipeValueWriteRoundTrips()
        {
            var device = InputSystem.AddDevice<XRHandDevice>();
            try
            {
                var swipeChildren = GetSwipeChildren(device);
                using (StateEvent.From(device, out var tapEventPtr))
                {
                    device.microgestureTapThumb.WriteValueIntoEvent(1f, tapEventPtr);
                    InputSystem.QueueEvent(tapEventPtr);
                }

                InputSystem.Update();

                for (var valueIndex = 0; valueIndex < k_SwipeValues.Length; valueIndex++)
                {
                    var value = k_SwipeValues[valueIndex];
                    using (StateEvent.From(device, out var swipeEventPtr))
                    {
                        device.microgestureSwipe.WriteValueIntoEvent(value, swipeEventPtr);
                        InputSystem.QueueEvent(swipeEventPtr);
                    }

                    InputSystem.Update();

                    Assert.That(device.microgestureSwipe.ReadValue(), Is.EqualTo(value));
                    for (var childIndex = 0; childIndex < swipeChildren.Length; childIndex++)
                    {
                        var swipeChild = swipeChildren[childIndex];
                        var expectedPressed = swipeChild.name == k_SwipeValuePressedChildren[valueIndex];
                        Assert.That(swipeChild.isPressed, Is.EqualTo(expectedPressed), value + ": " + swipeChild.name);
                    }

                    Assert.That(device.microgestureTapThumb.isPressed, Is.True, value.ToString());
                }
            }
            finally
            {
                InputSystem.RemoveDevice(device);
            }
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void SwipeAxisWriteThrows()
        {
            var device = InputSystem.AddDevice<XRHandDevice>();
            try
            {
                using (StateEvent.From(device, out var eventPtr))
                {
                    Assert.Throws<NotSupportedException>(() => device.microgestureSwipe.x.WriteValueIntoEvent(1f, eventPtr));
                }
            }
            finally
            {
                InputSystem.RemoveDevice(device);
            }
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void SubsystemMicrogesturesDriveDeviceControls([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            var subsystem = CreateMicrogestureCapableSubsystem();
            XRHandDevice device = null;
            try
            {
                var provider = StartWithMicrogestures(subsystem);
                device = XRHandDevice.Create(subsystem, handedness, XRHandSubsystem.UpdateSuccessFlags.None, XRHandSubsystem.UpdateType.Dynamic);
                var swipeChildren = GetSwipeChildren(device);

                for (var poseSourceIdx = 0; poseSourceIdx < k_PoseSources.Length; poseSourceIdx++)
                {
                    var poseSource = k_PoseSources[poseSourceIdx];
                    subsystem.UpdateHandsConfiguration(new XRHandSubsystemConfiguration { xrHandDevicePoseSource = poseSource });

                    for (var caseIdx = 0; caseIdx < k_Cases.Length; caseIdx++)
                    {
                        var testCase = k_Cases[caseIdx];
                        SetReportedMicrogestures(provider, handedness, (XRHandMicrogestureFlags)testCase.microgestures);
                        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                        InputSystem.Update();

                        AssertMicrogestureControls(device, swipeChildren, testCase, poseSource + " " + testCase.name);
                    }
                }
            }
            finally
            {
                if (device != null)
                    InputSystem.RemoveDevice(device);

                subsystem.Destroy();
            }
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void TrackingLossDoesNotReleaseDeviceMicrogestures([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            AssertMicrogesturesWhenUnavailable(handedness, true, (provider, isAvailable) =>
            {
                if (handedness == Handedness.Left)
                    provider.leftHandIsTracked = isAvailable;
                else
                    provider.rightHandIsTracked = isAvailable;
            });
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void UnsupportedMicrogesturesReleaseDeviceMicrogestures([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
        {
            AssertMicrogesturesWhenUnavailable(handedness, false, (provider, isAvailable) => provider.microgesturesEnabled = isAvailable);
        }

        static void AssertMicrogesturesWhenUnavailable(Handedness handedness, bool expectStillReported, Action<TestHandProvider, bool> setAvailable)
        {
            var subsystem = CreateMicrogestureCapableSubsystem();
            XRHandDevice device = null;
            try
            {
                var provider = StartWithMicrogestures(subsystem);
                device = XRHandDevice.Create(subsystem, handedness, XRHandSubsystem.UpdateSuccessFlags.None, XRHandSubsystem.UpdateType.Dynamic);
                var swipeChildren = GetSwipeChildren(device);

                for (var poseSourceIdx = 0; poseSourceIdx < k_PoseSources.Length; poseSourceIdx++)
                {
                    var poseSource = k_PoseSources[poseSourceIdx];
                    subsystem.UpdateHandsConfiguration(new XRHandSubsystemConfiguration { xrHandDevicePoseSource = poseSource });

                    setAvailable(provider, true);
                    SetReportedMicrogestures(provider, handedness, k_ActiveMicrogestures);
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                    InputSystem.Update();
                    AssertMicrogestureControls(device, swipeChildren, k_ActiveCase, poseSource + " available");

                    setAvailable(provider, false);
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                    InputSystem.Update();

                    if (expectStillReported)
                    {
                        var commonGestures = subsystem.GetCommonGestures(handedness);
                        for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
                        {
                            var gesture = k_Gestures[gestureIdx];
                            Assert.That(commonGestures.IsMicrogestureActive(gesture), Is.EqualTo((k_ActiveMicrogestures & gesture.ToMicrogestureFlag()) != 0), poseSource + " common gestures " + gesture);
                        }
                    }

                    // The device mirrors common gestures, which keep the last mask while the hand is untracked.
                    AssertMicrogestureControls(device, swipeChildren, expectStillReported ? k_ActiveCase : k_ReleasedCase, poseSource + " unavailable");
                }
            }
            finally
            {
                if (device != null)
                    InputSystem.RemoveDevice(device);

                subsystem.Destroy();
            }
        }

        static XRHandSubsystem CreateMicrogestureCapableSubsystem()
        {
            var cinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = k_MicrogestureSubsystemId,
                providerType = typeof(TestHandProvider),
                supportsMicrogestures = true,
            };
            return TestHandUtils.CreateTestSubsystem(cinfo);
        }

        static TestHandProvider StartWithMicrogestures(XRHandSubsystem subsystem)
        {
            subsystem.Start();

            var provider = (TestHandProvider)subsystem.GetProvider();
            provider.commonGestureBehavior = TestHandProvider.CommonGestureBehavior.CorePosesOnly;
            provider.microgesturesEnabled = true;
            return provider;
        }

        static void SetReportedMicrogestures(TestHandProvider provider, Handedness handedness, XRHandMicrogestureFlags microgestures)
        {
            // The other hand reports microgestures so a cross-hand leak shows up on the device under test.
            if (handedness == Handedness.Left)
            {
                provider.leftHandMicrogesturesToReport = microgestures;
                provider.rightHandMicrogesturesToReport = k_ActiveMicrogestures;
            }
            else
            {
                provider.leftHandMicrogesturesToReport = k_ActiveMicrogestures;
                provider.rightHandMicrogesturesToReport = microgestures;
            }
        }

        static void AssertMicrogestureControls(XRHandDevice device, ButtonControl[] swipeChildren, MicrogestureCase testCase, string context)
        {
            Assert.That(device.microgestureSwipe.ReadValue().x, Is.EqualTo(testCase.expectedX).Within(k_Tolerance), context);
            Assert.That(device.microgestureSwipe.ReadValue().y, Is.EqualTo(testCase.expectedY).Within(k_Tolerance), context);
            Assert.That(device.microgestureSwipe.x.ReadValue(), Is.EqualTo(testCase.expectedX).Within(k_Tolerance), context);
            Assert.That(device.microgestureSwipe.y.ReadValue(), Is.EqualTo(testCase.expectedY).Within(k_Tolerance), context);
            for (var childIdx = 0; childIdx < swipeChildren.Length; childIdx++)
            {
                var swipeChild = swipeChildren[childIdx];
                var expectedPressed = Array.IndexOf(testCase.expectedPressedChildren, swipeChild.name) >= 0;
                Assert.That(swipeChild.isPressed, Is.EqualTo(expectedPressed), context + ": " + swipeChild.name);
            }

            Assert.That(device.microgestureTapThumb.isPressed, Is.EqualTo(testCase.expectedTapPressed), context);
        }

        static ButtonControl[] GetSwipeChildren(XRHandDevice device)
        {
            return new[]
            {
                device.microgestureSwipe.swipeLeft,
                device.microgestureSwipe.swipeRight,
                device.microgestureSwipe.swipeForward,
                device.microgestureSwipe.swipeBackward,
            };
        }
    }
}
