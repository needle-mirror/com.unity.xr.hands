using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR.Hands;

namespace Unity.XR.Hands.Runtime.Tests
{
    [TestFixture]
    class HandInputDeviceTests : InputTestFixture
    {
        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void MetaAimHandCanRunInBackground()
        {
            var device = InputSystem.AddDevice<MetaAimHand>();

            Assert.That(device, Is.Not.Null);
            Assert.That(device.canRunInBackground, Is.True);

            var command = QueryCanRunInBackground.Create();
            Assert.That(device.ExecuteCommand(ref command), Is.EqualTo(InputDeviceCommand.GenericSuccess));
            Assert.That(command.canRunInBackground, Is.True);
        }

        [Test]
#if !ENABLE_INPUT_SYSTEM
        [Ignore("Layout is only registered with Input System when ENABLE_INPUT_SYSTEM is defined.")]
#endif
        public void XRHandDeviceCanRunInBackground()
        {
            var device = InputSystem.AddDevice<XRHandDevice>();

            Assert.That(device, Is.Not.Null);
            Assert.That(device.canRunInBackground, Is.True);

            var command = QueryCanRunInBackground.Create();
            Assert.That(device.ExecuteCommand(ref command), Is.EqualTo(InputDeviceCommand.GenericSuccess));
            Assert.That(command.canRunInBackground, Is.True);
        }
    }
}
