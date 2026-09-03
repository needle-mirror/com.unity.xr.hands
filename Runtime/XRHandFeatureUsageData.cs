#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
namespace UnityEngine.XR.Hands.Analytics
{
    /// <summary>
    /// Internal struct that tracks information about the XRHands systems during a frame in PlayMode.  This is primarily used
    /// by the XRHandsAnalytics system to gather PlayMode Analytics.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    struct XRHandFeatureUsageData
    {
#pragma warning disable UDR0001 // No method with RuntimeInitializeOnLoadMethod attribute -- Reset in `ResetData` triggered by analytics upon entering Play mode.
        static XRHandFeatureUsageData s_PlayModeInstance;

        bool m_XRHandSubsystemActive;
        bool m_XRHandCustomGestureActive;
        bool m_XRHandCustomGestureDebugActive;

        /// <summary>
        /// Flag that indicates that an XRHands subsystem is active during the Play Mode.
        /// </summary>
        public static bool xrHandSubsystemRuntimeUsed
        {
            get => s_PlayModeInstance.m_XRHandSubsystemActive;
            set => s_PlayModeInstance.m_XRHandSubsystemActive = value;
        }

        /// <summary>
        /// Flag that indicates that a user used a Custom Gesture during Play Mode.
        /// </summary>
        public static bool xrHandCustomGestureUsed
        {
            get => s_PlayModeInstance.m_XRHandCustomGestureActive;
            set => s_PlayModeInstance.m_XRHandCustomGestureActive = value;
        }

        /// <summary>
        /// Flag that indicates that a user used the XrHandShapeDebugUI during Play Mode.
        /// </summary>
        public static bool xrHandCustomGestureDebuggerUsed
        {
            get => s_PlayModeInstance.m_XRHandCustomGestureDebugActive;
            set => s_PlayModeInstance.m_XRHandCustomGestureDebugActive = value;
        }
#pragma warning restore UDR0001 // No method with RuntimeInitializeOnLoadMethod attribute

        public static void ResetData()
        {
            s_PlayModeInstance = new XRHandFeatureUsageData();
        }
    }
}
