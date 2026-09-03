---
uid: xrhands-openxr-features
---

# OpenXR features

The XR Hands package implements the following hand-related features for applications using the OpenXR plug-in and runtime:

| Feature | Description |
| :---        | :---               |
| [Hand Tracking Subsystem](xref:xrhands-openxr-hands-feature) | Implements the XRHandSubsystem for OpenXR. You must enable this feature to access any hand tracking data. |
| [Hand Tracking Data Source](xref:xrhands-openxr-data-source-feature) | Allows specifying preferred hand tracking data sources and querying the active source per hand. |
| [Hand Joints Motion Range](xref:xrhands-openxr-motion-range-feature) | Constrains hand joint poses to natural movement or controller-conforming movement. |
| [Meta Hand Tracking Aim](xref:xrhands-meta-aim-feature) | Implements the Meta Aim Hand extension to OpenXR. |
| [Meta Hand Tracking Frequency Hint](xref:xrhands-meta-frequency-hint-feature) | Allows requesting higher frequency hand tracking updates for improved responsiveness. |
| [Meta Hand Tracking Wide Motion Mode](xref:xrhands-meta-wide-motion-feature) | Enable inference-based hand tracking when hands are outside the camera tracking volume. |

The package also lets you augment hand tracking with additional OpenXR extensions:

| Capability | Description |
| :---        | :---               |
| [Hand tracking extensibility](xref:xrhands-openxr-hand-extensibility) | Lets you attach custom extension structures to the OpenXR hand tracker creation and joint location calls. Intended for device vendors and SDK authors. |

The package also provides a helper component for managing the hand subsystem lifecycle:

| Component | Description |
| :---        | :---               |
| [OpenXR Hand Subsystem Manager](xref:xrhands-openxr-subsystem-manager) | A MonoBehaviour that manages the lifecycle of the XRHandSubsystem. Toggle the component's enabled state to start and stop the subsystem. |

## Additional resources

* [OpenXR features](xref:openxr-features)
* [Khronos OpenXR specification](https://registry.khronos.org/OpenXR/specs/1.1/html/xrspec.html)
