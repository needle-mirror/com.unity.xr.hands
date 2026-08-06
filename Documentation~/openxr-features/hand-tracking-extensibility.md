---
uid: xrhands-openxr-hand-extensibility
---

# Hand tracking extensibility

Hand tracking extensibility lets you augment Unity's OpenXR hand tracking with additional OpenXR extensions. Use it to attach vendor-specific or standard extension structures to the OpenXR calls that Unity makes. You can extend both the call that creates a hand tracker and the call that locates hand joints each frame.

This feature targets developers who build OpenXR extension support, such as device vendors and SDK authors. If you only consume hand-tracking data, use the [Hand tracking feature](xref:xrhands-openxr-hands-feature) instead.

## How it works

Unity drives hand tracking through three OpenXR calls:

* `xrCreateHandTrackerEXT` creates a hand tracker for each hand. You can configure the created tracker, for example to opt in to controller-as-hands behavior.
* `xrLocateHandJointsEXT` retrieves the current joint poses for a hand every frame. You can both supply additional input to this call and read additional output from it.
* `xrDestroyHandTrackerEXT` destroys a hand tracker. You can't extend this call, but Unity notifies you through [OnHandTrackerDestroyed](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature) when it completes and reports the call's result code.

The create and locate calls accept a `next` chain of extension structures. Hand tracking extensibility gives you access to these chains from C# through the [OpenXRHandTrackingFeature](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature) base class. You add your extension structures to the relevant chain, and Unity passes them to the underlying OpenXR call.

You manage extension structures with the [XrStructureChain](xref:UnityEngine.XR.Hands.OpenXR.NativeInterop.XrStructureChain) class. A structure chain holds a linked list of OpenXR structures in stable unmanaged memory that's safe to pass to OpenXR. Each [XrStructureType](xref:UnityEngine.XR.OpenXR.NativeTypes.XrStructureType) can appear in a chain no more than once. These linked lists are appended to the appropriate parameter's `next` field.

## Requirements

To use hand tracking extensibility:

1. Install and enable the [OpenXR package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest) version 1.18.0 or later.
1. Enable the [Hand Tracking feature](xref:xrhands-openxr-hands-feature) in **Project Settings** &gt; **XR Plug-in Management** &gt; **OpenXR**.

When a compatible OpenXR package is present, Unity defines the `UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING` scripting symbol. The extensibility APIs are available only when Unity defines this symbol, so guard your feature code with it:

```csharp
#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
// Hand tracking extensibility code
#endif
```

## Create a hand tracking extensibility feature

Define your extension as an OpenXR feature that derives from [OpenXRHandTrackingFeature](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature). Unity manages the feature's lifecycle alongside the OpenXR instance and registers it for hand tracking dispatch callbacks.

In [OnInstanceCreate](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature), verify that OpenXR has enabled the extension your feature relies on before you call the base implementation. If OpenXR hasn't enabled the extension, return `false` without calling `base.OnInstanceCreate` to disable the feature for the current instance.

```csharp
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Hands.OpenXR;

#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
public class MyHandExtensionFeature : OpenXRHandTrackingFeature
{
    const string k_ExtensionString = "XR_VENDOR_my_extension";

    protected override bool OnInstanceCreate(ulong xrInstance)
    {
        if (!OpenXRRuntime.IsExtensionEnabled(k_ExtensionString))
            return false;

        // Registers the feature for hand tracking dispatch callbacks.
        return base.OnInstanceCreate(xrInstance);
    }
}
#endif
```

> [!NOTE]
> If you override [OnInstanceDestroy](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature), call `base.OnInstanceDestroy` so that Unity removes the feature from dispatch callbacks.

## Add structures at hand tracker creation

To configure the created hand tracker, override [OnHandTrackingCreateRequest](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature). Unity calls this method once per hand during `xrCreateHandTrackerEXT` and provides the extension chain attached to the tracker creation info. Add your structures with [XrStructureChain.TryAddNode](xref:UnityEngine.XR.Hands.OpenXR.NativeInterop.XrStructureChain.TryAddNode``1(``0)).

