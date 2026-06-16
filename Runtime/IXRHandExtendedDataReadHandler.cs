namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Non-generic tag interface for extended data handlers. Used internally
    /// to store handlers in a type-safe collection without boxing to
    /// `object`.
    /// </summary>
    public interface IXRHandExtendedDataReadHandler { }

    /// <summary>
    /// Interface for providing per-hand extended data of a specific type.
    /// Implementations are registered with
    /// <see cref="XRHandSubsystem.RegisterHandExtendedDataHandler{TData}"/>
    /// and queried via
    /// <see cref="XRHandSubsystem.TryGetExtendedData{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The type of extended data this handler provides.</typeparam>
    public interface IXRHandExtendedDataReadHandler<TData> : IXRHandExtendedDataReadHandler where TData : unmanaged
    {
        /// <summary>
        /// Attempts to retrieve extended data for the specified hand.
        /// </summary>
        /// <param name="handedness">Which hand to retrieve data for.</param>
        /// <param name="data">
        /// When this method returns `true`, contains the
        /// extended data for the specified hand.
        /// </param>
        /// <returns>
        /// `true` if data is provided for the specified hand;
        /// otherwise `false`.
        /// </returns>
        bool TryGetData(Handedness handedness, out TData data);
    }
}
