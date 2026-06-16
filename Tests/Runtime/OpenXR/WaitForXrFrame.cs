#if UNITY_OPENXR_PACKAGE
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.Features.Mock;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    /// <summary>
    /// Custom yield instruction that waits for xrEndFrame to be called within OpenXR.
    /// Copied from xr.sdk.openxr/Tests/Runtime/WaitForXrFrame.cs.
    /// </summary>
    internal class WaitForXrFrame : CustomYieldInstruction
    {
        int m_Frames;
        long m_Timeout;
        Stopwatch m_Timer;
        bool m_Subscribed;

        public override bool keepWaiting
        {
            get
            {
                if (m_Frames <= 0)
                    return false;

                if (!m_Subscribed)
                {
                    m_Subscribed = true;
                    MockRuntime.onScriptEvent += OnScriptEvent;
                    m_Timer = new Stopwatch();
                    m_Timer.Restart();
                }

                if (m_Timer.ElapsedMilliseconds < m_Timeout)
                    return true;

                MockRuntime.onScriptEvent -= OnScriptEvent;
                Assert.Fail("WaitForXrFrame: Timeout");
                return false;
            }
        }

        public WaitForXrFrame(int frames = 1, float timeout = 10.0f)
        {
            m_Frames = frames;
            m_Timeout = (long)(timeout * 1000.0);
        }

        void OnScriptEvent(MockRuntime.ScriptEvent evt, ulong param)
        {
            if (evt != MockRuntime.ScriptEvent.EndFrame)
                return;

            m_Frames--;
            if (m_Frames > 0)
                return;

            m_Frames = 0;
            MockRuntime.onScriptEvent -= OnScriptEvent;
        }
    }
}
#endif
