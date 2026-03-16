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
        /// <returns>True if interpolation succeeded, false if hands are not tracked.</returns>
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
        /// Interpolates aim state between two frames.
        /// Blends aim pose, pinch values, poke values, and tracking flags.
        /// </summary>
        /// <param name="current">The current aim state.</param>
        /// <param name="next">The next aim state.</param>
        /// <param name="blendScalar">The blend parameter in [0,1].</param>
        /// <param name="interpolated">Output interpolated aim state.</param>
        /// <returns>True if interpolation succeeded.</returns>
        public static bool TryInterpolateAimState(
            in XRHandAimState current,
            in XRHandAimState next,
            float blendScalar,
            out XRHandAimState interpolated)
        {
            interpolated = current;

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
                var interpolatedAimPose = InterpolatePose(currentAimPose, nextAimPose, blendScalar);
                interpolated.SetAimPose(interpolatedAimPose);
            }

            return true;
        }
    }
}
