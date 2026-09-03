using Unity.Collections;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Static utility class providing interpolation functions for hand tracking playback.
    /// Centralizes all blend scalar calculations and interpolation algorithms to eliminate
    /// code duplication and improve testability.
    /// </summary>
    static class PlaybackInterpolator
    {
        /// <summary>
        /// Calculates a blend scalar (t value) for interpolation based on timestamps.
        /// Returns a value in [0,1] representing how far elapsed time is between current and next.
        /// </summary>
        /// <param name="currentTime">The timestamp of the current frame.</param>
        /// <param name="nextTime">The timestamp of the next frame.</param>
        /// <param name="elapsedTime">The current elapsed time to interpolate at.</param>
        /// <returns>A clamped value between 0 and 1 representing interpolation position.</returns>
        public static float CalculateBlendScalar(
            float currentTime,
            float nextTime,
            float elapsedTime)
        {
            float frameDuration = nextTime - currentTime;
            if (frameDuration < Constants.k_Epsilon)
                return 0f;

            float timeIntoFrame = elapsedTime - currentTime;
            return Mathf.Clamp01(timeIntoFrame / frameDuration);
        }

        /// <summary>
        /// Calculates a blend scalar for interpolation between two capture frames.
        /// Convenience overload that extracts timestamps from frames.
        /// </summary>
        /// <param name="currentFrame">The current frame.</param>
        /// <param name="nextFrame">The next frame.</param>
        /// <param name="elapsedTime">The current elapsed time to interpolate at.</param>
        /// <returns>A clamped value between 0 and 1 representing interpolation position.</returns>
        public static float CalculateBlendScalar(
            XRHandCaptureFrame currentFrame,
            XRHandCaptureFrame nextFrame,
            float elapsedTime)
        {
            return CalculateBlendScalar(
                currentFrame.timestamp,
                nextFrame.timestamp,
                elapsedTime);
        }

        /// <summary>
        /// Linearly interpolates between two poses using position lerp and rotation slerp.
        /// </summary>
        /// <param name="start">The starting pose.</param>
        /// <param name="end">The ending pose.</param>
        /// <param name="t">The interpolation parameter, typically in [0,1].</param>
        /// <returns>The interpolated pose.</returns>
        public static Pose InterpolatePose(in Pose start, in Pose end, float t)
        {
            return new Pose(
                Vector3.Lerp(start.position, end.position, t),
                Quaternion.Slerp(start.rotation, end.rotation, t));
        }

        /// <summary>
        /// Linearly interpolates between two float values.
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="end">The ending value.</param>
        /// <param name="t">The interpolation parameter, typically in [0,1].</param>
        /// <returns>The interpolated value.</returns>
        public static float InterpolateValue(in float start, in float end, float t)
        {
            return Mathf.Lerp(start, end, t);
        }

        /// <summary>
        /// Interpolates all joints between two hand states.
        /// Blends positions, rotations, radii, and tracking states for each joint.
        /// </summary>
        /// <param name="handBefore">The hand state before the current time.</param>
        /// <param name="handAfter">The hand state after the current time.</param>
        /// <param name="blendScalar">The blend parameter in [0,1].</param>
        /// <param name="handedness">The handedness of the hand being interpolated.</param>
        /// <param name="outputJoints">Output array to write interpolated joints to.</param>
        /// <returns>Returns <see langword="true"/> if interpolation succeeded, <see langword="false"/> if hands are not tracked.</returns>
        public static bool TryInterpolateJoints(
            in XRHand handBefore,
            in XRHand handAfter,
            float blendScalar,
            Handedness handedness,
            NativeArray<XRHandJoint> outputJoints)
        {
            if (!handBefore.isTracked || !handAfter.isTracked)
                return false;

            for (int jointIndex = 0; jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            {
                var jointID = XRHandJointIDUtility.FromIndex(jointIndex);

                var jointBefore = handBefore.GetJoint(jointID);
                var jointAfter = handAfter.GetJoint(jointID);

                if (!jointBefore.TryGetPose(out var poseBefore) || !jointAfter.TryGetPose(out var poseAfter))
                {
                    // Write an explicit untracked joint rather than skipping, so we don't leave
                    // stale data from a previous frame in the outputJoints array.
                    outputJoints[jointIndex] = XRHandProviderUtility.CreateJoint(
                        handedness,
                        XRHandJointTrackingState.None,
                        jointID,
                        Pose.identity);
                    continue;
                }

                // Interpolate pose
                var interpolatedPose = InterpolatePose(poseBefore, poseAfter, blendScalar);

                // Interpolate radius if available
                float interpolatedRadius = 0f;
                if (jointBefore.TryGetRadius(out var radiusBefore) &&
                    jointAfter.TryGetRadius(out var radiusAfter))
                {
                    interpolatedRadius = InterpolateValue(radiusBefore, radiusAfter, blendScalar);
                }

                // Interpolate velocities if available
                Vector3 interpolatedLinearVelocity = Vector3.zero;
                if (jointBefore.TryGetLinearVelocity(out var linearVelBefore) &&
                    jointAfter.TryGetLinearVelocity(out var linearVelAfter))
                {
                    interpolatedLinearVelocity = Vector3.Lerp(linearVelBefore, linearVelAfter, blendScalar);
                }

                Vector3 interpolatedAngularVelocity = Vector3.zero;
                if (jointBefore.TryGetAngularVelocity(out var angularVelBefore) &&
                    jointAfter.TryGetAngularVelocity(out var angularVelAfter))
                {
                    interpolatedAngularVelocity = Vector3.Lerp(angularVelBefore, angularVelAfter, blendScalar);
                }

                // Blend tracking state (use threshold)
                var trackingState = blendScalar < 0.5f
                    ? jointBefore.trackingState
                    : jointAfter.trackingState;

                // Create interpolated joint
                outputJoints[jointIndex] = XRHandProviderUtility.CreateJoint(
                    handedness,
                    trackingState,
                    jointID,
                    interpolatedPose,
                    interpolatedRadius,
                    interpolatedLinearVelocity,
                    interpolatedAngularVelocity);
            }

            return true;
        }

        /// <summary>
        /// Interpolates common gestures state between two frames.
        /// Blends poses and float values. Booleans and flags are never interpolated.
        /// </summary>
        /// <param name="current">The current common gestures state.</param>
        /// <param name="next">The next common gestures state.</param>
        /// <param name="blendScalar">The blend parameter in [0,1].</param>
        /// <return>Returns interpolated common gestures state.</return>
        public static XRCommonHandGesturesState InterpolateCommonGesturesState(
            in XRCommonHandGesturesState current,
            in XRCommonHandGesturesState next,
            float blendScalar)
        {
            var interpolated = current;

            var currentFlags = current.flags;
            var nextFlags = next.flags;

            // Interpolate poses if both are valid.
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimPoseValid))
                interpolated.aimPoseInternal = InterpolatePose(current.aimPoseInternal, next.aimPoseInternal, blendScalar);
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGripPoseValid))
                interpolated.gripPoseInternal = InterpolatePose(current.gripPoseInternal, next.gripPoseInternal, blendScalar);
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchPoseValid))
                interpolated.pinchPoseInternal = InterpolatePose(current.pinchPoseInternal, next.pinchPoseInternal, blendScalar);
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPokePoseValid))
                interpolated.pokePoseInternal = InterpolatePose(current.pokePoseInternal, next.pokePoseInternal, blendScalar);

            // Interpolate float values if both are valid.
            // We could potentially still interpolate from some value to 0 when the next state's float property is not ready for smoother
            // updates, but the PlaybackGestureHandler currently does this same valid checking for the individual TryGet- methods.
            // So this is kept as a match for that behavior.
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsAimActivateValueValid))
                interpolated.aimActivateValueInternal = InterpolateValue(current.aimActivateValueInternal, next.aimActivateValueInternal, blendScalar);
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsGraspValueValid))
                interpolated.graspValueInternal = InterpolateValue(current.graspValueInternal, next.graspValueInternal, blendScalar);
            if (currentFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid) && nextFlags.HasGesturesFlag(XRCommonHandGesturesFlags.IsPinchValueValid))
                interpolated.pinchValueInternal = InterpolateValue(current.pinchValueInternal, next.pinchValueInternal, blendScalar);

            return interpolated;
        }

        /// <summary>
        /// Interpolates aim state between two frames.
        /// Blends aim pose and pinch values. Flags are never interpolated.
        /// </summary>
        /// <param name="current">The current aim state.</param>
        /// <param name="next">The next aim state.</param>
        /// <param name="blendScalar">The blend parameter in [0,1].</param>
        /// <return>Returns interpolated aim state.</return>
        public static XRHandAimState InterpolateAimState(
            in XRHandAimState current,
            in XRHandAimState next,
            float blendScalar)
        {
            var interpolated = current;

            // Interpolate pinch strength values
            interpolated.pinchStrengthIndex = InterpolateValue(
                current.pinchStrengthIndex,
                next.pinchStrengthIndex,
                blendScalar);

            interpolated.pinchStrengthMiddle = InterpolateValue(
                current.pinchStrengthMiddle,
                next.pinchStrengthMiddle,
                blendScalar);

            interpolated.pinchStrengthRing = InterpolateValue(
                current.pinchStrengthRing,
                next.pinchStrengthRing,
                blendScalar);

            interpolated.pinchStrengthLittle = InterpolateValue(
                current.pinchStrengthLittle,
                next.pinchStrengthLittle,
                blendScalar);

            // Interpolate aim pose if both are available
            if (current.TryGetAimPose(out var currentAimPose) &&
                next.TryGetAimPose(out var nextAimPose))
            {
                interpolated.aimPoseInternal = InterpolatePose(currentAimPose, nextAimPose, blendScalar);
            }

            return interpolated;
        }
    }
}
