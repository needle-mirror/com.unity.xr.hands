using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Playback;

/// <summary>
/// Unit tests for playback time controller classes extracted in Phase 2 of the refactoring.
/// These tests validate the behavior of FrameBasedTimeController and TimeBasedTimeController.
/// </summary>
class PlaybackTimeControllerTests
{
    List<XRHandCaptureSequence> m_CreatedSequences = new();

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

    /// <summary>
    /// Creates an empty <see cref="XRHandCaptureSequence"/> ScriptableObject, tracking it for cleanup.
    /// </summary>
    XRHandCaptureSequence CreateEmptySequence()
    {
        var sequence = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
        m_CreatedSequences.Add(sequence);
        return sequence;
    }

    /// <summary>
    /// Creates a simple test sequence with 3 frames at timestamps 0, 1, and 2 seconds.
    /// </summary>
    XRHandCaptureSequence CreateTestSequence()
    {
        var sequence = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
        m_CreatedSequences.Add(sequence);

        // Use reflection to initialize the internal frames list since there's no public API for testing
        var framesField = typeof(XRHandCaptureSequence).GetField("m_Frames",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(framesField, "m_Frames field not found via reflection - internal field name may have changed");

        var framesList = new List<XRHandCaptureFrame>();

        // Create 3 frames at 0s, 1s, and 2s
        // XRHandCaptureFrame constructor requires sequence and frame index
        framesList.Add(new XRHandCaptureFrame(sequence, default, 0) { timestamp = 0f });
        framesList.Add(new XRHandCaptureFrame(sequence, default, 1) { timestamp = 1f });
        framesList.Add(new XRHandCaptureFrame(sequence, default, 2) { timestamp = 2f });

        framesField.SetValue(sequence, framesList);

        // Set duration to match last frame timestamp
        var durationField = typeof(XRHandCaptureSequence).GetField("m_DurationInSeconds",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(durationField, "m_DurationInSeconds field not found via reflection - internal field name may have changed");
        durationField.SetValue(sequence, 2f);

        return sequence;
    }

    [Test]
    public void FrameBased_InitialState_StartsAtZero()
    {
        var controller = new FrameBasedTimeController();

        Assert.AreEqual(0, controller.FrameIndex, "Frame index should start at 0");
        Assert.AreEqual(0f, controller.ElapsedTime, "Elapsed time should start at 0");
    }

    [Test]
    public void FrameBased_Reset_ResetsToZero()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(0f, sequence);
        controller.ApplyFrameDelta(0f, sequence);

        controller.Reset();

        Assert.AreEqual(0, controller.FrameIndex, "Frame index should reset to 0");
        Assert.AreEqual(0f, controller.ElapsedTime, "Elapsed time should reset to 0");
    }

    [Test]
    public void FrameBased_ApplyFrameDelta_AdvancesFrameIndex()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(0f, sequence); // deltaTime is ignored

        Assert.AreEqual(1, controller.FrameIndex, "Frame index should advance to 1");
    }

    [Test]
    public void FrameBased_ApplyFrameDelta_SyncsElapsedTime()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(0f, sequence);

