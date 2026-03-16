using System;

namespace UnityEngine.XR.Hands.Capture
{
    [Flags]
    enum SequenceFlags
    {
        None = 0,
        CanSurfaceCommonPoseData = 1 << 0,
        SupportsAimPose = 1 << 1,
        SupportsAimActivateValue = 1 << 2,
        SupportsGraspValue = 1 << 3,
        SupportsGripPose = 1 << 4,
        SupportsPinchPose = 1 << 5,
        SupportsPinchValue = 1 << 6,
        SupportsPokePose = 1 << 7,
    }

    [Flags]
    enum FrameFlags
    {
        None = 0,

        // these two need to stay first so conversion from Handedness
        // doesn't need additional offsetting
        IsLeftSnapshotValid = 1 << 0,
        IsRightSnapshotValid = 1 << 1,

        IsLeftCommonGesturesValid = 1 << 2,
        IsRightCommonGesturesValid = 1 << 3,
        IsLeftAimStateValid = 1 << 4,
        IsRightAimStateValid = 1 << 5,
    }

    [Flags]
    enum SnapshotFlags
    {
        None = 0,

        // these two need to stay first so conversion from UpdateType
        // doesn't need additional offsetting
        IsDynamicHandValid = 1 << 0,
        IsBeforeRenderHandValid = 1 << 1,
    }

    [Flags]
    enum HandFlags
    {
        None = 0,
        AreAllJointPosesValid = 1 << 0,
        WasHandTrackedDuringCapture = 1 << 1,
    }
}
