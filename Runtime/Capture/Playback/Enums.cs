using System;

namespace UnityEngine.XR.Hands.Capture.Playback
{
    /// <summary>
    /// Options for how to create playback-only <see cref="XRHandSubsystem"/>s
    /// with <c>XRHandSubsystemDescriptor</c>'s <see cref="XRHandSubsystemDescriptor.CreatePlaybackOnly"/>.
    /// </summary>
    [Flags]
    enum XRHandPlaybackOnlySubsystemCreationOptions
    {
        /// <summary>
        /// Don't do anything extra, just call <see cref="XRHandSubsystemDescriptor.Create"/>
        /// on the relevant descriptor and return that.
        /// </summary>
        None = 0,

        /// <summary>
        /// Enabling this option results in a <see cref="XRHandProviderUtility.SubsystemUpdater"/>
        /// being created and managed for you so that the <c>XRHandSubsystem</c>'s
        /// <see cref="TryUpdateHands"/> is called automatically each frame.
        /// </summary>
        AutomaticallyManageUpdater = 1 << 0,
    }

    /// <summary>
    /// Options for how the playback of <see cref="XRHandCaptureSequence"/> data should behave
    /// through <see cref="XRHandPlayback"/>.
    /// </summary>
    [Flags]
    enum XRHandPlaybackOptions
    {
        /// <summary>
        /// Only default behavior, which entails time-based playback progression,
        /// stopping playback at the end of a <see cref="XRHandCaptureSequence"/>,
        /// and going back to surfacing data from a non-playback
        /// <see cref="XRHandSubsystemProvider"/> when playback is stopped.
        /// </summary>
        None = 0,

        /// <summary>
        /// If this is enabled, the default behavior of time-based playback
        /// will change to matching each frame, regardless of how much time passed.
        /// </summary>
        /// <remarks>
        /// While less useful for interaction, the deterministic nature of
        /// frame-based playback progression can be useful for other things,
        /// such as automated tests. If FPS is high, this can cause playback
        /// to appear faster than recording.
        /// </remarks>
        ProgressPlaybackBasedOnFrames = 1 << 0,

        /// <summary>
        /// If this is enabled, the default behavior of stopping playback
        /// automatically will change to automatically loop back to the start instead
        /// when there is no <see cref="XRHandCaptureSequence"/> data left to
        /// progress through during playback.
        /// </summary>
        LoopCaptureWhenPlaybackEnds = 1 << 1,

        /// <summary>
        /// If this is enabled, the default behavior of going back to the
        /// non-playback provider if available will be overridden to always
        /// report hand data from the <see cref="XRHandCaptureSequence"/> instead,
        /// continuing to present the same data to the <see cref="XRHandSubsystem"/>.
        /// </summary>
        AlwaysBlockPlatformData = 1 << 2,

        /// <summary>
        /// If this is enabled, the default behavior of <see cref="XRHandPlayback"/><c>.</c><see cref="XRHandPlayback.anchor"/>
        /// moving the origin of where the data was captured changes to have <c>XRHandPlayback.anchor</c>
        /// define the root pose of the hand in the first frame of playback.
        /// </summary>
        RootPoseLockedToAnchor = 1 << 3,
    }
}
