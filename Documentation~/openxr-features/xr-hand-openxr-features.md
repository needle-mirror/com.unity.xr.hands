---
uid: xrhands-openxr-features
---

# OpenXR features

The XR Hands package implements two hand-related features for applications using the OpenXR plug-in and runtime. These features include:

| Feature | Description |
| :---        | :---               |
| [Hand tracking](xref:xrhands-openxr-hands-feature) | Implements the XRHandSubsystem for OpenXR. You must enable this feature to access any hand tracking data.|
| [Meta Aim Hand](xref:xrhands-meta-aim-feature) | Implements the Meta Aim Hand extension to OpenXR. |

The package also provides a helper component for managing the hand subsystem lifecycle:

| Component | Description |
| :---        | :---               |
| [OpenXR Hand Subsystem Manager](xref:xrhands-openxr-subsystem-manager) | A MonoBehaviour that manages the lifecycle of the XRHandSubsystem. Toggle the component's enabled state to start and stop the subsystem. |

## Additional resources

* [OpenXR features](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html)
* [Khronos OpenXR specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html)
