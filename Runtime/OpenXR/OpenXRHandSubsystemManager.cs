#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Manages the lifecycle of the <see cref="XRHandSubsystem"/> and updater created by the
    /// <see cref="HandTracking"/> OpenXR feature. Toggle this component's
    /// <see cref="Behaviour.enabled"/> state to start and stop the subsystem.
    /// </summary>
    /// <remarks>
    /// On <see cref="Awake"/>, if the subsystem has not been created yet
    /// (for example when <see cref="HandTracking.automaticallyInitializeSubsystem"/>
    /// is <see langword="false"/>), the component disables itself. Re-enable
    /// it from your own code when ready and <see cref="OnEnable"/> will create and start the subsystem.
    /// </remarks>
    /// <example>
    /// <para>The following example mocks how a permissions checking script might respond
    /// to permission requests for hand tracking by enabling or disabling this component.</para>
    /// <code source="../../DocCodeSamples.Tests/SubsystemManagerSample.cs"/>
    /// </example>
    public class OpenXRHandSubsystemManager : MonoBehaviour
    {
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Disables this component if the subsystem has not been created yet.
        /// </summary>
        protected void Awake()
        {
            if (HandTracking.subsystem == null)
                enabled = false;
        }

        /// <summary>
        /// Called when the component becomes enabled and active.
        /// If the Hand Tracking OpenXR feature is enabled,
        /// creates the subsystem and updater if needed, then starts them.
        /// </summary>
        protected void OnEnable()
        {
            TryStartSubsystemIfAvailable();
        }

        /// <summary>
        /// Called when the component becomes disabled or inactive.
        /// Stops the subsystem and updater.
        /// </summary>
        protected void OnDisable()
        {
            HandTracking.StopSubsystemAndUpdater();
        }

        bool TryStartSubsystemIfAvailable()
        {
            HandTracking.EnsureSubsystemInitialized();

            if (HandTracking.subsystem == null)
                return false;

            if (!HandTracking.subsystem.running)
                HandTracking.StartSubsystemAndUpdater();

            return HandTracking.subsystem.running;
        }
    }
}

#endif
