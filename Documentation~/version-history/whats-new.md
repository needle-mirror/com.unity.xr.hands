---
uid: xrhands-whats-new
---
# What's new in version 1.0

Summary of changes in XR Hands package version 1.0.

The main updates in this release include:

**Added**

Added subsystem for cross-platform hand-tracking and accompanying types.

Types you might care about as a user:
- `XRHandSubsystemDescriptor`, which you can retrieve a `List` of with `SubsystemManager.GetSubsystemDescriptors`.
- `XRHandSubsystem`, which can be created with a call to `Create` on the above descriptor type.
- `XRHand`, which you can retrieve from `XRHandSubsystem` with its `leftHand` and `rightHand` properties. These contain joints and `rootPose` data, as well as its `Handedness`.
- `XRHandJoint`, which you can retrieve from each `XRHand` using `GetJoint` to query for joint tracking state, pose, radius, and velocity data on a per-joint basis.
- `XRHandJointIDUtility`, which contains extension methods for certain `enum`s listed below and also houses `FromIndex`, which you can use when looping over an array to get the corresponding `XRHandJointID` (useful when calling `XRHand.GetJoint`).

As well as these `enum`s:
- `XRHandJointTrackingState`, a flags-`enum` used to denote which fields are valid and can be retrieved via their `TryGet...` methods on `XRHandJoint`.
- `XRHandJointID`, used to identify each joint, and required when accessing joint data using `XRHand.GetJoint`. If looping over an array of joint data, use `XRHandJointIDUtility.FromIndex` to convert your index to `XRHandJointID`, which is required for `XRHand.GetJoint`.
- `Handedness`, used to identify which hand is referred to by an `XRHand` using its `handedness` property.
- `XRHandFingerID`, not used anywhere else in the API surfaced in this package besides its extension methods in `XRHandJointIDUtility`: `GetFrontJointID` and `GetBackJointID`, which together provider an inclusive range for `XRHandJointID`s spanned by the finger represented by `XRHandFingerID`.

The OpenXR package must be installed and in use for these to work:
- Added OpenXR support through `HandTracking` and `OpenXRHandProvider` types.
- Added support for Meta Hand Tracking Aim extension in OpenXR through `MetaHandTrackingAim`.

Additional types you may need to interact with if writing a provider (not a common use case):
- `XRHandSubsystemProvider`, which the subsystem asks for data whenever its `TryUpdateHands` is called (built-in Unity setup calls this each frame) and is also queried when the subsystem and provider are created for which common joints are in the provider's layout using `GetHandLayout`.
- `XRHandProviderUtility`, which providers should call into using `CreateJoint` during the `TryUpdateHands` per-frame call to fill out the left- and right-hand joint arrays. This same type also has a nested `SubsystemUpdater` type to be used for automatically updating the subsystem each frame. Users can respond to updates by subscribing to the subsystem's `updatedHands` callback.

Also added a visualizer sample, which demonstrates drawing using both meshes and per-joint prefabs. You can add this sample to your project through the Samples tab of this package's view in the Package Manager window. This sample has a mesh and script that assume OpenXR layout, so it is recommended you either use that plug-in or another one that conforms to the OpenXR hand joint layout.

For a full list of changes and updates in this package, see the [XR Hands package changelog](xref:xrhands-changelog).
