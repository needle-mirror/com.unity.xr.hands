using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands.ProviderImplementation;

using System.Text;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    struct FrameBuffer
    {
        internal FrameBuffer(float timestamp, XRHandRecordingOptions recordingOptions)
        {
            m_RecordingOptions = recordingOptions;
            m_FrameFlags = FrameFlags.None;
            m_Timestamp = timestamp;

            m_NumReadUpdateSuccessFlags = 0;
            m_NumReadSnapshotBuffers = 0;
            m_NumReadCommonGestures = 0;
            m_NumReadAimStates = 0;

            int numRelevantUpdateTypes = XRHandCaptureSequence.GetNumRelevantUpdateTypes(recordingOptions);
            m_UpdateSuccessFlags = new NativeArray<XRHandSubsystem.UpdateSuccessFlags>(numRelevantUpdateTypes, Allocator.Temp);

            m_SnapshotBuffers = new NativeArray<HandSnapshotBuffer>(k_NumHands, Allocator.Temp);
            for (int snapshotBufferIndex = 0; snapshotBufferIndex < m_SnapshotBuffers.Length; ++snapshotBufferIndex)
                m_SnapshotBuffers[snapshotBufferIndex] = new HandSnapshotBuffer(recordingOptions);

            m_CommonGestures = new NativeArray<XRCommonHandGesturesState>(k_NumHands, Allocator.Temp);
            m_AimStates = new NativeArray<XRHandAimState>(k_NumHands, Allocator.Temp);
            m_HeadPoseTrackingState = InputTrackingState.None;
            m_HeadPose = Pose.identity;
            m_MarkedAsValid = true;
        }

        internal void Dispose()
        {
            for (int snapshotBufferIndex = 0; snapshotBufferIndex < m_SnapshotBuffers.Length; ++snapshotBufferIndex)
                m_SnapshotBuffers[snapshotBufferIndex].Dispose();
            if (m_SnapshotBuffers.IsCreated)
                m_SnapshotBuffers.Dispose();

            if (m_CommonGestures.IsCreated)
                m_CommonGestures.Dispose();

            if (m_AimStates.IsCreated)
                m_AimStates.Dispose();

            if (m_UpdateSuccessFlags.IsCreated)
                m_UpdateSuccessFlags.Dispose();
        }

        internal void CaptureSingleUpdateStep(
            XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            SnapshotFlags TryCaptureHand(
                XRHandSubsystem.UpdateType updateType,
                in XRHand hand,
                NativeArray<HandBuffer> handBuffers)
            {
                int updateTypeIndex = updateType.ToIndex();

                var handBuffer = handBuffers[updateTypeIndex];
                handBuffer.m_RootPose = hand.rootPose;
                foreach (var jointID in HandsUtility.validJointIDs)
                {
                    var joint = hand.GetJoint(jointID);
                    if (!joint.TryGetPose(out var jointPose))
                    {
                        handBuffers[updateTypeIndex] = handBuffer;
                        return SnapshotFlags.None;
                    }

                    handBuffer.m_JointPoses[jointID.ToIndex()] = jointPose;
                }

                handBuffer.m_HandFlags |= HandFlags.AreAllJointPosesValid;
                if (hand.isTracked)
                    handBuffer.m_HandFlags |= HandFlags.WasHandTrackedDuringCapture;

                handBuffers[updateTypeIndex] = handBuffer;
                return updateType.ToSnapshotFlag();
            }

            AttemptEnsureValidHeadPose();

            m_UpdateSuccessFlags[(int)updateType] = updateSuccessFlags;

            foreach (var handedness in HandsUtility.validHandednessValues)
            {
                int handedIndex = handedness.ToIndex();
                var snapshotBuffer = m_SnapshotBuffers[handedIndex];
                var flag = TryCaptureHand(updateType, subsystem.GetHand(handedness), snapshotBuffer.m_HandBuffers);

                if (flag != SnapshotFlags.None)
                    m_FrameFlags |= handedness.ToFrameFlagForSnapshot();

                snapshotBuffer.m_SnapshotFlags |= flag;
                m_SnapshotBuffers[handedIndex] = snapshotBuffer;
            }
        }

        internal void CaptureUpdateTypeAgnosticData(XRHandSubsystem subsystem)
        {
            FrameFlags TryCaptureExtraHandData(
                XRHandSubsystem subsystem,
                Handedness handedness,
                NativeArray<XRCommonHandGesturesState> commonGestures,
                NativeArray<XRHandAimState> aimStates)
            {
                var frameFlags = FrameFlags.None;
                int handedIndex = handedness.ToIndex();

                if (subsystem.GetProvider().canSurfaceCommonPoseData)
                {
                    commonGestures[handedIndex] = new XRCommonHandGesturesState(subsystem.GetCommonGestures(handedness));
                    frameFlags |= handedness.ToFrameFlagForCommonGestures();
                }

                if (subsystem.TryGetAimState(handedness, out var aimStateForCurrentHand))
                {
                    aimStates[handedIndex] = aimStateForCurrentHand;
                    frameFlags |= handedness.ToFrameFlagForAimState();
                }

                return frameFlags;
            }

            m_FrameFlags |= TryCaptureExtraHandData(subsystem, Handedness.Left, m_CommonGestures, m_AimStates);
            m_FrameFlags |= TryCaptureExtraHandData(subsystem, Handedness.Right, m_CommonGestures, m_AimStates);
        }

        internal byte[] ConvertToRawFrame(MemoryStream stream, BinaryWriter writer)
        {
            stream.SetLength(0);
            writer.Write(m_RecordingOptions, this);
            return stream.ToArray();
        }

        void AttemptEnsureValidHeadPose()
        {
            if (m_HeadPoseTrackingState == (InputTrackingState.Position | InputTrackingState.Rotation))
                return;

            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (!device.isValid)
                return;

            if ((m_HeadPoseTrackingState & InputTrackingState.Position) == 0 &&
                device.TryGetFeatureValue(CommonUsages.devicePosition, out var position))
            {
                m_HeadPose.position = position;
                m_HeadPoseTrackingState |= InputTrackingState.Position;
            }

            if ((m_HeadPoseTrackingState & InputTrackingState.Rotation) == 0 &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation))
            {
                m_HeadPose.rotation = rotation;
                m_HeadPoseTrackingState |= InputTrackingState.Rotation;
            }
        }

        internal FrameFlags m_FrameFlags;
        internal float m_Timestamp;
        internal Pose m_HeadPose;
        internal InputTrackingState m_HeadPoseTrackingState;
        internal NativeArray<XRHandSubsystem.UpdateSuccessFlags> m_UpdateSuccessFlags;

        internal NativeArray<HandSnapshotBuffer> m_SnapshotBuffers;
        internal NativeArray<XRCommonHandGesturesState> m_CommonGestures;
        internal NativeArray<XRHandAimState> m_AimStates;

        // only here for switching off of, it's already part of the asset-wide
        // data and does not need to be captured on a per-frame basis
        internal readonly XRHandRecordingOptions m_RecordingOptions;

        // only for reading in
        internal int m_NumReadUpdateSuccessFlags;
        internal int m_NumReadSnapshotBuffers;
        internal int m_NumReadCommonGestures;
        internal int m_NumReadAimStates;

        // only useful/valid when preparing for writing out
        internal bool m_MarkedAsValid;

        internal const int k_NumPossibleUpdateTypesPerFrame = 2;
        internal const int k_NumHands = 2;

    }
}
