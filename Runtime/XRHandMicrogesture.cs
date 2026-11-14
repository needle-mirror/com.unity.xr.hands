namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// The microgesture to query or react to, such as a thumb swipe or tap.
    /// </summary>
    /// <remarks>
    /// Each value identifies one microgesture. To check whether a microgesture is active, pass a value to
    /// <see cref="XRCommonHandGestures.IsMicrogestureActive(XRHandMicrogesture)"/>. To check whether it became active
    /// or inactive in the latest dynamic update, use
    /// <see cref="XRCommonHandGestures.WasMicrogesturePerformedThisFrame(XRHandMicrogesture)"/> or
    /// <see cref="XRCommonHandGestures.WasMicrogestureCompletedThisFrame(XRHandMicrogesture)"/>. To react when a
    /// microgesture becomes active or inactive, subscribe to <see cref="XRCommonHandGestures.microgestureUpdated"/>.
    ///
    /// Microgestures are available only when the hand subsystem supports them. To check support, read
    /// <see cref="XRHandSubsystemDescriptor.supportsMicrogestures"/>.
    /// </remarks>
    public enum XRHandMicrogesture
    {
        /// <summary>
        /// No microgesture.
        /// </summary>
        /// <remarks>
        /// <see cref="XRCommonHandGestures.IsMicrogestureActive(XRHandMicrogesture)"/>,
        /// <see cref="XRCommonHandGestures.WasMicrogesturePerformedThisFrame(XRHandMicrogesture)"/>, and
        /// <see cref="XRCommonHandGestures.WasMicrogestureCompletedThisFrame(XRHandMicrogesture)"/> return <c>false</c>
        /// for this value.
        /// </remarks>
        None = 0,

        /// <summary>
        /// A thumb swipe to the left.
        /// </summary>
        SwipeLeft = 1,

        /// <summary>
        /// A thumb swipe to the right.
        /// </summary>
        SwipeRight = 2,

        /// <summary>
        /// A thumb swipe forward.
        /// </summary>
        SwipeForward = 3,

        /// <summary>
        /// A thumb swipe backward.
        /// </summary>
        SwipeBackward = 4,

        /// <summary>
        /// A thumb tap.
        /// </summary>
        TapThumb = 5,
    }
}
