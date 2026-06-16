using NUnit.Framework;
using UnityEngine.XR.Hands;

namespace UnityEditor.XR.Hands.Tests
{
    class XRHandSubsystemHandlerTests
    {
        class MockDataHandler : IXRHandExtendedDataReadHandler<int>
        {
            public int value;
            public bool active = true;

            public bool TryGetData(Handedness handedness, out int data)
            {
                data = value;
                return active;
            }
        }

        class MockConfigHandler : IXRHandConfigurationHandler<int>
        {
            public int config;
            public bool updated;

            public bool TryGetConfiguration(out int config)
            {
                config = this.config;
                return true;
            }

            public bool TryUpdateConfiguration(int config)
            {
                this.config = config;
                updated = true;
                return true;
            }
        }

        XRHandSubsystem m_Subsystem;

        [SetUp]
        public void SetUp()
        {
            m_Subsystem = new XRHandSubsystem();
        }

        [Test]
        public void TryGetExtendedData_NoHandler_ReturnsFalse()
        {
            Assert.IsFalse(m_Subsystem.TryGetExtendedData<int>(Handedness.Left, out _));
        }

        [Test]
        public void TryGetConfiguration_NoHandler_ReturnsFalse()
        {
            Assert.IsFalse(m_Subsystem.TryGetConfiguration<int>(out _));
        }

        [Test]
        public void TryUpdateConfiguration_NoHandler_ReturnsFalse()
        {
            Assert.IsFalse(m_Subsystem.TryUpdateConfiguration(42));
        }

        [Test]
        public void RegisterAndTryGetExtendedData_ReturnsHandlerData()
        {
            var handler = new MockDataHandler { value = 7 };
            m_Subsystem.RegisterHandExtendedDataHandler(handler);

            Assert.IsTrue(m_Subsystem.TryGetExtendedData<int>(Handedness.Left, out var data));
            Assert.AreEqual(7, data);
        }

        [Test]
        public void RegisterAndTryGetConfiguration_ReturnsHandlerConfig()
        {
            var handler = new MockConfigHandler { config = 99 };
            m_Subsystem.RegisterConfigurationHandler(handler);

            Assert.IsTrue(m_Subsystem.TryGetConfiguration<int>(out var config));
            Assert.AreEqual(99, config);
        }

        [Test]
        public void RegisterAndTryUpdateConfiguration_DispatchesToHandler()
        {
            var handler = new MockConfigHandler();
            m_Subsystem.RegisterConfigurationHandler(handler);

            Assert.IsTrue(m_Subsystem.TryUpdateConfiguration(42));
            Assert.IsTrue(handler.updated);
            Assert.AreEqual(42, handler.config);
        }

        [Test]
        public void UnregisterExtendedDataHandler_SubsequentGetReturnsFalse()
        {
            var handler = new MockDataHandler { value = 7 };
            m_Subsystem.RegisterHandExtendedDataHandler(handler);
            m_Subsystem.UnregisterHandExtendedDataHandler<int>();

            Assert.IsFalse(m_Subsystem.TryGetExtendedData<int>(Handedness.Left, out _));
        }

        [Test]
        public void UnregisterConfigurationHandler_SubsequentGetReturnsFalse()
        {
            var handler = new MockConfigHandler { config = 99 };
            m_Subsystem.RegisterConfigurationHandler(handler);
            m_Subsystem.UnregisterConfigurationHandler<int>();

            Assert.IsFalse(m_Subsystem.TryGetConfiguration<int>(out _));
        }
    }
}
