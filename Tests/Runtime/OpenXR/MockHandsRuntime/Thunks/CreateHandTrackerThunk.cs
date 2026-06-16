#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime
{
    /// <summary>
    /// Exception-safe native callback thunk for <c>xrCreateHandTrackerEXT</c>.
    /// Catches exceptions thrown by the mock delegate to prevent them from
    /// crossing the native/managed boundary.
    /// </summary>
    class CreateHandTrackerThunk
        : NativeCallbackThunk<CreateHandTrackerThunk, CreateHandTrackerThunk.Delegate>
    {
        public delegate XrResult Delegate(
            XrSession session,
            in XrHandTrackerCreateInfoEXT info,
            out XrHandTrackerEXT tracker);

        public CreateHandTrackerThunk() : base(Interceptor) { }

        [MonoPInvokeCallback(typeof(Delegate))]
        static XrResult Interceptor(
            XrSession session,
            in XrHandTrackerCreateInfoEXT info,
            out XrHandTrackerEXT tracker)
        {
            tracker = 0;
            if (s_Current?.mock == null) return XrResult.FunctionUnsupported;
            try { return s_Current.mock(session, in info, out tracker); }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }
    }
}
#endif
