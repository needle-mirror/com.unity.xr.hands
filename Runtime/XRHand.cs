using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Represents a hand from an <see cref="XRHandSubsystem"/>. Do not create
    /// this yourself - only use a hand from <see cref="XRHandSubsystem.leftHand"/>
    /// or <see cref="XRHandSubsystem.rightHand"/>.
    /// </summary>
    public struct XRHand
    {
        /// <summary>
        /// Retrieves an <see cref="XRHandJoint"/> by its ID.
        /// </summary>
        /// <param name="id">ID of the required joint.</param>
        /// <returns>The <see cref="XRHandJoint"/> corresponding the ID passed in.</returns>
        public XRHandJoint GetJoint(XRHandJointID id) => m_Joints[id.ToIndex()];
        internal NativeArray<XRHandJoint> m_Joints;

        /// <summary>
        /// Root pose for the hand. All joint data is relative to this parent pose.
        /// </summary>
        public Pose rootPose => m_RootPose;
        internal Pose m_RootPose;

        /// <summary>
        /// Represents which hand this is.
        /// </summary>
        public Handedness handedness => m_Handedness;
        Handedness m_Handedness;

        /// <summary>
        /// Whether the subsystem is currently tracking this hand's root pose and joints.
        /// </summary>
        public bool isTracked { get; internal set; }

        /// <summary>
        /// Returns a string representation of the XRHand.
        /// </summary>
        /// <returns>
        /// String representation of the value.
        /// </returns>
        public override string ToString()
        {
            return m_Handedness + " XRHand";
        }

        internal XRHand(Handedness handedness, Allocator allocator)
        {
            m_RootPose = Pose.identity;
            m_Handedness = handedness;
            m_Joints = new NativeArray<XRHandJoint>(XRHandJointID.EndMarker.ToIndex(), allocator);
            isTracked = false;
        }

        internal void Dispose()
        {
            if (m_Joints.IsCreated)
                m_Joints.Dispose();
        }
    }
}
