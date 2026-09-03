using UnityEngine;
using UnityEngine.XR.Hands;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

#if UNITY_6000_5_OR_NEWER
[NoAutoStaticsCleanup]
#endif
static class TestCommonGestureData
{
    public class PerHandCommonGestureData
    {
        public Pose aimPose { get; set; }
        public float aimActivateValue { get; set; }
        public bool aimActivatedState { get; set; }
        public Pose gripPose { get; set; }
        public float graspValue { get; set; }
        public bool graspFirmState { get; set; }
        public Pose pinchPose { get; set; }
        public float pinchValue { get; set; }
        public bool pinchTouchedState { get; set; }
        public Pose pokePose { get; set; }
        public bool aimPoseIsTracked { get; set; }
        public bool gripPoseIsTracked { get; set; }
        public bool pinchPoseIsTracked { get; set; }
        public bool pokePoseIsTracked { get; set; }
    }

    public static PerHandCommonGestureData leftHand { get; }
    public static PerHandCommonGestureData rightHand { get; }

    public static PerHandCommonGestureData GetCommonGestureData(Handedness handedness)
    {
        if (handedness == Handedness.Right)
            return rightHand;
        if (handedness == Handedness.Left)
            return leftHand;
        throw new System.ArgumentException("Invalid handedness");
    }

    public static XRCommonHandGesturesState GetCommonGesturesState(Handedness handedness, TestHandProvider.CommonGestureBehavior commonGestureBehavior = TestHandProvider.CommonGestureBehavior.Extended)
    {
        if (commonGestureBehavior == TestHandProvider.CommonGestureBehavior.Disabled)
        {
            return default;
        }

        var gestureData = GetCommonGestureData(handedness);
        var commonGestures = new XRCommonHandGesturesState
        {
            handedness = handedness,
            flags =
                XRCommonHandGesturesFlags.IsAimPoseValid |
                XRCommonHandGesturesFlags.IsGripPoseValid |
                XRCommonHandGesturesFlags.IsPinchPoseValid |
                XRCommonHandGesturesFlags.IsPokePoseValid |
                XRCommonHandGesturesFlags.HasExplicitIsTracked,
            aimPoseInternal = gestureData.aimPose,
            gripPoseInternal = gestureData.gripPose,
            pinchPoseInternal = gestureData.pinchPose,
            pokePoseInternal = gestureData.pokePose,
        };

        if (gestureData.aimPoseIsTracked)
            commonGestures.flags |= XRCommonHandGesturesFlags.IsAimPoseTracked;
        if (gestureData.gripPoseIsTracked)
            commonGestures.flags |= XRCommonHandGesturesFlags.IsGripPoseTracked;
        if (gestureData.pinchPoseIsTracked)
            commonGestures.flags |= XRCommonHandGesturesFlags.IsPinchPoseTracked;
        if (gestureData.pokePoseIsTracked)
            commonGestures.flags |= XRCommonHandGesturesFlags.IsPokePoseTracked;

        if (commonGestureBehavior.AreExtensionCommonGesturesEnabled())
        {
            commonGestures.flags |=
                XRCommonHandGesturesFlags.IsAimActivateValueValid |
                XRCommonHandGesturesFlags.IsGraspValueValid |
                XRCommonHandGesturesFlags.IsPinchValueValid |
                XRCommonHandGesturesFlags.IsAimActivatedStateValid |
                XRCommonHandGesturesFlags.IsGraspFirmStateValid |
                XRCommonHandGesturesFlags.IsPinchTouchedStateValid;
            commonGestures.aimActivateValueInternal = gestureData.aimActivateValue;
            commonGestures.graspValueInternal = gestureData.graspValue;
            commonGestures.pinchValueInternal = gestureData.pinchValue;
            commonGestures.isAimActivatedInternal = gestureData.aimActivatedState;
            commonGestures.isGraspFirmInternal = gestureData.graspFirmState;
            commonGestures.isPinchTouchedInternal = gestureData.pinchTouchedState;
        }

        return commonGestures;
    }

    static TestCommonGestureData()
    {
        leftHand = new PerHandCommonGestureData
        {
            aimPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            aimActivateValue = 0.5f,
            aimActivatedState = false,

            gripPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            graspValue = 0.5f,
            graspFirmState = false,

            pinchPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            pinchValue = 0.5f,
            pinchTouchedState = false,

            pokePose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),

            aimPoseIsTracked = false,
            gripPoseIsTracked = true,
            pinchPoseIsTracked = false,
            pokePoseIsTracked = true,
        };

        rightHand = new PerHandCommonGestureData
        {
            aimPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            aimActivateValue = 1.0f,
            aimActivatedState = true,

            gripPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            graspValue = 1.0f,
            graspFirmState = true,

            pinchPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            pinchValue = 1.0f,
            pinchTouchedState = true,

            pokePose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),

            aimPoseIsTracked = true,
            gripPoseIsTracked = false,
            pinchPoseIsTracked = true,
            pokePoseIsTracked = false,
        };
    }
}
