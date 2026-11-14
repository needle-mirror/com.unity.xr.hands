using System;

namespace UnityEngine.XR.Hands
{
    public partial class MetaAimHand
    {
        /// <summary>
        /// (Obsolete) Queues update events in the Input System based on the supplied hand.
        /// Use the overload with a first <c>isTracked</c> parameter instead.
        /// It is not recommended that you call this directly. This will be called
        /// for you when appropriate.
        /// </summary>
        /// <param name="aimFlags">
        /// The aim flags to update in the Input System.
        /// </param>
        /// <param name="aimPose">
        /// The aim pose to update in the Input System.
        /// </param>
        /// <param name="pinchIndex">
        /// The pinch strength for the index finger to update in the Input System.
        /// </param>
        /// <param name="pinchMiddle">
        /// The pinch strength for the middle finger to update in the Input System.
        /// </param>
        /// <param name="pinchRing">
        /// The pinch strength for the ring finger to update in the Input System.
        /// </param>
        /// <param name="pinchLittle">
        /// The pinch strength for the little finger to update in the Input System.
        /// </param>
        [Obsolete("This UpdateHand overload is obsolete. Use the overload with a first isTracked parameter instead.", true)]
        public void UpdateHand(
            MetaAimFlags aimFlags,
            Pose aimPose,
            float pinchIndex,
            float pinchMiddle,
            float pinchRing,
            float pinchLittle)
        {
            throw new NotSupportedException("UpdateHand without the isTracked parameter is no longer supported. Use the UpdateHand overload that has an additional first bool parameter for whether the hand is tracked.");
        }
    }
}
