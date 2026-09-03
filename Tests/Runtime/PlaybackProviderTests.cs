using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Playback;
using UnityEngine.XR.Hands.Capture.Recording;

class PlaybackProviderTests
{
    static readonly Handedness[] s_HandednessOptions =
    {
        Handedness.Left,
        Handedness.Right,
    };

    readonly List<XRHandCaptureSequence> m_CreatedSequences = new List<XRHandCaptureSequence>();

    [TearDown]
    public void TearDown()
    {
        foreach (var sequence in m_CreatedSequences)
        {
            if (sequence != null)
                Object.DestroyImmediate(sequence);
        }
        m_CreatedSequences.Clear();
    }

    // Guards against a regression where the PlaybackProvider forwarded most of the
    // common pose data to the subsystem but silently dropped some of it. The bool
    // states (TryGetAimActivatedState, TryGetGraspFirmState, TryGetPinchTouchedState)
    // were previously not implemented, so they fell through to the base implementation
    // and always reported failure with a default value.
    [Test]
    public void ProviderSurfacesAllCommonPoseData()
    {
        var subsystem = TestHandUtils.CreatePlaybackProviderTestSubsystem();
        Assert.That(subsystem, Is.Not.Null);

        var provider = subsystem.GetProvider() as PlaybackProvider;
        Assert.That(provider, Is.Not.Null);
        Assume.That(provider.canSurfaceCommonPoseData, Is.True);
        var descriptor = subsystem.subsystemDescriptor;
        Assume.That(descriptor.supportsAimPose, Is.True);
        Assume.That(descriptor.supportsAimActivateValue, Is.True);
        Assume.That(descriptor.supportsGraspValue, Is.True);
        Assume.That(descriptor.supportsGripPose, Is.True);
        Assume.That(descriptor.supportsPinchPose, Is.True);
        Assume.That(descriptor.supportsPinchValue, Is.True);
        Assume.That(descriptor.supportsPokePose, Is.True);

        // A single frame holds common gesture data for both hands, so the same
        // sequence can drive each hand's playback.
        var sequence = CreateCommonGestureSequence();
        provider.GetUserFacingPlayback(Handedness.Left).sourceCaptureSequence = sequence;
        provider.GetUserFacingPlayback(Handedness.Right).sourceCaptureSequence = sequence;

        subsystem.Start();
        var updateFlags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.That(updateFlags, Is.EqualTo(XRHandSubsystem.UpdateSuccessFlags.All));

        foreach (var handedness in s_HandednessOptions)
        {
            AssertProviderSurfacesCommonPoseData(handedness, provider);
            AssertSubsystemHasCommonPoseData(handedness, subsystem);
            AssertSubsystemHasJointData(handedness, subsystem);
        }

        subsystem.Destroy();
    }

