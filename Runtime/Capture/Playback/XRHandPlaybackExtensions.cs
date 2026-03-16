using UnityEngine.SubsystemsImplementation.Extensions;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Houses extension methods for <see cref="XRHandSubsystem"/> for playback
    /// of <see cref="XRHandCaptureSequence"/>.
    /// </summary>
    static class XRHandPlaybackExtensions
    {
        /// <summary>
        /// Retrieve playback controls for an <see cref="XRHandSubsystem"/> through
        /// <see cref="XRHandPlayback"/> by calling <c>XRHandSubsystem.GetPlayback</c>.
        /// </summary>
        /// <param name="subsystem">
        /// The subsystem to get the associated <see cref="XRHandPlayback"/>
        /// for.
        /// </param>
        /// <param name="handedness">
        /// The handedness associated with the instance of <see cref="XRHandPlayback"/>.
        /// to be returned.
        /// </param>
        /// <returns>
        /// The playback controls for the associated <see cref="XRHandSubsystem"/>.
        /// </returns>
        /// <remarks>
        /// There is only ever one <c>XRHandPlayback</c> for any given
        /// <see cref="XRHandSubsystem"/>.
        /// </remarks>
        public static XRHandPlayback GetPlayback(
            this XRHandSubsystem subsystem, Handedness handedness)
        {
            var userFacingPlayback = subsystem?.GetProvider() is PlaybackProvider provider
                ? provider.GetUserFacingPlayback(handedness) : null;
            return userFacingPlayback;
        }

        /// <summary>
        /// Checks whether this subsystem was created by calling
        /// <see cref="XRHandSubsystemDescriptor"/><c>.</c><see cref="XRHandSubsystemDescriptor.CreatePlaybackOnly"/>
        /// </summary>
        /// <param name="subsystem">
        /// The subsystem to get the associated <see cref="XRHandPlayback"/>
        /// for.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the given <see cref="XRHandSubsystem"/>
        /// was created only for playback, <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsOnlyForPlayback(this XRHandSubsystem subsystem)
            => (subsystem?.subsystemDescriptor ?? null) == PlaybackProvider.GetRegisteredDescriptor();
    }
}
