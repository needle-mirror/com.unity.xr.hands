using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.XR.Hands
{
    static class XRHandDeviceUtility
    {
        internal static unsafe bool TryExecuteCommand(InputDeviceCommand* commandPtr, out long result)
        {
            // This is a utility method called by MetaAimHand and XRHandDevice
            // since both devices share the same command handling.
            // This replicates the logic in XRToISXDevice::IOCTL (XRInputToISX.cpp).
            // This also shares logic with XRSimulatorUtility in XR Interaction Toolkit.
            var type = commandPtr->type;
            if (type == RequestSyncCommand.Type)
            {
                // The state is maintained by structs in the managed device, so no need for any change
                // when focus is regained. Returning success instructs Input System to not
                // reset the device.
                result = InputDeviceCommand.GenericSuccess;
                return true;
            }

            if (type == QueryCanRunInBackground.Type)
            {
                ((QueryCanRunInBackground*)commandPtr)->canRunInBackground = true;
                result = InputDeviceCommand.GenericSuccess;
                return true;
            }

            result = default;
            return false;
        }
    }
}