Unity clears this chain before each dispatch, so add your nodes unconditionally on every invocation, including when a [hand tracker restart](#restart-the-hand-tracker) recreates the tracker:

```csharp
protected override void OnHandTrackingCreateRequest(XrHandEXT hand, XrStructureChain extensionChain)
{
    extensionChain.TryAddNode(new XrMyCreateInfoVENDOR(/* ... */));
}
```

## Add and update structures for joint location

To supply input to the per-frame `xrLocateHandJointsEXT` call, add your structures to the locate-input chain — the chain of input structures for that call — returned by [GetLocateInputChain](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature). Unlike the creation chain, the locate-input chain persists across frames, so add each node once and then mutate it in place rather than re-adding it every frame.

Add the initial nodes when the session begins:

```csharp
protected override void OnSessionCreate(ulong xrSession)
{
    base.OnSessionCreate(xrSession);

    GetLocateInputChain(XrHandEXT.Left)?.TryAddNode(new XrMyLocateInfoVENDOR(/* ... */));
    GetLocateInputChain(XrHandEXT.Right)?.TryAddNode(new XrMyLocateInfoVENDOR(/* ... */));
}
```

When a value changes, update the existing node in place with [XrStructureChain.TryUpdateNode](xref:UnityEngine.XR.Hands.OpenXR.NativeInterop.XrStructureChain.TryUpdateNode``1(``0)). This preserves the chain's linkage and avoids allocating new unmanaged memory:

```csharp
GetLocateInputChain(hand)?.TryUpdateNode(new XrMyLocateInfoVENDOR(/* updated value */));
```

## Read joint location output

Reading output is a two-step process. The OpenXR runtime doesn't return output structures to you. It writes into structures you supply on the locate-output chain. First register an output structure once, then read its runtime-filled value every frame.

### Register the output structure

Add your output structure to the locate-output chain that [GetLocateOutputChain](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature) returns. Like the locate-input chain, this chain persists across frames, so add each node once when the session begins. Unity passes this chain to `xrLocateHandJointsEXT` so the runtime can fill it in place:

```csharp
protected override void OnSessionCreate(ulong xrSession)
{
    base.OnSessionCreate(xrSession);

    // Add any locate-input nodes here too — see "Add and update structures
    // for joint location". Keep all chain registration in one OnSessionCreate.

    GetLocateOutputChain(XrHandEXT.Left)?.TryAddNode(new XrMyOutputVENDOR(/* ... */));
    GetLocateOutputChain(XrHandEXT.Right)?.TryAddNode(new XrMyOutputVENDOR(/* ... */));
}
```

### Read the result

To read data that the OpenXR runtime fills in during `xrLocateHandJointsEXT`, override [OnLocateHandJointsResult](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature). Unity calls this method every frame for each hand. Read the output structures with [XrStructureChain.TryGetNode](xref:UnityEngine.XR.Hands.OpenXR.NativeInterop.XrStructureChain.TryGetNode``1(UnityEngine.XR.OpenXR.NativeTypes.XrStructureType,``0@)).

Check that `locateHandJointsResult.IsSuccess()` returns `true` and that `isActive` is `true` before you read the data, because the locate call can succeed even when the hand isn't active:

```csharp
protected override void OnLocateHandJointsResult(
    XrHandEXT hand,
    XrStructureChain outputChain,
    XrResult locateHandJointsResult,
    bool isActive)
{
    if (!locateHandJointsResult.IsSuccess() || !isActive)
        return;

    if (outputChain.TryGetNode(XrStructureType.MyOutputVENDOR, out XrMyOutputVENDOR output))
    {
        // Copy the fields you need to managed state.
    }
}
```

> [!NOTE]
> This method runs on every frame for each hand. Keep the implementation lightweight, copy only the fields you need, and avoid per-frame memory allocations.

## Expose configuration to applications

To let applications change your extension's settings at runtime, implement [IXRHandConfigurationHandler&lt;TConfig&gt;](xref:UnityEngine.XR.Hands.IXRHandConfigurationHandler`1) on your feature and register it with the [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem). Define a configuration type that holds your settings. Register the handler when Unity creates the subsystem, and remove it when Unity destroys the subsystem:

```csharp
public struct MyConfig
{
    public bool someOption;
}

public class MyHandExtensionFeature : OpenXRHandTrackingFeature, IXRHandConfigurationHandler<MyConfig>
{
    XRHandSubsystem m_Subsystem;

    protected override void OnHandSubsystemCreated(XRHandSubsystem subsystem)
    {
        base.OnHandSubsystemCreated(subsystem);
        m_Subsystem = subsystem;
        subsystem.RegisterConfigurationHandler(this);
    }

    protected override void OnHandSubsystemDestroyed(XRHandSubsystem subsystem)
    {
        if (m_Subsystem == subsystem)
        {
            m_Subsystem.UnregisterConfigurationHandler<MyConfig>();
            m_Subsystem = null;
        }

        base.OnHandSubsystemDestroyed(subsystem);
    }

    public bool TryGetConfiguration(out MyConfig config)
    {
        config = /* current settings */;
        return true;
    }

    public bool TryUpdateConfiguration(MyConfig config)
    {
        // Validate and apply, for example by updating a locate-input node.
        // Return false to reject the configuration and keep the current state.
        return true;
    }
}
```

Applications then read and change the configuration through the subsystem, using your configuration type to select the handler:

```csharp
if (subsystem.TryGetConfiguration(out MyConfig config))
{
    config.someOption = true;
    subsystem.TryUpdateConfiguration(config);
}
```

Both methods return `false` when the application hasn't registered a handler for the requested configuration type.

## Restart the hand tracker

Some configuration changes can't take effect on an existing tracker and require a fresh `xrCreateHandTrackerEXT` call. When you must apply a change to a structure you add in [OnHandTrackingCreateRequest](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature), call [RequestHandTrackerRestart](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature). Unity destroys and recreates the hand trackers at the start of the next update:

```csharp
RequestHandTrackerRestart();
```

Repeated restart requests within a single frame coalesce, so you don't need to filter duplicate calls. After a restart, Unity calls [OnHandTrackerDestroyed](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature) and then [OnHandTrackerCreated](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature) again within the same session, so these callbacks can trigger more than once per session.

Both callbacks receive the `XrResult` that the runtime returned for the call. Check it with `IsSuccess()` before you assume the operation succeeded; see the OpenXR specification for the codes each call can return.

## Reference implementations

The XR Hands package ships two OpenXR features built on hand tracking extensibility. Use them as working examples:

* [Hand Joints Motion Range](xref:xrhands-openxr-motion-range-feature) adds an `XrHandJointsMotionRangeInfoEXT` structure to the locate-input chain and updates it in place through a configuration handler, without a tracker restart.
* [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature) injects preferred data sources at tracker creation and reads the active source from the locate output each frame.

## Additional resources

* [Hand Tracking feature](xref:xrhands-openxr-hands-feature)
* [OpenXRHandTrackingFeature API reference](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandTrackingFeature)
* [XrStructureChain API reference](xref:UnityEngine.XR.Hands.OpenXR.NativeInterop.XrStructureChain)
* [Khronos OpenXR specification: XR_EXT_hand_tracking](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_EXT_hand_tracking)
