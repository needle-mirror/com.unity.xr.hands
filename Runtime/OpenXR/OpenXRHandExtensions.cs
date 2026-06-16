#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// OpenXR-specific extensions for XR Hands.
    /// </summary>
    public static class OpenXRHandExtensions
    {
        /// <summary>
        /// Get the <c>XrHandTrackerEXT</c> associated the given <see cref="Handedness"/>.
        /// Must be running with OpenXR for this to successfully return a valid handle.
        /// </summary>
        /// <remarks>
        /// Can be useful for OpenXR-specific operations involving hand-tracking,
        /// not usually required for general use.
        /// </remarks>
        /// <param name="handedness">
        /// Which hand to get the tracker handle for.
        /// </param>
        /// <returns>
        /// Returns the <c>XrHandTrackerEXT</c> associated with the given <see cref="Handedness"/>.
        /// </returns>
        internal static ulong GetOpenXRHandTrackerHandle(this Handedness handedness)
        {
            if (handedness != Handedness.Left && handedness != Handedness.Right)
                throw new ArgumentException("Handedness must be left or right!");

            return NativeApi.GetXrHandTrackerEXT(handedness);
        }

        static class NativeApi
        {
            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_GetXrHandTrackerEXT")]
            public static extern ulong GetXrHandTrackerEXT(Handedness handedness);
        }
    }
}
#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
