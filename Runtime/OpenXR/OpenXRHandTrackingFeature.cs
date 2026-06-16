#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using System;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Abstract base class for OpenXR hand tracking extensibility features.
    /// Subclass this to inject custom extension structures into
    /// <c>xrCreateHandTrackerEXT</c> and <c>xrLocateHandJointsEXT</c> calls,
    /// and to observe per-frame locate results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Override <see cref="OnHandTrackingCreateRequest"/> to populate the
    /// tracker creation extension chain. Call <see cref="GetLocateInputChain"/>
    /// to update locate-input structures at any time during a session.
    /// Override <see cref="OnLocateHandJointsResult"/> to read runtime-filled
    /// data from the locate-output chain each frame.
    /// </para>
    /// <para>
    /// The extension chains are stateful. Features add nodes once and then
    /// mutate them in place via <see cref="XrStructureChain.TryUpdateNode{TData}"/>.
    /// Don't re-add nodes per frame.
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class OpenXRHandTrackingFeature : OpenXRFeature
    {
        IHandTrackingExtensionProvider m_HandExtensionProvider;

        /// <summary>
        /// Registers this feature with the OpenXR hand tracking extension provider
        /// for create, destroy, and locate dispatches.
        /// </summary>
        /// <param name="xrInstance">The handle of the newly created <c>XrInstance</c>.</param>
        /// <returns>
        /// <c>true</c> if the base implementation succeeds. Otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Before calling <c>base.OnInstanceCreate</c>, use
        /// <see cref="OpenXRRuntime.IsExtensionEnabled"/> to verify that the OpenXR
        /// extension your feature depends on is enabled. If it is not, return
        /// <c>false</c> without invoking base. This disables the feature for
        /// the current instance. For more information, refer to <see cref="OpenXRFeature.OnInstanceCreate"/>.
        ///
        /// Otherwise, call <c>base.OnInstanceCreate</c> to register
        /// the subclass with the hand tracking extension provider for dispatch
        /// callbacks.
        /// </remarks>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
                return false;

            m_HandExtensionProvider = OpenXRSettings.Instance
                ?.GetFeature<HandTracking>()
                ?.GetOrCreateExtensionProvider();
            m_HandExtensionProvider?.Register(this);

            HandTracking.subsystemCreated += InvokeOnSubsystemCreated;
            HandTracking.destroyingSubsystem += InvokeOnSubsystemDestroyed;
            return true;
        }

        /// <summary>
        /// Unregisters this feature from the OpenXR hand tracking extension provider.
        /// </summary>
        /// <param name="xrInstance">The handle of the <c>XrInstance</c> being destroyed.</param>
        /// <remarks>
        /// Subclasses that override this method must call <c>base.OnInstanceDestroy</c>
        /// to unregister from dispatch callbacks.
        /// </remarks>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            HandTracking.destroyingSubsystem -= InvokeOnSubsystemDestroyed;
            HandTracking.subsystemCreated -= InvokeOnSubsystemCreated;
            m_HandExtensionProvider?.Unregister(this);
            m_HandExtensionProvider = null;
            base.OnInstanceDestroy(xrInstance);
        }

        void InvokeOnSubsystemCreated(HandTracking.SubsystemCreatedEventArgs args)
        {
            OnHandSubsystemCreated(args.subsystem);
        }

        void InvokeOnSubsystemDestroyed(HandTracking.DestroyingSubsystemEventArgs args)
        {
            OnHandSubsystemDestroyed(args.subsystem);
        }

        /// <summary>
        /// Called when the <see cref="XRHandSubsystem"/> is created.
        /// </summary>
        /// <param name="subsystem">The <see cref="XRHandSubsystem"/> instance that was just created.</param>
        /// <remarks>
        /// Override to register handlers on the subsystem instance. Pair any
        /// handlers registered here with matching unregistrations in
        /// <see cref="OnHandSubsystemDestroyed"/>.
        /// </remarks>
        protected virtual void OnHandSubsystemCreated(XRHandSubsystem subsystem)
        {
        }

        /// <summary>
        /// Called just before the <see cref="XRHandSubsystem"/> is destroyed.
        /// </summary>
        /// <param name="subsystem">The <see cref="XRHandSubsystem"/> instance about to be destroyed.</param>
        /// <remarks>
        /// Override to unregister any handlers registered in
        /// <see cref="OnHandSubsystemCreated"/>. The subsystem instance remains
        /// valid for the duration of this call.
        /// </remarks>
        protected virtual void OnHandSubsystemDestroyed(XRHandSubsystem subsystem)
        {
        }

        internal void InvokeOnHandTrackingCreateRequest(XrHandEXT hand, XrStructureChain extensionChain)
        {
            OnHandTrackingCreateRequest(hand, extensionChain);
        }

        internal void InvokeOnHandTrackerCreated(XrHandEXT hand, XrResult createResult)
        {
            OnHandTrackerCreated(hand, createResult);
        }

        internal void InvokeOnHandTrackerDestroyed(XrHandEXT hand, XrResult destroyResult)
        {
            OnHandTrackerDestroyed(hand, destroyResult);
        }

        internal void InvokeOnLocateHandJointsResult(
            XrHandEXT hand,
            XrStructureChain outputChain,
            XrResult locateHandJointsResult,
            bool isActive)
        {
            OnLocateHandJointsResult(hand, outputChain, locateHandJointsResult, isActive);
        }

        /// <summary>
        /// Called after <c>xrCreateHandTrackerEXT</c> completes for the given hand.
        /// </summary>
        /// <param name="hand">The hand for which the tracker was created.</param>
        /// <param name="createResult">The <c>XrResult</c> returned by <c>xrCreateHandTrackerEXT</c>.</param>
        /// <remarks>
        /// Triggers after every <c>xrCreateHandTrackerEXT</c> call, including failed
        /// calls. Check <paramref name="createResult"/> before assuming the tracker
        /// is valid. Might trigger multiple times within a single session when
        /// <see cref="RequestHandTrackerRestart"/> triggers a recreate.
        /// </remarks>
        protected virtual void OnHandTrackerCreated(XrHandEXT hand, XrResult createResult)
        {
        }

        /// <summary>
        /// Called after <c>xrDestroyHandTrackerEXT</c> completes for the given hand.
        /// </summary>
        /// <param name="hand">The hand for which the tracker was destroyed.</param>
        /// <param name="destroyResult">The <c>XrResult</c> returned by <c>xrDestroyHandTrackerEXT</c>.</param>
        /// <remarks>
        /// Might trigger multiple times within a single session when
        /// <see cref="RequestHandTrackerRestart"/> triggers a recreate. The destruction of a tracker
        /// doesn't imply the session is ending. A subsequent
        /// <see cref="OnHandTrackerCreated"/> might follow within the same session.
        /// </remarks>
        protected virtual void OnHandTrackerDestroyed(XrHandEXT hand, XrResult destroyResult)
        {
        }

        /// <summary>
        /// Called once per hand during <c>xrCreateHandTrackerEXT</c> to let features
        /// add extension structures to the tracker creation info chain.
        /// </summary>
        /// <param name="hand">The hand for which the tracker is being created.</param>
        /// <param name="extensionChain">The extension chain attached to the tracker creation info.</param>
        /// <remarks>
        /// Call <see cref="XrStructureChain.TryAddNode{TNodeType}"/> on
        /// <paramref name="extensionChain"/> to append a structure. Each
        /// <see cref="XrStructureType"/> can appear at most once. Attempting to
        /// add a duplicate returns <c>false</c>.
        ///
        /// The chain is cleared before each dispatch. Add nodes unconditionally
        /// on every invocation, including restarts triggered by
        /// <see cref="RequestHandTrackerRestart"/>.
        /// </remarks>
        protected virtual void OnHandTrackingCreateRequest(
            XrHandEXT hand,
            XrStructureChain extensionChain)
        {
        }

        /// <summary>
        /// Called every frame after <c>xrLocateHandJointsEXT</c> completes for the given hand.
        /// </summary>
        /// <param name="hand">The hand for which joints were located.</param>
        /// <param name="outputChain">The locate output chain populated by the OpenXR runtime.</param>
        /// <param name="locateHandJointsResult">The <c>XrResult</c> returned by <c>xrLocateHandJointsEXT</c>.</param>
        /// <param name="isActive">
        /// <c>true</c> if the hand is active and being tracked. Corresponds to
        /// <c>XrHandJointLocationsEXT.isActive</c>.
        /// </param>
        /// <remarks>
        /// Override to read data from the output chain using
        /// <see cref="XrStructureChain.TryGetNode{TData}"/>. The OpenXR runtime
        /// populates the chain on each call. Check that
        /// <paramref name="locateHandJointsResult"/> is <see cref="XrResult.Success"/> and that
        /// <paramref name="isActive"/> is <c>true</c> before reading the data. The
        /// locate call can succeed even when the hand is inactive.
        ///
        /// This method is called every frame for each hand. Keep the implementation
        /// lightweight. Copy any relevant fields to managed state and return
        /// quickly. Do not allocate.
        /// </remarks>
        protected virtual void OnLocateHandJointsResult(
            XrHandEXT hand,
            XrStructureChain outputChain,
            XrResult locateHandJointsResult,
            bool isActive)
        {
        }

        /// <summary>
        /// Returns the structure chain that will be appended to <c>XrHandJointsLocateInfoEXT</c>'s structure chain for the specified hand.
        /// </summary>
        /// <param name="hand">The hand for which to retrieve the locate output chain..</param>
        /// <returns>
        /// The <see cref="XrStructureChain"/> for the specified hand, or <c>null</c>
        /// when hand tracking extensibility is not available.
        /// </returns>
        /// <remarks>
        /// Call this method at any time during a session to update locate input
        /// structures. The return value is not <c>null</c> during any
        /// <c>OpenXRFeature</c> session callback, from <c>OnSessionCreate</c>
        /// through <c>OnSessionDestroy</c>.
        /// </remarks>
        protected XrStructureChain GetLocateInputChain(XrHandEXT hand)
        {
            return m_HandExtensionProvider?.GetLocateInputChain(hand);
        }

        /// <summary>
        /// Returns the locate output chain for the specified hand.
        /// </summary>
        /// <param name="hand">The hand for which to retrieve the locate output chain.</param>
        /// <returns>
        /// The <see cref="XrStructureChain"/> for the specified hand, or <c>null</c>
        /// when hand tracking extensibility is not available.
        /// </returns>
        /// <remarks>
        /// Call this method to access output data outside of
        /// <see cref="OnLocateHandJointsResult"/>. The return value is not
        /// <c>null</c> during any <c>OpenXRFeature</c> session callback, from
        /// <c>OnSessionCreate</c> through <c>OnSessionDestroy</c>.
        /// </remarks>
        protected XrStructureChain GetLocateOutputChain(XrHandEXT hand)
        {
            return m_HandExtensionProvider?.GetLocateOutputChain(hand);
        }

        /// <summary>
        /// Requests that the hand trackers be destroyed and recreated at the
        /// start of the next Dynamic update.
        /// </summary>
        /// <remarks>
        /// Use this when an extension configuration change requires a fresh
        /// <c>xrCreateHandTrackerEXT</c> call, for example when the value of a
        /// structure added in <see cref="OnHandTrackingCreateRequest"/> changes
        /// and must be applied again. Repeated restart requests within a single
        /// frame coalesce, so callers do not need to filter duplicate calls.
        /// </remarks>
        protected void RequestHandTrackerRestart()
        {
            m_HandExtensionProvider?.RequestRestart();
        }
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
