#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Implements <see cref="IHandTrackingExtensionProvider"/>. Created lazily
    /// by <see cref="HandTracking"/> when the first extension feature requests
    /// access. Extension features register themselves via
    /// <see cref="Register"/> for dispatch callbacks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Chain <b>contents</b> are session-scoped: nodes are added once per session
    /// (typically in <c>OnSessionCreate</c>) and cleared when the session is
    /// destroyed via <see cref="OnSessionEnd"/>. The chain objects themselves
    /// survive across sessions and are reused.
    /// </para>
    /// </remarks>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    internal sealed class HandExtensibilityManager : IHandTrackingExtensionProvider, IDisposable
    {
        XrStructureChain[] m_CreateChains;
        XrStructureChain[] m_LocateInputChains;
        XrStructureChain[] m_LocateOutputChains;

        readonly List<OpenXRHandTrackingFeature> m_RegisteredFeatures = new();

        /// <summary>
        /// Allocates the six per-hand chains (create, locate-input, locate-output
        /// for left and right hands).
        /// </summary>
        public HandExtensibilityManager()
        {
            m_CreateChains = new XrStructureChain[]
            {
                new XrStructureChain(),
                new XrStructureChain(),
            };

            m_LocateInputChains = new XrStructureChain[]
            {
                new XrStructureChain(),
                new XrStructureChain(),
            };

            m_LocateOutputChains = new XrStructureChain[]
            {
                new XrStructureChain(),
                new XrStructureChain(),
            };
        }

        /// <summary>
        /// Called by <see cref="HandTracking"/> when the session is destroyed.
        /// Clears all chain contents (disposes nodes) but keeps the chain
        /// objects alive for reuse on the next session.
        /// </summary>
        internal void OnSessionEnd()
        {
            ClearChains(m_CreateChains);
            ClearChains(m_LocateInputChains);
            ClearChains(m_LocateOutputChains);
        }

        /// <summary>
        /// Returns the create-info extension chain for the given hand.
        /// </summary>
        public XrStructureChain GetCreateChain(XrHandEXT hand)
            => m_CreateChains[ToHandIndex(hand)];

        /// <summary>
        /// Returns the locate-input extension chain for the given hand.
        /// </summary>
        public XrStructureChain GetLocateInputChain(XrHandEXT hand)
            => m_LocateInputChains[ToHandIndex(hand)];

        /// <summary>
        /// Returns the locate-output extension chain for the given hand.
        /// </summary>
        public XrStructureChain GetLocateOutputChain(XrHandEXT hand)
            => m_LocateOutputChains[ToHandIndex(hand)];

        /// <inheritdoc/>
        public void Register(OpenXRHandTrackingFeature feature)
        {
            if (feature != null && !m_RegisteredFeatures.Contains(feature))
                m_RegisteredFeatures.Add(feature);
        }

        /// <inheritdoc/>
        public void Unregister(OpenXRHandTrackingFeature feature)
        {
            m_RegisteredFeatures.Remove(feature);
        }

        /// <inheritdoc/>
        public void RequestRestart()
        {
            HandTracking.NativeApi.RequestRestart();
        }

        ~HandExtensibilityManager()
        {
            if (m_CreateChains != null || m_LocateInputChains != null || m_LocateOutputChains != null)
                Debug.LogError(
                    "HandExtensibilityManager was not disposed. Call Dispose() explicitly before the manager is garbage collected.");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposeChains(m_CreateChains);
            DisposeChains(m_LocateInputChains);
            DisposeChains(m_LocateOutputChains);

            m_CreateChains = null;
            m_LocateInputChains = null;
            m_LocateOutputChains = null;
            m_RegisteredFeatures.Clear();

            GC.SuppressFinalize(this);
        }

        static void ClearChains(XrStructureChain[] chains)
        {
            if (chains == null)
                return;

            for (int idx = 0; idx < chains.Length; idx++)
                chains[idx]?.Clear();
        }

        static void DisposeChains(XrStructureChain[] chains)
        {
            if (chains == null)
                return;

            // To ensure that we properly clean up, we need to catch and log exceptions instead of
            // relying on a try/finally. try/finally will rethrow, and cause us to leak every other
            // disposal we'd execute after the current one throws.
            for (int idx = 0; idx < chains.Length; idx++)
            {
                try
                {
                    chains[idx]?.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HandExtensibilityManager] Failed to dispose chain at index {idx}: {e}");
                }
            }
        }

        static int ToHandIndex(XrHandEXT hand) => hand == XrHandEXT.Right ? 1 : 0;

        unsafe XrBaseInStructure* BuildCreateChain(XrHandEXT hand)
        {
            var chain = GetCreateChain(hand);
            chain.Clear();
            foreach (var feature in m_RegisteredFeatures)
            {
                if (feature == null || !feature.enabled)
                    continue;
                try
                {
                    feature.InvokeOnHandTrackingCreateRequest(hand, chain);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[HandExtensibilityManager] Feature {feature.GetType().Name} threw: {ex}");
                }
            }
            return chain.GetHeadPointer();
        }

        void DispatchHandTrackerCreated(XrHandEXT hand, XrResult result)
        {
            foreach (var feature in m_RegisteredFeatures)
            {
                if (feature == null || !feature.enabled)
                    continue;
                try
                {
                    feature.InvokeOnHandTrackerCreated(hand, result);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[HandExtensibilityManager] Feature {feature.GetType().Name} threw: {ex}");
                }
            }
        }

        void DispatchHandTrackerDestroyed(XrHandEXT hand, XrResult result)
        {
            foreach (var feature in m_RegisteredFeatures)
            {
                if (feature == null || !feature.enabled)
                    continue;
                try
                {
                    feature.InvokeOnHandTrackerDestroyed(hand, result);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[HandExtensibilityManager] Feature {feature.GetType().Name} threw: {ex}");
                }
            }
        }

        internal void DispatchLocateResult(XrHandEXT hand, XrResult result, bool isActive)
        {
            var outputChain = GetLocateOutputChain(hand);
            foreach (var feature in m_RegisteredFeatures)
            {
                if (feature == null || !feature.enabled)
                    continue;
                try
                {
                    feature.InvokeOnLocateHandJointsResult(hand, outputChain, result, isActive);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[HandExtensibilityManager] Feature {feature.GetType().Name} threw: {ex}");
                }
            }
        }

        unsafe delegate void* CreateHandTrackerCallbackDelegate(uint hand);
        delegate void HandTrackerLifecycleCallbackDelegate(uint hand, int result);

        unsafe delegate void BeforeLocateHandJointsCallbackDelegate(uint hand, void** outInputChain, void** outOutputChain);
        delegate void AfterLocateHandJointsCallbackDelegate(uint hand, int result, uint isActive);

        static readonly unsafe CreateHandTrackerCallbackDelegate s_CreateCallback = OnCreateHandTrackerNative;
        static readonly HandTrackerLifecycleCallbackDelegate s_CreatedCallback = OnHandTrackerCreatedNative;
        static readonly HandTrackerLifecycleCallbackDelegate s_DestroyedCallback = OnHandTrackerDestroyedNative;
        static readonly unsafe BeforeLocateHandJointsCallbackDelegate s_BeforeLocateCallback = OnBeforeLocateNative;
        static readonly AfterLocateHandJointsCallbackDelegate s_AfterLocateCallback = OnAfterLocateNative;

        [MonoPInvokeCallback(typeof(CreateHandTrackerCallbackDelegate))]
        static unsafe void* OnCreateHandTrackerNative(uint hand)
        {
            try
            {
                var manager = HandTracking.extensibilityManager;
                if (manager == null)
                    return null;
                return manager.BuildCreateChain((XrHandEXT)hand);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandExtensibilityManager] Exception in OnCreateHandTrackerNative: {ex}");
                return null;
            }
        }

        [MonoPInvokeCallback(typeof(HandTrackerLifecycleCallbackDelegate))]
        static void OnHandTrackerCreatedNative(uint hand, int result)
        {
            try
            {
                HandTracking.extensibilityManager?.DispatchHandTrackerCreated((XrHandEXT)hand, (XrResult)result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandExtensibilityManager] Exception in OnHandTrackerCreatedNative: {ex}");
            }
        }

        [MonoPInvokeCallback(typeof(HandTrackerLifecycleCallbackDelegate))]
        static void OnHandTrackerDestroyedNative(uint hand, int result)
        {
            try
            {
                HandTracking.extensibilityManager?.DispatchHandTrackerDestroyed((XrHandEXT)hand, (XrResult)result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandExtensibilityManager] Exception in OnHandTrackerDestroyedNative: {ex}");
            }
        }

        [MonoPInvokeCallback(typeof(BeforeLocateHandJointsCallbackDelegate))]
        static unsafe void OnBeforeLocateNative(uint hand, void** outInputChain, void** outOutputChain)
        {
            try
            {
                var manager = HandTracking.extensibilityManager;
                if (manager == null)
                {
                    *outInputChain = null;
                    *outOutputChain = null;
                    return;
                }

                var xrHand = (XrHandEXT)hand;
                var input = manager.GetLocateInputChain(xrHand);
                var output = manager.GetLocateOutputChain(xrHand);
                *outInputChain = input != null ? input.GetHeadPointer() : null;
                *outOutputChain = output != null ? output.GetHeadPointer() : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandExtensibilityManager] Exception in OnBeforeLocateNative: {ex}");
                *outInputChain = null;
                *outOutputChain = null;
            }
        }

        [MonoPInvokeCallback(typeof(AfterLocateHandJointsCallbackDelegate))]
        static void OnAfterLocateNative(uint hand, int result, uint isActive)
        {
            try
            {
                HandTracking.extensibilityManager?.DispatchLocateResult(
                    (XrHandEXT)hand, (XrResult)result, isActive != 0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandExtensibilityManager] Exception in OnAfterLocateNative: {ex}");
            }
        }

        internal static void RegisterNativeCallbacks()
        {
            try
            {
                HandTracking.NativeApi.RegisterCreateHandTrackerCallback(
                    Marshal.GetFunctionPointerForDelegate(s_CreateCallback));
                HandTracking.NativeApi.RegisterHandTrackerLifecycleCallbacks(
                    Marshal.GetFunctionPointerForDelegate(s_CreatedCallback),
                    Marshal.GetFunctionPointerForDelegate(s_DestroyedCallback));
                HandTracking.NativeApi.RegisterLocateHandJointsCallbacks(
                    Marshal.GetFunctionPointerForDelegate(s_BeforeLocateCallback),
                    Marshal.GetFunctionPointerForDelegate(s_AfterLocateCallback));
            }
            catch (EntryPointNotFoundException)
            {
                Debug.LogWarning("[HandExtensibilityManager] Native callback registration not available. " +
                    "Extensibility features will not receive create/lifecycle notifications.");
            }
        }

        internal static void UnregisterNativeCallbacks()
        {
            try
            {
                HandTracking.NativeApi.RegisterCreateHandTrackerCallback(IntPtr.Zero);
                HandTracking.NativeApi.RegisterHandTrackerLifecycleCallbacks(IntPtr.Zero, IntPtr.Zero);
                HandTracking.NativeApi.RegisterLocateHandJointsCallbacks(IntPtr.Zero, IntPtr.Zero);
            }
            catch (EntryPointNotFoundException) { }
        }
    }
}

#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
