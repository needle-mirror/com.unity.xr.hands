using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// A subystem for detecting and tracking hands and their corresponding
    /// joint pose data.
    /// </summary>
    public partial class XRHandSubsystem
        : SubsystemWithProvider<XRHandSubsystem, XRHandSubsystemDescriptor, XRHandSubsystemProvider>
    {
        /// <summary>
        /// Constructs a subsystem. Do not invoke directly; call <c>Create</c>
        /// on the <see cref="XRHandSubsystemDescriptor"/> instead.
        /// </summary>
        public XRHandSubsystem()
        {
        }

        /// <summary>
        /// Gets the left <see cref="XRHand"/> that is being tracked by this
        /// subsystem.
        /// </summary>
        public XRHand leftHand => m_LeftHand;
        XRHand m_LeftHand;

        /// <summary>
        /// Gets the right <see cref="XRHand"/> that is being tracked by this
        /// subsystem.
        /// </summary>
        public XRHand rightHand => m_RightHand;
        XRHand m_RightHand;

        /// <summary>
        /// Gets a layout array that is marked true for each joint that will
        /// ever by tracked by the provider attached to this subsystem. To
        /// retrieve the matching index, call <c>.ToIndex()</c> on the
        /// <see cref="XRHandJointID"/> in question.
        /// </summary>
        /// <remarks>
        /// This array will already be valid as soon as you have a reference to
        /// a subsystem (in other words, it's filled out before the subsystem is
        /// returned by a call to <c>XRHandSubsystemDescriptor.Create</c>).
        /// </remarks>
        public NativeArray<bool> jointsInLayout => m_JointsInLayout;
        NativeArray<bool> m_JointsInLayout;

        /// <summary>
        /// Return type for <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// Describes what data on either hand was updated during the call.
        /// </summary>
        [Flags]
        public enum UpdateSuccessFlags
        {
            /// <summary>
            /// No data for either hand was successfully updated.
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
        /// Update type for <see cref="XRHandSubsystem.TryUpdateHands"/>.
        /// </summary>
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
        /// A callback for when hands a call to <see cref="TryUpdateHands"/> completes.
        /// Use this if you don't own the subsystem, but want to be made aware of changes,
        /// such as if you are driving visuals.
        /// </summary>
        public Action<XRHandSubsystem, UpdateSuccessFlags, UpdateType> updatedHands;

        /// <summary>
        /// A callback for when the subsystem begins tracking this hand's root pose and joints.
        /// </summary>
        /// <remarks>
        /// This is called before <see cref="updatedHands"/>.
        /// </remarks>
        public Action<XRHand> trackingAcquired;

        /// <summary>
        /// A callback for when the subsystem stops tracking this hand's root pose and joints.
        /// </summary>
        /// <remarks>
        /// This is called before <see cref="updatedHands"/>.
        /// </remarks>
        public Action<XRHand> trackingLost;

        /// <summary>
        /// Call this to have the subsystem retrieve changes from the provider.
        /// Changes will be reflected in <see cref="leftHand"/> and <see cref="rightHand"/>.
        /// </summary>
        /// <param name="updateType">
        /// Informs the provider which kind of timing the update is being
        /// requested under.
        /// </param>
        /// <returns>
        /// Returns <see cref="UpdateSuccessFlags"/> to describe what data was updated successfully.
        /// </returns>
        /// <remarks>
        /// If overriding this method in a derived type, it is expected that you
        /// call <c>base.TryUpdateHands(updateType)</c> and return what it
        /// returns.
        /// </remarks>
        public virtual unsafe UpdateSuccessFlags TryUpdateHands(UpdateType updateType)
        {
            if (!running)
                return UpdateSuccessFlags.None;

            var flags = provider.TryUpdateHands(
                updateType,
                ref m_LeftHand.m_RootPose,
                m_LeftHand.m_Joints,
                ref m_RightHand.m_RootPose,
                m_RightHand.m_Joints);

            var wasLeftHandTracked = m_LeftHand.isTracked;
            var success = UpdateSuccessFlags.LeftHandRootPose | UpdateSuccessFlags.LeftHandJoints;
            m_LeftHand.isTracked = (flags & success) == success;
            if (!wasLeftHandTracked && m_LeftHand.isTracked)
                trackingAcquired?.Invoke(m_LeftHand);
            else if (wasLeftHandTracked && !m_LeftHand.isTracked)
                trackingLost?.Invoke(m_LeftHand);

            var wasRightHandTracked = m_RightHand.isTracked;
            success = UpdateSuccessFlags.RightHandRootPose | UpdateSuccessFlags.RightHandJoints;
            m_RightHand.isTracked = (flags & success) == success;
            if (!wasRightHandTracked && m_RightHand.isTracked)
                trackingAcquired?.Invoke(m_RightHand);
            else if (wasRightHandTracked && !m_RightHand.isTracked)
                trackingLost?.Invoke(m_RightHand);

            if (updatedHands != null)
                updatedHands.Invoke(this, flags, updateType);

#pragma warning disable 618
            if (handsUpdated != null)
                handsUpdated.Invoke(flags, updateType);
#pragma warning restore 618

            return flags;
        }

        /// <summary>
        /// Called by Unity before the subsystem is returned from a call to <c>XRHandSubsystemDescriptor.Create</c>.
        /// </summary>
        protected override void OnCreate()
        {
            m_JointsInLayout = new NativeArray<bool>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);
            provider.GetHandLayout(m_JointsInLayout);

            m_LeftHand = new XRHand(Handedness.Left, Allocator.Persistent);
            m_RightHand = new XRHand(Handedness.Right, Allocator.Persistent);

            for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            {
                if (!m_JointsInLayout[jointIndex])
                {
                    var joint = XRHandJoint.willNeverBeValid;
                    joint.m_Id = XRHandJointIDUtility.FromIndex(jointIndex);
                    m_LeftHand.m_Joints[jointIndex] = joint;
                    m_RightHand.m_Joints[jointIndex] = joint;
                }
            }
        }

        /// <summary>
        /// Called by Unity before the subsystem is fully destroyed during a call to <c>XRHandSubsystem.Destroy</c>.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_LeftHand.Dispose();
            m_RightHand.Dispose();
            m_JointsInLayout.Dispose();
        }
    }
}
