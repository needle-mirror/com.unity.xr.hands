using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Hand tracking provider for the playback.
    /// </summary>
    class PlaybackProvider : XRHandSubsystemProvider
    {
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

        public override bool TryGetAimPose(Handedness handedness, out Pose aimPose)
        {
            aimPose = Pose.identity;
            return m_Playbacks[handedness.ToIndex()].TryGetAimPose(ref aimPose);
        }

        public override bool TryGetAimActivateValue(Handedness handedness, out float aimActivateValue)
        {
            aimActivateValue = 0f;
            return m_Playbacks[handedness.ToIndex()].TryGetAimActivateValue(ref aimActivateValue);
        }

        public override bool TryGetGraspValue(Handedness handedness, out float graspValue)
        {
            graspValue = 0f;
            return m_Playbacks[handedness.ToIndex()].TryGetGraspValue(ref graspValue);
        }

        public override bool TryGetGripPose(Handedness handedness, out Pose gripPose)
        {
            gripPose = Pose.identity;
            return m_Playbacks[handedness.ToIndex()].TryGetGripPose(ref gripPose);
        }

        public override bool TryGetPinchPose(Handedness handedness, out Pose pinchPose)
        {
            pinchPose = Pose.identity;
            return m_Playbacks[handedness.ToIndex()].TryGetPinchPose(ref pinchPose);
        }

        public override bool TryGetPinchValue(Handedness handedness, out float pinchValue)
        {
            pinchValue = 0f;
            return m_Playbacks[handedness.ToIndex()].TryGetPinchValue(ref pinchValue);
        }

        public override bool TryGetPokePose(Handedness handedness, out Pose pokePose)
        {
            pokePose = Pose.identity;
            return m_Playbacks[handedness.ToIndex()].TryGetPokePose(ref pokePose);
        }

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
            return m_Playbacks[handedness.ToIndex()].TryGetAimState(ref aimState);
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
