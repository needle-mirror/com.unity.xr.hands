---
uid: xrhands-meta-wide-motion-feature
---

# Meta Hand Tracking Wide Motion Mode OpenXR feature

Enable inference-based hand tracking when hands are outside the camera tracking volume.

The Wide Motion Mode feature enables the `XR_META_hand_tracking_wide_motion_mode2` OpenXR extension. When enabled, the runtime uses inference algorithms (based on prior tracking data, body movement, and additional sensors) to estimate hand poses even when hands are outside the normal camera tracking volume.

When you enable this feature, it automatically configures the [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature) feature to request both **Unobstructed** (optical tracking) and **UnobstructedWideMotion** (inference-based tracking) as preferred data sources. This configuration ensures the following:

- The project uses direct optical tracking when hands are visible to the headset cameras.
- The runtime falls back to wide-motion inference when hands leave the camera field of view.

> [!NOTE]
> Requesting `UnobstructedWideMotion` alone disables optical tracking entirely. This feature always requests both sources to ensure continuous hand tracking.

## Prerequisites

To use the Meta Hand Tracking Wide Motion Mode feature, your project must meet the following requirements:

* Install the [OpenXR package](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.19) `1.19.0` or newer.
* Enable the [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) feature and the [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature) feature. Validation rules alert you if either is missing.

### Android manifest requirements

When you enable this feature, the following entries are automatically added to the Android manifest at build time:

- `com.oculus.permission.BODY_TRACKING`: the permission wide motion mode needs to use body tracking data.
- `com.oculus.software.body_tracking`: declares body tracking as an optional device feature (`required="false"`).

## Enable Wide Motion Mode

To enable the Meta Hand Tracking Wide Motion Mode feature:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Enable **Meta Hand Tracking Wide Motion Mode** in the feature list.

## Query the active data source at runtime

You can check whether the runtime is using optical tracking or wide-motion inference by [accessing the subsystem](xref:xrhands-access-data), then querying the active data source:

```csharp
if (subsystem.TryGetExtendedData<HandTrackingDataSource>(Handedness.Left, out HandTrackingDataSource source))
{
    if (source == HandTrackingDataSource.UnobstructedWideMotion)
        Debug.Log("Left hand is using wide-motion inference");
    else if (source == HandTrackingDataSource.Unobstructed)
        Debug.Log("Left hand is using optical tracking");
}
```

## Tracking behavior

Be aware of the following when using Wide Motion Mode:

- Wide Motion Mode might produce less accurate pose estimates than direct optical tracking, particularly when hands have been out of view for extended periods.
- The runtime transitions back to optical tracking automatically when hands return to the camera tracking volume.

## Additional resources

* [Hand Tracking Data Source feature](xref:xrhands-openxr-data-source-feature)
* [Hand tracking feature](xref:xrhands-openxr-hands-feature)
