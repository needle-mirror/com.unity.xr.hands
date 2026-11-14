---
uid: xrhands-microgestures
---

# Query microgestures

Learn how to query microgestures from the XR Hands subsystem.

Microgestures are small thumb motions, such as swiping or tapping the thumb against the side of the index finger.

## Requirements {#requirements}

To use microgestures on OpenXR, your project must meet the following requirements:

* The [OpenXR Plugin package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest) version `7.0.0-pre.1` or newer.
* The following OpenXR features, enabled in the **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **OpenXR** window:
  * [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature)
  * [Hand Interaction Profile](xref:openxr-hand-interaction-profile)
  * [Hand Interaction Profile Microgestures](xref:openxr-hand-interaction-profile-microgestures)

These requirements apply in both [XRHandDevice](xref:xrhands-hand-device) configuration modes. The [Meta Hand Tracking Aim](xref:xrhands-meta-aim-feature) OpenXR feature doesn't provide microgestures.

Changes to the enabled features apply the next time you enter Play mode or launch the application.

## Check microgesture support {#check-support}

The [XRHandSubsystemDescriptor.supportsMicrogestures](xref:UnityEngine.XR.Hands.XRHandSubsystemDescriptor.supportsMicrogestures) property indicates whether the provider can supply microgestures. On OpenXR, this property is `true` when your project meets the [requirements](#requirements).

When the provider doesn't support microgestures, all microgesture queries return `false` and the subsystem doesn't raise the [microgestureUpdated](xref:UnityEngine.XR.Hands.XRCommonHandGestures.microgestureUpdated) event.

## Microgesture types {#microgesture-types}

The [XRHandMicrogesture](xref:UnityEngine.XR.Hands.XRHandMicrogesture) enum identifies each microgesture.

| **Value** | **Microgesture** |
| :-------- | :--------------- |
| `None` | No microgesture. Queries for this value always return `false`. |
| `SwipeLeft` | A thumb swipe to the left. |
| `SwipeRight` | A thumb swipe to the right. |
| `SwipeForward` | A thumb swipe forward. |
| `SwipeBackward` | A thumb swipe backward. |
| `TapThumb` | A thumb tap. |

## Query microgesture state {#query-state}

Each hand has its own [XRCommonHandGestures](xref:UnityEngine.XR.Hands.XRCommonHandGestures) instance. Get it from the `XRHandSubsystem` through [leftHandCommonGestures](xref:UnityEngine.XR.Hands.XRHandSubsystem.leftHandCommonGestures), [rightHandCommonGestures](xref:UnityEngine.XR.Hands.XRHandSubsystem.rightHandCommonGestures), or [GetCommonGestures](xref:UnityEngine.XR.Hands.XRHandSubsystem.GetCommonGestures*). Refer to [Access hand data: Get the XRHandSubsystem instance](xref:xrhands-access-data#get-instance) to learn how to get the subsystem.

`XRCommonHandGestures` provides the following methods to query a microgesture.

| **Method** | **Returns** |
| :--------- | :---------- |
| [IsMicrogestureActive](xref:UnityEngine.XR.Hands.XRCommonHandGestures.IsMicrogestureActive*) | `true` when the runtime recognizes the microgesture, as of the latest dynamic update. |
| [WasMicrogesturePerformedThisFrame](xref:UnityEngine.XR.Hands.XRCommonHandGestures.WasMicrogesturePerformedThisFrame*) | `true` if the microgesture became active in the latest dynamic update. |
| [WasMicrogestureCompletedThisFrame](xref:UnityEngine.XR.Hands.XRCommonHandGestures.WasMicrogestureCompletedThisFrame*) | `true` if the microgesture became inactive in the latest dynamic update. |

Only dynamic updates ([XRHandSubsystem.UpdateType.Dynamic](xref:UnityEngine.XR.Hands.XRHandSubsystem.UpdateType.Dynamic)) change the results of these methods. Before-render updates don't change them. By default, the subsystem runs one dynamic update per frame, so `ThisFrame` in the method names refers to that update.

All three methods return `false` for `XRHandMicrogesture.None` and for values not defined in the enum.

## Respond to microgesture changes {#respond-to-changes}

The [microgestureUpdated](xref:UnityEngine.XR.Hands.XRCommonHandGestures.microgestureUpdated) event of `XRCommonHandGestures` is raised at most once per dynamic update, and only in an update where one or more microgestures become active or inactive.

The event passes a [MicrogestureUpdatedEventArgs](xref:UnityEngine.XR.Hands.XRCommonHandGestures.MicrogestureUpdatedEventArgs) object, which exposes the following properties:

* `handedness` and `commonGestures`, which identify the hand and its `XRCommonHandGestures` instance.
* Three properties for each microgesture, with the same meaning as the query methods. For example, `swipeLeftActive`, `swipeLeftPerformedThisFrame`, and `swipeLeftCompletedThisFrame`.

> [!NOTE]
> The `MicrogestureUpdatedEventArgs` instance is valid only while the event is invoked, so don't keep a reference to it.

## Input System controls {#input-system-controls}

The `<XRHandDevice>` input device also exposes microgestures as Input System controls. Refer to [XR Hand Device: Microgestures](xref:xrhands-hand-device#microgestures).

## Limitations {#limitations}

[XR Hand Capture](xref:xrhands-xrhandcapture) recordings don't include microgesture data.

## Additional resources

* [XR Hand Device](xref:xrhands-hand-device)
* [Access hand data](xref:xrhands-access-data)
* OpenXR [Hand Interaction Profile](xref:openxr-hand-interaction-profile)
* [Implement a provider](xref:xrhands-implement-a-provider)
