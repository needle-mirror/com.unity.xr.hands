using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    struct HandSnapshotBuffer : IDisposable
    {
        public void Dispose()
        {
            foreach (var handBuffer in m_HandBuffers)
                handBuffer.Dispose();

            m_HandBuffers.Dispose();
        }

        internal HandSnapshotBuffer(XRHandRecordingOptions recordingOptions)
        {
            m_SnapshotFlags = SnapshotFlags.None;
            m_HandBuffers = new NativeArray<HandBuffer>(Constants.k_NumUpdateTypes, Allocator.Temp);
            foreach (var updateType in HandsUtility.validUpdateTypes)
                m_HandBuffers[updateType.ToIndex()] = new HandBuffer(Handedness.Invalid);
        }

        internal SnapshotFlags m_SnapshotFlags;
        internal NativeArray<HandBuffer> m_HandBuffers;
    }
}
