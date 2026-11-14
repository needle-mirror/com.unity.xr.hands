using System;

namespace UnityEngine.XR.Hands
{
    // Bit-flag form of XRHandMicrogesture. Backs XRCommonHandGesturesState storage and change detection; the public
    // API surfaces XRHandMicrogesture through XRCommonHandGestures.IsMicrogestureActive and microgestureUpdated.
    [Flags]
    enum XRHandMicrogestureFlags : uint
    {
        None = 0,
        SwipeLeft = 1 << 0,
        SwipeRight = 1 << 1,
        SwipeForward = 1 << 2,
        SwipeBackward = 1 << 3,
        TapThumb = 1 << 4,
    }
}
