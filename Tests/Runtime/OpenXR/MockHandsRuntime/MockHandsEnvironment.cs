#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.TestTooling;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime
{
    class MockHandsEnvironment : IDisposable
    {
        public const XrHandTrackerEXT k_LeftTrackerHandle = 0x1UL;
        public const XrHandTrackerEXT k_RightTrackerHandle = 0x2UL;

        const string k_HandTrackingExtensionName = "XR_EXT_hand_tracking";
        // Current version of XR_EXT_hand_tracking in the OpenXR spec.
        const uint k_DefaultHandTrackingExtensionVersion = 4;

        static bool s_ProviderRegistered;

        // System properties callback — not per-test-settable, stays static
        unsafe delegate XrResult GetSystemProperties_Delegate(
            XrBaseInStructure* systemPropertiesStruct);

        static readonly unsafe GetSystemProperties_Delegate s_SystemPropertiesDelegate =
            SystemProperties_HandTracking_MockCallback;

        static readonly unsafe IntPtr k_SystemPropertiesPtr =
            Marshal.GetFunctionPointerForDelegate(s_SystemPropertiesDelegate);

        readonly MockOpenXREnvironment m_Environment;

        public CreateHandTrackerThunk createHandTracker { get; }
        public LocateHandJointsThunk locateHandJoints { get; }
        public DestroyHandTrackerThunk destroyHandTracker { get; }

        public MockOpenXREnvironment Environment => m_Environment;

        public MockHandsEnvironment()
        {
            m_Environment = MockOpenXREnvironment.CreateEnvironment();
            createHandTracker = new CreateHandTrackerThunk();
            locateHandJoints = new LocateHandJointsThunk();
            destroyHandTracker = new DestroyHandTrackerThunk();
        }

        public void SetUpDefaultHandTrackingEnvironment(
            uint version = k_DefaultHandTrackingExtensionVersion)
        {
            m_Environment.AddSupportedExtension(k_HandTrackingExtensionName, version);
            m_Environment.Settings.RequestUseExtension(MockRuntime.XR_UNITY_null_gfx);
            m_Environment.Settings.EnableFeature<HandTracking>(true);
            RegisterProviderOnce();
        }

        public void RegisterProviderOnce()
        {
            if (s_ProviderRegistered)
                return;

            OpenXRHandProvider.Register();
            s_ProviderRegistered = true;
        }

        public void Start()
        {
            m_Environment.SetSysPropertiesFunctionForXrStructureType(
                (uint)XrStructureType.SystemHandTrackingPropertiesEXT,
                k_SystemPropertiesPtr);

            m_Environment.SetFunctionForInterceptor(
                "xrCreateHandTrackerEXT", createHandTracker.FunctionPointer);
            m_Environment.SetFunctionForInterceptor(
                "xrLocateHandJointsEXT", locateHandJoints.FunctionPointer);
            m_Environment.SetFunctionForInterceptor(
                "xrDestroyHandTrackerEXT", destroyHandTracker.FunctionPointer);

            createHandTracker.Activate();
            locateHandJoints.Activate();
            destroyHandTracker.Activate();

            m_Environment.Start();
        }

        public void Stop()
        {
            // NOTE: Order matters here. The native interops need to be active for longer than
            // the instance because features will clean up in session and instance loss. Keep
            // them alive until the environment is done cleaning up. This allows tests to validate
            // destruction cleanly.
            m_Environment?.Stop();

            createHandTracker.Deactivate();
            locateHandJoints.Deactivate();
            destroyHandTracker.Deactivate();
        }

        public static XrResult SuccessfulCreateHandTracker(
            XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker)
        {
            tracker = info.hand == XrHandEXT.Left ? k_LeftTrackerHandle : k_RightTrackerHandle;
            return XrResult.Success;
        }

        public static XrResult SuccessfulDestroyHandTracker(XrHandTrackerEXT tracker) => XrResult.Success;

        public void Dispose()
        {
            Stop();
            m_Environment?.Dispose();
        }

        [MonoPInvokeCallback(typeof(GetSystemProperties_Delegate))]
        static unsafe XrResult SystemProperties_HandTracking_MockCallback(
            XrBaseInStructure* systemPropertiesStruct)
        {
            if (systemPropertiesStruct == null)
                return XrResult.ValidationFailure;

            if (systemPropertiesStruct->type != XrStructureType.SystemHandTrackingPropertiesEXT)
                return XrResult.ValidationFailure;

            *(XrSystemHandTrackingPropertiesEXT*)systemPropertiesStruct =
                new XrSystemHandTrackingPropertiesEXT(systemPropertiesStruct->next, supportsHandTracking: true);

            return XrResult.Success;
        }
    }
}
#endif
