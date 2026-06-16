#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime
{
    /// <summary>
    /// Mock extension struct for testing chain persistence.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    unsafe struct XrMockChainTestInfoEXT
    {
        public XrStructureType type;
        public void* next;
        public uint value;

        public const XrStructureType k_Type = (XrStructureType)0x7FFFFF20;

        public XrMockChainTestInfoEXT(uint value)
        {
            type = k_Type;
            next = null;
            this.value = value;
        }
    }

    /// <summary>
    /// Mock extension struct for testing create chain data flow.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    unsafe struct XrMockCreateExtensionInfoEXT
    {
        public XrStructureType type;
        public void* next;
        public uint testValue;

        public const XrStructureType k_Type = (XrStructureType)0x7FFFFF10;

        public XrMockCreateExtensionInfoEXT(uint testValue)
        {
            type = k_Type;
            next = null;
            this.testValue = testValue;
        }
    }

    /// <summary>
    /// Mock input extension struct for testing locate input chain data flow.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    unsafe struct XrMockLocateInputInfoEXT
    {
        public XrStructureType type;
        public void* next;
        public uint inputValue;

        public const XrStructureType k_Type = (XrStructureType)0x7FFFFF11;

        public XrMockLocateInputInfoEXT(uint inputValue)
        {
            type = k_Type;
            next = null;
            this.inputValue = inputValue;
        }
    }

    /// <summary>
    /// Mock output extension struct for testing locate output chain data flow.
    /// Modeled after a realistic OpenXR output extension with multiple typed
    /// fields (boolean, enum-like uint, float) to validate proper marshaling
    /// across the managed-native-managed round trip.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    unsafe struct XrMockLocateOutputStateEXT
    {
        public XrStructureType type;
        public void* next;
        public uint isActive;       // XrBool32
        public uint dataSource;     // enum-like field
        public float confidence;    // floating-point field

        public const XrStructureType k_Type = (XrStructureType)0x7FFFFF12;

        public static XrMockLocateOutputStateEXT defaultValue => new()
        {
            type = k_Type,
            next = null,
            isActive = 0,
            dataSource = 0,
            confidence = 0f,
        };
    }

#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "Test Hand Tracking Extension",
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Standalone },
        Hidden = true,
        Priority = -1,
        FeatureId = featureId)]
#endif
    class TestHandTrackingExtensionFeature : OpenXRHandTrackingFeature
    {
        const string featureId = "com.unity.xr.hands.tests.testhandtrackingextension";
        internal Action<ulong> instanceCreate;
        internal Action<ulong> sessionCreate;
        internal Action<XrHandEXT, XrStructureChain> handTrackingCreateRequest;
        internal Action<XrHandEXT, XrResult> handTrackerCreated;
        internal Action<XrHandEXT, XrResult> handTrackerDestroyed;

        internal Action<XrHandEXT, XrStructureChain, XrResult, bool> locateResult;

        internal XrStructureChain GetInputChain(XrHandEXT hand)
            => GetLocateInputChain(hand);

        internal XrStructureChain GetOutputChain(XrHandEXT hand)
            => GetLocateOutputChain(hand);

        internal void TriggerRestartRequest()
            => RequestHandTrackerRestart();

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
                return false;
            instanceCreate?.Invoke(xrInstance);
            return true;
        }

        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);
            sessionCreate?.Invoke(xrSession);
        }

        protected override void OnHandTrackingCreateRequest(XrHandEXT hand, XrStructureChain extensionChain)
        {
            handTrackingCreateRequest?.Invoke(hand, extensionChain);
        }

        protected override void OnHandTrackerCreated(XrHandEXT hand, XrResult createResult)
        {
            handTrackerCreated?.Invoke(hand, createResult);
        }

        protected override void OnHandTrackerDestroyed(XrHandEXT hand, XrResult destroyResult)
        {
            handTrackerDestroyed?.Invoke(hand, destroyResult);
        }

        protected override void OnLocateHandJointsResult(
            XrHandEXT hand,
            XrStructureChain outputChain,
            XrResult locateHandJointsResult,
            bool isActive)
        {
            locateResult?.Invoke(hand, outputChain, locateHandJointsResult, isActive);
        }

        internal void ResetCallbacks()
        {
            instanceCreate = null;
            sessionCreate = null;
            handTrackingCreateRequest = null;
            handTrackerCreated = null;
            handTrackerDestroyed = null;
            locateResult = null;
        }
    }
}
#endif
