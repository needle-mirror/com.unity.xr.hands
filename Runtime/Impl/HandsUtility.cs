using System.Collections.ObjectModel;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Playback;
using UnityEngine.XR.Hands.Capture.Recording;

namespace UnityEngine.XR.Hands
{
    static class HandsUtility
    {
        public static bool IsValid(this Handedness handedness) => handedness == Handedness.Left || handedness == Handedness.Right;

        public static int ToIndex(this XRHandFingerID fingerID) => (int)fingerID;
        public static int ToIndex(this Handedness handedness) => (int)handedness - 1;
        public static int ToIndex(this XRHandSubsystem.UpdateType updateType) => (int)updateType;

        public static bool IsAnyFlagEnabled(this XRHandPlaybackOptions flagsBeingQueried, XRHandPlaybackOptions flagsToCheck) => (flagsBeingQueried & flagsToCheck) != 0;
        public static bool IsAnyFlagEnabled(this XRHandRecordingOptions flagsBeingQueried, XRHandRecordingOptions flagsToCheck) => (flagsBeingQueried & flagsToCheck) != 0;

        public static XRHandSubsystem.UpdateType GetOtherUpdateType(this XRHandSubsystem.UpdateType updateType)
            => (XRHandSubsystem.UpdateType)((int)(updateType + 1) % Constants.k_NumUpdateTypes);

        public static XRHandSubsystem.UpdateSuccessFlags ToRootPoseUpdateSuccessFlag(this Handedness handedness)
            => handedness == Handedness.Left ? XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose : XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose;
        public static XRHandSubsystem.UpdateSuccessFlags ToJointsUpdateSuccessFlag(this Handedness handedness)
            => handedness == Handedness.Left ? XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints : XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
        public static XRHandSubsystem.UpdateSuccessFlags ToUpdateSuccessFlags(this Handedness handedness)
            => handedness == Handedness.Left ? XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints : XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;

        public static FrameFlags ToFrameFlagForSnapshot(this Handedness handedness)
        {
            return handedness == Handedness.Left
                ? FrameFlags.IsLeftSnapshotValid
                : FrameFlags.IsRightSnapshotValid;
        }

        public static FrameFlags ToFrameFlagForCommonGestures(this Handedness handedness)
        {
            return handedness == Handedness.Left
                ? FrameFlags.IsLeftCommonGesturesValid
                : FrameFlags.IsRightCommonGesturesValid;
        }

        public static FrameFlags ToFrameFlagForAimState(this Handedness handedness)
        {
            return handedness == Handedness.Left
                ? FrameFlags.IsLeftAimStateValid
                : FrameFlags.IsRightAimStateValid;
        }

        public static SnapshotFlags ToSnapshotFlag(this XRHandSubsystem.UpdateType updateType)
            => (SnapshotFlags)(1 << updateType.ToIndex());

        internal static ReadOnlyCollection<XRHandSubsystem.UpdateType> validUpdateTypes => s_ValidUpdateTypesReadOnly;
        internal static ReadOnlyCollection<Handedness> validHandednessValues => s_ValidHandednessValuesReadOnly;
        internal static ReadOnlyCollection<XRHandFingerID> validFingerIDs => s_ValidFingerIDsReadOnly;
        internal static ReadOnlyCollection<XRHandJointID> validJointIDs => s_ValidJointIDsReadOnly;

        // blendScalar == 0 => interpolatedPose == poseBefore
        // blendScalar == 1 => interpolatedPose == poseAfter
        internal static void Interpolate(
            in Pose poseBefore, in Pose poseAfter,
            float blendScalar, out Pose interpolatedPose)
        {
            interpolatedPose = new Pose(
                Vector3.Lerp(poseBefore.position, poseAfter.position, blendScalar),
                Quaternion.Slerp(poseBefore.rotation, poseAfter.rotation, blendScalar));
        }

        static HandsUtility()
        {
            s_ValidUpdateTypes = new XRHandSubsystem.UpdateType[]
            {
                XRHandSubsystem.UpdateType.Dynamic,
                XRHandSubsystem.UpdateType.BeforeRender,
            };

            s_ValidHandednessValues = new Handedness[]
            {
                Handedness.Left,
                Handedness.Right,
            };

            s_ValidFingerIDs = new XRHandFingerID[]
            {
                XRHandFingerID.Thumb,
                XRHandFingerID.Index,
                XRHandFingerID.Middle,
                XRHandFingerID.Ring,
                XRHandFingerID.Little,
            };

            s_ValidJointIDs = new XRHandJointID[]
            {
                XRHandJointID.Wrist,
                XRHandJointID.Palm,
                XRHandJointID.ThumbMetacarpal,
                XRHandJointID.ThumbProximal,
                XRHandJointID.ThumbDistal,
                XRHandJointID.ThumbTip,
                XRHandJointID.IndexMetacarpal,
                XRHandJointID.IndexProximal,
                XRHandJointID.IndexIntermediate,
                XRHandJointID.IndexDistal,
                XRHandJointID.IndexTip,
                XRHandJointID.MiddleMetacarpal,
                XRHandJointID.MiddleProximal,
                XRHandJointID.MiddleIntermediate,
                XRHandJointID.MiddleDistal,
                XRHandJointID.MiddleTip,
                XRHandJointID.RingMetacarpal,
                XRHandJointID.RingProximal,
                XRHandJointID.RingIntermediate,
                XRHandJointID.RingDistal,
                XRHandJointID.RingTip,
                XRHandJointID.LittleMetacarpal,
                XRHandJointID.LittleProximal,
                XRHandJointID.LittleIntermediate,
                XRHandJointID.LittleDistal,
                XRHandJointID.LittleTip,
            };

            s_ValidUpdateTypesReadOnly = new ReadOnlyCollection<XRHandSubsystem.UpdateType>(s_ValidUpdateTypes);
            s_ValidHandednessValuesReadOnly = new ReadOnlyCollection<Handedness>(s_ValidHandednessValues);
            s_ValidFingerIDsReadOnly = new ReadOnlyCollection<XRHandFingerID>(s_ValidFingerIDs);
            s_ValidJointIDsReadOnly = new ReadOnlyCollection<XRHandJointID>(s_ValidJointIDs);
        }

        static readonly XRHandSubsystem.UpdateType[] s_ValidUpdateTypes;
        static readonly Handedness[] s_ValidHandednessValues;
        static readonly XRHandFingerID[] s_ValidFingerIDs;
        static readonly XRHandJointID[] s_ValidJointIDs;

        static readonly ReadOnlyCollection<XRHandSubsystem.UpdateType> s_ValidUpdateTypesReadOnly;
        static readonly ReadOnlyCollection<Handedness> s_ValidHandednessValuesReadOnly;
        static readonly ReadOnlyCollection<XRHandFingerID> s_ValidFingerIDsReadOnly;
        static readonly ReadOnlyCollection<XRHandJointID> s_ValidJointIDsReadOnly;
    }
}
