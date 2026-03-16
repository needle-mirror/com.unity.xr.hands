using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    struct HandBuffer : IDisposable
    {
        public void Dispose()
        {
            if (m_JointPoses.IsCreated)
                m_JointPoses.Dispose();
        }

        internal HandBuffer(Handedness handedness)
        {
            m_HandFlags = HandFlags.None;
            m_JointPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Temp);
            m_RootPose = Pose.identity;
            m_Handedness = handedness;
        }

        internal HandFlags m_HandFlags;
        internal NativeArray<Pose> m_JointPoses;
        internal Pose m_RootPose;

        // not written to file/stream, but is switched off of
        internal readonly Handedness m_Handedness;
    }
}
