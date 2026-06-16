#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;

namespace UnityEngine.XR.Hands.Tests.OpenXR.NativeInterop
{
    struct TooSmallStruct
    {
        public int value;
    }

    // Large enough to hold an XrBaseInStructure header, but default-constructed
    // so XrStructureType is Unknown (0) at offset 0.
    unsafe struct UninitializedHeaderStruct
    {
        public uint field0;
        public void* field1;
        public uint field2;
    }

    [TestFixture]
    unsafe class XrStructureNodeTests
    {
        const uint k_DefaultIntValue = 0x31415;
        const float k_DefaultFloatValue = 2.718f;
        const uint k_AltIntValue = 0x27182;
        const float k_AltFloatValue = 1.618f;
        const uint k_UpdatedIntValue = 0x16180;
        const float k_UpdatedFloatValue = 99.0f;

        XrMockFeatureAInfoEXT CreateMockA(
            uint intValue = k_DefaultIntValue,
            float floatValue = k_DefaultFloatValue)
            => new(intValue, floatValue);

        [Test]
        public void Constructor_ThrowsForStructSmallerThanHeader()
        {
            Assert.Throws<ArgumentException>(() =>
                new XrStructureNode<TooSmallStruct>(new TooSmallStruct { value = 1 }));
        }

        [Test]
        public void Constructor_ThrowsForUninitializedStructureType()
        {
            Assert.Throws<ArgumentException>(() =>
                new XrStructureNode<UninitializedHeaderStruct>(default));
        }

        [Test]
        public void Constructor_DoesNotThrowForValidStruct()
        {
            Assert.DoesNotThrow(() =>
            {
                var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());
                node.Dispose();
            });
        }

        [Test]
        public void Constructor_StoresDataAndProvidesValidBasePointer()
        {
            var expected = CreateMockA();
            using var node = new XrStructureNode<XrMockFeatureAInfoEXT>(expected);

            var basePtr = node.GetAsXrBaseInStructure();
            Assert.That((IntPtr)basePtr, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(basePtr->type, Is.EqualTo(MockStructureTypes.MockFeatureA));
        }

        [Test]
        public void TryGetData_ReturnsStoredValues()
        {
            var expected = CreateMockA(k_AltIntValue, k_AltFloatValue);
            using var node = new XrStructureNode<XrMockFeatureAInfoEXT>(expected);

            Assert.That(node.TryGetData(out var actual), Is.True);
            Assert.That(actual.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actual.floatValue, Is.EqualTo(expected.floatValue));
        }

        [Test]
        public void TrySetData_MatchingType_OverwritesValuesAtSameAddress()
        {
            using var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());

            var expectedAddress = (IntPtr)node.GetAsXrBaseInStructure();
            var expected = CreateMockA(k_UpdatedIntValue, k_UpdatedFloatValue);

            Assert.That(node.TrySetData(expected), Is.True);

            Assert.That((IntPtr)node.GetAsXrBaseInStructure(), Is.EqualTo(expectedAddress));
            Assert.That(node.TryGetData(out var actual), Is.True);
            Assert.That(actual.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actual.floatValue, Is.EqualTo(expected.floatValue));
        }

        [Test]
        public void TrySetData_DefaultStructWithUnknownType_ReturnsFalse()
        {
            using var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());

            // default(XrMockFeatureAInfoEXT) has XrStructureType.Unknown (0) at offset 0
            Assert.That(node.TrySetData(default(XrMockFeatureAInfoEXT)), Is.False);

            // Original data unchanged
            Assert.That(node.TryGetData(out var actual), Is.True);
            Assert.That(actual.intValue, Is.EqualTo(k_DefaultIntValue));
        }

        [Test]
        public void AfterDispose_AllAccessThrowsOrReturnsNull()
        {
            var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());
            node.Dispose();

            Assert.DoesNotThrow(() => node.Dispose());
            Assert.That((IntPtr)node.GetAsXrBaseInStructure(), Is.EqualTo(IntPtr.Zero));
            Assert.Throws<ObjectDisposedException>(() => node.TryGetData(out _));
            Assert.Throws<ObjectDisposedException>(() => node.TrySetData(CreateMockA(k_UpdatedIntValue, k_UpdatedFloatValue)));
        }

        [Test]
        public void FinalizerImpl_WithActiveResources_LogsError()
        {
            var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());

            LogAssert.Expect(LogType.Error,
                new System.Text.RegularExpressions.Regex("XrStructureNode.*was not disposed"));
            node.FinalizerImpl();

            node.Dispose();
        }

        [Test]
        public void FinalizerImpl_AfterDispose_DoesNotLogError()
        {
            var node = new XrStructureNode<XrMockFeatureAInfoEXT>(CreateMockA());
            node.Dispose();

            node.FinalizerImpl();
        }
    }
}
#endif
