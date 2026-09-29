---
uid: xrhands-process-joints
---
# Process joints

If you wish to process or filter joint data before it's returned to the subsystem caller, derive a `class` from `IXRHandProcessor` and register it with `XRHandSubsystem.RegisterProcessor` in a static method decorated with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`. `IXRHandProcessor`s with a lower `callbackOrder` value will be called sooner than this with higher values for their `callbackOrder`.

If you wish to access the joint data before anyone has had a chance to process the joints, you can use `XRHandSubsystem.preprocessJoints`.

> [!NOTE]
> This is an advanced use case. Typically, you should use the `XRHandSubsystem`'s `updatedHands` callback and just consume the data instead of altering it in any way.

Use extension methods in `UnityEngine.XR.Hands.Processing.XRHandProcessingUtility`. The below sample code demonstrates how to set pose data before it's returned to the subsystem.

```csharp
using UnityEngine.XR.Hands.Processing;

public class MyHandProcessor : MonoBehaviour, IXRHandProcessor
{
    public int callbackOrder => 0;

    public void ProcessJoints(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType)
    {
        var leftHand = subsystem.leftHand;
        leftHand.SetRootPose(Pose.identity);
        subsystem.SetCorrespondingHand(leftHand);

        var rightHand = subsystem.rightHand;
        rightHand.SetRootPose(Pose.identity);
        subsystem.SetCorrespondingHand(rightHand);

        var leftJoints = leftHand.GetRawJointArray();
        var rightJoints = rightHand.GetRawJointArray();
        for (int jointIndex = 0; jointIndex < leftJoints.Length; ++jointIndex)
        {
            var leftJoint = leftJoints[jointIndex];
            leftJoint.SetPose(Pose.identity);
            leftJoints[jointIndex] = leftJoint;

            var rightJoint = rightJoints[jointIndex];
            rightJoint.SetPose(Pose.identity);
            rightJoints[jointIndex] = rightJoint;
        }
    }

    void Update()
    {
        if (m_Registered)
            return;

        SubsystemManager.GetSubsystems(s_SubsystemsReuse);
        if (s_SubsystemsReuse.Count == 0)
            return;

        m_Subsystem = s_SubsystemsReuse[0];
        m_Subsystem.RegisterProcessor(this);
        m_Registered = true;
    }

    void OnDisable()
    {
        if (m_Subsystem != null)
        {
            m_Subsystem.UnregisterProcessor(this);
            m_Subsystem = null;
            m_Registered = false;
        }
    }

    bool m_Registered;
    XRHandSubsystem m_Subsystem;

    static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();
}
```

See sample `HandProcessor` script in the `HandVisualizer` sample for a more useful example of how to use this functionality.

## Process unextrapolated joints {#process-unextrapolated-joints}

If the provider also surfaces unextrapolated poses, through the [Meta Hand Tracking Unextrapolated Poses](xref:xrhands-meta-unextrapolated-poses-feature) OpenXR feature for example, retrieve those hands with [XRHandSubsystem.TryGetHand](xref:UnityEngine.XR.Hands.XRHandSubsystem.TryGetHand*) and [XRHandExtrapolation.Unextrapolated](xref:UnityEngine.XR.Hands.XRHandExtrapolation.Unextrapolated), and process them separately. Read and modify the root pose and the joints exactly as in the example above; only how you retrieve the hand differs:

```csharp
public void ProcessJoints(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType)
{
    if (updateType == XRHandSubsystem.UpdateType.Dynamic)
    {
        if (subsystem.TryGetHand(Handedness.Left, XRHandExtrapolation.Unextrapolated, out var leftHand))
        {
            // Your own processor logic

            leftHand.SetRootPose(Pose.identity);
            subsystem.SetCorrespondingHand(leftHand);
        }

        if (subsystem.TryGetHand(Handedness.Right, XRHandExtrapolation.Unextrapolated, out var rightHand))
        {
            // Your own processor logic

            rightHand.SetRootPose(Pose.identity);
            subsystem.SetCorrespondingHand(rightHand);
        }
    }
}
```

The subsystem refreshes the unextrapolated hands during the `Dynamic` update only, before processors run, which is why the previous example checks `updateType`.

`SetCorrespondingHand` writes back to the hand matching the [extrapolationMode](xref:UnityEngine.XR.Hands.XRHand.extrapolationMode) of the hand you pass it, so the two sets of poses never affect each other. Processing the extrapolated hands leaves the unextrapolated poses untouched, and the reverse. Passing a hand that wasn't retrieved from that subsystem in that mode throws an [InvalidOperationException](xref:System.InvalidOperationException).
