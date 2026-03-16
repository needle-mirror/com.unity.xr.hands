using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Playback;

/// <summary>
/// Unit tests for coordinate transformation classes extracted in Phase 1 of the refactoring.
/// These tests validate the behavior of WorldSpaceCoordinateTransform and RelativeToInitialRootTransform.
/// </summary>
class CoordinateTransformTests
{
    [Test]
    public void WorldSpaceTransform_WithIdentityAnchor_ReturnsIdenticalPose()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor = Pose.identity;
        var initialFrame = default(XRHandCaptureFrame); // Not used in this transform
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 2, 3), Quaternion.Euler(0, 45, 0));

        var result = transform.TransformPose(inputPose);

        Assert.AreEqual(inputPose.position, result.position, "Position should be identical with identity anchor");
        Assert.Less(Quaternion.Angle(inputPose.rotation, result.rotation), 0.01f, "Rotation should be identical with identity anchor");
    }

    [Test]
    public void WorldSpaceTransform_WithTranslatedAnchor_AppliesTranslation()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchorPosition = new Vector3(10, 0, 0);
        var anchor = new Pose(anchorPosition, Quaternion.identity);
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 2, 3), Quaternion.identity);

        var result = transform.TransformPose(inputPose);

        Vector3 expectedPosition = new Vector3(11, 2, 3); // anchorPosition + inputPose.position
        Assert.AreEqual(expectedPosition, result.position, "Position should include anchor translation");
    }

    [Test]
    public void WorldSpaceTransform_WithRotatedAnchor_AppliesRotation()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor = new Pose(Vector3.zero, Quaternion.Euler(0, 90, 0));
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 0, 0), Quaternion.identity);

        var result = transform.TransformPose(inputPose);

        // After 90-degree Y rotation, (1,0,0) should become approximately (0,0,-1)
        Assert.AreEqual(0f, result.position.x, 0.001f, "X should be approximately 0");
        Assert.AreEqual(0f, result.position.y, 0.001f, "Y should be approximately 0");
        Assert.AreEqual(-1f, result.position.z, 0.001f, "Z should be approximately -1");
    }

    [Test]
    public void WorldSpaceTransform_UpdateAnchor_ChangesTransformation()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor1 = new Pose(new Vector3(0, 0, 0), Quaternion.identity);
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor1, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 2, 3), Quaternion.identity);
        var result1 = transform.TransformPose(inputPose);

        var anchor2 = new Pose(new Vector3(10, 0, 0), Quaternion.identity);
        transform.UpdateAnchor(anchor2);
        var result2 = transform.TransformPose(inputPose);

        Assert.AreNotEqual(result1.position, result2.position, "Position should change after anchor update");
        Assert.AreEqual(new Vector3(11, 2, 3), result2.position, "Updated anchor should be applied");
    }

    [Test]
    public void RelativeToInitialRootTransform_WithNoHandData_UsesIdentityFallback()
    {
        var transform = new RelativeToInitialRootTransform();
        var anchor = Pose.identity;
        var emptyFrame = default(XRHandCaptureFrame); // Frame with no hand data
        transform.UpdateRootPose(anchor, emptyFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 2, 3), Quaternion.identity);

        var result = transform.TransformPose(inputPose);

        // When no initial hand data is available, identity transforms are used, so output should match input.
        Assert.AreEqual(inputPose.position, result.position, "With identity fallback and identity anchor, position should match input");
        Assert.Less(Quaternion.Angle(inputPose.rotation, result.rotation), 0.01f, "With identity fallback and identity anchor, rotation should match input");
    }

    [Test]
    public void TransformPose_WithMultipleSequentialCalls_RemainsStable()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor = new Pose(new Vector3(5, 5, 5), Quaternion.Euler(0, 45, 0));
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30));

        var result1 = transform.TransformPose(inputPose);
        var result2 = transform.TransformPose(inputPose);
        var result3 = transform.TransformPose(inputPose);

        Assert.AreEqual(result1.position, result2.position, "Sequential calls should produce identical positions");
        Assert.AreEqual(result1.position, result3.position, "Sequential calls should produce identical positions");
        Assert.AreEqual(result1.rotation, result2.rotation, "Sequential calls should produce identical rotations");
        Assert.AreEqual(result1.rotation, result3.rotation, "Sequential calls should produce identical rotations");
    }

    [Test]
    public void WorldSpaceTransform_WithZeroScale_HandlesGracefully()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor = Pose.identity;
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(Vector3.zero, Quaternion.identity);

        var result = transform.TransformPose(inputPose);

        Assert.AreEqual(Vector3.zero, result.position, "Zero input should produce zero output with identity anchor");
        Assert.AreEqual(Quaternion.identity, result.rotation, "Identity rotation should be preserved");
    }

    [Test]
    public void WorldSpaceTransform_WithExtremeValues_DoesNotCrash()
    {
        var transform = new WorldSpaceCoordinateTransform();
        var anchor = new Pose(new Vector3(10000, 10000, 10000), Quaternion.identity);
        var initialFrame = default(XRHandCaptureFrame);
        transform.UpdateRootPose(anchor, initialFrame, Handedness.Left);

        var inputPose = new Pose(new Vector3(10000, 10000, 10000), Quaternion.identity);

        Assert.DoesNotThrow(() => transform.TransformPose(inputPose), "Should handle extreme values without crashing");
    }
}
