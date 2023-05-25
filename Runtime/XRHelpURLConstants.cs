namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Constants for <see cref="HelpURLAttribute"/> for XR Hands.
    /// </summary>
    static class XRHelpURLConstants
    {
        /// <summary>
        /// Scripting API URL for <see cref="XRHandSkeletonDriver"/>.
        /// </summary>
        internal const string k_XRHandSkeletonDriver = k_BaseApi + k_BaseNamespace + nameof(XRHandSkeletonDriver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRHandTrackingEvents"/>.
        /// </summary>
        internal const string k_XRHandTrackingEvents = k_BaseApi + k_BaseNamespace + nameof(XRHandTrackingEvents) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="XRHandMeshController"/>.
        /// </summary>
        internal const string k_XRHandMeshController = k_BaseApi + k_BaseNamespace + nameof(XRHandMeshController) + ".html";

        const string k_BaseApi = "https://docs.unity3d.com/Packages/com.unity.xr.hands@1.1/api/";
        const string k_BaseNamespace = "UnityEngine.XR.Hands.";
    }
}
