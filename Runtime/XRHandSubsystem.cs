using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Hands.Analytics;
using UnityEngine.XR.Hands.Configuration;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.Processing;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// A subsystem for detecting and tracking hands and their corresponding
    /// joint pose data.
    /// </summary>
    /// <remarks>
    /// The <c>XRHandSubsystem</c> class is the main entry point for accessing hand tracking data
    /// provided by an XR device. A provider implementation that reads tracking data from the
    /// user's device and provides updates to this subsystem must also be available. The XR
    /// Hands package includes a provider implementation for OpenXR.
    ///
    /// Get an instance for this <c>XRHandSubsystem</c> from the active XR
    /// loader, as described in [Get the XRHandSubsystem instance](xref:xrhands-access-data#get-instance).
    ///
    /// For lowest latency, read the tracking data available from the <see cref="leftHand"/>
    /// and <see cref="rightHand"/> properties in a delegate function assigned to the
    /// <see cref="updatedHands"/> callback. This callback is invoked twice per frame, once near
    /// the <c>MonoBehaviour.Update</c> event and once near the <see cref="Application.onBeforeRender"/>
    /// event. The <see cref="UpdateType.BeforeRender"/> update provides the lowest latency between
    /// hand motion and rendering, but occurs too late to affect physics. In addition, trying to
    /// perform too much work during the <c>BeforeRender</c> callback can negatively impact framerate.
    /// For best results, update game logic affected by hand tracking in a
    /// <see cref="UpdateType.Dynamic"/> update and perform a final update of hand visuals in a
    /// <see cref="UpdateType.BeforeRender"/> update.
    ///
    /// Refer to [Hand tracking data](xref:xrhands-tracking-data) for more information.
    /// </remarks>
    public partial class XRHandSubsystem
        : SubsystemWithProvider<XRHandSubsystem, XRHandSubsystemDescriptor, XRHandSubsystemProvider>
    {
        /// <summary>
        /// Constructs a subsystem. Do not invoke directly.
        /// </summary>
        /// <remarks>
        /// Do not call this constructor if you are an application developer consuming hand tracking data.
        /// Instead, get an instance for this <c>XRHandTrackingSubsystem</c> from the active XR
        /// loader, as described in [Get the XRHandSubsystem instance](xref:xrhands-access-data#get-instance).
        ///
        /// If you are implementing an XR hand data provider, call <c>Create</c>
        /// on the <see cref="XRHandSubsystemDescriptor"/> or call
        /// <see cref="UnityEngine.XR.Management.XRLoaderHelper.CreateSubsystem"/>
        /// instead of invoking this constructor.
        /// </remarks>
        public XRHandSubsystem()
        {
        }

        /// <summary>
        /// Gets the left <see cref="XRHand"/> that is being tracked by this
        /// subsystem.
        /// </summary>
        /// <remarks>
        /// Check the <see cref="updateSuccessFlags"/> property to determine what data
        /// associated with this hand was successfully updated in the last update, if any.
        /// The <see cref="updateSuccessFlags"/> value is also passed to the callback
        /// function assigned to <see cref="updatedHands"/>.
        ///
        /// Refer to [Hand data model](xref:xrhands-data-model) for a description of the
        /// available hand tracking data.
        /// </remarks>
        public XRHand leftHand => GetHand(Handedness.Left);

        /// <summary>
        /// Gets the right <see cref="XRHand"/> that is being tracked by this
        /// subsystem.
        /// </summary>
        /// <remarks>
        /// Check the <see cref="updateSuccessFlags"/> property to determine what data
        /// associated with this hand was successfully updated in the last update, if any.
        /// The <see cref="updateSuccessFlags"/> value is also passed to the callback
        /// function assigned to <see cref="updatedHands"/>.
        ///
        /// Refer to [Hand data model](xref:xrhands-data-model) for a description of the
        /// available hand tracking data.
        /// </remarks>
        public XRHand rightHand => GetHand(Handedness.Right);

        /// <summary>
        /// Gets the <see cref="XRHand"/> associated with the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// The <see cref="Handedness"/> you wish to retrieve the associated
        /// <see cref="XRHand"/> for. <c>GetHand</c> will throw an exception
        /// if anything other than <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// is supplied.
        /// </param>
        /// <returns>
        /// The <see cref="XRHand"/> associated with the given <see cref="Handedness"/>.
        /// </returns>
        /// <remarks>
        /// Will only ever return either <see cref="leftHand"/>
        /// or <see cref="rightHand"/>.
        /// </remarks>
        public XRHand GetHand(Handedness handedness)
        {
            if (!handedness.IsValid())
                throw new ArgumentException($"Cannot get hand for Handedness.{handedness}. Only Handedness.Left and Handedness.Right are valid.", nameof(handedness));

            if (!TryGetHand(handedness, out var hand))
                throw new InvalidOperationException("Hand subsystem state is not initialized.");

            return hand;
        }

        /// <summary>
        /// Attempts to get the <see cref="XRHand"/> associated with the given
        /// <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// The <see cref="Handedness"/> you wish to retrieve the associated
        /// <see cref="XRHand"/> for.
        /// </param>
        /// <param name="hand">
        /// If <c>TryGetHand</c> returns <see langword="true"/>, this will be filled
        /// out with the associated hand data requested.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid <see cref="Handedness.Left"/>
        /// or <see cref="Handedness.Right"/> hand was requested and the subsystem
        /// is initialized, returns <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Will only ever successfully retrieve either <see cref="leftHand"/>
        /// or <see cref="rightHand"/>.
        /// </remarks>
        public bool TryGetHand(Handedness handedness, out XRHand hand)
        {
            if (!handedness.IsValid() || m_StatePerHand == null)
            {
                hand = default;
                return false;
            }

            hand = m_StatePerHand[handedness.ToIndex()].m_Hand;
            return true;
        }

        /// <summary>
        /// Invoked when UpdateHandsConfiguration is called and after subsystem processing has occurred.
        /// </summary>
        internal Action<XRHandSubsystemConfigurationUpdatedEventArgs> configurationUpdated { get; set; }

        XRHandSubsystemConfiguration m_XRHandSubsystemConfiguration;
        internal XRHandSubsystemConfiguration handSubsystemConfiguration => m_XRHandSubsystemConfiguration;

        /// <summary>
        /// Updates the current subsystem configuration to newConfiguration. Invokes <see cref="configurationUpdated"/>
        /// once local processing has been completed to notify consumers that they may need to update their
        /// configuration.
        ///
        /// See <see cref="XRHandSubsystemConfiguration"/> for more details on individual parameters, what they
        /// affect, and when the update takes effect.
        /// </summary>
        /// <param name="newConfiguration">The new configuration to be used by the hands subsystem.</param>
        public void UpdateHandsConfiguration(XRHandSubsystemConfiguration newConfiguration)
        {
            m_XRHandSubsystemConfiguration = newConfiguration;

            if (configurationUpdated != null)
            {
                configurationUpdated(
                    new XRHandSubsystemConfigurationUpdatedEventArgs(this, m_XRHandSubsystemConfiguration));
            }
        }

        /// <summary>
        /// Indicates which joints in the <see cref="XRHandJointID"/> list are
        /// supported by the current hand data provider.
        /// </summary>
        /// <remarks>
        /// Hand data providers might not support tracking every joint in the
        /// <see cref="XRHandJointID"/> list. This array contains an element for
        /// each possible joint. A value of true indicates the current provider
        /// supports tracking the associated joint.
        ///
        /// To get the correct array index for a joint, call
        /// <see cref="XRHandJointIDUtility.ToIndex(XRHandJointID)"/> on the
        /// <see cref="XRHandJointID"/> in question.
        ///
        /// Refer to [Get supported joints array](xref:xrhands-access-data#joint-layout)
        /// for additional information.
        ///
        /// This array will already be valid as soon as you have a reference to
        /// a subsystem (in other words, it's filled out before the subsystem is
        /// returned by a call to <c>XRHandSubsystemDescriptor.Create</c>).
        /// </remarks>
        public NativeArray<bool> jointsInLayout => m_JointsInLayout;

        /// <summary>
        /// Describes what data on either hand was updated during the most recent hand update.
        /// </summary>
        /// <remarks>
        /// This property updated every time the hand data is updated, which only occurs while this
        /// XRHandSubsystem is running.
        ///
        /// The <see cref="updateSuccessFlags"/> value is also passed to the callback
        /// function assigned to <see cref="updatedHands"/>.
        /// </remarks>
        /// <value>The flags for the most recent update. Applies to the <see cref="leftHand"/>
        /// and <see cref="rightHand"/> properties.</value>
        public UpdateSuccessFlags updateSuccessFlags
        {
            get => m_UpdateSuccessFlags;
            protected set => m_UpdateSuccessFlags = value;
        }

        /// <summary>
        /// Describes what data on either hand was updated during the call.
        /// </summary>
        /// <seealso cref="updateSuccessFlags"/>
        [Flags]
        public enum UpdateSuccessFlags
        {
            /// <summary>
            /// No data was successfully updated for either hand.
            /// </summary>
            None = 0,

            /// <summary>
            /// The root pose of <see cref="XRHandSubsystem.leftHand"/> was updated.
            /// </summary>
            LeftHandRootPose = 1 << 0,

            /// <summary>
            /// The joints in <see cref="XRHandSubsystem.leftHand"/> were updated.
            /// </summary>
            LeftHandJoints = 1 << 1,

            /// <summary>
            /// The root pose of <see cref="XRHandSubsystem.rightHand"/> was updated.
            /// </summary>
            RightHandRootPose = 1 << 2,

            /// <summary>
            /// The joints in <see cref="XRHandSubsystem.rightHand"/> were updated.
            /// </summary>
            RightHandJoints = 1 << 3,

            /// <summary>
            /// All possible valid data retrieved (hand root poses, and joints for both hands).
            /// </summary>
            All = LeftHandRootPose | LeftHandJoints | RightHandRootPose | RightHandJoints
        }

        /// <summary>
        /// The timing of a hand update during a frame.
        /// </summary>
        /// <seealso cref="updatedHands"/>
        /// <seealso cref="TryUpdateHands(UpdateType)"/>
        public enum UpdateType
        {
            /// <summary>
            /// Corresponds to timing similar or close to <c>MonoBehaviour.Update</c>.
            /// </summary>
            Dynamic,

            /// <summary>
            /// Corresponds to timing similar or close to <see cref="Application.onBeforeRender"/>.
            /// </summary>
            BeforeRender
        }

        /// <summary>
        /// A callback invoked for each hand update.
        /// </summary>
        /// <remarks>
        /// This callback is invoked twice per frame, once near
        /// the <c>MonoBehaviour.Update</c> event and once near the <see cref="Application.onBeforeRender"/>
        /// event. The <see cref="UpdateType.BeforeRender"/> update provides the lowest latency between
        /// hand motion and rendering, but occurs too late to affect physics. In addition, trying to
        /// perform too much work during the <c>BeforeRender</c> callback can negatively impact framerate.
        /// For best results, update game logic affected by hand tracking in a
        /// <see cref="UpdateType.Dynamic"/> update and update hand visuals in a
        /// <see cref="UpdateType.BeforeRender"/> update.
        ///
        /// The delegate assigned to this property must take three parameters, which have the
        /// following types and assigned values when the callback is invoked:
        ///
        /// * <see cref="XRHandSubsystem"/>: contains a reference to this subsystem.
        /// * <see cref="UpdateSuccessFlags"/>: the flags indicating which data could be updated.
        /// * <see cref="UpdateType"/>: the update timing.
        /// </remarks>
        public Action<XRHandSubsystem, UpdateSuccessFlags, UpdateType> updatedHands;

        /// <summary>
        /// A callback invoked when the subsystem begins tracking a hand's root pose and joints.
        /// </summary>
        /// <remarks>
        /// This is called before <see cref="updatedHands"/>.
        ///
        /// The delegate assigned to this property must take one parameter of type
        /// <see cref="XRHand"/>, which is assigned a reference to the hand whose tracking was acquired.
        /// </remarks>
        public Action<XRHand> trackingAcquired;

        /// <summary>
        /// A callback invoked when the subsystem stops tracking a hand's root pose and joints.
        /// </summary>
        /// <remarks>
        /// This is called before <see cref="updatedHands"/>.
        ///
        /// The delegate assigned to this property must take one parameter of type
        /// <see cref="XRHand"/>, which is assigned a reference to the hand whose tracking was lost.
        /// </remarks>
        public Action<XRHand> trackingLost;

        /// <summary>
        /// Gets common hand gestures getters and callbacks for the left hand.
        /// </summary>
        public XRCommonHandGestures leftHandCommonGestures => GetCommonGestures(Handedness.Left);

        /// <summary>
        /// Gets common hand gestures getters and callbacks for the right hand.
        /// </summary>
        public XRCommonHandGestures rightHandCommonGestures => GetCommonGestures(Handedness.Right);

        /// <summary>
        /// Attempts to get the <see cref="XRCommonHandGestures"/> associated with the given
        /// <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// The <see cref="Handedness"/> you wish to retrieve the associated
        /// <see cref="XRCommonHandGestures"/> for.
        /// </param>
        /// <param name="commonGestures">
        /// If <c>TryGetCommonGestures</c> returns <see langword="true"/>, this will be filled
        /// out with the associated common gestures data requested.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if a valid <see cref="Handedness.Left"/>
        /// or <see cref="Handedness.Right"/> hand was requested and the subsystem
        /// is initialized, returns <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Will only ever successfully retrieve either <see cref="leftHandCommonGestures"/>
        /// or <see cref="rightHandCommonGestures"/>.
        /// </remarks>
        public bool TryGetCommonGestures(Handedness handedness, out XRCommonHandGestures commonGestures)
        {
            if (!handedness.IsValid() || m_StatePerHand == null)
            {
                commonGestures = null;
                return false;
            }

            commonGestures = m_StatePerHand[handedness.ToIndex()].m_CommonGestures;
            return true;
        }

        /// <summary>
        /// Gets the <see cref="XRCommonHandGestures"/> associated with the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// The <see cref="Handedness"/> you wish to retrieve the associated
        /// <see cref="XRCommonHandGestures"/> for. <c>GetCommonGestures</c> will throw an exception
        /// if anything other than <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// is supplied.
        /// </param>
        /// <returns>
        /// The <see cref="XRCommonHandGestures"/> associated with the given <see cref="Handedness"/>.
        /// </returns>
        /// <remarks>
        /// Will only ever return either <see cref="leftHandCommonGestures"/>
        /// or <see cref="rightHandCommonGestures"/>.
        /// </remarks>
        public XRCommonHandGestures GetCommonGestures(Handedness handedness)
        {
            if (!handedness.IsValid())
                throw new ArgumentException($"Cannot get common gestures for Handedness.{handedness}. Only Handedness.Left and Handedness.Right are valid.", nameof(handedness));

            if (!TryGetCommonGestures(handedness, out var commonGestures))
                throw new InvalidOperationException("Hand subsystem state is not initialized.");

            return commonGestures;
        }

        /// <summary>
        /// Request an update from the hand data provider. Application developers
        /// consuming hand tracking data should not call this function.
        /// </summary>
        /// <param name="updateType">
        /// Informs the provider which kind of timing the update is being
        /// requested under.
        /// </param>
        /// <returns>
        /// Returns <see cref="UpdateSuccessFlags"/> to describe what data was updated successfully.
        /// </returns>
        /// <remarks>
        /// This function must be called by the subsystem implementation to request an update from
        /// the hand data provider.
        ///
        /// When an update is complete, the updated data is available from the <see cref="leftHand"/> and
        /// <see cref="rightHand"/> properties. The <see cref="updatedHands"/> callback is invoked.
        ///
        /// The update is performed immediately. If you request an update timing that occurs in the
        /// future, for example, requesting <see cref="UpdateType.BeforeRender"/> from a
        /// <c>MonoBehaviour.Update</c> function, then the provider predicts what the hand data
        /// will be at the requested time.
        ///
        /// If overriding this method in a derived type, it is expected that you
        /// call <c>base.TryUpdateHands(updateType)</c> and return what it
        /// returns.
        /// </remarks>
        public virtual unsafe UpdateSuccessFlags TryUpdateHands(UpdateType updateType)
        {
            using (s_TryUpdateHandsMarker.Auto())
            {
#if UNITY_EDITOR && (ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER)
                XRHandFeatureUsageData.xrHandSubsystemRuntimeUsed = true;
#endif

                if (!running)
                    return UpdateSuccessFlags.None;

                var leftHand = GetHand(Handedness.Left);
                var rightHand = GetHand(Handedness.Right);
                if (!leftHand.isValid || !rightHand.isValid)
                    return UpdateSuccessFlags.None;

                m_Actions.beginTryUpdateHands?.Invoke(updateType);
                var leftPose = leftHand.m_RootPose;
                var rightPose = rightHand.m_RootPose;

                using (s_ProviderTryUpdateHandsMarker.Auto())
                {
                    m_UpdateSuccessFlags = provider.TryUpdateHands(
                        updateType,
                        ref leftPose,
                        leftHand.m_Joints,
                        ref rightPose,
                        rightHand.m_Joints);
                }

                leftHand.m_RootPose = leftPose;
                rightHand.m_RootPose = rightPose;
                XRFingerShapeMath.ClearFingerStateCaches();

                using (s_TrackingEventsMarker.Auto())
                {
                    var wasLeftHandTracked = leftHand.isTracked;
                    var success = UpdateSuccessFlags.LeftHandRootPose | UpdateSuccessFlags.LeftHandJoints;
                    leftHand.isTracked = (updateSuccessFlags & success) == success;
                    m_StatePerHand[Handedness.Left.ToIndex()].m_Hand = leftHand;
                    if (!wasLeftHandTracked && leftHand.isTracked)
                        trackingAcquired?.Invoke(leftHand);
                    else if (wasLeftHandTracked && !leftHand.isTracked)
                        trackingLost?.Invoke(leftHand);

                    var wasRightHandTracked = rightHand.isTracked;
                    success = UpdateSuccessFlags.RightHandRootPose | UpdateSuccessFlags.RightHandJoints;
                    rightHand.isTracked = (updateSuccessFlags & success) == success;
                    m_StatePerHand[Handedness.Right.ToIndex()].m_Hand = rightHand;
                    if (!wasRightHandTracked && rightHand.isTracked)
                        trackingAcquired?.Invoke(rightHand);
                    else if (wasRightHandTracked && !rightHand.isTracked)
                        trackingLost?.Invoke(rightHand);
                }

                using (s_PostUpdateMarker.Auto())
                {
                    preprocessJoints?.Invoke(this, updateSuccessFlags, updateType);

                    // this needs to be an option (enabled by default?) instead of blanket disabling
                    if (provider.AllowJointProcessing())
                    {
                        for (int processorIndex = 0; processorIndex < m_Processors.Count; ++processorIndex)
                            m_Processors[processorIndex].ProcessJoints(this, updateSuccessFlags, updateType);
                    }

                    if (updateType == UpdateType.Dynamic)
                    {
                        // Avoid foreach allocation by iterating handedness values directly
                        using (s_RetrieveCommonPoseDataMarker.Auto())
                        {
                            RetrieveCommonPoseData(Handedness.Left, m_StatePerHand[Handedness.Left.ToIndex()].m_CommonGestures);
                            RetrieveCommonPoseData(Handedness.Right, m_StatePerHand[Handedness.Right.ToIndex()].m_CommonGestures);
                        }

                    }

#if UNITY_OPENXR_PACKAGE
                    if (provider is OpenXR.OpenXRHandProvider openXRProvider)
                        openXRProvider.FlushMetaAimChanges();
#endif // UNITY_OPENXR_PACKAGE

                    // Avoid foreach allocation by iterating handedness values directly
                    {
                        int handedIndex = Handedness.Left.ToIndex();
                        var statePerHand = m_StatePerHand[handedIndex];
                        statePerHand.m_IsAimStateValid = provider.TryGetAimState(
                            Handedness.Left, out statePerHand.m_AimState);
                    }
                    {
                        int handedIndex = Handedness.Right.ToIndex();
                        var statePerHand = m_StatePerHand[handedIndex];
                        statePerHand.m_IsAimStateValid = provider.TryGetAimState(
                            Handedness.Right, out statePerHand.m_AimState);
                    }

                    if (updatedHands != null)
                        updatedHands.Invoke(this, updateSuccessFlags, updateType);

#pragma warning disable 618
                    if (handsUpdated != null)
                        handsUpdated.Invoke(updateSuccessFlags, updateType);
#pragma warning restore 618

                    return updateSuccessFlags;
                }
            }
        }

        /// <summary>
        /// Attempts to get aim state for the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// Denotes the hand you wish to obtain aim state for.
        /// </param>
        /// <param name="aimState">
        /// If <c>TryGetAimState</c> returns <see langword="true"/>, this will
        /// filled out with aim state data for the given <see cref="Handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if <c>TryGetAimState</c> succeeds and
        /// fills out aim state for the given <see cref="Handedness"/>.
        /// </returns>
        public bool TryGetAimState(Handedness handedness, out XRHandAimState aimState)
        {
            if (!handedness.IsValid() || m_StatePerHand == null)
            {
                aimState = default;
                return false;
            }

            int handedIndex = handedness.ToIndex();
            var statePerHand = m_StatePerHand[handedIndex];
            aimState = statePerHand.m_IsAimStateValid ? statePerHand.m_AimState : default;
            return statePerHand.m_IsAimStateValid;
        }

        /// <summary>
        /// Attempt to retrieve hand mesh data from the platform. Only called when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <param name="result">
        /// Output data for hand meshes.
        /// </param>
        /// <param name="queryParams">
        /// Input data for hand meshes.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and either hand has
        /// valid data. Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// If this returns <see langword="true"/>, it is your responsibility to
        /// clean up the data by calling <see cref="XRHandMeshData.Dispose"/>
        /// on each of those contained in the filled-out <paramref name="result"/>,
        /// or by calling <see cref="XRHandMeshDataQueryResult.Dispose"/> on the
        /// <paramref name="result"/> itself.
        /// </remarks>
        public bool TryGetMeshData(out XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams)
        {
            result = new XRHandMeshDataQueryResult();
            result.leftHand = new XRHandMeshData(Handedness.Left);
            result.rightHand = new XRHandMeshData(Handedness.Right);

            var ret = provider.TryGetMeshData(ref result, ref queryParams);

            // users should only have to worry about calling Dispose if this
            // returns true, so ensure that in case the provider mistakenly
            // set some data but still returned false
            if (!ret)
            {
                result.leftHand = InvalidateMeshData(result.leftHand);
                result.rightHand = InvalidateMeshData(result.rightHand);
            }

            return ret;
        }

        static XRHandMeshData InvalidateMeshData(XRHandMeshData meshData)
        {
            meshData.Dispose();
            meshData.m_IsRootPoseValid = false;
            return meshData;
        }

        /// <summary>
        /// This is called after the subsystem retrieves joint data from the
        /// provider, and before and <see cref="IXRHandProcessor"/>s'
        /// <see cref="IXRHandProcessor.ProcessJoints"/> are called.
        /// </summary>
        public Action<XRHandSubsystem, UpdateSuccessFlags, UpdateType> preprocessJoints;

        /// <summary>
        /// Describes which version of authored hand meshes is detected for use.
        /// </summary>
        public XRDetectedHandMeshLayout detectedHandMeshLayout => provider.detectedHandMeshLayout; // deprecate? should add new one either way

        /// <summary>
        /// Registers a processor for hand joint data.
        /// </summary>
        /// <param name="processor">
        /// The processor to register for this <see cref="XRHandSubsystem"/>.
        /// </param>
        /// <typeparam name="TProcessor">
        /// The type of the processor to register.
        /// </typeparam>
        public void RegisterProcessor<TProcessor>(TProcessor processor)
            where TProcessor : class, IXRHandProcessor
        {
            if (processor == null)
                throw new ArgumentException("Processor cannot be null.", nameof(processor));

            m_Processors.Add(processor);
            m_Processors.Sort(CompareProcessors);
        }

        /// <summary>
        /// Unregisters a processor for hand joint data.
        /// </summary>
        /// <param name="processor">
        /// The processor to unregister from this <see cref="XRHandSubsystem"/>.
        /// </param>
        /// <typeparam name="TProcessor">
        /// The type of the processor to register.
        /// </typeparam>
        public void UnregisterProcessor<TProcessor>(TProcessor processor)
            where TProcessor : class, IXRHandProcessor
        {
            m_Processors.Remove(processor);
        }

        /// <summary>
        /// Called by Unity before the subsystem is returned from a call to <c>XRHandSubsystemDescriptor.Create</c>.
        /// </summary>
        protected override void OnCreate()
        {
            m_StatePerHand = new StatePerHand[Constants.k_NumHands];
            foreach (var handedness in HandsUtility.validHandednessValues)
                m_StatePerHand[handedness.ToIndex()] = new StatePerHand(handedness);

            XRFingerShapeMath.OnCreateSubsystem(provider);

            m_JointsInLayout = new NativeArray<bool>(Constants.k_NumJointsPerHand, Allocator.Persistent);
            provider.GetHandLayout(m_JointsInLayout);
            foreach (var handedness in HandsUtility.validHandednessValues)
                m_StatePerHand[handedness.ToIndex()].m_Hand.ApplyJointLayout(m_JointsInLayout);
            provider.SubscribeToSubsystemActions(ref m_Actions);
        }

        /// <inheritdoc/>
        protected override void OnStart()
        {
            base.OnStart();
            XRHandSubsystemDescriptor.OnSubsystemStarted(this);
        }

        /// <inheritdoc/>
        protected override void OnStop()
        {
            base.OnStop();
            XRHandSubsystemDescriptor.OnSubsystemStopped(this);
        }

        /// <summary>
        /// Called by Unity before the subsystem is fully destroyed during a call to <c>XRHandSubsystem.Destroy</c>.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            var previousAllowDisposalFor = XRHand.allowDisposalFor;
            XRHand.allowDisposalFor = XRHand.LifetimeType.Subsystem;
            try
            {
                foreach (var perHandState in m_StatePerHand)
                    perHandState.OnDestroy();
            }
            finally
            {
                XRHand.allowDisposalFor = previousAllowDisposalFor;
            }

            if (m_JointsInLayout.IsCreated)
                m_JointsInLayout.Dispose();

            XRHandSubsystemDescriptor.OnSubsystemDestroyed(this);
        }

        internal unsafe void SetHand(XRHand hand)
        {
            if (hand.m_Joints.GetUnsafePtr() != GetHand(hand.handedness).m_Joints.GetUnsafePtr())
            {
                var handednessName = hand.handedness.ToString().ToLower();
                throw new InvalidOperationException(
                    $"Cannot overwrite the {handednessName} hand with a hand that was not first retrieved from the subsystem's {handednessName}Hand property!");
            }
            m_StatePerHand[hand.handedness.ToIndex()].m_Hand = hand;
        }

        void RetrieveCommonPoseData(Handedness handedness, XRCommonHandGestures commonGestures)
        {
            if (!provider.canSurfaceCommonPoseData)
                return;

            if (subsystemDescriptor.supportsAimPose)
            {
                if (provider.TryGetAimPose(handedness, out var aimPose))
                    commonGestures.UpdateAimPose(aimPose);
                else
                    commonGestures.InvalidateAimPose();
            }

            if (subsystemDescriptor.supportsAimActivateValue)
            {
                if (provider.TryGetAimActivateValue(handedness, out var aimActiveValue))
                    commonGestures.UpdateAimActivateValue(aimActiveValue);
                else
                    commonGestures.InvalidateAimActivateValue();

                if (provider.TryGetAimActivatedState(handedness, out var isAimActivated))
                    commonGestures.UpdateAimActivatedState(isAimActivated);
                else
                    commonGestures.InvalidateAimActivatedState();
            }

            if (subsystemDescriptor.supportsGraspValue)
            {
                if (provider.TryGetGraspValue(handedness, out var graspValue))
                    commonGestures.UpdateGraspValue(graspValue);
                else
                    commonGestures.InvalidateGraspValue();

                if (provider.TryGetGraspFirmState(handedness, out var isGraspFirm))
                    commonGestures.UpdateGraspFirmState(isGraspFirm);
                else
                    commonGestures.InvalidateGraspFirmState();
            }

            if (subsystemDescriptor.supportsGripPose)
            {
                if (provider.TryGetGripPose(handedness, out var gripPose))
                    commonGestures.UpdateGripPose(gripPose);
                else
                    commonGestures.InvalidateGripPose();
            }

            if (subsystemDescriptor.supportsPinchPose)
            {
                if (provider.TryGetPinchPose(handedness, out var pinchPose))
                    commonGestures.UpdatePinchPose(pinchPose);
                else
                    commonGestures.InvalidatePinchPose();
            }

            if (subsystemDescriptor.supportsPinchValue)
            {
                if (provider.TryGetPinchValue(handedness, out var pinchValue))
                    commonGestures.UpdatePinchValue(pinchValue);
                else
                    commonGestures.InvalidatePinchValue();

                if (provider.TryGetPinchTouchedState(handedness, out var isPinchTouched))
                    commonGestures.UpdatePinchTouchedState(isPinchTouched);
                else
                    commonGestures.InvalidatePinchTouchedState();
            }

            if (subsystemDescriptor.supportsPokePose)
            {
                if (provider.TryGetPokePose(handedness, out var pokePose))
                    commonGestures.UpdatePokePose(pokePose);
                else
                    commonGestures.InvalidatePokePose();
            }
        }

        class StatePerHand
        {
            internal StatePerHand(Handedness handedness)
            {
                m_Hand = new XRHand(Allocator.Persistent, handedness, XRHand.LifetimeType.Subsystem);
                m_CommonGestures = new XRCommonHandGestures(handedness);
            }

            internal void OnDestroy() => m_Hand.Dispose();

            internal XRHand m_Hand;
            internal XRCommonHandGestures m_CommonGestures;
            internal XRHandAimState m_AimState;
            internal bool m_IsAimStateValid;
        }

        StatePerHand[] m_StatePerHand;

        UpdateSuccessFlags m_UpdateSuccessFlags;
        NativeArray<bool> m_JointsInLayout;

        List<IXRHandProcessor> m_Processors = new List<IXRHandProcessor>();

        // Profiler markers
        static readonly ProfilerMarker s_TryUpdateHandsMarker = new ProfilerMarker("XRHandSubsystem.TryUpdateHands");
        static readonly ProfilerMarker s_RetrieveCommonPoseDataMarker = new ProfilerMarker("XRHandSubsystem.RetrieveCommonPoseData");
        static readonly ProfilerMarker s_TrackingEventsMarker = new ProfilerMarker("XRHandSubsystem.TrackingEvents");
        static readonly ProfilerMarker s_ProviderTryUpdateHandsMarker = new ProfilerMarker("XRHandSubsystem.Provider.TryUpdateHands");
        static readonly ProfilerMarker s_PostUpdateMarker = new ProfilerMarker("XRHandSubsystem.PostUpdate");

        XRHandSubsystemActions m_Actions;

        static int CompareProcessors(IXRHandProcessor a, IXRHandProcessor b)
            => a.callbackOrder.CompareTo(b.callbackOrder);
    }
}
