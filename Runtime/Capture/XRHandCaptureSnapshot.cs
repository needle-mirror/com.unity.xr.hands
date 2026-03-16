using System;
using Unity.Collections;
using UnityEngine.XR.Hands.Capture.Recording;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// Per-<see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.UpdateType"/>
    /// data for a given frame (from <see cref="XRHandCaptureFrame"/>) for a given
    /// <see cref="Handedness"/>.
    /// </summary>
    [Serializable]
    struct XRHandCaptureSnapshot
    {
        /// <summary>
        /// Denotes which hand this captured tracking data is associated with.
        /// </summary>
        /// <value>
        /// If this is a valid object successfully retrieved from <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetHandSnapshot"/>,
        /// this will only ever be <see cref="Handedness.Left"/> or <see cref="Handedness.Right"/>.
        /// </value>
        public Handedness handedness => m_Handedness;

        /// <summary>
        /// Checks whether the corresponding <see cref="XRHand"/><c>.</c><see cref="XRHand.isTracked"/>
        /// reported as <see langword="true"/> when the data was captured.
        /// </summary>
        /// <param name="updateType">
        /// The update type passed to <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.TryUpdateHands"/>
        /// you wish to query for captured per-<see cref="XRHandSubsystem.UpdateType"/> tracking data.
        /// If a value of <see cref="XRHandSubsystem.UpdateType.BeforeRender"/> is supplied
        /// but <see cref="sourceCaptureSequence"/>'s <see cref="XRHandCaptureSequence.optionsRecordedWith"/>
        /// does not have <see cref="XRHandRecordingOptions"/><c>.</c><see cref="XRHandRecordingOptions.AlsoCaptureBeforeRender"/>
        /// enabled, <c>IsHandTracked</c> will always fail.
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
        public bool IsHandTracked(XRHandSubsystem.UpdateType updateType = XRHandSubsystem.UpdateType.Dynamic)
        {
            if (!TryGetFlatHandFlagsIndex(updateType, out int flatHandFlagsIndex))
                return false;

            return (m_ContainingAsset.flattenedHandFlags[flatHandFlagsIndex] & HandFlags.WasHandTrackedDuringCapture) != 0;
        }

        /// <summary>
        /// Reconstructs the <see cref="XRHand"/> from this <see cref="XRHandCaptureFrame"/>,
        /// with <see cref="Handedness"/> of <see cref="handedness"/>, for the
        /// <see cref="XRHandSubsystem.UpdateType.Dynamic"/> step of
        /// <see cref="XRHandSubsystem.TryUpdateHands"/> during capture.
        /// </summary>
        /// <param name="allocator">
        /// The <see cref="Allocator"/> to create the <see cref="NativeArray"/>
        /// of <see cref="XRHandJoint"/>s with in the requested <see cref="XRHand"/>.
        /// </param>
        /// <param name="hand">
        /// If <c>TryGetHand</c> returns <see langword="true"/>, this will be filled out with
        /// the requested <see cref="XRHand"/> data for the <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// step of this frame of the capture, for the hand denoted by <see cref="handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the <see cref="XRHand"/> is successfully
        /// filled out with valid captured tracking data. If this returns <see langword="false"/>,
        /// you should assume that the corresponding <see cref="XRHand"/> during capture did not
        /// have any valid tracking data at the time.
        /// </returns>
        /// <remarks>
        /// <para><see cref="XRHand"/>s successfully retrieved this way require you to call
        /// <c>XRHand.</c><see cref="XRHand.Dispose"/> when you are done accessing its data,
        /// or you will leak memory.</para>
        /// <para>This overload of <c>TryGetHand</c> is just a shortcut for
        /// calling the other one with <see cref="XRHandSubsytem.UpdateType.Dynamic"/>
        /// for the <see cref="XRHandSubsystem.Update"/>.</para>
        /// </remarks>
        public bool TryGetHand(Allocator allocator, out XRHand hand)
            => TryGetHand(XRHandSubsystem.UpdateType.Dynamic, allocator, out hand);

        /// <summary>
        /// Reconstructs the <see cref="XRHand"/> from this <see cref="XRHandCaptureFrame"/>,
        /// with <see cref="Handedness"/> of <see cref="handedness"/>, for the
        /// given <see cref="XRHandSubsystem.UpdateType"/> step of
        /// <see cref="XRHandSubsystem.TryUpdateHands"/> during capture.
        /// </summary>
        /// <param name="updateType">
        /// The update type passed to <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.TryUpdateHands"/>
        /// you wish to query for <see cref="XRHand"/> data for the given <see cref="XRHandSubsystem.UpdateType"/>
        /// and this <c>XRHandCaptureSnapshot</c>'s <see cref="handedness"/>.
        /// If a value of <see cref="XRHandSubsystem.UpdateType.BeforeRender"/> is supplied
        /// but <see cref="sourceCaptureSequence"/>'s <see cref="XRHandCaptureSequence.optionsRecordedWith"/>
        /// does not have <see cref="XRHandRecordingOptions"/><c>.</c><see cref="XRHandRecordingOptions.AlsoCaptureBeforeRender"/>
        /// enabled, <c>TryGetHand</c> will always fail.
        /// </param>
        /// <param name="allocator">
        /// The <see cref="Allocator"/> to create the <see cref="NativeArray"/>
        /// of <see cref="XRHandJoint"/>s with in the requested <see cref="XRHand"/>.
        /// </param>
        /// <param name="hand">
        /// If <c>TryGetHand</c> returns <see langword="true"/>, this will be filled out with
        /// the requested <see cref="XRHand"/> data for the <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// step of this frame of the capture, for the hand denoted by <see cref="handedness"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the <see cref="XRHand"/> is successfully
        /// filled out with valid captured tracking data. If this returns <see langword="false"/>,
        /// you should assume that the corresponding <see cref="XRHand"/> during capture did not
        /// have any valid tracking data at the time.
        /// </returns>
        /// <remarks>
        /// <para><see cref="XRHand"/>s successfully retrieved this way require you to call
        /// <c>XRHand.</c><see cref="XRHand.Dispose"/> when you are done accessing its data,
        /// or you will leak memory.</para>
        /// </remarks>
        public bool TryGetHand(
            XRHandSubsystem.UpdateType updateType, Allocator allocator, out XRHand hand)
        {
            if (!m_Handedness.IsValid() ||
                !TryGetFlatData(updateType, out bool isTracked, out var rootPose, out int flatHandJointsIndex))
            {
                hand = default;
                return false;
            }

            // non-capture/-playback XRHands, if valid at all, always have a full hand joint
            // NativeArray - and GetRawJointArray exists, so we need to keep that assumption in place
            hand = new XRHand(m_Handedness, allocator, isTracked, rootPose);

            if (flatHandJointsIndex != Constants.k_InvalidIndex)
            {
                for (int jointIndex = 0; jointIndex < hand.m_Joints.Length; ++jointIndex)
                    hand.m_Joints[jointIndex] = m_ContainingAsset.flattenedHandJoints[flatHandJointsIndex + jointIndex];
            }
            else
            {
                using (var handJointsInLayout = m_ContainingAsset.GetHandJointsInLayout(Allocator.Temp))
                {
                    foreach (var jointID in HandsUtility.validJointIDs)
                    {
                        int jointIndex = jointID.ToIndex();
                        var jointTrackingState = handJointsInLayout[jointIndex] ? XRHandJointTrackingState.None : XRHandJointTrackingState.WillNeverBeValid;
                        hand.m_Joints[jointIndex] = XRHandProviderUtility.CreateJoint(m_Handedness, jointTrackingState, jointID, Pose.identity);
                    }
                }
            }

            return true;
        }

        // for reading in a binary file that was recorded on-device and importing/converting into an XRHandCaptureSequence
        internal XRHandCaptureSnapshot(XRHandCaptureSequence containingAsset, in HandSnapshotBuffer snapshotBuffer, Handedness handedness)
        {
            m_ContainingAsset = containingAsset;

            m_Handedness = handedness;
            m_SnapshotFlags = snapshotBuffer.m_SnapshotFlags;

            m_HandFlagsIndex = Constants.k_InvalidIndex;
            m_HandJointsIndex = Constants.k_InvalidIndex;
            m_RootPoseIndex = Constants.k_InvalidIndex;

            foreach (var updateType in HandsUtility.validUpdateTypes)
            {
                if (!XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, m_ContainingAsset.optionsRecordedWith))
                    continue;

                var handBuffer = snapshotBuffer.m_HandBuffers[updateType.ToIndex()];

                EnsureRootPoseIndexValid();
                m_ContainingAsset.flattenedHandRootPoses.Add(handBuffer.m_RootPose);

                EnsureHandFlagsIndexValid();
                m_ContainingAsset.flattenedHandFlags.Add(handBuffer.m_HandFlags);

                EnsureHandJointsIndexValid();
                foreach (var jointID in HandsUtility.validJointIDs)
                {
                    var joint = XRHandProviderUtility.CreateJoint(
                        handedness,
                        XRHandJointTrackingState.Pose,
                        jointID,
                        handBuffer.m_JointPoses[jointID.ToIndex()]);
                    m_ContainingAsset.flattenedHandJoints.Add(joint);
                }
            }
        }

        // for updating a 1.7 version of XRHandCaptureSequence to newest (starting with 1.8)
        internal XRHandCaptureSnapshot(
            XRHandCaptureSequence containingAsset, Handedness handedness,
            bool isHandTracked, bool areAllJointsValid, XRHandJoint[] joints)
        {
            m_ContainingAsset = containingAsset;
            m_Handedness = handedness;
            m_SnapshotFlags = SnapshotFlags.IsDynamicHandValid;

            var handFlags = HandFlags.None;
            if (isHandTracked)
                handFlags |= HandFlags.WasHandTrackedDuringCapture;

            var rootPose = Pose.identity;
            if (areAllJointsValid && joints != null && joints.Length >= Constants.k_NumJointsPerHand)
            {
                handFlags |= HandFlags.AreAllJointPosesValid;
                if (joints[XRHandJointID.Wrist.ToIndex()].TryGetPose(out var wristPose))
                    rootPose = wristPose;

                m_HandJointsIndex = m_ContainingAsset.flattenedHandJoints.Count;
                foreach (var joint in joints)
                    m_ContainingAsset.flattenedHandJoints.Add(joint);
            }
            else
            {
                m_HandJointsIndex = Constants.k_InvalidIndex;
            }

            m_HandFlagsIndex = m_ContainingAsset.flattenedHandFlags.Count;
            m_ContainingAsset.flattenedHandFlags.Add(handFlags);

            m_RootPoseIndex = m_ContainingAsset.flattenedHandRootPoses.Count;
            m_ContainingAsset.flattenedHandRootPoses.Add(rootPose);
        }

        internal void EnsureHandFlagsIndexValid()
        {
            if (m_ContainingAsset != null && m_HandFlagsIndex == Constants.k_InvalidIndex)
                m_HandFlagsIndex = m_ContainingAsset.flattenedHandFlags.Count;
        }

        internal void EnsureHandJointsIndexValid()
        {
            if (m_ContainingAsset != null && m_HandJointsIndex == Constants.k_InvalidIndex)
                m_HandJointsIndex = m_ContainingAsset.flattenedHandJoints.Count;
        }

        internal void EnsureRootPoseIndexValid()
        {
            if (m_ContainingAsset != null && m_RootPoseIndex == Constants.k_InvalidIndex)
                m_RootPoseIndex = m_ContainingAsset.flattenedHandRootPoses.Count;
        }

        bool TryGetFlatHandFlagsIndex(XRHandSubsystem.UpdateType updateType, out int flatHandFlagsIndex)
        {
            if (!m_Handedness.IsValid() ||
                m_HandFlagsIndex == Constants.k_InvalidIndex || (m_SnapshotFlags & updateType.ToSnapshotFlag()) == 0)
            {
                flatHandFlagsIndex = Constants.k_InvalidIndex;
                return false;
            }

            flatHandFlagsIndex = m_HandFlagsIndex;

            if (updateType == XRHandSubsystem.UpdateType.BeforeRender && ((m_SnapshotFlags & SnapshotFlags.IsDynamicHandValid) != 0))
                ++flatHandFlagsIndex;

            return true;
        }

        bool TryGetFlatData(XRHandSubsystem.UpdateType updateType, out bool isTracked, out Pose rootPose, out int flatHandJointsIndex)
        {
            if (!TryGetFlatHandFlagsIndex(updateType, out int flatHandFlagsIndex))
            {
                flatHandJointsIndex = Constants.k_InvalidIndex;
                rootPose = Pose.identity;
                isTracked = false;
                return false;
            }

            var handFlags = m_ContainingAsset.flattenedHandFlags[flatHandFlagsIndex];
            isTracked = (handFlags & HandFlags.WasHandTrackedDuringCapture) != 0;

            if (m_HandJointsIndex != Constants.k_InvalidIndex && (handFlags & HandFlags.AreAllJointPosesValid) != 0)
            {
                flatHandJointsIndex = m_HandJointsIndex;

                if (updateType == XRHandSubsystem.UpdateType.BeforeRender && ((m_SnapshotFlags & SnapshotFlags.IsDynamicHandValid) != 0))
                    flatHandJointsIndex += Constants.k_NumJointsPerHand;
            }
            else
            {
                flatHandJointsIndex = Constants.k_InvalidIndex;
            }

            rootPose = m_RootPoseIndex != Constants.k_InvalidIndex
                ? m_ContainingAsset.flattenedHandRootPoses[m_RootPoseIndex]
                : Pose.identity;
            return true;
        }

        [SerializeReference]
        XRHandCaptureSequence m_ContainingAsset;

        [SerializeField]
        SnapshotFlags m_SnapshotFlags;

        [SerializeField]
        Handedness m_Handedness;

        [SerializeField]
        int m_HandFlagsIndex;

        [SerializeField]
        int m_HandJointsIndex;

        [SerializeField]
        int m_RootPoseIndex;
    }
}
