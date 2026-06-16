#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.Tests.OpenXR.NativeInterop
{
    [TestFixture]
    unsafe class XrStructureChainTests
    {
        // Default test values for MockFeatureA — hex representations of math constants
        const uint k_MockA_UintValue = 0x31415;
        const float k_MockA_FloatValue = 2.718f;

        // Alternate values for update/duplicate scenarios
        const uint k_MockA_AltUintValue = 0x27182;
        const float k_MockA_AltFloatValue = 1.618f;

        // Update values
        const uint k_MockA_UpdatedUintValue = 0x16180;
        const float k_MockA_UpdatedFloatValue = 3.141f;

        XrMockFeatureAInfoEXT CreateMockA() =>
            new(k_MockA_UintValue, k_MockA_FloatValue);

        [Test]
        public void CreateEmpty_HasDefaultState()
        {
            using var chain = new XrStructureChain();

            Assert.That(chain.count, Is.EqualTo(0));
            Assert.That((IntPtr)chain.GetHeadPointer(), Is.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void TryAddNode_SingleNode_HeadPointerIsValid()
        {
            using var chain = new XrStructureChain();
            var expected = CreateMockA();

            Assert.That(chain.TryAddNode(expected), Is.True);

            Assert.That(chain.count, Is.EqualTo(1));
            var head = chain.GetHeadPointer();
            Assert.That((IntPtr)head, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(head->type, Is.EqualTo(MockStructureTypes.MockFeatureA));
            Assert.That((IntPtr)head->next, Is.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void TryAddNode_MultipleNodes_NextPointersFormChain()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            var expectedValues = stackalloc float[] { 1.0f, 2.0f };
            chain.TryAddNode(new XrMockFeatureBInfoEXT(2, expectedValues));

            Assert.That(chain.count, Is.EqualTo(2));

            var first = chain.GetHeadPointer();
            Assert.That(first->type, Is.EqualTo(MockStructureTypes.MockFeatureA));

            var second = (XrBaseInStructure*)first->next;
            Assert.That((IntPtr)second, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(second->type, Is.EqualTo(MockStructureTypes.MockFeatureB));
            Assert.That((IntPtr)second->next, Is.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void TryAddNode_DuplicateType_ReturnsFalse()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            Assert.That(
                chain.TryAddNode(new XrMockFeatureAInfoEXT(k_MockA_AltUintValue, k_MockA_AltFloatValue)),
                Is.False);

            Assert.That(chain.count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_ResetsToEmptyAndCanReuse()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            chain.Clear();

            Assert.That(chain.count, Is.EqualTo(0));
            Assert.That((IntPtr)chain.GetHeadPointer(), Is.EqualTo(IntPtr.Zero));

            // Verify the chain is reusable after clear
            var expected = new XrMockFeatureAInfoEXT(k_MockA_AltUintValue, k_MockA_AltFloatValue);
            chain.TryAddNode(expected);

            Assert.That(chain.count, Is.EqualTo(1));
            Assert.That(chain.GetHeadPointer()->type, Is.EqualTo(MockStructureTypes.MockFeatureA));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            chain.Dispose();
            Assert.DoesNotThrow(() => chain.Dispose());
        }

        [Test]
        public void TryGetNode_EmptyChain_ReturnsFalse()
        {
            using var chain = new XrStructureChain();

            Assert.That(
                chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out _),
                Is.False);
        }

        [Test]
        public void TryGetNode_MatchingType_ReturnsData()
        {
            using var chain = new XrStructureChain();
            var expected = CreateMockA();
            chain.TryAddNode(expected);

            Assert.That(
                chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out var actual),
                Is.True);
            Assert.That(actual.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actual.floatValue, Is.EqualTo(expected.floatValue));
        }

        [Test]
        public void TryGetNode_NonExistentType_ReturnsFalse()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            Assert.That(
                chain.TryGetNode<XrMockFeatureBInfoEXT>(MockStructureTypes.MockFeatureB, out _),
                Is.False);
        }

        [Test]
        public void TryGetNode_MultiNodeChain_ReturnsCorrectNode()
        {
            using var chain = new XrStructureChain();
            var expectedA = CreateMockA();
            chain.TryAddNode(expectedA);

            var values = stackalloc float[] { 1.0f, 2.0f };
            var expectedB = new XrMockFeatureBInfoEXT(2, values);
            chain.TryAddNode(expectedB);

            Assert.That(
                chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out var actualA),
                Is.True);
            Assert.That(actualA.intValue, Is.EqualTo(expectedA.intValue));
            Assert.That(actualA.floatValue, Is.EqualTo(expectedA.floatValue));

            Assert.That(
                chain.TryGetNode<XrMockFeatureBInfoEXT>(MockStructureTypes.MockFeatureB, out var actualB),
                Is.True);
            Assert.That(actualB.valueCount, Is.EqualTo(expectedB.valueCount));
            Assert.That((IntPtr)actualB.values, Is.EqualTo((IntPtr)expectedB.values));
        }

        [Test]
        public void TryUpdateNode_EmptyChain_ReturnsFalse()
        {
            using var chain = new XrStructureChain();

            Assert.That(chain.TryUpdateNode(CreateMockA()), Is.False);
        }

        [Test]
        public void TryUpdateNode_SingleNode_UpdatesDataAndPreservesPointer()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            var expectedAddress = (IntPtr)chain.GetHeadPointer();
            var expected = new XrMockFeatureAInfoEXT(k_MockA_UpdatedUintValue, k_MockA_UpdatedFloatValue);

            Assert.That(chain.TryUpdateNode(expected), Is.True);

            var head = chain.GetHeadPointer();
            Assert.That((IntPtr)head, Is.EqualTo(expectedAddress));
            Assert.That((IntPtr)head->next, Is.EqualTo(IntPtr.Zero));

            chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out var actual);
            Assert.That(actual.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actual.floatValue, Is.EqualTo(expected.floatValue));
        }

        [Test]
        public void TryUpdateNode_MiddleNode_PreservesChainLinkage()
        {
            using var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            var values = stackalloc float[] { 1.0f, 2.0f };
            var expectedB = new XrMockFeatureBInfoEXT(2, values);
            chain.TryAddNode(expectedB);

            var expectedHeadAddr = (IntPtr)chain.GetHeadPointer();
            var expectedNextAddr = (IntPtr)chain.GetHeadPointer()->next;

            var expected = new XrMockFeatureAInfoEXT(k_MockA_UpdatedUintValue, k_MockA_UpdatedFloatValue);
            Assert.That(chain.TryUpdateNode(expected), Is.True);

            var head = chain.GetHeadPointer();
            Assert.That((IntPtr)head, Is.EqualTo(expectedHeadAddr));
            Assert.That((IntPtr)head->next, Is.EqualTo(expectedNextAddr));

            chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out var actualA);
            Assert.That(actualA.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actualA.floatValue, Is.EqualTo(expected.floatValue));

            chain.TryGetNode<XrMockFeatureBInfoEXT>(MockStructureTypes.MockFeatureB, out var actualB);
            Assert.That(actualB.valueCount, Is.EqualTo(expectedB.valueCount));
            Assert.That((IntPtr)actualB.values, Is.EqualTo((IntPtr)expectedB.values));
        }

        [Test]
        public void TryUpdateNode_NonExistentType_ReturnsFalse()
        {
            using var chain = new XrStructureChain();
            var expected = CreateMockA();
            chain.TryAddNode(expected);

            var values = stackalloc float[] { 1.0f };
            Assert.That(chain.TryUpdateNode(new XrMockFeatureBInfoEXT(1, values)), Is.False);

            chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out var actual);
            Assert.That(actual.intValue, Is.EqualTo(expected.intValue));
            Assert.That(actual.floatValue, Is.EqualTo(expected.floatValue));
        }

        [Test]
        public void FinalizerImpl_WithActiveResources_LogsError()
        {
            var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());

            LogAssert.Expect(LogType.Error,
                new System.Text.RegularExpressions.Regex("XrStructureChain was not disposed"));
            chain.FinalizerImpl();

            chain.Dispose();
        }

        [Test]
        public void FinalizerImpl_AfterDispose_DoesNotLogError()
        {
            var chain = new XrStructureChain();
            chain.TryAddNode(CreateMockA());
            chain.Dispose();

            chain.FinalizerImpl();
        }

        [Test]
        public void FinalizerImpl_EmptyChain_DoesNotLogError()
        {
            var chain = new XrStructureChain();
            chain.FinalizerImpl();
            chain.Dispose();
        }

        [Test]
        public void TryAddNode_AfterDispose_ThrowsObjectDisposedException()
        {
            var chain = new XrStructureChain();
            chain.Dispose();

            Assert.Throws<ObjectDisposedException>(() => chain.TryAddNode(CreateMockA()));
        }

        [Test]
        public void TryGetNode_AfterDispose_ThrowsObjectDisposedException()
        {
            var chain = new XrStructureChain();
            chain.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
                chain.TryGetNode<XrMockFeatureAInfoEXT>(MockStructureTypes.MockFeatureA, out _));
        }

        [Test]
        public void TryUpdateNode_AfterDispose_ThrowsObjectDisposedException()
        {
            var chain = new XrStructureChain();
            chain.Dispose();

            Assert.Throws<ObjectDisposedException>(() => chain.TryUpdateNode(CreateMockA()));
        }

        [Test]
        public void GetHeadPointer_AfterDispose_ThrowsObjectDisposedException()
        {
            var chain = new XrStructureChain();
            chain.Dispose();

            Assert.Throws<ObjectDisposedException>(() => chain.GetHeadPointer());
        }

        [Test]
        public void Clear_AfterDispose_ThrowsObjectDisposedException()
        {
            var chain = new XrStructureChain();
            chain.Dispose();

            Assert.Throws<ObjectDisposedException>(() => chain.Clear());
        }
    }
}
#endif
