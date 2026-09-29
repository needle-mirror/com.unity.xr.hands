#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.OpenXR;

/// <summary>
/// Samples demonstrating how to query and update the hand joints motion range
/// at runtime through the <see cref="XRHandSubsystem"/> configuration handler API.
/// </summary>
public class HandJointsMotionRangeSample : MonoBehaviour
{
#pragma warning disable CS0649 // Assigned by the consuming application; intentionally unset in this doc-only sample.
    XRHandSubsystem m_Subsystem;
#pragma warning restore CS0649

#region get_hand_joints_motion_range_sample
    void GetMotionRange()
    {
        if (m_Subsystem.TryGetConfiguration<HandJointsMotionRangeConfig>(out HandJointsMotionRangeConfig config))
        {
            Debug.Log($"Left: {config.leftMotionRange}, Right: {config.rightMotionRange}");
        }
    }
#endregion

#region update_hand_joints_motion_range_sample
    void UpdateMotionRange()
    {
        HandJointsMotionRangeConfig newConfig = new HandJointsMotionRangeConfig
        {
            leftMotionRange = HandJointsMotionRange.ConformingToController,
            rightMotionRange = HandJointsMotionRange.ConformingToController,
        };

        if (m_Subsystem.TryUpdateConfiguration(newConfig))
        {
            Debug.Log("Motion range updated successfully.");
        }
    }
#endregion
}
#endif // UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
