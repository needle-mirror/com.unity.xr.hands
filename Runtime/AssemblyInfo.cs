using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Unity.XR.Hands.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Tests.OpenXR.MockHandsRuntime")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Editor.Tests.OpenXR.NativeInterop")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Analytics.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Analytics.Hooks.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.Interaction.Toolkit")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Samples.HandCapture")]
[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Features.MetaHandMeshData")]


// Enable C# 9.0 init-only properties
namespace System.Runtime.CompilerServices
{
    static class IsExternalInit { }
}
