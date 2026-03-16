using System.IO;
using Unity.Collections;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    static class SerializationExtensions
    {
        public static void Write(this BinaryWriter writer, XRHandJointTrackingState trackingState) => writer.Write((int)trackingState);
        public static void Write(this BinaryWriter writer, Handedness handedness) => writer.Write((int)handedness);
        public static void Write(this BinaryWriter writer, XRDetectedHandMeshLayout meshLayout) => writer.Write((int)meshLayout);
        public static void Write(this BinaryWriter writer, XRHandRecordingOptions recordingOptions) => writer.Write((int)recordingOptions);
        public static void Write(this BinaryWriter writer, FrameFlags frameFlags) => writer.Write((int)frameFlags);
        public static void Write(this BinaryWriter writer, HandFlags HandFlags) => writer.Write((int)HandFlags);
        public static void Write(this BinaryWriter writer, XRHandSubsystem.UpdateType updateType) => writer.Write((int)updateType);
        public static void Write(this BinaryWriter writer, XRHandSubsystem.UpdateSuccessFlags successFlags) => writer.Write((int)successFlags);
        public static void Write(this BinaryWriter writer, XRCommonHandGesturesFlags gesturesFlags) => writer.Write((int)gesturesFlags);
        public static void Write(this BinaryWriter writer, AimFlags aimFlags) => writer.Write((int)aimFlags);
        public static void Write(this BinaryWriter writer, InputTrackingState trackingState) => writer.Write((int)trackingState);
        public static void Write(this BinaryWriter writer, SnapshotFlags snapshotFlags) => writer.Write((int)snapshotFlags);
        public static void Write(this BinaryWriter writer, SequenceFlags sequenceFlags) => writer.Write((int)sequenceFlags);

        public static XRHandJointTrackingState ReadJointTrackingState(this BinaryReader reader) => (XRHandJointTrackingState)reader.ReadInt32();
        public static Handedness ReadHandedness(this BinaryReader reader) => (Handedness)reader.ReadInt32();
        public static XRDetectedHandMeshLayout ReadDetectedMeshLayout(this BinaryReader reader) => (XRDetectedHandMeshLayout)reader.ReadInt32();
        public static XRHandRecordingOptions ReadRecordingOptions(this BinaryReader reader) => (XRHandRecordingOptions)reader.ReadInt32();
        public static FrameFlags ReadFrameFlags(this BinaryReader reader) => (FrameFlags)reader.ReadInt32();
        public static HandFlags ReadHandFlags(this BinaryReader reader) => (HandFlags)reader.ReadInt32();
        public static XRHandSubsystem.UpdateType ReadUpdateType(this BinaryReader reader) => (XRHandSubsystem.UpdateType)reader.ReadInt32();
        public static XRHandSubsystem.UpdateSuccessFlags ReadUpdateSuccessFlags(this BinaryReader reader) => (XRHandSubsystem.UpdateSuccessFlags)reader.ReadInt32();
        public static XRCommonHandGesturesFlags ReadCommonGesturesFlags(this BinaryReader reader) => (XRCommonHandGesturesFlags)reader.ReadInt32();
        public static AimFlags ReadAimFlags(this BinaryReader reader) => (AimFlags)reader.ReadInt32();
        public static InputTrackingState ReadInputTrackingState(this BinaryReader reader) => (InputTrackingState)reader.ReadInt32();
        public static SnapshotFlags ReadSnapshotFlags(this BinaryReader reader) => (SnapshotFlags)reader.ReadInt32();
        public static SequenceFlags ReadSequenceFlags(this BinaryReader reader) => (SequenceFlags)reader.ReadInt32();

        public static void Write(this BinaryWriter writer, in Vector3 position)
        {
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);
        }

        public static void ReadVector3(this BinaryReader reader, out Vector3 position)
        {
            position = new Vector3();
            position.x = reader.ReadSingle();
            position.y = reader.ReadSingle();
            position.z = reader.ReadSingle();
        }

        public static void Write(this BinaryWriter writer, in Quaternion rotation)
        {
            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(rotation.w);
        }

        public static void ReadQuaternion(this BinaryReader reader, out Quaternion rotation)
        {
            rotation = new Quaternion();
            rotation.x = reader.ReadSingle();
            rotation.y = reader.ReadSingle();
            rotation.z = reader.ReadSingle();
            rotation.w = reader.ReadSingle();
        }

        public static void Write(this BinaryWriter writer, in Pose pose)
        {
            writer.Write(pose.position);
            writer.Write(pose.rotation);
        }

        public static void ReadPose(this BinaryReader reader, out Pose pose)
        {
            ReadVector3(reader, out var position);
            ReadQuaternion(reader, out var rotation);
            pose = new Pose(position, rotation);
        }

        public static void WriteHandJointLayout(this BinaryWriter writer, NativeArray<bool> flags)
        {
            const int k_StartingBit = 1 << Constants.k_NumBitsInByte - 1;
            writer.Write(flags.Length);

            int packed = 0;
            int numBitsPacked = 0;

            for (int flagIndex = 0; flagIndex < flags.Length; ++flagIndex)
            {
                if (flags[flagIndex])
                    packed |= k_StartingBit >> numBitsPacked;

                ++numBitsPacked;
                if (numBitsPacked == Constants.k_NumBitsInByte)
                {
                    writer.Write((byte)packed);
                    numBitsPacked = 0;
                    packed = 0;
                }
            }

            if (numBitsPacked != 0)
            {
                packed >>= Constants.k_NumBitsInByte - numBitsPacked;
                writer.Write((byte)packed);
            }
        }

        public static byte[] ReadHandJointLayout(this BinaryReader reader)
        {
            int numFlags = reader.ReadInt32();

            int numPackedByteFlags = numFlags / Constants.k_NumBitsInByte;
            if (numFlags != numPackedByteFlags * Constants.k_NumBitsInByte)
                ++numPackedByteFlags;

            var packedByteFlags = new byte[numPackedByteFlags];
            for (int flagChunkIndex = 0; flagChunkIndex < numPackedByteFlags; ++flagChunkIndex)
                packedByteFlags[flagChunkIndex] = reader.ReadByte();

            return packedByteFlags;
        }

        public static void Write(this BinaryWriter writer, XRHandRecordingOptions recordingOptions, in FrameBuffer frameBuffer)
        {
            writer.Write(frameBuffer.m_FrameFlags);
            writer.Write(frameBuffer.m_Timestamp);
            writer.Write(frameBuffer.m_HeadPoseTrackingState);
            if (frameBuffer.m_HeadPoseTrackingState != 0)
                writer.Write(frameBuffer.m_HeadPose);

            foreach (var updateType in HandsUtility.validUpdateTypes)
            {
                if (XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, recordingOptions))
                    writer.Write(frameBuffer.m_UpdateSuccessFlags[(int)updateType]);
            }

            foreach (var handedness in HandsUtility.validHandednessValues)
            {
                int handedIndex = handedness.ToIndex();

                var snapshot = frameBuffer.m_SnapshotBuffers[handedIndex];
                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForSnapshot()) != 0)
                    writer.Write(recordingOptions, frameBuffer.m_SnapshotBuffers[handedIndex]);

                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForCommonGestures()) != 0)
                    writer.Write(frameBuffer.m_CommonGestures[handedIndex]);

                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForAimState()) != 0)
                    writer.Write(frameBuffer.m_AimStates[handedIndex]);
            }
        }

        public static void ReadFrameBuffer(this BinaryReader reader, XRHandRecordingOptions recordingOptions, out FrameBuffer frameBuffer)
        {
            var frameFlags = reader.ReadFrameFlags();
            frameBuffer = new FrameBuffer(reader.ReadSingle(), recordingOptions);
            frameBuffer.m_FrameFlags = frameFlags;
            frameBuffer.m_HeadPoseTrackingState = reader.ReadInputTrackingState();
            if (frameBuffer.m_HeadPoseTrackingState != 0)
                reader.ReadPose(out frameBuffer.m_HeadPose);

            foreach (var updateType in HandsUtility.validUpdateTypes)
            {
                if (!XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, recordingOptions))
                    continue;

                frameBuffer.m_UpdateSuccessFlags[(int)updateType] = reader.ReadUpdateSuccessFlags();
                ++frameBuffer.m_NumReadUpdateSuccessFlags;
            }

            foreach (var handedness in HandsUtility.validHandednessValues)
            {
                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForSnapshot()) != 0)
                {
                    reader.ReadSnapshotBuffer(recordingOptions, handedness, out var snapshotBuffer);
                    frameBuffer.m_SnapshotBuffers[frameBuffer.m_NumReadSnapshotBuffers] = snapshotBuffer;
                    ++frameBuffer.m_NumReadSnapshotBuffers;
                }

                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForCommonGestures()) != 0)
                {
                    reader.ReadCommonGestures(out var commonGestures);
                    frameBuffer.m_CommonGestures[frameBuffer.m_NumReadCommonGestures] = commonGestures;
                    ++frameBuffer.m_NumReadCommonGestures;
                }

                if ((frameBuffer.m_FrameFlags & handedness.ToFrameFlagForAimState()) != 0)
                {
                    reader.ReadAimState(out var aimState);
                    frameBuffer.m_AimStates[frameBuffer.m_NumReadAimStates] = aimState;
                    ++frameBuffer.m_NumReadAimStates;
                }
            }
        }

        public static void Write(this BinaryWriter writer, XRHandRecordingOptions recordingOptions, in HandSnapshotBuffer snapshotBuffer)
        {
            writer.Write(snapshotBuffer.m_SnapshotFlags);

            foreach (var updateType in HandsUtility.validUpdateTypes)
            {
                int updateTypeIndex = (int)updateType;
                var flag = updateType.ToSnapshotFlag();
                if (XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, recordingOptions) && (snapshotBuffer.m_SnapshotFlags & flag) != 0)
                    writer.Write(snapshotBuffer.m_HandBuffers[updateTypeIndex]);
            }
        }

        public static void ReadSnapshotBuffer(this BinaryReader reader, XRHandRecordingOptions recordingOptions, Handedness handedness, out HandSnapshotBuffer snapshotBuffer)
        {
            snapshotBuffer = new HandSnapshotBuffer(recordingOptions);
            snapshotBuffer.m_SnapshotFlags = reader.ReadSnapshotFlags();

            foreach (var updateType in HandsUtility.validUpdateTypes)
            {
                int updateTypeIndex = (int)updateType;
                var flag = (SnapshotFlags)(1 << updateTypeIndex);
                if (XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, recordingOptions) && (snapshotBuffer.m_SnapshotFlags & flag) != 0)
                {
                    reader.ReadHandBuffer(handedness, out var handBuffer);
                    snapshotBuffer.m_HandBuffers[updateTypeIndex] = handBuffer;
                }
            }
        }

        public static void Write(this BinaryWriter writer, in HandBuffer handBuffer)
        {
            writer.Write(handBuffer.m_HandFlags);
            writer.Write(handBuffer.m_RootPose);

            if ((handBuffer.m_HandFlags & HandFlags.AreAllJointPosesValid) == 0)
                return;

            writer.Write(handBuffer.m_JointPoses.Length);
            foreach (var jointID in HandsUtility.validJointIDs)
                writer.Write(handBuffer.m_JointPoses[jointID.ToIndex()]);
        }

        public static void ReadHandBuffer(this BinaryReader reader, Handedness handedness, out HandBuffer handBuffer)
        {
            handBuffer = new HandBuffer(handedness);
            handBuffer.m_HandFlags = reader.ReadHandFlags();
            reader.ReadPose(out handBuffer.m_RootPose);

            if ((handBuffer.m_HandFlags & HandFlags.AreAllJointPosesValid) == 0)
                return;

            int jointArrayLength = reader.ReadInt32();
            if (jointArrayLength != Constants.k_NumJointsPerHand)
                Debug.LogWarning("Array length does not match what we assumed for joint poses in a hand.");

            for (int jointIndex = 0; jointIndex < jointArrayLength; ++jointIndex)
            {
                ReadPose(reader, out var pose);
                handBuffer.m_JointPoses[jointIndex] = pose;
            }
        }

        public static void Write(this BinaryWriter writer, in XRCommonHandGesturesState commonHandGestures)
        {
            writer.Write(commonHandGestures.handedness);

            var flags = commonHandGestures.flags;
            writer.Write(flags);

            if ((flags & XRCommonHandGesturesFlags.IsAimPoseValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidAimPose);

            if ((flags & XRCommonHandGesturesFlags.IsAimActivateValueValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidAimActivateValue);

            if ((flags & XRCommonHandGesturesFlags.IsGraspValueValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidGraspValue);

            if ((flags & XRCommonHandGesturesFlags.IsGripPoseValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidGripPose);

            if ((flags & XRCommonHandGesturesFlags.IsPinchPoseValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidPinchPose);

            if ((flags & XRCommonHandGesturesFlags.IsPinchValueValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidPinchValue);

            if ((flags & XRCommonHandGesturesFlags.IsPokePoseValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidPokePose);

            if ((flags & XRCommonHandGesturesFlags.IsAimActivatedStateValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidIsAimActivated);

            if ((flags & XRCommonHandGesturesFlags.IsGraspFirmStateValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidIsGraspFirm);

            if ((flags & XRCommonHandGesturesFlags.IsPinchTouchedStateValid) != 0)
                writer.Write(commonHandGestures.possiblyInvalidIsPinchTouched);
        }

        public static void ReadCommonGestures(this BinaryReader reader, out XRCommonHandGesturesState commonHandGesturesState)
            => commonHandGesturesState = new XRCommonHandGesturesState(reader);

        public static void Write(this BinaryWriter writer, in XRHandAimState aimState)
        {
            writer.Write(aimState.handedness);
            writer.Write(aimState.flags);
            writer.Write(aimState.trackingState);

            writer.Write(aimState.reserved0);
            writer.Write(aimState.reserved1);

            writer.Write(aimState.pinchStrengthIndex);
            writer.Write(aimState.pinchStrengthMiddle);
            writer.Write(aimState.pinchStrengthRing);
            writer.Write(aimState.pinchStrengthLittle);

            if (aimState.TryGetAimPose(out var aimPose))
                writer.Write(aimPose);
        }

        public static void ReadAimState(this BinaryReader reader, out XRHandAimState aimState)
            => aimState = new XRHandAimState(reader);

        public static void Write(this BinaryWriter writer, XRFingerShapeConfiguration config)
        {
            writer.Write(config.minimumFullCurlDegrees1);
            writer.Write(config.maximumFullCurlDegrees1);
            writer.Write(config.minimumFullCurlDegrees2);
            writer.Write(config.maximumFullCurlDegrees2);
            writer.Write(config.minimumFullCurlDegrees3);
            writer.Write(config.maximumFullCurlDegrees3);

            writer.Write(config.minimumBaseCurlDegrees);
            writer.Write(config.maximumBaseCurlDegrees);

            writer.Write(config.minimumTipCurlDegrees1);
            writer.Write(config.maximumTipCurlDegrees1);
            writer.Write(config.minimumTipCurlDegrees2);
            writer.Write(config.maximumTipCurlDegrees2);

            writer.Write(config.minimumPinchDistance);
            writer.Write(config.maximumPinchDistance);

            writer.Write(config.minimumSpreadDegrees);
            writer.Write(config.maximumSpreadDegrees);
        }

        public static void ReadFingerShapeConfiguration(this BinaryReader reader, out XRFingerShapeConfigurationState state)
        {
            float minimumFullCurlDegrees1 = reader.ReadSingle();
            float maximumFullCurlDegrees1 = reader.ReadSingle();
            float minimumFullCurlDegrees2 = reader.ReadSingle();
            float maximumFullCurlDegrees2 = reader.ReadSingle();
            float minimumFullCurlDegrees3 = reader.ReadSingle();
            float maximumFullCurlDegrees3 = reader.ReadSingle();

            float minimumBaseCurlDegrees = reader.ReadSingle();
            float maximumBaseCurlDegrees = reader.ReadSingle();

            float minimumTipCurlDegrees1 = reader.ReadSingle();
            float maximumTipCurlDegrees1 = reader.ReadSingle();
            float minimumTipCurlDegrees2 = reader.ReadSingle();
            float maximumTipCurlDegrees2 = reader.ReadSingle();

            float minimumPinchDistance = reader.ReadSingle();
            float maximumPinchDistance = reader.ReadSingle();

            float minimumSpreadDegrees = reader.ReadSingle();
            float maximumSpreadDegrees = reader.ReadSingle();

            state = new XRFingerShapeConfigurationState
            {
                minimumFullCurlDegrees1 = minimumFullCurlDegrees1,
                maximumFullCurlDegrees1 = maximumFullCurlDegrees1,
                minimumFullCurlDegrees2 = minimumFullCurlDegrees2,
                maximumFullCurlDegrees2 = maximumFullCurlDegrees2,
                minimumFullCurlDegrees3 = minimumFullCurlDegrees3,
                maximumFullCurlDegrees3 = maximumFullCurlDegrees3,

                minimumBaseCurlDegrees = minimumBaseCurlDegrees,
                maximumBaseCurlDegrees = maximumBaseCurlDegrees,

                minimumTipCurlDegrees1 = minimumTipCurlDegrees1,
                maximumTipCurlDegrees1 = maximumTipCurlDegrees1,
                minimumTipCurlDegrees2 = minimumTipCurlDegrees2,
                maximumTipCurlDegrees2 = maximumTipCurlDegrees2,

                minimumPinchDistance = minimumPinchDistance,
                maximumPinchDistance = maximumPinchDistance,

                minimumSpreadDegrees = minimumSpreadDegrees,
                maximumSpreadDegrees = maximumSpreadDegrees,
            };
        }
    }
}
