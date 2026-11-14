using NUnit.Framework;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Recording;
using UnityEngine;

class XRHandCaptureTests
{
    [Test]
    public void TestXRHandRecordingStatusChangedEventArgsEquality()
    {
        TestHandUtils.CreateTwoTestSubsystems(out var testSubsystem1, out var testSubsystem2);

        var recordingName1 = "TestRecording";
        var recordingName2 = "DifferentRecording";

        var errorMessage1 = "Test error message";
        var errorMessage2 = "Different error message";

        var elapsedTime1 = 1.5f;
        var elapsedTime2 = 2.0f;

        var args1 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };

        var args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };

        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.StoppedManually,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime2,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName2,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem2,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = 1.5f,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage2,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }

    [Test]
    public void TestXRHandCapturedFrameEquality()
    {
        XRHandCaptureFrame frame1 = new XRHandCaptureFrame();
        XRHandCaptureFrame frame2 = new XRHandCaptureFrame();
        Assert.IsTrue(frame1.Equals(frame2));
        Assert.IsTrue(frame1 == frame2);
        Assert.IsFalse(frame1 != frame2);

        frame1.timestamp = 1.0f;
        frame2.timestamp = 2.0f;
        Assert.IsFalse(frame1.Equals(frame2));
        Assert.IsFalse(frame1 == frame2);
        Assert.IsTrue(frame1 != frame2);
    }

    [Test]
    public void TestXRHandRecordingInitializeArgsEquality()
    {
        XRHandRecordingInitializeArgs args1 = new XRHandRecordingInitializeArgs();
        XRHandRecordingInitializeArgs args2 = new XRHandRecordingInitializeArgs();
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        TestHandUtils.CreateTwoTestSubsystems(out var testSubsystem1, out var testSubsystem2);
        args1 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem1 };
        args2 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem1 };
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem2 };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingInitializeArgs { subsystem = null };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }

    [Test]
    public void TestXRHandRecordingSaveArgsEquality()
    {
        XRHandRecordingSaveArgs args1 = new XRHandRecordingSaveArgs();
        XRHandRecordingSaveArgs args2 = new XRHandRecordingSaveArgs();
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        var recordingName1 = "TestRecording";
        var recordingName2 = "DifferentRecording";

        args1 = new XRHandRecordingSaveArgs { recordingName = recordingName1 };
        args2 = new XRHandRecordingSaveArgs { recordingName = recordingName1 };
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingSaveArgs { recordingName = recordingName2 };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingSaveArgs { recordingName = null };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }

    // NOTE: This test *must* be updated when our stance on microgesture support in captures changes. But in the
    // meantime, this makes sure we don't accidentally leak the bits into a sequence.
    [Test]
    [Category("Level.Integration")]
    [NUnit.Framework.Property("Covers", "Input")]
    [Description("Capturing a frame from a subsystem reporting microgestures drops the microgesture data while " +
                 "keeping the other common gestures: the live state reports the microgesture, the captured " +
                 "microgesture mask is empty, and the captured flags and frame flag match the live state.")]
    public void CapturedFrameDoesNotIncludeMicrogestures()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem(new XRHandSubsystemDescriptor.Cinfo
        {
            id = "TestHandProviderMicrogestures",
            providerType = typeof(TestHandProvider),
            supportsMicrogestures = true,
        });
        var frameBuffer = new FrameBuffer(0f, XRHandRecordingOptions.None);
        try
        {
            subsystem.Start();

            var provider = (TestHandProvider)subsystem.GetProvider();
            provider.commonGestureBehavior = TestHandProvider.CommonGestureBehavior.CorePosesOnly;
            provider.microgesturesEnabled = true;
            provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.TapThumb;

            subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

            var liveState = subsystem.leftHandCommonGestures.stateInternal;
            Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(XRHandMicrogesture.TapThumb), Is.True);
            Assert.That(liveState.microgesturesInternal, Is.EqualTo(XRHandMicrogestureFlags.TapThumb));

            frameBuffer.CaptureUpdateTypeAgnosticData(subsystem);

            var capturedState = frameBuffer.m_CommonGestures[Handedness.Left.ToIndex()];
            Assert.That(frameBuffer.m_FrameFlags & Handedness.Left.ToFrameFlagForCommonGestures(),
                Is.EqualTo(Handedness.Left.ToFrameFlagForCommonGestures()));
            Assert.That(capturedState.microgesturesInternal, Is.EqualTo(XRHandMicrogestureFlags.None));
            Assert.That(capturedState.flags, Is.EqualTo(liveState.flags));
        }
        finally
        {
            frameBuffer.Dispose();
            subsystem.Destroy();
        }
    }

    [Test]
    public void TestMetaAimFlagsRoundTripThroughXRHandAimState()
    {
        // Verify that MetaAimFlags survive the round-trip:
        //   XRHandAimState.UpdateToAimRepresentation -> MetaAimHandState(in XRHandAimState)
        // The reserved0/reserved1 fields must encode low/high 32 bits consistently.
        // The (1 << 40) is a dummy flag that will be set in the upper 32-bits.
        const MetaAimFlags testFlags = MetaAimFlags.Computed | MetaAimFlags.Valid | MetaAimFlags.IndexPinching |
            MetaAimFlags.DominantHand | MetaAimFlags.MenuPressed | (MetaAimFlags)(1UL << 40);

        var aimState = new XRHandAimState();
        aimState.UpdateToAimRepresentation(
            Handedness.Left,
            true,
            testFlags,
            Pose.identity,
            pinchIndex: 0.5f,
            pinchMiddle: 0.25f,
            pinchRing: 0.1f,
            pinchLittle: 0.75f);

        // Ensure low and high 32-bits were correctly separated
        Assert.That(aimState.reserved0, Is.EqualTo(unchecked((int)testFlags)), "Low 32 bits incorrect.");
        Assert.That(aimState.reserved1, Is.EqualTo(1 << (40 - 32)), "High 32 bits incorrect.");

        var metaState = new MetaAimHandState(in aimState);
        Assert.That(metaState.aimFlags, Is.EqualTo(testFlags),
            "MetaAimFlags must survive the XRHandAimState -> MetaAimHandState round-trip without corruption.");
        Assert.That(metaState.handedness, Is.EqualTo(Handedness.Left));
        Assert.That(metaState.pinchStrengthIndex, Is.EqualTo(0.5f).Within(0.0001f));
    }
}
