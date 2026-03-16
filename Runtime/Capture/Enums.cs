using System;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// Options to change the behavior of the recording of an entire capture.
    /// You can set this behavior with <see cref="XRHandRecordingInitializeArgs"/><c>.</c><see cref="XRHandRecordingInitializeArgs.recordingOptions"/>
    /// before passing it to <see cref="XRHandRecordingBlob"/><c>.</c><see cref="XRHandRecordingBlob.TryInitialize"/>.
    /// </summary>
    [Flags]
    enum XRHandRecordingOptions
    {
        /// <summary>
        /// Only capture during the <see cref="XRHandSubsystem.UpdateType.Dynamic"/>
        /// update step, reducing the size of the resulting asset.
        /// </summary>
        None = 0,

        /// <summary>
        /// Additionally, include the <see cref="XRHandSubsystem.UpdateType.BeforeRender"/>
        /// data for <see cref="XRHand"/>s in the resulting
        /// <see cref="XRHandCaptureSequence"/> asset when imported.
        /// Note that <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetHandSnapshot"/>
        /// will always fail with an update type of <see cref="XRHandSubsystem.UpdateType.BeforeRender"/>
        /// if <c>AlsoCaptureBeforeRender</c> was disabled when the
        /// <c>XRHandCaptureSequence</c> asset was captured.
        /// </summary>
        AlsoCaptureBeforeRender = 1 << 0,
    }
}
