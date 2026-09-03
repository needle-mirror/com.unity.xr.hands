#if OPENXR_1_19_OR_NEWER
using System;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime
{
    /// <summary>
    /// Exception-safe native callback thunk for <c>xrSetHandTrackingFrequencyHintMETA</c>.
    /// Catches exceptions thrown by the mock delegate to prevent them from
    /// crossing the native/managed boundary.
    /// </summary>
    class SetHandTrackingFrequencyHintThunk
        : NativeCallbackThunk<SetHandTrackingFrequencyHintThunk, SetHandTrackingFrequencyHintThunk.Delegate>
    {
        public delegate XrResult Delegate(XrSession session, int frequencyHint);

        public SetHandTrackingFrequencyHintThunk() : base(Interceptor) { }

        [MonoPInvokeCallback(typeof(Delegate))]
        static XrResult Interceptor(XrSession session, int frequencyHint)
        {
            if (s_Current?.mock == null) return XrResult.FunctionUnsupported;
            try { return s_Current.mock(session, frequencyHint); }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }
    }
}
#endif
