using System;

namespace UnityEngine.XR.Hands.Gestures
{
    /// <summary>
    /// Configuration values for how to calculate <see cref="XRFingerShape"/>
    /// stored in <see cref="XRFingerShapeConfiguration"/>.
    /// Defines minimum and maximum angles or distances between joints to normalize finger joints into each
    /// <see cref="XRFingerStateType"/>'s <c>0</c> to <c>1</c> value.
    /// </summary>
    [Serializable]
    public struct XRFingerShapeConfigurationState : IEquatable<XRFingerShapeConfigurationState>
    {
        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRFingerShapeConfigurationState"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRFingerShapeConfigurationState"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRFingerShapeConfigurationState other)
        {
            return m_MinimumFullCurlDegrees1 == other.m_MinimumFullCurlDegrees1
                && m_MaximumFullCurlDegrees1 == other.m_MaximumFullCurlDegrees1
                && m_MinimumFullCurlDegrees2 == other.m_MinimumFullCurlDegrees2
                && m_MaximumFullCurlDegrees2 == other.m_MaximumFullCurlDegrees2
                && m_MinimumFullCurlDegrees3 == other.m_MinimumFullCurlDegrees3
                && m_MaximumFullCurlDegrees3 == other.m_MaximumFullCurlDegrees3
                && m_MinimumBaseCurlDegrees == other.m_MinimumBaseCurlDegrees
                && m_MaximumBaseCurlDegrees == other.m_MaximumBaseCurlDegrees
                && m_MinimumTipCurlDegrees1 == other.m_MinimumTipCurlDegrees1
                && m_MaximumTipCurlDegrees1 == other.m_MaximumTipCurlDegrees1
                && m_MinimumTipCurlDegrees2 == other.m_MinimumTipCurlDegrees2
                && m_MaximumTipCurlDegrees2 == other.m_MaximumTipCurlDegrees2
                && m_MinimumPinchDistance == other.m_MinimumPinchDistance
                && m_MaximumPinchDistance == other.m_MaximumPinchDistance
                && m_MinimumSpreadDegrees == other.m_MinimumSpreadDegrees
                && m_MaximumSpreadDegrees == other.m_MaximumSpreadDegrees;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRFingerShapeConfigurationState"/> and <see cref="Equals(XRFingerShapeConfigurationState)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => obj is XRFingerShapeConfigurationState other && Equals(other);

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRFingerShapeConfigurationState"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            int hash = HashCodeUtil.Combine(
                m_MinimumFullCurlDegrees1.GetHashCode(),
                m_MaximumFullCurlDegrees1.GetHashCode(),
                m_MinimumFullCurlDegrees2.GetHashCode(),
                m_MaximumFullCurlDegrees2.GetHashCode(),
                m_MinimumFullCurlDegrees3.GetHashCode(),
                m_MaximumFullCurlDegrees3.GetHashCode());

            hash = HashCodeUtil.Combine(
                hash,
                m_MinimumBaseCurlDegrees.GetHashCode(),
                m_MaximumBaseCurlDegrees.GetHashCode(),
                m_MinimumTipCurlDegrees1.GetHashCode(),
                m_MaximumTipCurlDegrees1.GetHashCode(),
                m_MinimumTipCurlDegrees2.GetHashCode(),
                m_MaximumTipCurlDegrees2.GetHashCode());

