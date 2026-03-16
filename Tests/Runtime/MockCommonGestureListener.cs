using NUnit.Framework;
using UnityEngine.XR.Hands;

class MockCommonGestureListener
{
    public class PerHandMockGestureListener
    {
        public TestHandUtils.MockHandAction<XRCommonHandGestures.AimPoseUpdatedEventArgs> aimPoseUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.AimActivateValueUpdatedEventArgs> aimActivateValueUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.AimActivatedStateUpdatedEventArgs> aimActivatedStateUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.GripPoseUpdatedEventArgs> gripPoseUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.GraspValueUpdatedEventArgs> graspValueUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.GraspFirmStateUpdatedEventArgs> graspFirmStateUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.PinchPoseUpdatedEventArgs> pinchPoseUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.PinchValueUpdatedEventArgs> pinchValueUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.PinchTouchedStateUpdatedEventArgs> pinchTouchedStateUpdated { get; }
        public TestHandUtils.MockHandAction<XRCommonHandGestures.PokePoseUpdatedEventArgs> pokePoseUpdated { get; }

        public PerHandMockGestureListener()
        {
            aimPoseUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.AimPoseUpdatedEventArgs>();
            aimActivateValueUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.AimActivateValueUpdatedEventArgs>();
            aimActivatedStateUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.AimActivatedStateUpdatedEventArgs>();
            gripPoseUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.GripPoseUpdatedEventArgs>();
            graspValueUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.GraspValueUpdatedEventArgs>();
            graspFirmStateUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.GraspFirmStateUpdatedEventArgs>();
            pinchPoseUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.PinchPoseUpdatedEventArgs>();
            pinchValueUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.PinchValueUpdatedEventArgs>();
            pinchTouchedStateUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.PinchTouchedStateUpdatedEventArgs>();
            pokePoseUpdated =
                new TestHandUtils.MockHandAction<XRCommonHandGestures.PokePoseUpdatedEventArgs>();
        }

        public void AssertAllConditions()
        {
            aimPoseUpdated.AssertMockSatisfied();
            aimActivateValueUpdated.AssertMockSatisfied();
            aimActivatedStateUpdated.AssertMockSatisfied();
            gripPoseUpdated.AssertMockSatisfied();
            graspValueUpdated.AssertMockSatisfied();
            graspFirmStateUpdated.AssertMockSatisfied();
            pinchPoseUpdated.AssertMockSatisfied();
            pinchValueUpdated.AssertMockSatisfied();
            pinchTouchedStateUpdated.AssertMockSatisfied();
            pokePoseUpdated.AssertMockSatisfied();
        }

        public void RegisterForCallbacks(XRCommonHandGestures commonGestures)
        {
            commonGestures.aimPoseUpdated += aimPoseUpdated.InvokeMock;
            commonGestures.aimActivateValueUpdated += aimActivateValueUpdated.InvokeMock;
            commonGestures.aimActivatedStateUpdated += aimActivatedStateUpdated.InvokeMock;
            commonGestures.gripPoseUpdated += gripPoseUpdated.InvokeMock;
            commonGestures.graspValueUpdated += graspValueUpdated.InvokeMock;
            commonGestures.graspFirmStateUpdated += graspFirmStateUpdated.InvokeMock;
            commonGestures.pinchPoseUpdated += pinchPoseUpdated.InvokeMock;
            commonGestures.pinchValueUpdated += pinchValueUpdated.InvokeMock;
            commonGestures.pinchTouchedStateUpdated += pinchTouchedStateUpdated.InvokeMock;
            commonGestures.pokePoseUpdated += pokePoseUpdated.InvokeMock;
        }
    }

    public PerHandMockGestureListener leftHandMocks { get; private set; }
    public PerHandMockGestureListener rightHandMocks { get; private set; }

    public MockCommonGestureListener()
    {
        leftHandMocks = new PerHandMockGestureListener();
        rightHandMocks = new PerHandMockGestureListener();
    }

    public void SubscribeToAllGestureUpdates(XRHandSubsystem subsystem)
    {
        leftHandMocks.RegisterForCallbacks(subsystem.leftHandCommonGestures);
        rightHandMocks.RegisterForCallbacks(subsystem.rightHandCommonGestures);
    }

    public void AssertAllConditions()
    {
        leftHandMocks.AssertAllConditions();
        rightHandMocks.AssertAllConditions();
    }
}
