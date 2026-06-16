#if UNITY_EDITOR && (ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER)

using UnityEngine.XR.Hands.Analytics;

namespace UnityEditor.XR.Hands.Analytics
{
    /// <summary>
    /// Class that contains the XR Hands analytics PlayMode usage hooks.
    /// This class listen to PlayMode changes, build the PlayMode payload and send it to the analytics server.
    /// </summary>
    [InitializeOnLoad]
    class AnalyticsPlayModeUsageHooks
    {
        static AnalyticsPlayModeUsageHooks()
        {
            if (!EditorAnalytics.enabled)
                return;

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (EditorAnalytics.enabled)
                        XRHandFeatureUsageData.ResetData();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    if (EditorAnalytics.enabled)
                        XRHandsAnalytics.playmodeUsageEvent.Send(GetPlayModeUsagePayload());
                    break;
            }
        }

        static XRHandsPlaymodeUsageEvent.Payload GetPlayModeUsagePayload()
        {
            var payload = new XRHandsPlaymodeUsageEvent.Payload
            {
                customGestureUsed = XRHandFeatureUsageData.xrHandCustomGestureUsed,
                subsystemRuntimeUsed = XRHandFeatureUsageData.xrHandSubsystemRuntimeUsed,
                customGestureDebuggerUsed = XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed,
            };

            return payload;
        }
    }
}
#endif // UNITY_EDITOR && (ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER)
