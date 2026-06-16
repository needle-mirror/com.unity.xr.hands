namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Global settings for the recording session.
    /// </summary>
    public static class XRHandRecordingSettings
    {
        const float k_TimeLimitInSecondsDefault = 60f;

        /// <summary>
        /// The maximum recording duration in seconds.
        /// A recording is stopped automatically when this limit is reached.
        /// Default value is 60 seconds.
        /// </summary>
        public static float timeLimitInSeconds { get; set; } = k_TimeLimitInSecondsDefault;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            timeLimitInSeconds = k_TimeLimitInSecondsDefault;
        }
    }
}
