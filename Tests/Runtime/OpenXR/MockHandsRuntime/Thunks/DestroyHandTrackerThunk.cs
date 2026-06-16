#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime
{
    /// <summary>
    /// Exception-safe native callback thunk for <c>xrDestroyHandTrackerEXT</c>.
    /// Catches exceptions thrown by the mock delegate to prevent them from
    /// crossing the native/managed boundary.
    /// </summary>
    class DestroyHandTrackerThunk
        : NativeCallbackThunk<DestroyHandTrackerThunk, DestroyHandTrackerThunk.Delegate>
    {
        public delegate XrResult Delegate(XrHandTrackerEXT handTracker);

        public DestroyHandTrackerThunk() : base(Interceptor) { }

        [MonoPInvokeCallback(typeof(Delegate))]
        static XrResult Interceptor(XrHandTrackerEXT handTracker)
        {
            if (s_Current?.mock == null) return XrResult.RuntimeFailure;
            try { return s_Current.mock(handTracker); }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }
    }
}
#endif
