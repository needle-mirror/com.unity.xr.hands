using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace UnityEngine.XR.Hands.Samples.Capture
{
#if UNITY_6000_5_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public class RecorderUserJourneyManager
    {
        public enum UserJourneyState
        {
            Instruction,
            ReadyToRecord,
            Recording,
            RecordingStopped,
            RecordingSaved
        }

        static readonly HashSet<UserJourneyState> ValidJourneyStates = new()
        {
            UserJourneyState.Instruction,
            UserJourneyState.ReadyToRecord,
            UserJourneyState.Recording,
            UserJourneyState.RecordingStopped,
            UserJourneyState.RecordingSaved
        };

        public struct JourneyStateChangeEventArgs
        {
            public UserJourneyState previousState { get; }
            public UserJourneyState currentState { get; }

            public JourneyStateChangeEventArgs(UserJourneyState previousState, UserJourneyState currentState)
            {
                this.previousState = previousState;
                this.currentState = currentState;
            }
        }

        public Action<JourneyStateChangeEventArgs> onJourneyStateChanged;

        UserJourneyState m_CurrentState = UserJourneyState.Instruction;

        public void SetUserJourneyState(UserJourneyState state)
        {
            if (!ValidJourneyStates.Contains(state))
                throw new ArgumentOutOfRangeException(nameof(state), $"Invalid state value: {state}");

            JourneyStateChangeEventArgs args = new JourneyStateChangeEventArgs(m_CurrentState, state);

            m_CurrentState = state;
            onJourneyStateChanged?.Invoke(args);
        }

        public void SetUserJourneyState(int state) => SetUserJourneyState((UserJourneyState)state);
    }
}
