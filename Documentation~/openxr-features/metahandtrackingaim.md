---
uid: xrhands-meta-aim-feature
---
# Meta Hand Tracking Aim OpenXR feature

Unity provides support for the Meta Hand Tracking Aim OpenXR extension specified by Khronos. For this extension to work, you must also enable the [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) OpenXR feature.

This extension requires you to install the [OpenXR Plugin package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest). For this extension to work when deployed to a Meta Quest device, the OpenXR Plugin package version must be `1.6.0` or newer.

Enable this OpenXR feature to expose the [MetaAimHand](xref:UnityEngine.XR.Hands.MetaAimHand) input device within the [Unity Input System](xref:input-system-index), which is a type of [Tracked Input Device](xref:input-system-tracked-input-devices).

## Available controls

The following table outlines the mapping between the OpenXR paths and Unity's implementation:

|**Data**|**Binding Path**|**Type**|
|----|----|----|
| Position | `<MetaAimHand>/devicePosition`<br/>(or `<TrackedDevice>/devicePosition`) | Vector3 |
| Rotation | `<MetaAimHand>/deviceRotation`<br/>(or `<TrackedDevice>/deviceRotation`) | Quaternion |
| Is Tracked | `<MetaAimHand>/isTracked`<br/>(or `<TrackedDevice>/isTracked`) | Boolean |
| Tracking State | `<MetaAimHand>/trackingState`<br/>(or `<TrackedDevice>/trackingState`) | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) |
| Aim Flags | `<MetaAimHand>/aimFlags` | Integer ([flags enum](xref:UnityEngine.XR.Hands.MetaAimFlags)) |
| Index Pinch Strength | `<MetaAimHand>/pinchStrengthIndex` | Float<br/>0 to 1 |
| Middle Pinch Strength | `<MetaAimHand>/pinchStrengthMiddle` | Float<br/>0 to 1 |
| Ring Pinch Strength | `<MetaAimHand>/pinchStrengthRing` | Float<br/>0 to 1 |
| Little Pinch Strength | `<MetaAimHand>/pinchStrengthLittle` | Float<br/>0 to 1 |
| Is Index Pressed | `<MetaAimHand>/indexPressed` | Boolean |
| Is Middle Pressed | `<MetaAimHand>/middlePressed` | Boolean |
| Is Ring Pressed | `<MetaAimHand>/ringPressed` | Boolean |
| Is Little Pressed | `<MetaAimHand>/littlePressed` | Boolean |

To specify a particular hand, you can add `{LeftHand}` or `{RightHand}` after the `<MetaAimHand>` in the binding, such as `<MetaAimHand>{LeftHand}/devicePosition`.

For more information about the Meta Hand Tracking Aim extension, see the [OpenXR Specification: XR_FB_hand_tracking_aim](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_FB_hand_tracking_aim).
