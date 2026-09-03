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
    public void InterpolateAimState_AtZero_MatchesCurrentState()
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

        var result = PlaybackInterpolator.InterpolateAimState(current, next, 0f);

        const float range = 1e-5f;
        Assert.That(result.pinchStrengthIndex, Is.EqualTo(current.pinchStrengthIndex).Within(range), "Index should match current at t=0");
        Assert.That(result.pinchStrengthMiddle, Is.EqualTo(current.pinchStrengthMiddle).Within(range), "Middle should match current at t=0");
        Assert.That(result.pinchStrengthRing, Is.EqualTo(current.pinchStrengthRing).Within(range), "Ring should match current at t=0");
        Assert.That(result.pinchStrengthLittle, Is.EqualTo(current.pinchStrengthLittle).Within(range), "Little should match current at t=0");
    }

    [Test]
    public void InterpolateAimState_AtOne_MatchesNextState()
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

        var result = PlaybackInterpolator.InterpolateAimState(current, next, 1f);

        const float range = 1e-5f;
        Assert.That(result.pinchStrengthIndex, Is.EqualTo(next.pinchStrengthIndex).Within(range), "Index should match next at t=1");
        Assert.That(result.pinchStrengthMiddle, Is.EqualTo(next.pinchStrengthMiddle).Within(range), "Middle should match next at t=1");
        Assert.That(result.pinchStrengthRing, Is.EqualTo(next.pinchStrengthRing).Within(range), "Ring should match next at t=1");
        Assert.That(result.pinchStrengthLittle, Is.EqualTo(next.pinchStrengthLittle).Within(range), "Little should match next at t=1");
    }

    [Test]
    public void InterpolateAimState_InterpolatesValues()
    {
        var current = new XRHandAimState();
        current.pinchStrengthIndex = 0.2f;
        current.pinchStrengthMiddle = 0.3f;
        current.pinchStrengthRing = 0f;
        current.pinchStrengthLittle = 1f;
        current.aimPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity);
        current.aimStateFlags = AimStateFlags.HasAimPose;

        var next = new XRHandAimState();
        next.pinchStrengthIndex = 0.8f;
        next.pinchStrengthMiddle = 0.7f;
        next.pinchStrengthRing = 1f;
        next.pinchStrengthLittle = 0f;
        next.aimPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity);
        next.aimStateFlags = AimStateFlags.HasAimPose;

        var result = PlaybackInterpolator.InterpolateAimState(current, next, 0.5f);

        const float range = 1e-5f;
        Assert.That(result.pinchStrengthIndex, Is.EqualTo(0.5f).Within(range), "Index should be interpolated");
        Assert.That(result.pinchStrengthMiddle, Is.EqualTo(0.5f).Within(range), "Middle should be interpolated");
        Assert.That(result.pinchStrengthRing, Is.EqualTo(0.5f).Within(range), "Ring should be interpolated");
        Assert.That(result.pinchStrengthLittle, Is.EqualTo(0.5f).Within(range), "Little should be interpolated");
        Assert.That(result.aimPoseInternal, Is.EqualTo(new Pose(new Vector3(15f, 0f, 0f), Quaternion.identity)), "Aim pose should be interpolated");
    }

    [Test]
    public void InterpolateAimState_DoesNotInterpolateDiscreteValues()
    {
        var current = new XRHandAimState
        {
            pinchStrengthIndex = 1f,
            aimPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            aimStateFlags = AimStateFlags.IsTracked | AimStateFlags.IsIndexPressed | AimStateFlags.HasAimPose,
        };

        var next = new XRHandAimState
        {
            pinchStrengthIndex = 0.7f,
            aimPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            aimStateFlags = AimStateFlags.HasAimPose,
        };

        var result = PlaybackInterpolator.InterpolateAimState(current, next, 0.8f);

        const float range = 1e-5f;
        Assert.That(result.pinchStrengthIndex, Is.EqualTo(0.76f).Within(range), "Index should be interpolated");
        Assert.That(result.aimPoseInternal, Is.EqualTo(new Pose(new Vector3(18f, 0f, 0f), Quaternion.identity)), "Aim pose should be interpolated");
        Assert.That(result.aimStateFlags, Is.EqualTo(current.aimStateFlags), "Flags should not be interpolated");
    }

    [Test]
    public void InterpolateAimState_DoesNotInterpolateToInvalidPose()
    {
        var current = new XRHandAimState
        {
            aimPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            aimStateFlags = AimStateFlags.HasAimPose,
        };

        var next = new XRHandAimState
        {
            aimPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            aimStateFlags = AimStateFlags.None,
        };

        var result = PlaybackInterpolator.InterpolateAimState(current, next, 0.8f);

        Assert.That(result.aimPoseInternal, Is.EqualTo(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity)), "Aim pose should not be interpolated to invalid pose");
        Assert.That(result.aimStateFlags, Is.EqualTo(current.aimStateFlags), "Flags should not be interpolated");
    }

    [Test]
    public void InterpolateCommonGesturesState_InterpolatesValues()
    {
        var current = new XRCommonHandGesturesState
        {
            aimActivateValueInternal = 0.2f,
            graspValueInternal = 0.3f,
            pinchValueInternal = 0f,
            isAimActivatedInternal = false,
            isGraspFirmInternal = false,
            isPinchTouchedInternal = false,
            aimPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            gripPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            pinchPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            pokePoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            flags =
                XRCommonHandGesturesFlags.IsAimPoseValid |
                XRCommonHandGesturesFlags.IsGripPoseValid |
                XRCommonHandGesturesFlags.IsPinchPoseValid |
                XRCommonHandGesturesFlags.IsPokePoseValid |
                XRCommonHandGesturesFlags.IsAimActivateValueValid |
                XRCommonHandGesturesFlags.IsGraspValueValid |
                XRCommonHandGesturesFlags.IsPinchValueValid |
                XRCommonHandGesturesFlags.IsAimActivatedStateValid |
                XRCommonHandGesturesFlags.IsGraspFirmStateValid |
                XRCommonHandGesturesFlags.IsPinchTouchedStateValid,
        };

        var next = new XRCommonHandGesturesState
        {
            aimActivateValueInternal = 0.8f,
            graspValueInternal = 0.7f,
            pinchValueInternal = 1f,
            isAimActivatedInternal = true,
            isGraspFirmInternal = true,
            isPinchTouchedInternal = true,
            aimPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            gripPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            pinchPoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            pokePoseInternal = new Pose(new Vector3(20f, 0f, 0f), Quaternion.identity),
            flags =
                XRCommonHandGesturesFlags.IsAimPoseValid |
                XRCommonHandGesturesFlags.IsGripPoseValid |
                XRCommonHandGesturesFlags.IsPinchPoseValid |
                XRCommonHandGesturesFlags.IsPokePoseValid |
                XRCommonHandGesturesFlags.IsAimActivateValueValid |
                XRCommonHandGesturesFlags.IsGraspValueValid |
                XRCommonHandGesturesFlags.IsPinchValueValid |
                XRCommonHandGesturesFlags.IsAimActivatedStateValid |
                XRCommonHandGesturesFlags.IsGraspFirmStateValid |
                XRCommonHandGesturesFlags.IsPinchTouchedStateValid,
        };

        var result = PlaybackInterpolator.InterpolateCommonGesturesState(current, next, 0.5f);

        const float range = 1e-5f;
        Assert.That(result.aimActivateValueInternal, Is.EqualTo(0.5f).Within(range));
        Assert.That(result.graspValueInternal, Is.EqualTo(0.5f).Within(range));
        Assert.That(result.pinchValueInternal, Is.EqualTo(0.5f).Within(range));
        Assert.That(result.isAimActivatedInternal, Is.EqualTo(current.isAimActivatedInternal));
        Assert.That(result.isGraspFirmInternal, Is.EqualTo(current.isGraspFirmInternal));
        Assert.That(result.isPinchTouchedInternal, Is.EqualTo(current.isPinchTouchedInternal));
        Assert.That(result.aimPoseInternal, Is.EqualTo(new Pose(new Vector3(15f, 0f, 0f), Quaternion.identity)));
        Assert.That(result.gripPoseInternal, Is.EqualTo(new Pose(new Vector3(15f, 0f, 0f), Quaternion.identity)));
        Assert.That(result.pinchPoseInternal, Is.EqualTo(new Pose(new Vector3(15f, 0f, 0f), Quaternion.identity)));
        Assert.That(result.pokePoseInternal, Is.EqualTo(new Pose(new Vector3(15f, 0f, 0f), Quaternion.identity)));
        Assert.That(result.flags, Is.EqualTo(current.flags), "Flags should not be interpolated");
    }

    [Test]
    public void InterpolateCommonGesturesState_DoesNotInterpolateToInvalidValues()
    {
        var current = new XRCommonHandGesturesState
        {
            aimActivateValueInternal = 0.2f,
            graspValueInternal = 0.3f,
            pinchValueInternal = 1f,
            isAimActivatedInternal = true,
            isGraspFirmInternal = true,
            isPinchTouchedInternal = true,
            aimPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            gripPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            pinchPoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            pokePoseInternal = new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity),
            flags =
                XRCommonHandGesturesFlags.IsAimPoseValid |
                XRCommonHandGesturesFlags.IsGripPoseValid |
                XRCommonHandGesturesFlags.IsPinchPoseValid |
                XRCommonHandGesturesFlags.IsPokePoseValid |
                XRCommonHandGesturesFlags.IsAimActivateValueValid |
                XRCommonHandGesturesFlags.IsGraspValueValid |
                XRCommonHandGesturesFlags.IsPinchValueValid |
                XRCommonHandGesturesFlags.IsAimActivatedStateValid |
                XRCommonHandGesturesFlags.IsGraspFirmStateValid |
                XRCommonHandGesturesFlags.IsPinchTouchedStateValid,
        };

        var next = new XRCommonHandGesturesState
        {
            flags = XRCommonHandGesturesFlags.None,
        };

        var result = PlaybackInterpolator.InterpolateCommonGesturesState(current, next, 0.8f);

        const float range = 1e-5f;
        Assert.That(result.aimActivateValueInternal, Is.EqualTo(current.aimActivateValueInternal).Within(range));
        Assert.That(result.graspValueInternal, Is.EqualTo(current.graspValueInternal).Within(range));
        Assert.That(result.pinchValueInternal, Is.EqualTo(current.pinchValueInternal).Within(range));
        Assert.That(result.isAimActivatedInternal, Is.EqualTo(current.isAimActivatedInternal));
        Assert.That(result.isGraspFirmInternal, Is.EqualTo(current.isGraspFirmInternal));
        Assert.That(result.isPinchTouchedInternal, Is.EqualTo(current.isPinchTouchedInternal));
        Assert.That(result.aimPoseInternal, Is.EqualTo(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity)), "Pose should not be interpolated to invalid pose");
        Assert.That(result.gripPoseInternal, Is.EqualTo(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity)), "Pose should not be interpolated to invalid pose");
        Assert.That(result.pinchPoseInternal, Is.EqualTo(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity)), "Pose should not be interpolated to invalid pose");
        Assert.That(result.pokePoseInternal, Is.EqualTo(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity)), "Pose should not be interpolated to invalid pose");
        Assert.That(result.flags, Is.EqualTo(current.flags), "Flags should not be interpolated");
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
