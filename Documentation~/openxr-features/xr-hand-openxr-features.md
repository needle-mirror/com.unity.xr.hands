---
uid: xrhands-openxr-features
---

# OpenXR features

The XR Hands package implements the following hand-related features for applications using the OpenXR plug-in and runtime:

| Feature | Description |
| :---        | :---               |
| [Hand tracking](xref:xrhands-openxr-hands-feature) | Implements the XRHandSubsystem for OpenXR. You must enable this feature to access any hand tracking data.|
| [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature) | Allows specifying preferred hand tracking data sources and querying the active source per hand. |
| [Hand Joints Motion Range](xref:xrhands-openxr-motion-range-feature) | Constrains hand joint poses to natural movement or controller-conforming movement. |
| [Meta Aim Hand](xref:xrhands-meta-aim-feature) | Implements the Meta Aim Hand extension to OpenXR. |

The package also provides a helper component for managing the hand subsystem lifecycle:

| Component | Description |
| :---        | :---               |
| [OpenXR Hand Subsystem Manager](xref:xrhands-openxr-subsystem-manager) | A MonoBehaviour that manages the lifecycle of the XRHandSubsystem. Toggle the component's enabled state to start and stop the subsystem. |

## Additional resources

* [OpenXR features](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html)
* [Khronos OpenXR specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html)
