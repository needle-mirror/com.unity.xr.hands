#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.Tests.OpenXR.NativeInterop
{
    /// <summary>
    /// Mock <see cref="XrStructureType"/> constants for testing. Uses high
    /// values to avoid conflicts with real OpenXR types.
    /// </summary>
    static class MockStructureTypes
    {
        public const XrStructureType MockFeatureA = (XrStructureType)0x7FFFFF01;
        public const XrStructureType MockFeatureB = (XrStructureType)0x7FFFFF02;
    }

    /// <summary>
    /// Mock OpenXR input structure for testing. Follows the standard
    /// <c>type</c> / <c>next</c> header layout with two payload fields.
    /// </summary>
    /// <remarks>
    /// > [!WARNING]
    /// > Don't initialize this struct with the default parameterless constructor.
    /// > Use a constructor with parameters to ensure that <see cref="type"/> is correctly initialized
    /// > to <see cref="MockStructureTypes.MockFeatureA"/>.
    /// </remarks>
    readonly unsafe struct XrMockFeatureAInfoEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType"/> of this struct: <see cref="MockStructureTypes.MockFeatureA"/>.
        /// </summary>
        public XrStructureType type { get; }

        /// <summary>
        /// <c>null</c> or a pointer to the next structure in a structure chain.
        /// </summary>
        public void* next { get; }

        /// <summary>
        /// An arbitrary integer payload for test validation.
        /// </summary>
        public uint intValue { get; }

        /// <summary>
        /// An arbitrary floating-point payload for test validation.
        /// </summary>
        public float floatValue { get; }

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="next">The next pointer.</param>
        /// <param name="intValue">An arbitrary integer payload.</param>
        /// <param name="floatValue">An arbitrary floating-point payload.</param>
        public XrMockFeatureAInfoEXT(void* next, uint intValue, float floatValue)
        {
            type = MockStructureTypes.MockFeatureA;
            this.next = next;
            this.intValue = intValue;
            this.floatValue = floatValue;
        }

        /// <summary>
        /// Construct an instance with a <c>null</c> next pointer.
        /// </summary>
        /// <param name="intValue">An arbitrary integer payload.</param>
        /// <param name="floatValue">An arbitrary floating-point payload.</param>
        public XrMockFeatureAInfoEXT(uint intValue, float floatValue)
            : this(null, intValue, floatValue) { }
    }

    /// <summary>
    /// Second mock OpenXR input structure with a count/pointer array pair,
    /// used to test chains with heterogeneous node types.
    /// </summary>
    /// <remarks>
    /// > [!WARNING]
    /// > Don't initialize this struct with the default parameterless constructor.
    /// > Use a constructor with parameters to ensure that <see cref="type"/> is correctly initialized
    /// > to <see cref="MockStructureTypes.MockFeatureB"/>.
    /// </remarks>
    readonly unsafe struct XrMockFeatureBInfoEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType"/> of this struct: <see cref="MockStructureTypes.MockFeatureB"/>.
        /// </summary>
        public XrStructureType type { get; }

        /// <summary>
        /// <c>null</c> or a pointer to the next structure in a structure chain.
        /// </summary>
        public void* next { get; }

        /// <summary>
        /// The number of elements in <see cref="values"/>.
        /// </summary>
        public uint valueCount { get; }

        /// <summary>
        /// Pointer to an array of floating-point values.
        /// </summary>
        public float* values { get; }

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="next">The next pointer.</param>
        /// <param name="valueCount">The number of elements in <paramref name="values"/>.</param>
        /// <param name="values">Pointer to an array of floating-point values.</param>
        public XrMockFeatureBInfoEXT(void* next, uint valueCount, float* values)
        {
            type = MockStructureTypes.MockFeatureB;
            this.next = next;
            this.valueCount = valueCount;
            this.values = values;
        }

        /// <summary>
        /// Construct an instance with a <c>null</c> next pointer.
        /// </summary>
        /// <param name="valueCount">The number of elements in <paramref name="values"/>.</param>
        /// <param name="values">Pointer to an array of floating-point values.</param>
        public XrMockFeatureBInfoEXT(uint valueCount, float* values)
            : this(null, valueCount, values) { }
    }
}
#endif
