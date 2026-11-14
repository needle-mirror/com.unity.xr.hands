---
uid: xrhands-implement-a-provider
---
# Implement a provider

To implement a provider for the subsystem in this package (for example, say you are a hardware manufacturer for a new XR device), Unity recommends that your implementation inherit from [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem) if you have platform-specific data you wish to surface. This subsystem base class has a corresponding abstract class called [XRHandSubsystemProvider](xref:UnityEngine.XR.Hands.ProviderImplementation.XRHandSubsystemProvider), which is the primary interface you must implement to support on your platform.

Subsystem implementations should be independent from each other. For example, your implementation of the [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem) should have the same behavior whether or not other subsystem implementations are also active in a user's scene.

## Register a subsystem descriptor

Each subsystem type has a corresponding subsystem descriptor type. Your provider should create and register a subsystem descriptor instance with Unity's [SubsystemManager](https://docs.unity3d.com/ScriptReference/SubsystemManager.html) to enable runtime discovery and activation of subsystems. To register your subsystem descriptor, include a static void method with the `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` attribute as shown in the example below, and in it call `XRHandSubsystemDescriptor.Register` with subsystem descriptor information you are registering.

```csharp
// This class defines a hand subsystem provider
class MyHandProvider : XRHandSubsystemProvider
{
    // ...
}

// This class defines a hand subsystem
class MyHandSubsystem : XRHandSubsystem
{
    public int myPlatformSpecificData => provider.RetrievePlatformSpecificData();

    // This method registers the subsystem descriptor with the SubsystemManager
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void RegisterDescriptor()
    {
        var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "My-Hands",
            providerType = typeof(MyHandProvider),
            subsystemTypeOverride = typeof(MyHandSubsystem)
        };
        XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
    }
}
```

## Supply microgestures

If your platform's runtime recognizes thumb microgestures, your provider can report them to the subsystem.

To supply microgestures from your provider, follow these steps:

1. When you register the subsystem descriptor, set [XRHandSubsystemDescriptor.Cinfo.supportsMicrogestures](xref:UnityEngine.XR.Hands.XRHandSubsystemDescriptor.Cinfo.supportsMicrogestures) to `true`. The registered descriptor reports this value through [XRHandSubsystemDescriptor.supportsMicrogestures](xref:UnityEngine.XR.Hands.XRHandSubsystemDescriptor.supportsMicrogestures).
2. In your provider, override [canSurfaceCommonPoseData](xref:UnityEngine.XR.Hands.ProviderImplementation.XRHandSubsystemProvider.canSurfaceCommonPoseData) to return `true`.
3. In your provider, override [IsMicrogestureActive](xref:UnityEngine.XR.Hands.ProviderImplementation.XRHandSubsystemProvider.IsMicrogestureActive*) to return `true` while the runtime reports the gesture as active, and `false` otherwise.

The subsystem calls `IsMicrogestureActive` only during dynamic updates (`XRHandSubsystem.UpdateType.Dynamic`), and only when both `canSurfaceCommonPoseData` and `supportsMicrogestures` are `true`. The default implementation returns `false`.

The subsystem uses these results to update the microgesture state in [XRCommonHandGestures](xref:UnityEngine.XR.Hands.XRCommonHandGestures) and the microgesture controls on the XR Hand Device. For more information, refer to [Microgestures](xref:xrhands-microgestures) and [XR Hand Device: Microgestures](xref:xrhands-hand-device#microgestures).

## Implement an XR Loader

An `XRLoader` is responsible for creating and destroying subsystem instances based on the settings found in **Project Settings** > **XR Plug-in Management**. All provider plug-ins must implement an `XRLoader`. For more information on authoring an `XRLoader`, see the [XR Plug-in Management provider documentation](https://docs.unity3d.com/Packages/com.unity.xr.management@latest?subfolder=/manual/Provider.html).
