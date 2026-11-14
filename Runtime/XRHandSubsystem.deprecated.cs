using System;

namespace UnityEngine.XR.Hands
{
    public partial class XRHandSubsystem
    {
        /// <summary>
        /// (Obsolete) No longer invoked. Use <see cref="updatedHands"/> instead.
        /// </summary>
        [Obsolete("handsUpdated is obsolete and is no longer invoked. Use updatedHands instead.", true)]
        public Action<UpdateSuccessFlags, UpdateType> handsUpdated;
    }
}