        Assert.AreEqual(1f, controller.ElapsedTime, "Elapsed time should sync to frame 1's timestamp (1.0s)");
    }

    [Test]
    public void FrameBased_ApplyFrameDelta_ClampsAtEnd()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();
        var finalFrameIndex = sequence.frames.Count - 1;

        controller.ApplyFrameDelta(0f, sequence); // frame 1
        controller.ApplyFrameDelta(0f, sequence); // frame 2
        controller.ApplyFrameDelta(0f, sequence); // wraps to frame 0

        Assert.AreEqual(finalFrameIndex, controller.FrameIndex, "Frame index should clamp to frame count - 1");
        Assert.AreEqual(sequence.frames[finalFrameIndex].timestamp, controller.ElapsedTime, "Elapsed time should clamp to last frame timestamp");
    }

    [Test]
    public void FrameBased_IsComplete_ReturnsTrueAtLastFrame()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(0f, sequence); // frame 1
        controller.ApplyFrameDelta(0f, sequence); // frame 2 (last frame)

        bool isComplete = controller.IsComplete(sequence, looping: false);

        Assert.IsTrue(isComplete, "Should be complete at last frame");
    }

    [Test]
    public void FrameBased_IsComplete_ReturnsFalseWhenLooping()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(0f, sequence);
        controller.ApplyFrameDelta(0f, sequence);

        bool isComplete = controller.IsComplete(sequence, looping: true);

        Assert.IsFalse(isComplete, "Should never complete when looping");
    }

    [Test]
    public void FrameBased_NeedsInterpolation_AlwaysReturnsFalse()
    {
        var controller = new FrameBasedTimeController();
        var frame = new XRHandCaptureFrame { timestamp = 1f };

        bool needsInterpolation = controller.NeedsInterpolation(frame);

        Assert.IsFalse(needsInterpolation, "Frame-based should never need interpolation");
    }

    [Test]
    public void FrameBased_TryGetInterpolationFrames_AlwaysReturnsFalse()
    {
        var controller = new FrameBasedTimeController();
        var sequence = CreateTestSequence();

        bool result = controller.TryGetInterpolationFrames(
            sequence,
            out var currentFrame,
            out var nextFrame,
            out var blendScalar);

        Assert.IsFalse(result, "Frame-based should never provide interpolation frames");
        Assert.AreEqual(0f, blendScalar, "Blend scalar should be 0");
    }

    [Test]
    public void TimeBased_InitialState_StartsAtZero()
    {
        var controller = new TimeBasedTimeController();

        Assert.AreEqual(0, controller.FrameIndex, "Frame index should start at 0");
        Assert.AreEqual(0f, controller.ElapsedTime, "Elapsed time should start at 0");
    }

    [Test]
    public void TimeBased_Reset_ResetsToZero()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(1.5f, sequence);

        controller.Reset();

        Assert.AreEqual(0, controller.FrameIndex, "Frame index should reset to 0");
        Assert.AreEqual(0f, controller.ElapsedTime, "Elapsed time should reset to 0");
    }

    [Test]
    public void TimeBased_ApplyFrameDelta_AccumulatesElapsedTime()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(0.5f, sequence);
        controller.ApplyFrameDelta(0.3f, sequence);

        Assert.AreEqual(0.8f, controller.ElapsedTime, 0.001f, "Elapsed time should accumulate");
    }

    [Test]
    public void TimeBased_ApplyFrameDelta_UpdatesFrameIndexToClosest()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();

        controller.ApplyFrameDelta(0.4f, sequence); // elapsed = 0.4s, current frame is 0
        int frameAtPointFour = controller.FrameIndex;

        controller.ApplyFrameDelta(0.7f, sequence); // elapsed = 1.1s, current frame is 1
        int frameAtOnePointOne = controller.FrameIndex;

        controller.ApplyFrameDelta(1.0f, sequence); // elapsed = 2.1s, current frame is 2
        int frameAtTwoPointOne = controller.FrameIndex;

        Assert.AreEqual(0, frameAtPointFour, "At 0.4s should be in frame 0");
        Assert.AreEqual(1, frameAtOnePointOne, "At 1.1s should be in frame 1");
        Assert.AreEqual(2, frameAtTwoPointOne, "At 2.1s should be in frame 2");
    }

    [Test]
    public void TimeBased_IsComplete_ReturnsTrueWhenExceedingDuration()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(2.5f, sequence); // elapsed = 2.5s, exceeds last frame at 2.0s

        bool isComplete = controller.IsComplete(sequence, looping: false);

        Assert.IsTrue(isComplete, "Should be complete when elapsed time exceeds last frame timestamp");
    }

    [Test]
    public void TimeBased_IsComplete_ReturnsFalseWhenLooping()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(2.5f, sequence);

        bool isComplete = controller.IsComplete(sequence, looping: true);

        Assert.IsFalse(isComplete, "Should never complete when looping");
    }

    [Test]
    public void TimeBased_NeedsInterpolation_ReturnsFalseAtExactFrameTime()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(1f, sequence); // elapsed = 1.0s, exactly at frame 1

        var currentFrame = sequence.frames[1];

        bool needsInterpolation = controller.NeedsInterpolation(currentFrame);

        Assert.IsFalse(needsInterpolation, "Should not need interpolation at exact frame time");
    }

    [Test]
    public void TimeBased_NeedsInterpolation_ReturnsTrueBetweenFrames()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(0.5f, sequence); // elapsed = 0.5s, between frames 0 and 1

        var currentFrame = sequence.frames[0];

        bool needsInterpolation = controller.NeedsInterpolation(currentFrame);

        Assert.IsTrue(needsInterpolation, "Should need interpolation between frames");
    }

    [Test]
    public void TimeBased_TryGetInterpolationFrames_SucceedsBetweenFrames()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(0.5f, sequence); // elapsed = 0.5s, halfway between frames 0 and 1

        bool result = controller.TryGetInterpolationFrames(
            sequence,
            out var currentFrame,
            out var nextFrame,
            out var blendScalar);

        Assert.IsTrue(result, "Should successfully get interpolation frames");
        Assert.AreEqual(0f, currentFrame.timestamp, "Current frame should be frame 0");
        Assert.AreEqual(1f, nextFrame.timestamp, "Next frame should be frame 1");
        Assert.AreEqual(0.5f, blendScalar, 0.001f, "Blend scalar should be 0.5 (halfway)");
    }

    [Test]
    public void TimeBased_TryGetInterpolationFrames_CalculatesCorrectBlendScalar()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(0.25f, sequence); // elapsed = 0.25s, 25% between frames 0 and 1

        bool result = controller.TryGetInterpolationFrames(
            sequence,
            out var currentFrame,
            out var nextFrame,
            out var blendScalar);

        Assert.IsTrue(result, "Should successfully get interpolation frames");
        Assert.AreEqual(0.25f, blendScalar, 0.001f, "Blend scalar should be 0.25");
    }

    [Test]
    public void TimeBased_TryGetInterpolationFrames_FailsAtLastFrame()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(2f, sequence); // elapsed = 2.0s, at last frame

        bool result = controller.TryGetInterpolationFrames(
            sequence,
            out var currentFrame,
            out var nextFrame,
            out var blendScalar);

        Assert.IsFalse(result, "Should fail at last frame (no next frame to interpolate to)");
    }

    [Test]
    public void TimeBased_TryGetInterpolationFrames_FailsAtExactFrameTime()
    {
        var controller = new TimeBasedTimeController();
        var sequence = CreateTestSequence();
        controller.ApplyFrameDelta(1f, sequence); // elapsed = 1.0s, exactly at frame 1

        bool result = controller.TryGetInterpolationFrames(
            sequence,
            out var currentFrame,
            out var nextFrame,
            out var blendScalar);

        Assert.IsFalse(result, "Should fail at exact frame time (no interpolation needed)");
    }

    [Test]
    public void FrameBased_WithNullSequence_DoesNotCrash()
    {
        var controller = new FrameBasedTimeController();

        Assert.DoesNotThrow(() => controller.ApplyFrameDelta(0f, null));
        Assert.DoesNotThrow(() => controller.IsComplete(null, false));
    }

    [Test]
    public void TimeBased_WithNullSequence_DoesNotCrash()
    {
        var controller = new TimeBasedTimeController();

        Assert.DoesNotThrow(() => controller.ApplyFrameDelta(1f, null));
        Assert.DoesNotThrow(() => controller.IsComplete(null, false));
        Assert.DoesNotThrow(() => controller.TryGetInterpolationFrames(null, out _, out _, out _));
    }

    [Test]
    public void FrameBased_WithEmptySequence_DoesNotCrash()
    {
        var controller = new FrameBasedTimeController();
        var emptySequence = CreateEmptySequence();

        Assert.DoesNotThrow(() => controller.ApplyFrameDelta(0f, emptySequence));
        Assert.IsTrue(controller.IsComplete(emptySequence, false), "Empty sequence should be complete");
    }

    [Test]
    public void TimeBased_WithEmptySequence_DoesNotCrash()
    {
        var controller = new TimeBasedTimeController();
        var emptySequence = CreateEmptySequence();

        Assert.DoesNotThrow(() => controller.ApplyFrameDelta(1f, emptySequence));
        Assert.IsTrue(controller.IsComplete(emptySequence, false), "Empty sequence should be complete");
    }
}
