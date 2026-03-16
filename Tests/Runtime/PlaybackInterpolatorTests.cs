using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture.Playback;

/// <summary>
/// Unit tests for PlaybackInterpolator utility class extracted in Phase 3 of the refactoring.
/// These tests validate all interpolation algorithms and blend scalar calculations.
/// </summary>
class PlaybackInterpolatorTests
{
    [Test]
    public void CalculateBlendScalar_WithNormalValues_ReturnsCorrectValue()
    {
        float currentTime = 1.0f;
        float nextTime = 2.0f;
        float elapsedTime = 1.5f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.5f, result, 0.001f, "Blend scalar should be 0.5 when halfway between frames");
    }

    [Test]
    public void CalculateBlendScalar_AtStart_ReturnsZero()
    {
        float currentTime = 1.0f;
        float nextTime = 2.0f;
        float elapsedTime = 1.0f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.0f, result, 0.001f, "Blend scalar should be 0 at start time");
    }

    [Test]
    public void CalculateBlendScalar_AtEnd_ReturnsOne()
    {
        float currentTime = 1.0f;
        float nextTime = 2.0f;
        float elapsedTime = 2.0f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(1.0f, result, 0.001f, "Blend scalar should be 1 at end time");
    }

    [Test]
    public void CalculateBlendScalar_BeforeStart_ReturnsZero()
    {
        float currentTime = 1.0f;
        float nextTime = 2.0f;
        float elapsedTime = 0.5f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.0f, result, 0.001f, "Blend scalar should be clamped to 0 before start");
    }

    [Test]
    public void CalculateBlendScalar_AfterEnd_ReturnsOne()
    {
        float currentTime = 1.0f;
        float nextTime = 2.0f;
        float elapsedTime = 3.0f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(1.0f, result, 0.001f, "Blend scalar should be clamped to 1 after end");
    }

    [Test]
    public void CalculateBlendScalar_WithSameTimes_ReturnsZero()
    {
        float currentTime = 1.0f;
        float nextTime = 1.0f;
        float elapsedTime = 1.0f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.0f, result, 0.001f, "Blend scalar should be 0 when current and next times are equal");
    }

    [Test]
    public void CalculateBlendScalar_QuarterWay_ReturnsPointTwoFive()
    {
        float currentTime = 0.0f;
        float nextTime = 1.0f;
        float elapsedTime = 0.25f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.25f, result, 0.001f, "Blend scalar should be 0.25 when quarter way through");
    }

    [Test]
    public void InterpolatePose_AtZero_ReturnsStartPose()
    {
        var start = new Pose(new Vector3(0, 0, 0), Quaternion.identity);
        var end = new Pose(new Vector3(10, 10, 10), Quaternion.Euler(0, 90, 0));

        var result = PlaybackInterpolator.InterpolatePose(start, end, 0f);

        Assert.AreEqual(start.position, result.position, "Position should match start at t=0");
        // Quaternions can represent the same rotation as q or -q, so check angle difference instead
        float angleDiff = Quaternion.Angle(start.rotation, result.rotation);
        Assert.IsTrue(angleDiff < 0.01f, $"Rotation should match start at t=0, angle difference: {angleDiff}");
    }

    [Test]
    public void InterpolatePose_AtOne_ReturnsEndPose()
    {
        var start = new Pose(new Vector3(0, 0, 0), Quaternion.identity);
        var end = new Pose(new Vector3(10, 10, 10), Quaternion.Euler(0, 90, 0));

        var result = PlaybackInterpolator.InterpolatePose(start, end, 1f);

        Assert.AreEqual(end.position, result.position, "Position should match end at t=1");
        // Quaternions can represent the same rotation as q or -q, so check angle difference instead
        float angleDiff = Quaternion.Angle(end.rotation, result.rotation);
        Assert.IsTrue(angleDiff < 0.01f, $"Rotation should match end at t=1, angle difference: {angleDiff}");
    }

    [Test]
    public void InterpolatePose_AtHalf_ReturnsMiddlePose()
    {
        var start = new Pose(new Vector3(0, 0, 0), Quaternion.identity);
        var end = new Pose(new Vector3(10, 0, 0), Quaternion.identity);

        var result = PlaybackInterpolator.InterpolatePose(start, end, 0.5f);

        Vector3 expectedPosition = new Vector3(5, 0, 0);
        TestHandUtils.AssertAreApproximatelyEqual(expectedPosition, result.position, "Position should be halfway between start and end");
    }

    [Test]
    public void InterpolatePose_WithIdentityPoses_ReturnsIdentity()
    {
        var start = Pose.identity;
        var end = Pose.identity;

        var result = PlaybackInterpolator.InterpolatePose(start, end, 0.5f);

        Assert.AreEqual(Pose.identity.position, result.position, "Position should be identity");
        float angleDiff = Quaternion.Angle(Pose.identity.rotation, result.rotation);
        Assert.IsTrue(angleDiff < 0.01f, $"Rotation should be identity, angle difference: {angleDiff}");
    }

    [Test]
    public void InterpolateValue_AtZero_ReturnsStart()
    {
        float start = 0f;
        float end = 10f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 0f);

        Assert.AreEqual(0f, result, 0.001f, "Value should be start at t=0");
    }

    [Test]
    public void InterpolateValue_AtOne_ReturnsEnd()
    {
        float start = 0f;
        float end = 10f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 1f);

        Assert.AreEqual(10f, result, 0.001f, "Value should be end at t=1");
    }

    [Test]
    public void InterpolateValue_AtHalf_ReturnsMiddle()
    {
        float start = 0f;
        float end = 10f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 0.5f);

        Assert.AreEqual(5f, result, 0.001f, "Value should be halfway between start and end");
    }

    [Test]
    public void InterpolateValue_WithNegativeValues_Works()
    {
        float start = -10f;
        float end = 10f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 0.5f);

        Assert.AreEqual(0f, result, 0.001f, "Should interpolate correctly with negative values");
    }

    [Test]
    public void InterpolateValue_WithSameValues_ReturnsSameValue()
    {
        float start = 5f;
        float end = 5f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 0.5f);

        Assert.AreEqual(5f, result, 0.001f, "Should return same value when start equals end");
    }

    [Test]
    public void TryInterpolateAimState_AtZero_MatchesCurrentState()
    {
        var current = new XRHandAimState();
        current.pinchStrengthIndex = 0.2f;
        current.pinchStrengthMiddle = 0.3f;
        current.pinchStrengthRing = 0.4f;
        current.pinchStrengthLittle = 0.5f;

        var next = new XRHandAimState();
        next.pinchStrengthIndex = 0.8f;
        next.pinchStrengthMiddle = 0.7f;
        next.pinchStrengthRing = 0.6f;
        next.pinchStrengthLittle = 0.5f;

        bool success = PlaybackInterpolator.TryInterpolateAimState(current, next, 0f, out var result);

        Assert.IsTrue(success, "Interpolation should succeed");
        Assert.AreEqual(0.2f, result.pinchStrengthIndex, 0.001f, "Index should match current at t=0");
        Assert.AreEqual(0.3f, result.pinchStrengthMiddle, 0.001f, "Middle should match current at t=0");
        Assert.AreEqual(0.4f, result.pinchStrengthRing, 0.001f, "Ring should match current at t=0");
        Assert.AreEqual(0.5f, result.pinchStrengthLittle, 0.001f, "Little should match current at t=0");
    }

    [Test]
    public void TryInterpolateAimState_AtOne_MatchesNextState()
    {
        var current = new XRHandAimState();
        current.pinchStrengthIndex = 0.2f;
        current.pinchStrengthMiddle = 0.3f;
        current.pinchStrengthRing = 0.4f;
        current.pinchStrengthLittle = 0.5f;

        var next = new XRHandAimState();
        next.pinchStrengthIndex = 0.8f;
        next.pinchStrengthMiddle = 0.7f;
        next.pinchStrengthRing = 0.6f;
        next.pinchStrengthLittle = 0.5f;

        bool success = PlaybackInterpolator.TryInterpolateAimState(current, next, 1f, out var result);

        Assert.IsTrue(success, "Interpolation should succeed");
        Assert.AreEqual(0.8f, result.pinchStrengthIndex, 0.001f, "Index should match next at t=1");
        Assert.AreEqual(0.7f, result.pinchStrengthMiddle, 0.001f, "Middle should match next at t=1");
        Assert.AreEqual(0.6f, result.pinchStrengthRing, 0.001f, "Ring should match next at t=1");
        Assert.AreEqual(0.5f, result.pinchStrengthLittle, 0.001f, "Little should match next at t=1");
    }

    [Test]
    public void TryInterpolateAimState_AtHalf_InterpolatesValues()
    {
        var current = new XRHandAimState();
        current.pinchStrengthIndex = 0.0f;
        current.pinchStrengthMiddle = 0.0f;
        current.pinchStrengthRing = 0.0f;
        current.pinchStrengthLittle = 0.0f;

        var next = new XRHandAimState();
        next.pinchStrengthIndex = 1.0f;
        next.pinchStrengthMiddle = 1.0f;
        next.pinchStrengthRing = 1.0f;
        next.pinchStrengthLittle = 1.0f;

        bool success = PlaybackInterpolator.TryInterpolateAimState(current, next, 0.5f, out var result);

        Assert.IsTrue(success, "Interpolation should succeed");
        Assert.AreEqual(0.5f, result.pinchStrengthIndex, 0.001f, "Index should be halfway");
        Assert.AreEqual(0.5f, result.pinchStrengthMiddle, 0.001f, "Middle should be halfway");
        Assert.AreEqual(0.5f, result.pinchStrengthRing, 0.001f, "Ring should be halfway");
        Assert.AreEqual(0.5f, result.pinchStrengthLittle, 0.001f, "Little should be halfway");
    }

    [Test]
    public void TryInterpolateAimState_InterpolatesPinchValues()
    {
        // Note: We only test pinch value interpolation here because XRHandAimState
        // requires proper initialization from capture frames to use SetAimPose.
        // The aim pose interpolation is tested implicitly through integration tests
        // where aim states come from real capture frames.

        var current = new XRHandAimState();
        current.pinchStrengthIndex = 0.2f;
        current.pinchStrengthMiddle = 0.3f;

        var next = new XRHandAimState();
        next.pinchStrengthIndex = 0.8f;
        next.pinchStrengthMiddle = 0.7f;

        bool success = PlaybackInterpolator.TryInterpolateAimState(current, next, 0.5f, out var result);

        Assert.IsTrue(success, "Interpolation should succeed");
        Assert.AreEqual(0.5f, result.pinchStrengthIndex, 0.001f, "Index pinch should be interpolated");
        Assert.AreEqual(0.5f, result.pinchStrengthMiddle, 0.001f, "Middle pinch should be interpolated");
    }

    [Test]
    public void CalculateBlendScalar_WithVerySmallFrameDuration_ReturnsZero()
    {
        float currentTime = 1.0f;
        float nextTime = 1.0f + (Constants.k_Epsilon * 0.5f); // Less than epsilon
        float elapsedTime = 1.5f;

        float result = PlaybackInterpolator.CalculateBlendScalar(currentTime, nextTime, elapsedTime);

        Assert.AreEqual(0.0f, result, 0.001f, "Should return 0 when frame duration is below epsilon");
    }

    [Test]
    public void InterpolatePose_WithLargeValues_Works()
    {
        var start = new Pose(new Vector3(1000, 2000, 3000), Quaternion.Euler(45, 90, 135));
        var end = new Pose(new Vector3(5000, 6000, 7000), Quaternion.Euler(135, 180, 225));

        var result = PlaybackInterpolator.InterpolatePose(start, end, 0.5f);

        // Should not throw and should produce reasonable values
        Assert.IsTrue(result.position.x > start.position.x && result.position.x < end.position.x);
        Assert.IsTrue(result.position.y > start.position.y && result.position.y < end.position.y);
        Assert.IsTrue(result.position.z > start.position.z && result.position.z < end.position.z);
    }

    [Test]
    public void InterpolateValue_WithVeryLargeValues_Works()
    {
        float start = 1000000f;
        float end = 2000000f;

        float result = PlaybackInterpolator.InterpolateValue(start, end, 0.5f);

        Assert.AreEqual(1500000f, result, 1f, "Should handle large values");
    }
}
