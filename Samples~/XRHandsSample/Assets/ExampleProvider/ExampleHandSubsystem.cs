using System;
using UnityEngine.XR.Hands.Example.ProviderImplementation;

namespace UnityEngine.XR.Hands.Example
{
    public class ExampleHandSubsystem : XRHandSubsystem
    {
        public ExampleHandSubsystem() => ExampleHandExtensions.subsystem = this;

        public Action<bool> leftFistClosed;
        public Action<bool> rightFistClosed;

        public override UpdateSuccessFlags TryUpdateHands(UpdateType updateType)
        {
            var successFlags = base.TryUpdateHands(updateType);

            var exampleProvider = (provider as ExampleHandProvider);
            exampleProvider.GetEventFireBools(
                out bool isLeftClosed, out bool leftHasEventToFire,
                out bool isRightClosed, out bool rightHasEventToFire);

            // fire events after base class has finished its TryUpdateHands
            // call so that users can query up-to-date joint data during the
            // response to the action call

            if (leftHasEventToFire && leftFistClosed != null)
                leftFistClosed.Invoke(isLeftClosed);

            if (rightHasEventToFire && rightFistClosed != null)
                rightFistClosed.Invoke(isRightClosed);

            return successFlags;
        }

        internal bool IsFistClosed(Handedness handedness)
        {
            var exampleProvider = provider as ExampleHandProvider;
            return exampleProvider.IsFistClosed(handedness);
        }
    }
}
