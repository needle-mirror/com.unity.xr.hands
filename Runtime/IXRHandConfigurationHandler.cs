namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Non-generic tag interface for configuration handlers. Used internally
    /// to store handlers in a type-safe collection without boxing to
    /// `object`.
    /// </summary>
    public interface IXRHandConfigurationHandler { }

    /// <summary>
    /// Interface for managing typed configuration state.
    /// Implementations are registered with
    /// <see cref="XRHandSubsystem.RegisterConfigurationHandler{TConfig}"/>
    /// and queried via
    /// <see cref="XRHandSubsystem.TryGetConfiguration{TConfig}"/> and
    /// <see cref="XRHandSubsystem.TryUpdateConfiguration{TConfig}"/>.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type this handler manages.</typeparam>
    public interface IXRHandConfigurationHandler<TConfig> : IXRHandConfigurationHandler
    {
        /// <summary>
        /// Attempts to retrieve the current configuration.
        /// </summary>
        /// <param name="config">
        /// When this method returns `true`, contains the
        /// current configuration.
        /// </param>
        /// <returns>
        /// `true` if the configuration is provided;
        /// otherwise `false`.
        /// </returns>
        bool TryGetConfiguration(out TConfig config);

        /// <summary>
        /// Stages an updated configuration. Depending on the implementation,
        /// changes may take effect immediately or at a later point
        /// (e.g., on the next hand tracker creation).
        /// </summary>
        /// <param name="config">The new configuration to stage.</param>
        /// <returns>
        /// `true` if the configuration was accepted;
        /// otherwise `false`.
        /// </returns>
        bool TryUpdateConfiguration(TConfig config);
    }
}
