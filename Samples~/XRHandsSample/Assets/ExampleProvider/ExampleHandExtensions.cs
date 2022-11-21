using System;

namespace UnityEngine.XR.Hands.Example
{
    public enum Finger
    {
        Invalid,
        Thumb,
        Index,
        Middle,
        Ring,
        Little
    }

    public enum FingerConfidence
    {
        None,
        Low,
        High
    }

    public static class ExampleHandExtensions
    {
        public static bool IsFistClosed(this XRHand hand) => subsystem.IsFistClosed(hand.handedness);

        public static FingerConfidence GetConfidence(this XRHand hand, Finger finger)
        {
            if (finger == Finger.Invalid)
                return FingerConfidence.None;

            if (hand.IsFistClosed() && finger == Finger.Little)
                return FingerConfidence.Low;

            return FingerConfidence.High;
        }

        public static int GetKey(this XRHand hand, XRHandJoint joint)
        {
            if (!joint.Equals(hand.GetJoint(joint.id)))
                throw new InvalidOperationException("The supplied joint either belongs to a different hand, or the hand and joint were retrieved from different frames.");

            // actual platform-specific data may need to look up data in the
            // provider, this is just an example an access pattern for how to
            // expose joint-specific data
            return (int)(hand.handedness - 1) * XRHandJointID.EndMarker.ToIndex() + joint.id.ToIndex();
        }

        internal static ExampleHandSubsystem subsystem { get; set; }
    }
}