            return HashCodeUtil.Combine(
                hash,
                m_MinimumPinchDistance.GetHashCode(),
                m_MaximumPinchDistance.GetHashCode(),
                m_MinimumSpreadDegrees.GetHashCode(),
                m_MaximumSpreadDegrees.GetHashCode());
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRFingerShapeConfigurationState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(XRFingerShapeConfigurationState lhs, XRFingerShapeConfigurationState rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRFingerShapeConfigurationState)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(XRFingerShapeConfigurationState lhs, XRFingerShapeConfigurationState rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// The minimum degrees between vectors from the first extension
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumFullCurlDegrees1
        {
            get => m_MinimumFullCurlDegrees1;
            set => m_MinimumFullCurlDegrees1 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the first extension
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumFullCurlDegrees1
        {
            get => m_MaximumFullCurlDegrees1;
            set => m_MaximumFullCurlDegrees1 = value;
        }

        /// <summary>
        /// The minimum angle between the second extension
        /// joint and its closest neighbors.
        /// </summary>
        public float minimumFullCurlDegrees2
        {
            get => m_MinimumFullCurlDegrees2;
            set => m_MinimumFullCurlDegrees2 = value;
        }

        /// <summary>
        /// The maximum angle between the second extension
        /// joint and its closest neighbors.
        /// </summary>
        public float maximumFullCurlDegrees2
        {
            get => m_MaximumFullCurlDegrees2;
            set => m_MaximumFullCurlDegrees2 = value;
        }

        /// <summary>
        /// The minimum angle between the third extension
        /// joint and its closest neighbors. Ignored on the thumb.
        /// </summary>
        public float minimumFullCurlDegrees3
        {
            get => m_MinimumFullCurlDegrees3;
            set => m_MinimumFullCurlDegrees3 = value;
        }

        /// <summary>
        /// The maximum angle between the third extension
        /// joint and its closest neighbors. Ignored on the thumb.
        /// </summary>
        public float maximumFullCurlDegrees3
        {
            get => m_MaximumFullCurlDegrees3;
            set => m_MaximumFullCurlDegrees3 = value;
        }

        /// <summary>
        /// The minimum angle between the central flex joint and
        /// its closest neighbors. When the angle between those two vectors is
        /// less than or equal to this value, the flex value will be <c>1</c>.
        /// </summary>
        public float minimumBaseCurlDegrees
        {
            get => m_MinimumBaseCurlDegrees;
            set => m_MinimumBaseCurlDegrees = value;
        }

        /// <summary>
        /// The maximum angle between the central flex joint and
        /// its closest neighbors. When the angle between those two vectors is
        /// greater than or equal to this value, the flex value will be <c>0</c>.
        /// </summary>
        public float maximumBaseCurlDegrees
        {
            get => m_MaximumBaseCurlDegrees;
            set => m_MaximumBaseCurlDegrees = value;
        }

        /// <summary>
        /// The minimum degrees between vectors from the first curl
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumTipCurlDegrees1
        {
            get => m_MinimumTipCurlDegrees1;
            set => m_MinimumTipCurlDegrees1 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the first curl
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumTipCurlDegrees1
        {
            get => m_MaximumTipCurlDegrees1;
            set => m_MaximumTipCurlDegrees1 = value;
        }

        /// <summary>
        /// The minimum degrees between vectors from the second curl
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumTipCurlDegrees2
        {
            get => m_MinimumTipCurlDegrees2;
            set => m_MinimumTipCurlDegrees2 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the second curl
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumTipCurlDegrees2
        {
            get => m_MaximumTipCurlDegrees2;
            set => m_MaximumTipCurlDegrees2 = value;
        }

        /// <summary>
        /// The minimum distance between each finger tip and the thumb tip
        /// to calculate pinch values for. Values below or equal to this will
        /// result in a pinch value of <c>1</c>.
        /// </summary>
        public float minimumPinchDistance
        {
            get => m_MinimumPinchDistance;
            set => m_MinimumPinchDistance = value;
        }

        /// <summary>
        /// The maximum distance between each finger tip and the thumb tip
        /// which allows for non-zero pinch values.
        /// </summary>
        public float maximumPinchDistance
        {
            get => m_MaximumPinchDistance;
            set => m_MaximumPinchDistance = value;
        }

        /// <summary>
        /// The minimum degrees for splay between this finger and the next.
        /// Not used for the little finger.
        /// </summary>
        public float minimumSpreadDegrees
        {
            get => m_MinimumSpreadDegrees;
            set => m_MinimumSpreadDegrees = value;
        }

        /// <summary>
        /// The maximum degrees for splay between this finger and the next.
        /// Not used for the little finger.
        /// </summary>
        public float maximumSpreadDegrees
        {
            get => m_MaximumSpreadDegrees;
            set => m_MaximumSpreadDegrees = value;
        }

        [SerializeField]
        float m_MinimumFullCurlDegrees1;

        [SerializeField]
        float m_MaximumFullCurlDegrees1;

        [SerializeField]
        float m_MinimumFullCurlDegrees2;

        [SerializeField]
        float m_MaximumFullCurlDegrees2;

        [SerializeField]
        float m_MinimumFullCurlDegrees3;

        [SerializeField]
        float m_MaximumFullCurlDegrees3;

        [SerializeField]
        float m_MinimumBaseCurlDegrees;

        [SerializeField]
        float m_MaximumBaseCurlDegrees;

        [SerializeField]
        float m_MinimumTipCurlDegrees1;

        [SerializeField]
        float m_MaximumTipCurlDegrees1;

        [SerializeField]
        float m_MinimumTipCurlDegrees2;

        [SerializeField]
        float m_MaximumTipCurlDegrees2;

        [SerializeField]
        float m_MinimumPinchDistance;

        [SerializeField]
        float m_MaximumPinchDistance;

        [SerializeField]
        float m_MinimumSpreadDegrees;

        [SerializeField]
        float m_MaximumSpreadDegrees;
    }
}
