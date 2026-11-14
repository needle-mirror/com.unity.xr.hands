---
uid: xrhands-hand-device
---

# XR Hand Device

The [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem) exposes the [XRHandDevice](xref:UnityEngine.XR.Hands.XRHandDevice) input device within the [Unity Input System](xref:input-system-index), which is a type of [TrackedDevice](xref:input-system-tracked-input-devices). This is automatically done when the subsystem is running, such as when enabling the [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) OpenXR feature.

## Configuration

The runtime value of [XRHandSubsystem.handSubsystemConfiguration](xref:UnityEngine.XR.Hands.XRHandSubsystem.handSubsystemConfiguration) drastically alters the meaning of the `<XRHandDevice>` input device controls, essentially switching it between two entirely different input devices. You can set the configuration on the `XRHandSubsystem` to choose between the [XRHandSubsystemConfiguration.xrHandDevicePoseSource](xref:UnityEngine.XR.Hands.Configuration.XRHandSubsystemConfiguration.xrHandDevicePoseSource) options. Refer to [Access hand data: Get the XRHandSubsystem instance](xref:xrhands-access-data#get-instance) to learn how to get the `XRHandSubsystem` to change the configuration.

A simple way to change from the default [Legacy](#legacy) mode to [Common Gestures](#common-gestures) mode is to add the following component to a GameObject:

[!code-cs [update_hands_configuration_sample](../../Tests/Editor/DocCodeSamples/UpdateHandsConfigurationSample.cs)]

The Common Gestures mode is the recommended path to use for your OpenXR project. The Legacy mode is default to avoid breaking existing projects which already made use of the `<XRHandDevice>` and the locations of each of the poses, and for maximum compatibility in projects which are not targeting OpenXR which may not support common gestures.

> [!NOTE]
> When using Common Gestures mode on OpenXR, you must also enable the [Hand Interaction Profile](xref:openxr-hand-interaction-profile) for full functionality (or enable [Meta Hand Tracking Aim OpenXR feature](xref:xrhands-meta-aim-feature) for a subset of functionality). Refer to [Common Gestures](#common-gestures) for more details.

### XRHandSubsystemProvider implementers

For advanced users or for platform providers, the [`XRHandSubsystemDescriptor`](xref:UnityEngine.XR.Hands.XRHandSubsystemDescriptor) contains properties which indicate whether the provider can supply each of the [Poses](#common-gestures-poses), [Gesture values](#gesture-values), or [Microgestures](#microgestures). The [`XRHandSubsystemProvider.canSurfaceCommonPoseData`](xref:UnityEngine.XR.Hands.ProviderImplementation.XRHandSubsystemProvider.canSurfaceCommonPoseData) must also be implemented to indicate that the provider can supply data for populating [`XRCommonHandGestures`](xref:UnityEngine.XR.Hands.XRCommonHandGestures).

## Available controls {#available-controls}

The following tables outline the mapping between the data sources and Unity's control paths on the input device, along with any applicable OpenXR path for each value.

To specify a particular hand, you can add `{LeftHand}` or `{RightHand}` after the `<XRHandDevice>` in the binding, such as `<XRHandDevice>{LeftHand}/devicePosition`.

Some controls defined in the `<XRHandDevice>` are not supplied a value depending on the configuration mode, as outlined in [Configuration](#configuration).

> [!NOTE]
> The set of "device" pose bindings differs between configuration modes. Those four binding paths (`devicePosition`, `deviceRotation`, `isTracked`, and `trackingState`) will either map to the Wrist pose or the Grip pose.

### Legacy (Default) {#legacy}

The following tables apply when the configuration mode is set to [XRHandDevicePoseSource.LegacyJointRecognition](xref:UnityEngine.XR.Hands.Configuration.XRHandDevicePoseSource.LegacyJointRecognition):

#### Poses {#legacy-poses}

These poses represent the joint poses from a hand, specifically [XRHandJointID.Wrist](xref:UnityEngine.XR.Hands.XRHandJointID.Wrist), [XRHandJointID.Palm](xref:UnityEngine.XR.Hands.XRHandJointID.Palm), [XRHandJointID.IndexTip](xref:UnityEngine.XR.Hands.XRHandJointID.IndexTip), and [XRHandJointID.ThumbTip](xref:UnityEngine.XR.Hands.XRHandJointID.ThumbTip). Refer to [Joint nomenclature](xref:xrhands-data-model#joint-nomenclature) for a visual representation of each joint.

You can bind to these poses with [input actions](xref:input-system-actions) to use in your components, such as [Tracked Pose Driver](xref:input-system-tracked-input-devices#tracked-pose-driver), instead of querying them directly from an [XRHand](xref:UnityEngine.XR.Hands.XRHand). Refer to [Access hand data: Get joint data](xref:xrhands-access-data#get-joint-data) to learn how to get hand joint poses through scripting API instead.

|**Data**|**Binding Path**|**Type**|**OpenXR or Unity Path**|
|---|---|---|---|
| Wrist Position | `<XRHandDevice>/wristPosition`<br/>`<XRHandDevice>/devicePosition`<br/>(or `<TrackedDevice>/devicePosition`) | Vector3 | [`XR_HAND_JOINT_WRIST_EXT`](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#convention-of-hand-joints) |
| Wrist Rotation | `<XRHandDevice>/wristRotation`<br/>`<XRHandDevice>/deviceRotation`<br/>(or `<TrackedDevice>/deviceRotation`) | Quaternion | `XR_HAND_JOINT_WRIST_EXT` |
| Wrist Is Tracked | `<XRHandDevice>/wristIsTracked`<br/>`<XRHandDevice>/isTracked`<br/>(or `<TrackedDevice>/isTracked`) | Boolean | `XR_HAND_JOINT_WRIST_EXT` |
| Wrist Tracking State | `<XRHandDevice>/wristTrackingState`<br/>`<XRHandDevice>/trackingState`<br/>(or `<TrackedDevice>/trackingState`) | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `XR_HAND_JOINT_WRIST_EXT` |
| Palm Position | `<XRHandDevice>/gripPosition` | Vector3 | [`XR_HAND_JOINT_PALM_EXT`](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#convention-of-hand-joints) |
| Palm Rotation | `<XRHandDevice>/gripRotation` | Quaternion | `XR_HAND_JOINT_PALM_EXT` |
| Palm Is Tracked | `<XRHandDevice>/gripIsTracked` | Boolean | `XR_HAND_JOINT_PALM_EXT` |
| Palm Tracking State | `<XRHandDevice>/gripTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `XR_HAND_JOINT_PALM_EXT` |
| Index Tip Position | `<XRHandDevice>/pokePosition` | Vector3 | [`XR_HAND_JOINT_INDEX_TIP_EXT`](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#convention-of-hand-joints) |
| Index Tip Rotation | `<XRHandDevice>/pokeRotation` | Quaternion | `XR_HAND_JOINT_INDEX_TIP_EXT` |
| Index Tip Is Tracked | `<XRHandDevice>/pokeIsTracked` | Boolean | `XR_HAND_JOINT_INDEX_TIP_EXT` |
| Index Tip Tracking State | `<XRHandDevice>/pokeTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `XR_HAND_JOINT_INDEX_TIP_EXT` |
| Thumb Tip Position | `<XRHandDevice>/pinchPosition` | Vector3 | [`XR_HAND_JOINT_THUMB_TIP_EXT`](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#convention-of-hand-joints) |
| Thumb Tip Rotation | `<XRHandDevice>/pinchRotation` | Quaternion | `XR_HAND_JOINT_THUMB_TIP_EXT` |
| Thumb Tip Is Tracked | `<XRHandDevice>/pinchIsTracked` | Boolean | `XR_HAND_JOINT_THUMB_TIP_EXT` |
| Thumb Tip Tracking State | `<XRHandDevice>/pinchTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `XR_HAND_JOINT_THUMB_TIP_EXT` |
| Aim Position | `<XRHandDevice>/aimPosition` | Vector3 | `/input/aim/pose`<br/>`<MetaAimHand>/devicePosition` |
| Aim Rotation | `<XRHandDevice>/aimRotation` | Quaternion | `/input/aim/pose`<br/>`<MetaAimHand>/deviceRotation` |
| Aim Is Tracked | `<XRHandDevice>/aimIsTracked` | Boolean | `/input/aim/pose`<br/>`<MetaAimHand>/isTracked` |
| Aim Tracking State | `<XRHandDevice>/aimTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `/input/aim/pose`<br/>`<MetaAimHand>/trackingState` |

> [!NOTE]
> Legacy mode does not supply any [gesture values](#gesture-values). Those nine binding paths (`graspValue`, `graspFirm`, etc.) will always remain default values.

### Common Gestures {#common-gestures}

For this input device to work on OpenXR, you must also enable the [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) OpenXR feature and enable the [Hand Interaction Profile](xref:openxr-hand-interaction-profile) within the **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **OpenXR** window. If the Hand Interaction Profile is not enabled, enabling the [Meta Hand Tracking Aim OpenXR feature](xref:xrhands-meta-aim-feature) will provide a subset of poses, namely the Aim pose. The [OpenXR Plugin package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest) must be at version `1.8.1` or newer. You must also enable this mode in the `XRHandSubsystem` configuration, as outlined in [Configuration](#configuration).

The following tables apply when the configuration mode is set to [XRHandDevicePoseSource.CommonGestures](xref:UnityEngine.XR.Hands.Configuration.XRHandDevicePoseSource.CommonGestures):

#### Poses {#common-gestures-poses}

These poses represent the poses from a hand interaction profile, specifically [Grip pose](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_grip_pose), [Poke pose](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_poke_pose), [Pinch pose](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_pinch_pose), and [Aim pose](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_aim_pose), and the [wrist joint pose](xref:xrhands-data-model#joint-nomenclature).

You can bind to these poses with [input actions](xref:input-system-actions) to use in your components like [Tracked Pose Driver](xref:input-system-tracked-input-devices#tracked-pose-driver) instead of the [HandInteraction](xref:openxr-hand-interaction-profile) poses to support hand playback overriding the values or for supporting simulation.

|**Data**|**Binding Path(s)**|**Type**|**OpenXR or Unity Path**|
|---|---|---|---|
| Wrist Position | `<XRHandDevice>/wristPosition` | Vector3 | [`XR_HAND_JOINT_WRIST_EXT`](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#convention-of-hand-joints) |
| Wrist Rotation | `<XRHandDevice>/wristRotation` | Quaternion | `XR_HAND_JOINT_WRIST_EXT` |
| Wrist Is Tracked | `<XRHandDevice>/wristIsTracked` | Boolean | `XR_HAND_JOINT_WRIST_EXT` |
| Wrist Tracking State | `<XRHandDevice>/wristTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `XR_HAND_JOINT_WRIST_EXT` |
| Grip Position | `<XRHandDevice>/gripPosition`<br/>`<XRHandDevice>/devicePosition`<br/>(or `<TrackedDevice>/devicePosition`) | Vector3 | `/input/grip/pose` |
| Grip Rotation | `<XRHandDevice>/gripRotation`<br/>`<XRHandDevice>/deviceRotation`<br/>(or `<TrackedDevice>/deviceRotation`) | Quaternion | `/input/grip/pose` |
| Grip Is Tracked | `<XRHandDevice>/gripIsTracked`<br/>`<XRHandDevice>/isTracked`<br/>(or `<TrackedDevice>/isTracked`) | Boolean | `/input/grip/pose` |
| Grip Tracking State | `<XRHandDevice>/gripTrackingState`<br/>`<XRHandDevice>/trackingState`<br/>(or `<TrackedDevice>/trackingState`) | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `/input/grip/pose` |
| Poke Position | `<XRHandDevice>/pokePosition` | Vector3 | `/input/poke_ext/pose` |
| Poke Rotation | `<XRHandDevice>/pokeRotation` | Quaternion | `/input/poke_ext/pose` |
| Poke Is Tracked | `<XRHandDevice>/pokeIsTracked` | Boolean | `/input/poke_ext/pose` |
| Poke Tracking State | `<XRHandDevice>/pokeTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `/input/poke_ext/pose` |
| Pinch Position | `<XRHandDevice>/pinchPosition` | Vector3 | `/input/pinch_ext/pose` |
| Pinch Rotation | `<XRHandDevice>/pinchRotation` | Quaternion | `/input/pinch_ext/pose` |
| Pinch Is Tracked | `<XRHandDevice>/pinchIsTracked` | Boolean | `/input/pinch_ext/pose` |
| Pinch Tracking State | `<XRHandDevice>/pinchTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `/input/pinch_ext/pose` |
| Aim Position | `<XRHandDevice>/aimPosition` | Vector3 | `/input/aim/pose`<br/>`<MetaAimHand>/devicePosition` |
| Aim Rotation | `<XRHandDevice>/aimRotation` | Quaternion | `/input/aim/pose`<br/>`<MetaAimHand>/deviceRotation` |
| Aim Is Tracked | `<XRHandDevice>/aimIsTracked` | Boolean | `/input/aim/pose`<br/>`<MetaAimHand>/isTracked` |
| Aim Tracking State | `<XRHandDevice>/aimTrackingState` | Integer ([flags enum](xref:UnityEngine.XR.InputTrackingState)) | `/input/aim/pose`<br/>`<MetaAimHand>/trackingState` |

#### Gesture values {#gesture-values}

These values represent three groups of action inputs from a hand interaction profile, specifically [Grasp action](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_grasp_action), [Pinch action](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_pinch_action), and [Aim activate action](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#_aim_activate_action).

|**Data**|**Binding Path**|**Type**|**OpenXR Path**|
|---|---|---|---|
| The extent to which a user is making a fist | `<XRHandDevice>/graspValue` | Float<br/>0 to 1 | `/input/grasp_ext/value` |
| Is the user making a fist | `<XRHandDevice>/graspFirm` | Boolean | `/input/grasp_ext/value` |
| Precondition to making a fist | `<XRHandDevice>/graspReady` | Boolean | `/input/grasp_ext/ready_ext` |
| The extent to which a user is pinching | `<XRHandDevice>/pinchValue` | Float<br/>0 to 1 | `/input/pinch_ext/value` |
| Is the user pinching | `<XRHandDevice>/pinchTouched` | Boolean | `/input/pinch_ext/value` |
| Precondition to making a pinch | `<XRHandDevice>/pinchReady` | Boolean | `/input/pinch_ext/ready_ext` |
| The extent to which a user is aim pinching | `<XRHandDevice>/aimActivateValue` | Float<br/>0 to 1 | `/input/aim_activate_ext/value` |
| Is the user aim pinching | `<XRHandDevice>/aimActivated` | Boolean | `/input/aim_activate_ext/value` |
| Precondition to making an aim pinch | `<XRHandDevice>/aimActivateReady` | Boolean | `/input/aim_activate_ext/ready_ext` |

> [!NOTE]
> These values are not available when using [Legacy](#legacy) mode. Those nine binding paths (`graspValue`, `graspFirm`, etc.) will always remain default values.

### Microgestures {#microgestures}

These values represent thumb microgestures, such as a thumb swipe or a thumb tap. The device supplies these values in both Legacy and Common Gestures modes. On OpenXR, you must also enable the [Hand Interaction Profile](xref:openxr-hand-interaction-profile) and the **Hand Interaction Profile Microgestures** OpenXR features.

Each microgesture value is active when the runtime recognizes the gesture. To respond to each swipe or tap, use the `performed` callback or `WasPerformedThisFrame` on an input action bound to the control, rather than polling the value.

|**Data**|**Binding Path**|**Type**|**OpenXR Path**|
|---|---|---|---|
| The direction of a thumb swipe | `<XRHandDevice>/microgestureSwipe` | Vector2 | `/input/swipe_left_meta/click`<br/>`/input/swipe_right_meta/click`<br/>`/input/swipe_forward_meta/click`<br/>`/input/swipe_backward_meta/click` |
| Whether the runtime recognizes a left thumb swipe | `<XRHandDevice>/microgestureSwipe/swipeLeft` | Boolean | `/input/swipe_left_meta/click` |
| Whether the runtime recognizes a right thumb swipe | `<XRHandDevice>/microgestureSwipe/swipeRight` | Boolean | `/input/swipe_right_meta/click` |
| Whether the runtime recognizes a forward thumb swipe | `<XRHandDevice>/microgestureSwipe/swipeForward` | Boolean | `/input/swipe_forward_meta/click` |
| Whether the runtime recognizes a backward thumb swipe | `<XRHandDevice>/microgestureSwipe/swipeBackward` | Boolean | `/input/swipe_backward_meta/click` |
| Whether the runtime recognizes a thumb tap | `<XRHandDevice>/microgestureTapThumb` | Boolean | `/input/tap_thumb_meta/click` |

> [!NOTE]
> These values are not available when the provider doesn't support microgestures, as indicated by [XRHandSubsystemDescriptor.supportsMicrogestures](xref:UnityEngine.XR.Hands.XRHandSubsystemDescriptor.supportsMicrogestures). Those six binding paths will always remain default values.

## Additional resources

* [Meta Hand Tracking Aim OpenXR feature](xref:xrhands-meta-aim-feature)
* OpenXR [Hand Interaction Profile](xref:openxr-hand-interaction-profile)
* [Microgestures](xref:xrhands-microgestures)
