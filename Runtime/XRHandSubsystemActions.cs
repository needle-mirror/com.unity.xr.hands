using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Actions that can be performed by the XRHandSubsystem provider that providers can subscribe to.
    /// </summary>
    struct XRHandSubsystemActions
    {
        /// <summary>
        /// An action that is fired before the <see cref="XRHandSubsystem"/> calls <see cref="XRHandSubsystemProvider.TryUpdateHands"/>.
        /// </summary>
        public Action<XRHandSubsystem.UpdateType> beginTryUpdateHands;
    }
}
