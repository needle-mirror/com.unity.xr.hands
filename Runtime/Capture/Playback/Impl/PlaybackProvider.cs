using Unity.Collections;
using UnityEngine.XR.Hands.ProviderImplementation;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Hand tracking provider for the playback.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    class PlaybackProvider : XRHandSubsystemProvider
    {
        // These static fields should not be reset between Play mode sessions since registration list of subsystem descriptors are not cleared.
        static XRHandSubsystemDescriptor s_RegisteredDescriptor;
        static bool s_SubsystemRegistered;

        XRHandPlayback[] m_Playbacks;

        internal override void SubscribeToSubsystemActions(ref XRHandSubsystemActions actions)
        {
            m_Playbacks = new[] {
                new XRHandPlayback(ref actions, Handedness.Left),
                new XRHandPlayback(ref actions, Handedness.Right)
            };
        }

        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            for (int jointIndex = 0; jointIndex < handJointsInLayout.Length; ++jointIndex)
                handJointsInLayout[jointIndex] = true;
        }

        protected internal override bool AllowJointProcessing() => false;

        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose, NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose, NativeArray<XRHandJoint> rightHandJoints)
        {
            var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;
            successFlags |= m_Playbacks[Handedness.Left.ToIndex()].TryUpdateHand(updateType, ref leftHandRootPose, leftHandJoints);
            successFlags |= m_Playbacks[Handedness.Right.ToIndex()].TryUpdateHand(updateType, ref rightHandRootPose, rightHandJoints);

            return successFlags;
        }

        public override bool canSurfaceCommonPoseData => true;

        internal override bool TryGetCommonGesturesState(Handedness handedness, out XRCommonHandGesturesState commonGestures) =>
            m_Playbacks[handedness.ToIndex()].TryGetCommonGesturesState(out commonGestures);

        public override bool TryGetAimPose(Handedness handedness, out Pose aimPose) =>
            m_Playbacks[handedness.ToIndex()].TryGetAimPose(out aimPose);

        public override bool TryGetAimActivateValue(Handedness handedness, out float aimActivateValue) =>
            m_Playbacks[handedness.ToIndex()].TryGetAimActivateValue(out aimActivateValue);

        public override bool TryGetAimActivatedState(Handedness handedness, out bool isAimActivated) =>
            m_Playbacks[handedness.ToIndex()].TryGetAimActivatedState(out isAimActivated);

        public override bool TryGetGraspValue(Handedness handedness, out float graspValue) =>
            m_Playbacks[handedness.ToIndex()].TryGetGraspValue(out graspValue);

        public override bool TryGetGraspFirmState(Handedness handedness, out bool isGraspFirm) =>
            m_Playbacks[handedness.ToIndex()].TryGetGraspFirmState(out isGraspFirm);

        public override bool TryGetGripPose(Handedness handedness, out Pose gripPose) =>
            m_Playbacks[handedness.ToIndex()].TryGetGripPose(out gripPose);

        public override bool TryGetPinchPose(Handedness handedness, out Pose pinchPose) =>
            m_Playbacks[handedness.ToIndex()].TryGetPinchPose(out pinchPose);

        public override bool TryGetPinchValue(Handedness handedness, out float pinchValue) =>
            m_Playbacks[handedness.ToIndex()].TryGetPinchValue(out pinchValue);

        public override bool TryGetPinchTouchedState(Handedness handedness, out bool isPinched) =>
            m_Playbacks[handedness.ToIndex()].TryGetPinchTouchedState(out isPinched);

        public override bool TryGetPokePose(Handedness handedness, out Pose pokePose) =>
            m_Playbacks[handedness.ToIndex()].TryGetPokePose(out pokePose);

        public override void Start()
        { }

        public override void Stop()
        { }

        public override void Destroy()
        { }

        public override XRDetectedHandMeshLayout detectedHandMeshLayout
            => m_Playbacks[Handedness.Right.ToIndex()].detectedHandMeshLayout;

        public override bool TryGetAimState(Handedness handedness, out XRHandAimState aimState)
        {
            aimState = default;
            return m_Playbacks[handedness.ToIndex()].TryGetAimState(out aimState);
        }

        internal void Initialize(XRHandPlayback leftPlayback, XRHandPlayback rightPlayback)
        {
            m_Playbacks = new XRHandPlayback[]
            {
                leftPlayback,
                rightPlayback,
            };
        }

        internal XRHandPlayback GetUserFacingPlayback(Handedness handedness)
        {
            return m_Playbacks[handedness.ToIndex()];
        }

        internal static XRHandSubsystemDescriptor GetRegisteredDescriptor() => s_RegisteredDescriptor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            // "ResetStaticsOnLoad()" section of this RuntimeInitializeOnLoadMethod
            // Do not reset s_RegisteredDescriptor or s_SubsystemRegistered since registration list of subsystem descriptors are not cleared.

            if (!s_SubsystemRegistered)
            {
                var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
                {
                    id = Constants.k_PlaybackDescriptorID,
                    providerType = typeof(PlaybackProvider),
                    supportsAimPose = true,
                    supportsAimActivateValue = true,
                    supportsGraspValue = true,
                    supportsGripPose = true,
                    supportsPinchPose = true,
                    supportsPinchValue = true,
                    supportsPokePose = true,
                };
                s_RegisteredDescriptor = XRHandSubsystemDescriptor.RegisterInternal(handsSubsystemCinfo);
                s_SubsystemRegistered = true;
            }
        }
    }
}
