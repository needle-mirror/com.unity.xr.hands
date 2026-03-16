using System;
using UnityEngine;
using UnityEngine.XR.Hands.Capture.Recording;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// A single frame of captured hand data.
    /// </summary>
    [Serializable]
    public struct XRHandCaptureFrame : IEquatable<XRHandCaptureFrame>
    {
        /// <summary>
        /// The timestamp of this captured frame in seconds since the start of the recording.
        /// </summary>
        /// <value>The time at which this frame was captured.</value>
        public float timestamp
        {
            get => m_Timestamp;
            internal set => m_Timestamp = value;
        }

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRHandCaptureFrame"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            return HashCodeUtil.Combine(
                HashCodeUtil.ReferenceHash(m_ContainingAsset),
                m_FrameFlags.GetHashCode(),
                m_Timestamp.GetHashCode(),
                m_AlreadyFlatIndex.GetHashCode(),
                m_HandSnapshotsIndex.GetHashCode(),
                m_CommonGesturesIndex.GetHashCode(),
                m_AimStatesIndex.GetHashCode());
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandCaptureFrame"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandCaptureFrame"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRHandCaptureFrame other)
        {
            return ReferenceEquals(m_ContainingAsset, other.m_ContainingAsset) &&
                m_FrameFlags == other.m_FrameFlags &&
                m_Timestamp == other.m_Timestamp &&
                m_AlreadyFlatIndex == other.m_AlreadyFlatIndex &&
                m_HandSnapshotsIndex == other.m_HandSnapshotsIndex &&
                m_CommonGesturesIndex == other.m_CommonGesturesIndex &&
                m_AimStatesIndex == other.m_AimStatesIndex;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandCaptureFrame"/> and <see cref="Equals(XRHandCaptureFrame)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => (obj is XRHandCaptureFrame other) && Equals(other);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHandCaptureFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(XRHandCaptureFrame lhs, XRHandCaptureFrame rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHandCaptureFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(XRHandCaptureFrame lhs, XRHandCaptureFrame rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Checks whether the corresponding <see cref="XRHand"/><c>.</c><see cref="XRHand.isTracked"/>
        /// reported as <see langword="true"/> when the data was captured.
        /// </summary>
        /// <param name="handedness">
        /// Denotes which hand you wish to get tracking data for. Must be
        /// either <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// if <c>IsHandTracked</c> to have a chance to succeed.
        /// </param>
        /// <returns>
        /// Returns what the corresponding <see cref="XRHand"/><c>.</c><see cref="XRHand.isTracked"/>
        /// reported when the tracking data was captured.
        /// </returns>
        /// <remarks>
        /// Will be the same as what <see cref="XRHand.isTracked"/> reports
        /// <see cref="Handedness"/> and <see cref="XRHandSubsystem.UpdateType"/>
        /// passed to <c>IsHandTracked</c> on the same <c>XRHandCaptureFrame</c>
        /// if you pass you same <see cref="Handedness"/> to
        /// <see cref="TryGetHandSnapshot"/> and the same
        /// <see cref="XRHandSubsystem.UpdateType"/> to the resulting
        /// <see cref="XRHandCaptureSnapshot"/><c>.</c><see cref="XRHandCaptureSnapshot.TryGetHand"/>,
        /// assuming both of the latter calls were to succeed (if either were to fail,
        /// <c>IsHandTracked</c> would return <see langword="false"/>).
        /// </remarks>
        public bool IsHandTracked(Handedness handedness)
            => IsHandTracked(handedness, XRHandSubsystem.UpdateType.Dynamic);

        /// <inheritdoc cref="IsHandTracked(Handedness)"/>
        /// <param name="updateType">
        /// The update type passed to <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.TryUpdateHands"/>
        /// you wish to query for captured per-<see cref="XRHandSubsystem.UpdateType"/> tracking data.
        /// If a value of <see cref="XRHandSubsystem.UpdateType.BeforeRender"/> is supplied
        /// but <see cref="sourceCaptureSequence"/>'s <see cref="XRHandCaptureSequence.optionsRecordedWith"/>
        /// does not have <see cref="XRHandRecordingOptions"/><c>.</c><see cref="XRHandRecordingOptions.AlsoCaptureBeforeRender"/>
        /// enabled, <c>IsHandTracked</c> will always fail.
        /// </param>
        public bool IsHandTracked(Handedness handedness, XRHandSubsystem.UpdateType updateType)
            => TryGetHandSnapshot(handedness, out var handCaptureSnapshot) && handCaptureSnapshot.IsHandTracked(updateType);

        /// <summary>
        /// Attempts to retrieve a joint from the captured hand data for the
        /// <see cref="XRHandSubsystem.UpdateType.Dynamic"/> update step.
        /// </summary>
        /// <param name="joint">The retrieved joint if this returns <see langword="true"/>.</param>
        /// <param name="handedness">The handedness of the hand to retrieve the joint from.</param>
        /// <param name="id">The ID of the joint to retrieve.</param>
        /// <returns>Returns <see langword="true"/> if the joint was successfully retrieved.</returns>
        /// <remarks>Use <see cref="TryGetHandSnapshot"/> and <see cref="XRHandCaptureSnapshot.TryGetHand"/>
        /// to access joint data for a specific <see cref="XRHandSubsystem.UpdateType"/>.</remarks>
        public bool TryGetJoint(out XRHandJoint joint, Handedness handedness, XRHandJointID id)
        {
            if (!TryGetHandSnapshot(handedness, out var snapshot) ||
                !snapshot.TryGetHand(Unity.Collections.Allocator.Temp, out var hand))
            {
                joint = default;
                return false;
            }
            joint = hand.GetJoint(id);
            hand.Dispose();
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the per-<see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.UpdateType"/>
        /// data during this frame of capture, such as <see cref="XRHand"/>, associated with the
        /// given <see cref="Handedess"/>.
        /// </summary>
        /// <param name="handedness">
        /// Denotes which hand you wish to get tracking data for. Must be
        /// either <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// if <c>TryGetHandSnapshot</c> to have a chance to succeed.
        /// </param>
        /// <param name="snapshot">
        /// If <c>TryGetHandSnapshot</c> returns <see langword="true"/>, this
        /// will be filled out with the data required to ask for corresponding
        /// per-<see cref="XRHandSubsystem.UpdateType"/> data, such as <see cref="XRHand"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the <see cref="XRHandCaptureSnapshot"/> was successfully
        /// retrieved and can be successfully queried for captured tracking data.
        /// </returns>
        internal bool TryGetHandSnapshot(
            Handedness handedness,
            out XRHandCaptureSnapshot snapshot)
        {
            if (!TryGetFlatHandSnapshotIndex(handedness, out int flatHandSnapshotIndex))
            {
                snapshot = default;
                return false;
            }

            snapshot = m_ContainingAsset.flattenedHandSnapshots[flatHandSnapshotIndex];
            return true;
        }

        /// <summary>
        /// Reports the <see cref="XRHandSubsystem.UpdateSuccessFlags"/> that <see cref="XRHandSubsystem"/>
        /// reported during capture this frame for the given <see cref="XRHandSubsystem.UpdateType"/>.
        /// </summary>
        /// <param name="updateType">
        /// The <see cref="XRHandSubsystem.UpdateType"/> you wish to obtain captured
        /// tracking data for.
        /// </param>
        /// <returns>
        /// The <see cref="XRHandSubsystem.UpdateSuccessFlags"/> that <see cref="XRHandSubsystem"/>
        /// reported. If <see cref="XRHandRecordingOptions.AlsoCaptureBeforeRender"/> option
        /// was disabled when the tracking data was captured, <c>GetUpdateSuccessFlags</c>
        /// will always return <c>XRHandSubsystem.UpdateSuccessFlags.</c><see cref="XRHandSubsystem.UpdateSuccessFlags.None"/>.
        /// </returns>
        public XRHandSubsystem.UpdateSuccessFlags GetUpdateSuccessFlags(XRHandSubsystem.UpdateType updateType = XRHandSubsystem.UpdateType.Dynamic)
        {
            if (!TryGetFlatUpdateSuccessFlagsIndex(updateType, out int flatUpdateSuccessFlagsIndex))
                return XRHandSubsystem.UpdateSuccessFlags.None;

            return m_ContainingAsset.flattenedUpdateSuccessFlags[flatUpdateSuccessFlagsIndex];
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="XRCommonHandGestures"/> data
        /// available this frame during capture for the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// Denotes which hand you wish to get tracking data for. Must be
        /// either <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// if <c>TryGetCommonGestures</c> to have a chance to succeed.
        /// </param>
        /// <param name="commonGestures">
        /// If <c>TryGetCommonGestures</c> returns <see langword="true"/>, this
        /// will be filled out with the data required to ask for corresponding
        /// <see cref="XRCommonHandGestures"/> data available this frame during capture
        /// for the given <see cref="Handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the <see cref="XRCommonHandGesturesState"/>
        /// was successfully retrieved and can be successfully queried for captured tracking data.
        /// </returns>
        internal bool TryGetCommonGestures(Handedness handedness, out XRCommonHandGesturesState commonGestures)
        {
            if (!TryGetFlatCommonGesturesIndex(handedness, out int flatCommonGesturesIndex))
            {
                commonGestures = default;
                return false;
            }

            commonGestures = m_ContainingAsset.flattenedCommonGestures[flatCommonGesturesIndex];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="XRHandAimState"/> data
        /// available this frame during capture for the given <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness">
        /// Denotes which hand you wish to get tracking data for. Must be
        /// either <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>
        /// if <c>TryGetAimState</c> to have a chance to succeed.
        /// </param>
        /// <param name="aimState">
        /// If <c>TryGetAimState</c> returns <see langword="true"/>, this
        /// will be filled out with the data required to ask for corresponding
        /// <see cref="XRHandAimState"/> data available this frame during capture
        /// for the given <see cref="Handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the <see cref="XRHandAimState"/>
        /// was successfully retrieved and can be successfully queried for captured tracking data.
        /// </returns>
        public bool TryGetAimState(Handedness handedness, out XRHandAimState aimState)
        {
            if (!TryGetFlatAimStatesIndex(handedness, out int flatAimStatesIndex))
            {
                aimState = default;
                return false;
            }

            aimState = m_ContainingAsset.flattenedAimStates[flatAimStatesIndex];
            return true;
        }

        /// <summary>
        /// The captured tracking data that this frame is a part of and comes from.
        /// </summary>
        public XRHandCaptureSequence containingAsset
        {
            get => m_ContainingAsset;
            internal set => m_ContainingAsset = value;
        }

        /// <summary>
        /// The index into the <see cref="containingAsset"/>'s
        /// <see cref="XRHandCaptureSequence"/><c>.</c><see cref="XRHandCaptureSequence.frames"/>
        /// that this <c>XRHandCaptureFrame</c> is a part of and comes from.
        /// </summary>
        public int frameIndexInContainingAsset => m_AlreadyFlatIndex;

        /// <summary>
        /// Attempt to get the pose of the center eye pose from the frame of
        /// capture. Useful for lining up the scene camera or offseting it from
        /// something related to where the HMD was positioned or oriented
        /// during capture.
        /// </summary>
        /// <param name="headPose">
        /// If <c>TryGetHeadPose</c> returns an <see cref="InputTrackingState"/>
        /// with <see cref="InputTrackingState.Position"/> set, <see cref="Pose.position"/>
        /// will be filled out with a valid center eye position from the time of capture.
        /// If <c>TryGetHeadPose</c> returns an <see cref="InputTrackingState"/>
        /// with <see cref="InputTrackingState.Rotation"/> set, <see cref="Pose.rotation"/>
        /// will be filled out with a valid rotation of the center eye rotation from the
        /// time of capture.
        /// </param>
        /// <returns>
        /// If valid, <see cref="Pose.position"/> and <see cref="Pose.rotation"/>
        /// may independently be from the <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// or <see cref="XRHandSubsystem.UpdateType.BeforeRender"/> step, depending on
        /// whether the data was available during the <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// step and if the <see cref="XRHandRecordingOptions"/><c>.</c><see cref="XRHandRecordingOptions.AlsoCaptureBeforeRender"/>
        /// option was enabled during capture.
        /// </returns>
        public InputTrackingState TryGetHeadPose(out Pose headPose)
        {
            headPose = m_HeadPose;
            return m_HeadPoseTrackingState;
        }

        // for reading in a binary file that was recorded on-device and importing/converting into an XRHandCaptureSequence
        internal XRHandCaptureFrame(XRHandCaptureSequence sequence, in FrameBuffer frameBuffer)
        {
            m_ContainingAsset = sequence;
            m_AlreadyFlatIndex = sequence.frames.Count;
            m_HandSnapshotsIndex = Constants.k_InvalidIndex;
            m_CommonGesturesIndex = Constants.k_InvalidIndex;
            m_AimStatesIndex = Constants.k_InvalidIndex;

            m_FrameFlags = frameBuffer.m_FrameFlags;
            m_Timestamp = frameBuffer.m_Timestamp;

            m_HeadPoseTrackingState = frameBuffer.m_HeadPoseTrackingState;
            m_HeadPose = frameBuffer.m_HeadPose;

            for (int updateSuccessFlagsIndex = 0; updateSuccessFlagsIndex < frameBuffer.m_NumReadUpdateSuccessFlags; ++updateSuccessFlagsIndex)
                sequence.flattenedUpdateSuccessFlags.Add(frameBuffer.m_UpdateSuccessFlags[updateSuccessFlagsIndex]);

            if (frameBuffer.m_NumReadSnapshotBuffers != 0)
            {
                int numSnapshotsCopied = 0;
                m_HandSnapshotsIndex = sequence.flattenedHandSnapshots.Count;
                foreach (var handedness in HandsUtility.validHandednessValues)
                {
                    if ((m_FrameFlags & handedness.ToFrameFlagForSnapshot()) == 0)
                        continue;

                    sequence.flattenedHandSnapshots.Add(
                        new XRHandCaptureSnapshot(m_ContainingAsset, frameBuffer.m_SnapshotBuffers[numSnapshotsCopied], handedness));
                    ++numSnapshotsCopied;
                }
            }

            if (frameBuffer.m_NumReadCommonGestures != 0)
            {
                m_CommonGesturesIndex = sequence.flattenedCommonGestures.Count;
                for (int commonGesturesIndex = 0; commonGesturesIndex < frameBuffer.m_NumReadCommonGestures; ++commonGesturesIndex)
                    sequence.flattenedCommonGestures.Add(frameBuffer.m_CommonGestures[commonGesturesIndex]);
            }

            if (frameBuffer.m_NumReadAimStates != 0)
            {
                m_AimStatesIndex = sequence.flattenedAimStates.Count;
                for (int aimStateIndex = 0; aimStateIndex < frameBuffer.m_NumReadAimStates; ++aimStateIndex)
                    sequence.flattenedAimStates.Add(frameBuffer.m_AimStates[aimStateIndex]);
            }

            m_LeftHandJoints = null;
            m_RightHandJoints = null;
            m_IsLeftHandTracked = false;
            m_IsRightHandTracked = false;
            m_AreAllLeftJointsValid = false;
            m_AreAllRightJointsValid = false;
        }

        // for updating a 1.7 version of XRHandCaptureSequence to newest (starting with 1.8)
        internal XRHandCaptureFrame(XRHandCaptureSequence containingAsset, XRHandCaptureFrame preFlatteningFrame, int frameIndex)
        {
            m_LeftHandJoints = preFlatteningFrame.m_LeftHandJoints;
            m_RightHandJoints = preFlatteningFrame.m_RightHandJoints;
            m_IsLeftHandTracked = preFlatteningFrame.m_IsLeftHandTracked;
            m_IsRightHandTracked = preFlatteningFrame.m_IsRightHandTracked;
            m_AreAllLeftJointsValid = preFlatteningFrame.m_AreAllLeftJointsValid;
            m_AreAllRightJointsValid = preFlatteningFrame.m_AreAllRightJointsValid;

            m_ContainingAsset = containingAsset;
            m_Timestamp = preFlatteningFrame.m_Timestamp;
            m_FrameFlags = FrameFlags.None;
            m_AlreadyFlatIndex = frameIndex;

            m_HandSnapshotsIndex = Constants.k_InvalidIndex;
            m_CommonGesturesIndex = Constants.k_InvalidIndex;
            m_AimStatesIndex = Constants.k_InvalidIndex;

            m_HeadPoseTrackingState = InputTrackingState.None;
            m_HeadPose = Pose.identity;

            foreach (var handedness in HandsUtility.validHandednessValues)
            {
                bool isTracked = preFlatteningFrame.IsInitialLayoutHandTracked(handedness);
                bool areAllJointsValid = preFlatteningFrame.AreAllInitialLayoutJointsValid(handedness);

                if (!isTracked && !areAllJointsValid)
                    continue;

                var snapshot = new XRHandCaptureSnapshot(
                    m_ContainingAsset, handedness,
                    isTracked, areAllJointsValid,
                    preFlatteningFrame.GetInitialLayoutHandJoints(handedness));

                if (m_HandSnapshotsIndex == Constants.k_InvalidIndex)
                    m_HandSnapshotsIndex = m_ContainingAsset.flattenedHandSnapshots.Count;

                m_ContainingAsset.flattenedHandSnapshots.Add(snapshot);
                m_FrameFlags |= handedness.ToFrameFlagForSnapshot();
            }
        }

        internal FrameFlags flags => m_FrameFlags;

        bool TryGetFlatUpdateSuccessFlagsIndex(XRHandSubsystem.UpdateType updateType, out int flatUpdateSuccessFlagsIndex)
        {
            if (!XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, m_ContainingAsset.optionsRecordedWith))
            {
                flatUpdateSuccessFlagsIndex = Constants.k_InvalidIndex;
                return false;
            }

            flatUpdateSuccessFlagsIndex = m_AlreadyFlatIndex;
            if ((m_ContainingAsset.optionsRecordedWith & XRHandRecordingOptions.AlsoCaptureBeforeRender) != 0)
            {
                flatUpdateSuccessFlagsIndex *= 2;
                if (updateType == XRHandSubsystem.UpdateType.BeforeRender)
                    ++flatUpdateSuccessFlagsIndex;
            }

            return true;
        }

        bool TryGetFlatHandSnapshotIndex(Handedness handedness, out int flatHandSnapshotIndex)
        {
            if (!handedness.IsValid() ||
                m_HandSnapshotsIndex == Constants.k_InvalidIndex || (m_FrameFlags & handedness.ToFrameFlagForSnapshot()) == 0)
            {
                flatHandSnapshotIndex = Constants.k_InvalidIndex;
                return false;
            }

            flatHandSnapshotIndex = m_HandSnapshotsIndex;
            if (handedness == Handedness.Right && (m_FrameFlags & Handedness.Left.ToFrameFlagForSnapshot()) != 0)
                ++flatHandSnapshotIndex;

            return true;
        }

        bool TryGetFlatCommonGesturesIndex(Handedness handedness, out int flatCommonGesturesIndex)
        {
            if (!handedness.IsValid() || m_CommonGesturesIndex == Constants.k_InvalidIndex ||
                (m_FrameFlags & handedness.ToFrameFlagForCommonGestures()) == 0)
            {
                flatCommonGesturesIndex = Constants.k_InvalidIndex;
                return false;
            }

            flatCommonGesturesIndex = m_CommonGesturesIndex;
            if (handedness == Handedness.Right && (m_FrameFlags & Handedness.Left.ToFrameFlagForCommonGestures()) != 0)
                ++flatCommonGesturesIndex;

            return true;
        }

        bool TryGetFlatAimStatesIndex(Handedness handedness, out int flatAimStatesIndex)
        {
            if (!handedness.IsValid() ||
                m_AimStatesIndex == Constants.k_InvalidIndex ||
                (m_FrameFlags & handedness.ToFrameFlagForAimState()) == 0)
            {
                flatAimStatesIndex = Constants.k_InvalidIndex;
                return false;
            }

            flatAimStatesIndex = m_AimStatesIndex;
            if (handedness == Handedness.Right && (m_FrameFlags & Handedness.Left.ToFrameFlagForAimState()) != 0)
                ++flatAimStatesIndex;

            return true;
        }

        bool IsInitialLayoutHandTracked(Handedness handedness)
            => handedness == Handedness.Left ? m_IsLeftHandTracked : m_IsRightHandTracked;

        bool AreAllInitialLayoutJointsValid(Handedness handedness)
            => handedness == Handedness.Left ? m_AreAllLeftJointsValid : m_AreAllRightJointsValid;

        XRHandJoint[] GetInitialLayoutHandJoints(Handedness handedness)
            => handedness == Handedness.Left ? m_LeftHandJoints : m_RightHandJoints;

        [SerializeReference]
        XRHandCaptureSequence m_ContainingAsset;

        [SerializeField]
        FrameFlags m_FrameFlags;

        [SerializeField]
        float m_Timestamp;

        [SerializeField]
        int m_AlreadyFlatIndex;

        [SerializeField]
        int m_HandSnapshotsIndex;

        [SerializeField]
        int m_CommonGesturesIndex;

        [SerializeField]
        int m_AimStatesIndex;

        [SerializeField]
        InputTrackingState m_HeadPoseTrackingState;

        [SerializeField]
        Pose m_HeadPose;

        // deprecated fields below this line (should we rename these to make that more obvious using FormerlySerializedAs?)

        [SerializeField]
        XRHandJoint[] m_LeftHandJoints;

        [SerializeField]
        XRHandJoint[] m_RightHandJoints;

        [SerializeField]
        bool m_IsLeftHandTracked;

        [SerializeField]
        bool m_IsRightHandTracked;

        [SerializeField]
        bool m_AreAllLeftJointsValid;

        [SerializeField]
        bool m_AreAllRightJointsValid;
    }
}
