---
uid: xrhands-openxr-subsystem-manager
---

# OpenXR Hand Subsystem Manager component

The `OpenXRHandSubsystemManager` component manages the lifecycle of the [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem) and its updater, which are created by the OpenXR [Hand Tracking feature](xref:xrhands-openxr-hands-feature). Use this component to control when the hand subsystem starts and stops.

## Overview {#overview}

By default, the OpenXR Hand Tracking feature automatically creates and starts the hand subsystem when the OpenXR session begins. The `OpenXRHandSubsystemManager` component serves two purposes:

- [Start and stop control](#toggle-hand-tracking-at-runtime): Ties the subsystem lifecycle to the component's enabled state. Enabling the component starts the subsystem; disabling it stops the subsystem.
- [Deferred creation](#deferred-initialization): When the **Auto Start Subsystem** setting on the Hand Tracking Subsystem feature is unchecked, the subsystem is not created at startup. You can then use this component to create and start the subsystem at a later time, for example, after the user grants a required permission. Refer to [Deferred initialization](#deferred-initialization) section for details.

## Setup {#setup}

Add the `OpenXRHandSubsystemManager` component to a GameObject in your scene.

> [!TIP]
> The **HandVisualizer** sample includes a **Hand Debug Visualizer** prefab that already has the `OpenXRHandSubsystemManager` component attached. You can use this prefab as a starting point.

### Deferred initialization {#deferred-initialization}

To defer hand tracking until a condition is met (such as a user granting permission):

1. Open **Project Settings > XR Plug-in Management > OpenXR**.
2. Click the gear icon next to **Hand Tracking Subsystem** and uncheck **Auto Start Subsystem**.
    ![Configure OpenXR Hand Tracking Feature](../images/hand-tracking-feature-settings-auto-start-subsystem.png)<br/>*Configure OpenXR Hand Tracking feature to disable Auto Start Subsystem*
3. Add an `OpenXRHandSubsystemManager` component to your scene. Because the subsystem is not created at startup, the component begins in a disabled state.
4. In your code, enable the component when you are ready to start hand tracking:

```csharp
// After permission is granted or your setup condition is met:
handSubsystemManager.enabled = true;
```

### Toggle hand tracking at runtime {#toggle-hand-tracking-at-runtime}

You can toggle hand tracking on and off at any time by enabling and disabling the component:

[!code-cs [subsystem-manager-sample](../../DocCodeSamples.Tests/SubsystemManagerSample.cs)]

## Additional resources

* [Hand Tracking OpenXR feature](xref:xrhands-openxr-hands-feature)
* [OpenXRHandSubsystemManager API](xref:UnityEngine.XR.Hands.OpenXR.OpenXRHandSubsystemManager)
