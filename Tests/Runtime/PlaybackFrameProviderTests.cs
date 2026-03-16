using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Playback;

/// <summary>
/// Unit tests for PlaybackFrameProvider extracted in Phase 5 of the refactoring.
/// These tests validate frame retrieval, validation, and hand data extraction.
/// </summary>
class PlaybackFrameProviderTests
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
    /// Creates a simple test sequence with frames at timestamps 0..frameCount-1 seconds.
    /// </summary>
    XRHandCaptureSequence CreateTestSequence(int frameCount = 3)
    {
        var sequence = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
        m_CreatedSequences.Add(sequence);

        // Use reflection to initialize the internal frames list since there's no public API for testing
        var framesField = typeof(XRHandCaptureSequence).GetField("m_Frames",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(framesField, "m_Frames field not found via reflection - internal field name may have changed");

        var framesList = new List<XRHandCaptureFrame>();

        // Create frames
        for (int i = 0; i < frameCount; i++)
        {
            framesList.Add(new XRHandCaptureFrame(sequence, default, i) { timestamp = i });
        }

        framesField.SetValue(sequence, framesList);

        // Set duration to match last frame timestamp
        var durationField = typeof(XRHandCaptureSequence).GetField("m_DurationInSeconds",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(durationField, "m_DurationInSeconds field not found via reflection - internal field name may have changed");
        durationField.SetValue(sequence, (float)(frameCount - 1));

        return sequence;
    }

    [Test]
    public void Constructor_WithNullSequence_InitializesCorrectly()
    {
        var provider = new PlaybackFrameProvider(null, Handedness.Left);

        Assert.AreEqual(0, provider.frameCount, "frameCount should be 0 for null sequence");
        Assert.IsFalse(provider.hasFrames, "hasFrames should be false for null sequence");
        Assert.IsFalse(provider.isSequenceValid, "isSequenceValid should be false for null sequence");
    }

    [Test]
    public void Constructor_WithEmptySequence_InitializesCorrectly()
    {
        var emptySequence = CreateEmptySequence();

        var provider = new PlaybackFrameProvider(emptySequence, Handedness.Right);

        Assert.AreEqual(0, provider.frameCount, "frameCount should be 0 for empty sequence");
        Assert.IsFalse(provider.hasFrames, "hasFrames should be false for empty sequence");
    }

    [Test]
    public void Constructor_WithValidSequence_InitializesCorrectly()
    {
        var sequence = CreateTestSequence(5);

        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.AreEqual(5, provider.frameCount, "frameCount should match sequence frame count");
        Assert.IsTrue(provider.hasFrames, "hasFrames should be true for valid sequence");
        Assert.IsTrue(provider.isSequenceValid, "isSequenceValid should be true for valid sequence");
    }

    [Test]
    public void FrameCount_ReflectsSequenceFrameCount()
    {
        var sequence = CreateTestSequence(10);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        int count = provider.frameCount;

        Assert.AreEqual(10, count, "frameCount should return correct number of frames");
    }

    [Test]
    public void UpdateSequence_ChangesFrameSource()
    {
        var sequence1 = CreateTestSequence(3);
        var sequence2 = CreateTestSequence(7);
        var provider = new PlaybackFrameProvider(sequence1, Handedness.Left);

        provider.UpdateSequence(sequence2);

        Assert.AreEqual(7, provider.frameCount, "frameCount should reflect new sequence");
    }

    [Test]
    public void UpdateSequence_ToNull_HandlesGracefully()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        provider.UpdateSequence(null);

        Assert.AreEqual(0, provider.frameCount, "frameCount should be 0 after setting null sequence");
        Assert.IsFalse(provider.hasFrames, "hasFrames should be false after setting null sequence");
    }

    [Test]
    public void UpdateSequence_FromNullToValid_Works()
    {
        var provider = new PlaybackFrameProvider(null, Handedness.Right);
        var sequence = CreateTestSequence(4);

        provider.UpdateSequence(sequence);

        Assert.AreEqual(4, provider.frameCount, "frameCount should reflect new sequence");
        Assert.IsTrue(provider.hasFrames, "hasFrames should be true after setting valid sequence");
    }

    [Test]
    public void TryGetCurrentFrame_WithNullSequence_ReturnsFalse()
    {
        var provider = new PlaybackFrameProvider(null, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(0, out var frame);

        Assert.IsFalse(success, "Should return false for null sequence");
    }

    [Test]
    public void TryGetCurrentFrame_WithValidIndex_ReturnsTrue()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(2, out var frame);

        Assert.IsTrue(success, "Should return true for valid index");
        Assert.AreEqual(2f, frame.timestamp, "Should return correct frame");
    }

    [Test]
    public void TryGetCurrentFrame_WithNegativeIndex_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(-1, out var frame);

        Assert.IsFalse(success, "Should return false for negative index");
    }

    [Test]
    public void TryGetCurrentFrame_WithIndexEqualToCount_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(5, out var frame);

        Assert.IsFalse(success, "Should return false for index equal to count");
    }

    [Test]
    public void TryGetCurrentFrame_WithIndexGreaterThanCount_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(10, out var frame);

        Assert.IsFalse(success, "Should return false for index greater than count");
    }

    [Test]
    public void TryGetCurrentFrame_AtFirstFrame_Works()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(0, out var frame);

        Assert.IsTrue(success, "Should return true for first frame");
        Assert.AreEqual(0f, frame.timestamp, "Should return first frame");
    }

    [Test]
    public void TryGetCurrentFrame_AtLastFrame_Works()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetCurrentFrame(4, out var frame);

        Assert.IsTrue(success, "Should return true for last frame");
        Assert.AreEqual(4f, frame.timestamp, "Should return last frame");
    }

    [Test]
    public void TryGetNextFrame_WithValidIndex_ReturnsNextFrame()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetNextFrame(2, out var frame);

        Assert.IsTrue(success, "Should return true for valid index");
        Assert.AreEqual(3f, frame.timestamp, "Should return next frame (index + 1)");
    }

    [Test]
    public void TryGetNextFrame_AtLastFrame_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetNextFrame(4, out var frame);

        Assert.IsFalse(success, "Should return false when at last frame (no next frame exists)");
    }

    [Test]
    public void TryGetNextFrame_AtSecondToLastFrame_ReturnsLastFrame()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetNextFrame(3, out var frame);

        Assert.IsTrue(success, "Should return true for second-to-last frame");
        Assert.AreEqual(4f, frame.timestamp, "Should return last frame");
    }

    [Test]
    public void TryGetInterpolationFrames_InMiddleOfSequence_ReturnsTrue()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetInterpolationFrames(2, out var current, out var next);

        Assert.IsTrue(success, "Should return true in middle of sequence");
        Assert.AreEqual(2f, current.timestamp, "Current frame should be at index 2");
        Assert.AreEqual(3f, next.timestamp, "Next frame should be at index 3");
    }

    [Test]
    public void TryGetInterpolationFrames_AtLastFrame_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetInterpolationFrames(4, out var current, out var next);

        Assert.IsFalse(success, "Should return false at last frame (no next frame to interpolate to)");
    }

    [Test]
    public void TryGetInterpolationFrames_WithSingleFrameSequence_ReturnsFalse()
    {
        var sequence = CreateTestSequence(1);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetInterpolationFrames(0, out var current, out var next);

        Assert.IsFalse(success, "Should return false for single-frame sequence");
    }

    [Test]
    public void TryGetInterpolationFrames_AtFirstFrame_Works()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetInterpolationFrames(0, out var current, out var next);

        Assert.IsTrue(success, "Should return true at first frame");
        Assert.AreEqual(0f, current.timestamp, "Current should be first frame");
        Assert.AreEqual(1f, next.timestamp, "Next should be second frame");
    }

    [Test]
    public void TryGetInterpolationFrames_WithInvalidIndex_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool success = provider.TryGetInterpolationFrames(-1, out var current, out var next);

        Assert.IsFalse(success, "Should return false for invalid index");
    }

    [Test]
    public void IsFrameIndexValid_WithValidIndex_ReturnsTrue()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.IsTrue(provider.IsFrameIndexValid(0), "Index 0 should be valid");
        Assert.IsTrue(provider.IsFrameIndexValid(2), "Index 2 should be valid");
        Assert.IsTrue(provider.IsFrameIndexValid(4), "Index 4 should be valid");
    }

    [Test]
    public void IsFrameIndexValid_WithInvalidIndex_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.IsFalse(provider.IsFrameIndexValid(-1), "Negative index should be invalid");
        Assert.IsFalse(provider.IsFrameIndexValid(5), "Index equal to count should be invalid");
        Assert.IsFalse(provider.IsFrameIndexValid(10), "Index greater than count should be invalid");
    }

    [Test]
    public void IsFrameIndexValid_WithNullSequence_ReturnsFalse()
    {
        var provider = new PlaybackFrameProvider(null, Handedness.Left);

        Assert.IsFalse(provider.IsFrameIndexValid(0), "Any index should be invalid for null sequence");
    }

    [Test]
    public void CanInterpolate_InMiddleOfSequence_ReturnsTrue()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.IsTrue(provider.CanInterpolate(0), "Should be able to interpolate from first frame");
        Assert.IsTrue(provider.CanInterpolate(2), "Should be able to interpolate from middle frame");
        Assert.IsTrue(provider.CanInterpolate(3), "Should be able to interpolate from second-to-last frame");
    }

    [Test]
    public void CanInterpolate_AtLastFrame_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool canInterpolate = provider.CanInterpolate(4);

        Assert.IsFalse(canInterpolate, "Cannot interpolate from last frame (no next frame)");
    }

    [Test]
    public void CanInterpolate_WithInvalidIndex_ReturnsFalse()
    {
        var sequence = CreateTestSequence(5);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.IsFalse(provider.CanInterpolate(-1), "Cannot interpolate with negative index");
        Assert.IsFalse(provider.CanInterpolate(5), "Cannot interpolate with index >= count");
    }

    [Test]
    public void CanInterpolate_WithSingleFrameSequence_ReturnsFalse()
    {
        var sequence = CreateTestSequence(1);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        bool canInterpolate = provider.CanInterpolate(0);

        Assert.IsFalse(canInterpolate, "Cannot interpolate in single-frame sequence");
    }

    [Test]
    public void CanInterpolate_WithTwoFrameSequence_WorksForFirstFrameOnly()
    {
        var sequence = CreateTestSequence(2);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Left);

        Assert.IsTrue(provider.CanInterpolate(0), "Should be able to interpolate from frame 0 to frame 1");
        Assert.IsFalse(provider.CanInterpolate(1), "Cannot interpolate from last frame");
    }

    [Test]
    public void EdgeCase_EmptySequenceHandling()
    {
        var emptySequence = CreateEmptySequence();
        var provider = new PlaybackFrameProvider(emptySequence, Handedness.Left);

        Assert.IsFalse(provider.TryGetCurrentFrame(0, out _), "Should fail to get frame from empty sequence");
        Assert.IsFalse(provider.TryGetNextFrame(0, out _), "Should fail to get next frame from empty sequence");
        Assert.IsFalse(provider.TryGetInterpolationFrames(0, out _, out _), "Should fail to get interpolation frames from empty sequence");
        Assert.IsFalse(provider.IsFrameIndexValid(0), "No index should be valid for empty sequence");
        Assert.IsFalse(provider.CanInterpolate(0), "Cannot interpolate in empty sequence");
    }

    [Test]
    public void EdgeCase_SequenceReplacementDuringAccess()
    {
        var sequence1 = CreateTestSequence(3);
        var sequence2 = CreateTestSequence(7);
        var provider = new PlaybackFrameProvider(sequence1, Handedness.Left);

        bool success1 = provider.TryGetCurrentFrame(1, out var frame1);

        // Replace sequence
        provider.UpdateSequence(sequence2);

        // Get frame from new sequence
        bool success2 = provider.TryGetCurrentFrame(5, out var frame2);

        Assert.IsTrue(success1, "Should succeed with first sequence");
        Assert.AreEqual(1f, frame1.timestamp, "Should get correct frame from first sequence");
        Assert.IsTrue(success2, "Should succeed with second sequence");
        Assert.AreEqual(5f, frame2.timestamp, "Should get correct frame from second sequence");
    }

    [Test]
    public void EdgeCase_BoundaryConditionsForAllMethods()
    {
        var sequence = CreateTestSequence(3);
        var provider = new PlaybackFrameProvider(sequence, Handedness.Right);

        // Frame 0
        Assert.IsTrue(provider.TryGetCurrentFrame(0, out _), "Frame 0 should be accessible");
        Assert.IsTrue(provider.TryGetNextFrame(0, out _), "Next from frame 0 should be accessible");
        Assert.IsTrue(provider.IsFrameIndexValid(0), "Frame 0 should be valid");
        Assert.IsTrue(provider.CanInterpolate(0), "Should be able to interpolate from frame 0");

        // Frame 1
        Assert.IsTrue(provider.TryGetCurrentFrame(1, out _), "Frame 1 should be accessible");
        Assert.IsTrue(provider.TryGetNextFrame(1, out _), "Next from frame 1 should be accessible");
        Assert.IsTrue(provider.IsFrameIndexValid(1), "Frame 1 should be valid");
        Assert.IsTrue(provider.CanInterpolate(1), "Should be able to interpolate from frame 1");

        // Frame 2 (last)
        Assert.IsTrue(provider.TryGetCurrentFrame(2, out _), "Frame 2 should be accessible");
        Assert.IsFalse(provider.TryGetNextFrame(2, out _), "Next from frame 2 should not be accessible");
        Assert.IsTrue(provider.IsFrameIndexValid(2), "Frame 2 should be valid");
        Assert.IsFalse(provider.CanInterpolate(2), "Cannot interpolate from last frame");

        // Frame 3 (out of bounds)
        Assert.IsFalse(provider.TryGetCurrentFrame(3, out _), "Frame 3 should not be accessible");
        Assert.IsFalse(provider.TryGetNextFrame(3, out _), "Next from frame 3 should not be accessible");
        Assert.IsFalse(provider.IsFrameIndexValid(3), "Frame 3 should not be valid");
        Assert.IsFalse(provider.CanInterpolate(3), "Cannot interpolate from out-of-bounds index");
    }
}
