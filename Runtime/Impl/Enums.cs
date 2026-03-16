using System;

namespace UnityEngine.XR.Hands
{
    [Flags]
    enum AimFlags
    {
        None = 0,
        IsTracked = 1 << 0,
        IsIndexPressed = 1 << 1,
        IsMiddlePressed = 1 << 2,
        IsRingPressed = 1 << 3,
        IsLittlePressed = 1 << 4,
        IsAimPoseValid = 1 << 5,
    }
}
