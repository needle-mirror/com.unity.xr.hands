using NUnit.Framework;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Processing;
using UnityEngine.XR.Hands.ProviderImplementation;

class UnextrapolatedPosesTests
{
    const long k_SecondCaptureTime = TestHandData.unextrapolatedCaptureTime + 5000L;

    // The next value a new member would take, so this also fails if one is added without updating IsValid.
    const XRHandExtrapolation k_UndefinedExtrapolation = (XRHandExtrapolation)2;

    static XRHandSubsystem CreateAndStartSubsystem(
        bool unextrapolatedPosesSupported, out TestHandProvider provider)
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        provider = subsystem.GetProvider() as TestHandProvider;
        Assert.IsNotNull(provider);
        provider.unextrapolatedPosesSupported = unextrapolatedPosesSupported;
        subsystem.Start();
        return subsystem;
    }

    [Test]
    public void CapabilityAbsent_UnextrapolatedHandIsUnavailable()
    {
        var subsystem = CreateAndStartSubsystem(false, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        foreach (var handedness in TestHandUtils.handednessOptions)
        {
            Assert.IsFalse(
                subsystem.TryGetHand(handedness, XRHandExtrapolation.Unextrapolated, out _),
                $"{handedness}: unextrapolated poses should be unavailable without provider support.");

            var exception = Assert.Throws<System.InvalidOperationException>(
                () => subsystem.GetHand(handedness, XRHandExtrapolation.Unextrapolated));
            Assert.That(exception.Message, Does.Contain("Unextrapolated poses are unavailable"));

            Assert.IsTrue(
                subsystem.TryGetHand(handedness, XRHandExtrapolation.Extrapolated, out var extrapolatedHand),
                $"{handedness}: extrapolated poses should still resolve.");
            TestHandUtils.AssertAreApproximatelyEqual(
                handedness == Handedness.Left ? TestHandData.leftRoot : TestHandData.rightRoot,
                extrapolatedHand.rootPose);
        }

        subsystem.Destroy();
    }

    [Test]
    public void CapabilityPresent_SurfacesHandWithItsMode([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        Assert.IsTrue(subsystem.TryGetHand(handedness, XRHandExtrapolation.Unextrapolated, out var unextrapolatedHand));
        Assert.AreEqual(XRHandExtrapolation.Unextrapolated, unextrapolatedHand.extrapolationMode);
        Assert.AreEqual(handedness, unextrapolatedHand.handedness);

        Assert.IsTrue(subsystem.TryGetHand(handedness, XRHandExtrapolation.Extrapolated, out var extrapolatedHand));
        Assert.AreEqual(XRHandExtrapolation.Extrapolated, extrapolatedHand.extrapolationMode);
        Assert.AreEqual(handedness, extrapolatedHand.handedness);

        subsystem.Destroy();
    }

    [Test]
    public void UnextrapolatedPosesUpdateOnlyOnDynamic()
    {
        var subsystem = CreateAndStartSubsystem(true, out var provider);

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);
        Assert.AreEqual(0, provider.numTryUpdateUnextrapolatedPosesCalls,
            "BeforeRender should not request unextrapolated poses.");

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.AreEqual(TestHandUtils.handednessOptions.Length, provider.numTryUpdateUnextrapolatedPosesCalls,
            "Dynamic should request unextrapolated poses exactly once per hand.");

        subsystem.Destroy();
    }

    [Test]
    public void ExtrapolatedAndUnextrapolatedHandsHoldDistinctData([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var isLeftHand = handedness == Handedness.Left;
        var expectedExtrapolatedRoot = isLeftHand ? TestHandData.leftRoot : TestHandData.rightRoot;

        var extrapolatedHand = subsystem.GetHand(handedness, XRHandExtrapolation.Extrapolated);
        TestHandUtils.AssertAreApproximatelyEqual(expectedExtrapolatedRoot, extrapolatedHand.rootPose);

        var unextrapolatedHand = subsystem.GetHand(handedness, XRHandExtrapolation.Unextrapolated);
        TestHandUtils.AssertAreApproximatelyEqual(
            TestHandData.ToUnextrapolated(expectedExtrapolatedRoot), unextrapolatedHand.rootPose);

        var expectedExtrapolatedJoints = isLeftHand ? TestHandData.leftHand : TestHandData.rightHand;
        for (var jointIndex = 0; jointIndex < TestHandData.jointsInLayout.Length; ++jointIndex)
        {
            if (!TestHandData.jointsInLayout[jointIndex])
                continue;

            var jointId = XRHandJointIDUtility.FromIndex(jointIndex);

            Assert.IsTrue(extrapolatedHand.GetJoint(jointId).TryGetPose(out var extrapolatedPose));
            TestHandUtils.AssertAreApproximatelyEqual(expectedExtrapolatedJoints[jointIndex], extrapolatedPose);

            Assert.IsTrue(unextrapolatedHand.GetJoint(jointId).TryGetPose(out var unextrapolatedPose));
            TestHandUtils.AssertAreApproximatelyEqual(
                TestHandData.ToUnextrapolated(expectedExtrapolatedJoints[jointIndex]), unextrapolatedPose);
        }

        var otherHandedness = isLeftHand ? Handedness.Right : Handedness.Left;
        var otherUnextrapolatedHand = subsystem.GetHand(otherHandedness, XRHandExtrapolation.Unextrapolated);
        Assert.AreNotEqual(unextrapolatedHand.rootPose, otherUnextrapolatedHand.rootPose,
            "Each hand should carry its own unextrapolated data.");

        subsystem.Destroy();
    }

    [Test]
    public void CaptureTimeIsAbsentUntilATrackedUpdateReportsOne()
    {
        var subsystem = CreateAndStartSubsystem(true, out _);

        var beforeAnyUpdate = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
        Assert.IsFalse(beforeAnyUpdate.TryGetTimestamp(out var absentTimestamp),
            "No capture time should be reported before the first update.");
        Assert.AreEqual(0L, absentTimestamp);

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var tracked = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
        Assert.IsTrue(tracked.isTracked);
        Assert.IsTrue(tracked.TryGetTimestamp(out var trackedTimestamp));
        Assert.AreEqual(TestHandData.unextrapolatedCaptureTime, trackedTimestamp);

        subsystem.Destroy();
    }

    [Test]
    public void UntrackedHandRetainsItsCaptureTime()
    {
        var subsystem = CreateAndStartSubsystem(true, out var provider);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        provider.unextrapolatedPosesTracked = false;
        provider.unextrapolatedCaptureTime = k_SecondCaptureTime;
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var untracked = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
        Assert.IsFalse(untracked.isTracked);
        Assert.IsTrue(untracked.TryGetTimestamp(out var retainedTimestamp),
            "An untracked hand should keep the capture time of its last good poses.");
        Assert.AreEqual(TestHandData.unextrapolatedCaptureTime, retainedTimestamp,
            "The retained capture time should be the last tracked one, not the newer unused one.");

        subsystem.Destroy();
    }

    [Test]
    public void TrackedHandWithoutACaptureTimeClearsTheRetainedOne()
    {
        var subsystem = CreateAndStartSubsystem(true, out var provider);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        provider.unextrapolatedCaptureTime = null;
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var cleared = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
        Assert.IsTrue(cleared.isTracked);
        Assert.IsFalse(cleared.TryGetTimestamp(out var clearedTimestamp),
            "A tracked hand whose provider reports no capture time should clear the retained one.");
        Assert.AreEqual(0L, clearedTimestamp);

        subsystem.Destroy();
    }

    [Test]
    public void SetHandRoutesByExtrapolationMode([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.extrapolationOptions))] XRHandExtrapolation extrapolation)
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var otherExtrapolation = extrapolation == XRHandExtrapolation.Unextrapolated
            ? XRHandExtrapolation.Extrapolated
            : XRHandExtrapolation.Unextrapolated;

        var untouchedRootPoseBefore = subsystem.GetHand(Handedness.Left, otherExtrapolation).rootPose;

        var hand = subsystem.GetHand(Handedness.Left, extrapolation);
        var movedRootPose = new Pose(hand.rootPose.position + new Vector3(1f, 2f, 3f), hand.rootPose.rotation);
        hand.SetRootPose(movedRootPose);
        subsystem.SetCorrespondingHand(hand);

        TestHandUtils.AssertAreApproximatelyEqual(
            movedRootPose, subsystem.GetHand(Handedness.Left, extrapolation).rootPose);
        TestHandUtils.AssertAreApproximatelyEqual(
            untouchedRootPoseBefore, subsystem.GetHand(Handedness.Left, otherExtrapolation).rootPose);

        subsystem.Destroy();
    }

    [Test]
    public void SetHandRejectsInvalidHand()
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        Assert.Throws<System.ArgumentException>(() => subsystem.SetCorrespondingHand(default(XRHand)));

        subsystem.Destroy();
    }

    [Test]
    public void CapabilityBecomingAvailableMidSessionSurfacesTheHand()
    {
        var subsystem = CreateAndStartSubsystem(false, out var provider);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        Assert.IsFalse(subsystem.TryGetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated, out _));
        Assert.AreEqual(0, provider.numTryUpdateUnextrapolatedPosesCalls);

        provider.unextrapolatedPosesSupported = true;
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        Assert.IsTrue(subsystem.TryGetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated, out var hand),
            "The unextrapolated hand should surface once the provider reports support.");
        Assert.AreEqual(XRHandExtrapolation.Unextrapolated, hand.extrapolationMode);
        TestHandUtils.AssertAreApproximatelyEqual(
            TestHandData.ToUnextrapolated(TestHandData.leftRoot), hand.rootPose);

        subsystem.Destroy();
    }

    [Test]
    public void SingleArgumentAccessorsReturnTheExtrapolatedHand([ValueSource(typeof(TestHandUtils), nameof(TestHandUtils.handednessOptions))] Handedness handedness)
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var expectedRootPose = handedness == Handedness.Left ? TestHandData.leftRoot : TestHandData.rightRoot;

        var fromGetHand = subsystem.GetHand(handedness);
        Assert.AreEqual(XRHandExtrapolation.Extrapolated, fromGetHand.extrapolationMode);
        TestHandUtils.AssertAreApproximatelyEqual(expectedRootPose, fromGetHand.rootPose);

        Assert.IsTrue(subsystem.TryGetHand(handedness, out var fromTryGetHand));
        Assert.AreEqual(XRHandExtrapolation.Extrapolated, fromTryGetHand.extrapolationMode);
        TestHandUtils.AssertAreApproximatelyEqual(expectedRootPose, fromTryGetHand.rootPose);

        subsystem.Destroy();
    }

    [Test]
    public void EqualsAndGetHashCodeAccountForTimestamp()
    {
        var subsystem = CreateAndStartSubsystem(true, out var provider);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var firstCapture = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);

        provider.unextrapolatedCaptureTime = k_SecondCaptureTime;
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var laterCapture = subsystem.GetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated);
        Assert.IsFalse(firstCapture.Equals(laterCapture),
            "Hands differing only in timestamp should not be equal.");
        Assert.AreNotEqual(firstCapture.GetHashCode(), laterCapture.GetHashCode(),
            "GetHashCode should account for the timestamp.");

        subsystem.Destroy();
    }

    static void ClearJointTracking(XRHand hand)
    {
        var joints = hand.GetRawJointArray();
        for (var jointIndex = 0; jointIndex < joints.Length; ++jointIndex)
        {
            joints[jointIndex] = XRHandProviderUtility.CreateJoint(
                hand.handedness,
                XRHandJointTrackingState.None,
                XRHandJointIDUtility.FromIndex(jointIndex),
                Pose.identity);
        }
    }

    // The shared finger shape cache was keyed by handedness and finger alone, so the second mode
    // queried in an update was handed the first one's result without its joints ever being read.
    // Clearing the second hand's joint tracking makes that unambiguous: it can produce no value at
    // all, so any value coming back has to be the other mode's.
    [TestCase(XRHandExtrapolation.Extrapolated, XRHandExtrapolation.Unextrapolated, TestName = "ExtrapolatedFirst")]
    [TestCase(XRHandExtrapolation.Unextrapolated, XRHandExtrapolation.Extrapolated, TestName = "UnextrapolatedFirst")]
    public void FingerShapeCacheDoesNotLeakBetweenModes(
        XRHandExtrapolation first, XRHandExtrapolation second)
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        var firstHand = subsystem.GetHand(Handedness.Left, first);
        var secondHand = subsystem.GetHand(Handedness.Left, second);
        ClearJointTracking(secondHand);

        Assert.IsTrue(
            firstHand.CalculateFingerShape(XRHandFingerID.Index, XRFingerShapeTypes.FullCurl)
                .TryGetFullCurl(out _),
            $"{first}: a fully tracked hand should produce a full curl.");

        Assert.IsFalse(
            secondHand.CalculateFingerShape(XRHandFingerID.Index, XRFingerShapeTypes.FullCurl)
                .TryGetFullCurl(out _),
            $"{second}: an untracked hand must not be served the {first} hand's cached curl.");

        subsystem.Destroy();
    }

    [Test]
    public void UndefinedExtrapolationIsRejected()
    {
        var subsystem = CreateAndStartSubsystem(true, out _);
        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        foreach (var handedness in TestHandUtils.handednessOptions)
        {
            Assert.IsFalse(subsystem.TryGetHand(handedness, k_UndefinedExtrapolation, out _),
                $"{handedness}: an undefined extrapolation must not silently resolve to a defined mode.");

            var exception = Assert.Throws<System.ArgumentException>(
                () => subsystem.GetHand(handedness, k_UndefinedExtrapolation));
            Assert.AreEqual("extrapolation", exception.ParamName);
        }

        subsystem.Destroy();
    }
}
