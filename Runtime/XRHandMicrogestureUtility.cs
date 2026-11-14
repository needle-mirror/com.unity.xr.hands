namespace UnityEngine.XR.Hands
{
    static class XRHandMicrogestureUtility
    {
        internal const XRHandMicrogestureFlags k_AllMicrogestures = XRHandMicrogestureFlags.SwipeLeft | XRHandMicrogestureFlags.SwipeRight |
            XRHandMicrogestureFlags.SwipeForward | XRHandMicrogestureFlags.SwipeBackward | XRHandMicrogestureFlags.TapThumb;

        internal const XRHandMicrogesture k_FirstMicrogesture = XRHandMicrogesture.SwipeLeft;
        internal const XRHandMicrogesture k_LastMicrogesture = XRHandMicrogesture.TapThumb;

        internal static XRHandMicrogestureFlags ToMicrogestureFlag(this XRHandMicrogesture microgesture)
        {
            switch (microgesture)
            {
                case XRHandMicrogesture.SwipeLeft: return XRHandMicrogestureFlags.SwipeLeft;
                case XRHandMicrogesture.SwipeRight: return XRHandMicrogestureFlags.SwipeRight;
                case XRHandMicrogesture.SwipeForward: return XRHandMicrogestureFlags.SwipeForward;
                case XRHandMicrogesture.SwipeBackward: return XRHandMicrogestureFlags.SwipeBackward;
                case XRHandMicrogesture.TapThumb: return XRHandMicrogestureFlags.TapThumb;
                default: return XRHandMicrogestureFlags.None;
            }
        }
    }
}