    static void AssertProviderSurfacesCommonPoseData(Handedness handedness, PlaybackProvider provider)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        var message = $"Handedness: {handedness}";

        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(handedness, provider.TryGetAimPose), Is.EqualTo((true, expectedData.aimPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(handedness, provider.TryGetGripPose), Is.EqualTo((true, expectedData.gripPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(handedness, provider.TryGetPinchPose), Is.EqualTo((true, expectedData.pinchPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(handedness, provider.TryGetPokePose), Is.EqualTo((true, expectedData.pokePose)), message);

        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(handedness, provider.TryGetAimActivateValue), Is.EqualTo((true, expectedData.aimActivateValue)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(handedness, provider.TryGetGraspValue), Is.EqualTo((true, expectedData.graspValue)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(handedness, provider.TryGetPinchValue), Is.EqualTo((true, expectedData.pinchValue)), message);

        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(handedness, provider.TryGetAimActivatedState), Is.EqualTo((true, expectedData.aimActivatedState)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(handedness, provider.TryGetGraspFirmState), Is.EqualTo((true, expectedData.graspFirmState)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(handedness, provider.TryGetPinchTouchedState), Is.EqualTo((true, expectedData.pinchTouchedState)), message);
    }

    static void AssertSubsystemHasCommonPoseData(Handedness handedness, XRHandSubsystem handSubsystem)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        var message = $"Handedness: {handedness}";
        var commonGestures = handSubsystem.GetCommonGestures(handedness);

        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(commonGestures.TryGetAimPose), Is.EqualTo((true, expectedData.aimPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(commonGestures.TryGetGripPose), Is.EqualTo((true, expectedData.gripPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(commonGestures.TryGetPinchPose), Is.EqualTo((true, expectedData.pinchPose)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(commonGestures.TryGetPokePose), Is.EqualTo((true, expectedData.pokePose)), message);

        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(commonGestures.TryGetAimActivateValue), Is.EqualTo((true, expectedData.aimActivateValue)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(commonGestures.TryGetGraspValue), Is.EqualTo((true, expectedData.graspValue)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<float>(commonGestures.TryGetPinchValue), Is.EqualTo((true, expectedData.pinchValue)), message);

        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(commonGestures.TryGetAimActivatedState), Is.EqualTo((true, expectedData.aimActivatedState)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(commonGestures.TryGetGraspFirmState), Is.EqualTo((true, expectedData.graspFirmState)), message);
        Assert.That(TestHandUtils.InvokeTryGetFunc<bool>(commonGestures.TryGetPinchTouchedState), Is.EqualTo((true, expectedData.pinchTouchedState)), message);

        var expectedState = TestCommonGestureData.GetCommonGesturesState(handedness);
        Assert.That(commonGestures.stateInternal.flags, Is.EqualTo(expectedState.flags), message);
        Assert.That(commonGestures.stateInternal, Is.EqualTo(expectedState), message);
    }

    static void AssertSubsystemHasJointData(Handedness handedness, XRHandSubsystem handSubsystem)
    {
        var expectedData = handedness == Handedness.Left ? TestHandData.leftHand : TestHandData.rightHand;
        var message = $"Handedness: {handedness}";
        var hand = handSubsystem.GetHand(handedness);

        for (var index = 0; index < TestHandData.jointsInLayout.Length; ++index)
        {
            if (!TestHandData.jointsInLayout[index])
                continue;

            var joint = hand.GetJoint(XRHandJointIDUtility.FromIndex(index));
            var expectedPose = expectedData[index];
            Assert.That(TestHandUtils.InvokeTryGetFunc<Pose>(joint.TryGetPose), Is.EqualTo((true, expectedPose)), message);
        }

        Assert.That(hand.rootPose, Is.EqualTo(expectedData[XRHandJointID.Wrist.ToIndex()]), message);
        Assert.That(hand.isTracked, Is.True, message);
    }

    /// <summary>
    /// Creates a capture sequence with a single frame holding common gesture data
    /// (from <see cref="TestCommonGestureData"/>) for both hands.
    /// </summary>
    XRHandCaptureSequence CreateCommonGestureSequence()
    {
        var sequence = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
        m_CreatedSequences.Add(sequence);

        sequence.InitializeBeforeRecordingImport();
        sequence.flags =
            SequenceFlags.CanSurfaceCommonPoseData |
            SequenceFlags.SupportsAimPose |
            SequenceFlags.SupportsAimActivateValue |
            SequenceFlags.SupportsGraspValue |
            SequenceFlags.SupportsGripPose |
            SequenceFlags.SupportsPinchPose |
            SequenceFlags.SupportsPinchValue |
            SequenceFlags.SupportsPokePose;

        // Build a single frame that references common gestures for both hands.
        var frameBuffer = new FrameBuffer(0f, XRHandRecordingOptions.None)
        {
            m_FrameFlags =
                FrameFlags.IsLeftCommonGesturesValid |
                FrameFlags.IsRightCommonGesturesValid |
                FrameFlags.IsLeftSnapshotValid |
                FrameFlags.IsRightSnapshotValid,

            // One for each hand set below
            m_NumReadCommonGestures = 2,
            m_NumReadSnapshotBuffers = 2,
        };
        frameBuffer.m_CommonGestures[Handedness.Left.ToIndex()] = TestCommonGestureData.GetCommonGesturesState(Handedness.Left);
        frameBuffer.m_CommonGestures[Handedness.Right.ToIndex()] = TestCommonGestureData.GetCommonGesturesState(Handedness.Right);

        frameBuffer.m_SnapshotBuffers[Handedness.Left.ToIndex()] = CreateHandSnapshotBuffer(Handedness.Left);
        frameBuffer.m_SnapshotBuffers[Handedness.Right.ToIndex()] = CreateHandSnapshotBuffer(Handedness.Right);

        sequence.InitializeBeforeRecordingImport();
        sequence.AddFrame(frameBuffer);

        // We no longer need the NativeArray stored in the FrameBuffer now that it has been converted into an XRHandCaptureFrame
        frameBuffer.Dispose();

        return sequence;
    }

    static HandSnapshotBuffer CreateHandSnapshotBuffer(Handedness handedness)
    {
        var handBuffer = new HandBuffer(handedness);
        handBuffer.m_HandFlags = HandFlags.AreAllJointPosesValid | HandFlags.WasHandTrackedDuringCapture;
        var testPoses = handedness == Handedness.Left ? TestHandData.leftHand : TestHandData.rightHand;
        var testRootPose = handedness == Handedness.Left ? TestHandData.leftRoot : TestHandData.rightRoot;
        NativeArray<Pose>.Copy(src: testPoses, dst: handBuffer.m_JointPoses);
        handBuffer.m_RootPose = testRootPose;

        var snapshotBuffer = new HandSnapshotBuffer(XRHandRecordingOptions.None);
        // Allocated with 2 for UpdateType.Dynamic and UpdateType.BeforeRender,
        // but we only set data for Dynamic.
        snapshotBuffer.m_SnapshotFlags = SnapshotFlags.IsDynamicHandValid;
        Assume.That(snapshotBuffer.m_HandBuffers.Length, Is.EqualTo(2));
        snapshotBuffer.m_HandBuffers[(int)XRHandSubsystem.UpdateType.Dynamic] = handBuffer;

        return snapshotBuffer;
    }
}
