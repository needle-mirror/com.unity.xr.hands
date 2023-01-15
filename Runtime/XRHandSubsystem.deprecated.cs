using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// A subystem for detecting and tracking hands and their corresponding
    /// joint pose data.
    /// </summary>
    public partial class XRHandSubsystem
    {
        /// <summary>
        /// A callback for when hands a call to <see cref="TryUpdateHands"/> completes.
        /// Use this if you don't own the subsystem, but want to be made aware of changes,
        /// such as if you are driving visuals.
        /// </summary>
        [Obsolete("Use updatedHands instead.")]
        public Action<UpdateSuccessFlags, UpdateType> handsUpdated;
    }
}
