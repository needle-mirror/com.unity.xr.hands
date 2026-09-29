---
uid: xrhands-meta-unextrapolated-poses-feature
---
# Meta Hand Tracking Unextrapolated Poses OpenXR feature

Access hand joint poses that the runtime has not extrapolated, along with the time at which they were captured.

Unity OpenXR provides support for the `XR_META_hand_tracking_unextrapolated_poses` extension. By default, the runtime extrapolates hand joint poses forward in time so that rendered hands stay aligned with the user's real hands despite hand-tracking latency. This extension surfaces the poses as they were captured, without that extrapolation, together with the capture time.

This feature makes unextrapolated poses available in addition to the extrapolated poses. The hands you retrieve without specifying an [extrapolation mode](xref:UnityEngine.XR.Hands.XRHandExtrapolation) are unaffected by this feature.

Enable this feature when your application needs the poses as the runtime captured them, together with the time of capture, rather than the runtime's estimate of where the hands are now. For rendering hands, continue to use the extrapolated poses.

## Prerequisites

To use the Meta Hand Tracking Unextrapolated Poses feature, your project must meet the following requirements:

* Install the [OpenXR package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.19) `1.19.0-pre.2` or newer.
* Enable the [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) feature. A validation rule alerts you if it's missing.

## Enable the feature

To enable the Meta Hand Tracking Unextrapolated Poses feature:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Enable **Meta Hand Tracking Unextrapolated Poses** in the feature list.

If the current runtime doesn't support the extension, this package disables the feature and hand tracking continues to operate normally with extrapolated poses only.

## Check that the feature is active

Check the feature's `enabled` property once the hand subsystem is running. By then, `enabled` is `false` if the feature is off in the project settings, or if the runtime doesn't support the extension:

```csharp
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;

var feature = OpenXRSettings.Instance.GetFeature<MetaHandTrackingUnextrapolatedPoses>();
if (feature != null && feature.enabled)
{
    // Unextrapolated poses are available.
}
```

## Access unextrapolated poses

An unextrapolated hand is an ordinary [XRHand](xref:UnityEngine.XR.Hands.XRHand): subscribe to updates, read the root pose, iterate the joints, and check tracking state exactly as described in [Access hand data](xref:xrhands-access-data). The only difference is how you retrieve it: pass [XRHandExtrapolation.Unextrapolated](xref:UnityEngine.XR.Hands.XRHandExtrapolation.Unextrapolated) to [XRHandSubsystem.TryGetHand](xref:UnityEngine.XR.Hands.XRHandSubsystem.TryGetHand*) instead of reading [leftHand](xref:UnityEngine.XR.Hands.XRHandSubsystem.leftHand) or [rightHand](xref:UnityEngine.XR.Hands.XRHandSubsystem.rightHand):

```csharp
void OnUpdatedHands(XRHandSubsystem subsystem,
    XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
    XRHandSubsystem.UpdateType updateType)
{
    if (updateType == XRHandSubsystem.UpdateType.Dynamic)
    {
        if (subsystem.TryGetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated, out XRHand hand))
        {
            // Use hand as you would subsystem.leftHand
        }
    }
}
```

#### When to read unextrapolated poses

Unextrapolated poses are refreshed once per frame, during the `Dynamic` update, which is why the example checks `updateType`. A `BeforeRender` callback sees the same values a second time.

#### When the call fails:

`TryGetHand` returns `false` when the current provider can't surface unextrapolated poses. Treat that as the result of an individual call, not as a capability check. Determine support from the feature's `enabled` property, as shown in [Check that the feature is active](#check-that-the-feature-is-active). [XRHandSubsystem.GetHand](xref:UnityEngine.XR.Hands.XRHandSubsystem.GetHand*) takes the same extrapolation argument but throws an [InvalidOperationException](xref:System.InvalidOperationException) instead of returning `false`.

#### Interpret the tracking data

[updateSuccessFlags](xref:UnityEngine.XR.Hands.XRHandSubsystem.updateSuccessFlags), [trackingAcquired](xref:UnityEngine.XR.Hands.XRHandSubsystem.trackingAcquired), [trackingLost](xref:UnityEngine.XR.Hands.XRHandSubsystem.trackingLost), and [XRHandTrackingEvents](xref:UnityEngine.XR.Hands.XRHandTrackingEvents) report tracking for the extrapolated hands only. For an unextrapolated hand, check its [isTracked](xref:UnityEngine.XR.Hands.XRHand.isTracked) property and its joints' [trackingState](xref:UnityEngine.XR.Hands.XRHandJoint.trackingState) instead.

[XRHand.extrapolationMode](xref:UnityEngine.XR.Hands.XRHand.extrapolationMode) tells you which set of poses a hand holds.

## Get the capture time

Call [XRHand.TryGetTimestamp](xref:UnityEngine.XR.Hands.XRHand.TryGetTimestamp*) to get the time at which the poses were captured:

```csharp
if (hand.TryGetTimestamp(out long timestamp))
    Debug.Log($"Poses captured at {timestamp}");
```

The timestamp is the raw `XrTime` value reported by the OpenXR runtime, on that runtime's clock. It isn't a Unity time value, so only compare it against other `XrTime` from the same clock. The capture time is available only on hands retrieved with `XRHandExtrapolation.Unextrapolated`, so `TryGetTimestamp` returns `false` for extrapolated hands.

When a hand isn't tracked, it keeps the capture time of its last successfully tracked poses.

## Important considerations

Be aware of the following when using unextrapolated poses:

* These poses lag behind the user's real hands by the hand-tracking latency of the device. Keep driving hand visuals from the extrapolated hands, `leftHand` and `rightHand`, which the runtime extrapolates to compensate for that latency.
* An existing joint processor leaves these poses untouched. Refer to [Process unextrapolated joints](xref:xrhands-process-joints#process-unextrapolated-joints) to process them as well.
* Other hand tracking features, such as [Hand Joints Motion Range](xref:xrhands-openxr-motion-range-feature) and [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature), apply only to extrapolated hands.

> [!WARNING]
> The runtime reports incorrect joint velocities for unextrapolated poses. Read [XRHandJoint.TryGetLinearVelocity](xref:UnityEngine.XR.Hands.XRHandJoint.TryGetLinearVelocity*) and [XRHandJoint.TryGetAngularVelocity](xref:UnityEngine.XR.Hands.XRHandJoint.TryGetAngularVelocity*) from the extrapolated hands instead, or derive the velocities you need from successive unextrapolated poses and their capture times.

## Additional resources

* [Hand Tracking Subsystem feature](xref:xrhands-openxr-hands-feature)
* [Access hand data](xref:xrhands-access-data)
* [XRHandExtrapolation API](xref:UnityEngine.XR.Hands.XRHandExtrapolation)
* [MetaHandTrackingUnextrapolatedPoses API](xref:UnityEngine.XR.Hands.OpenXR.MetaHandTrackingUnextrapolatedPoses)
* [OpenXR Specification: XR_META_hand_tracking_unextrapolated_poses](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html#XR_META_hand_tracking_unextrapolated_poses)
