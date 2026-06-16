#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Provides access to OpenXR hand tracking extension structure chains
    /// and manages feature registration for dispatch callbacks.
    /// </summary>
    /// <remarks>
    /// Extension features obtain this interface from
    /// <see cref="HandTracking.GetOrCreateExtensionProvider"/> during
    /// <c>OnInstanceCreate</c>, then call <see cref="Register"/> to
    /// participate in hand tracking extensibility dispatch.
    /// </remarks>
    internal interface IHandTrackingExtensionProvider
    {
        /// <summary>
        /// Returns the create-info extension chain for the given hand.
        /// </summary>
        XrStructureChain GetCreateChain(XrHandEXT hand);

        /// <summary>
        /// Returns the locate-input extension chain for the given hand.
        /// </summary>
        XrStructureChain GetLocateInputChain(XrHandEXT hand);

        /// <summary>
        /// Returns the locate-output extension chain for the given hand.
        /// </summary>
        XrStructureChain GetLocateOutputChain(XrHandEXT hand);

        /// <summary>
        /// Registers an extension feature for dispatch callbacks. Registered
        /// features receive <c>OnHandTrackingCreateRequest</c>,
        /// <c>OnLocateHandJointsResult</c>, and lifecycle notifications.
        /// </summary>
        void Register(OpenXRHandTrackingFeature feature);

        /// <summary>
        /// Unregisters an extension feature. The feature will no longer
        /// receive dispatch callbacks.
        /// </summary>
        void Unregister(OpenXRHandTrackingFeature feature);

        /// <summary>
        /// Requests that the hand trackers be destroyed and recreated at the
        /// start of the next Dynamic update.
        /// </summary>
        /// <remarks>
        /// Repeat restart requests within a frame coalesce, so callers do not need to worry about repeated calls.
        /// </remarks>
        void RequestRestart();
    }
}
#endif
