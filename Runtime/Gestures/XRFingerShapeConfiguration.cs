using System;

namespace UnityEngine.XR.Hands.Gestures
{
    /// <summary>
    /// Configuration values for how to calculate <see cref="XRFingerShape"/>.
    /// Defines minimum and maximum angles or distances between joints to normalize finger joints into each
    /// <see cref="XRFingerShapeType"/>'s <c>0</c> to <c>1</c> value.
    /// </summary>
    [Serializable]
    public class XRFingerShapeConfiguration
    {
        /// <summary>
        /// Creates a <c>XRFingerShapeConfiguration</c> as a copy of
        /// the configuration stored in the supplied <see cref="XRFingerShapeConfigurationState"/>.
        /// </summary>
        /// <param name="state">
        /// The stored state to copy configuration data from.
        /// </param>
        public XRFingerShapeConfiguration(in XRFingerShapeConfigurationState state)
            => m_State = state;

        /// <summary>
        /// Creates a <c>XRFingerShapeConfiguration</c> initialized with a default
        /// <see cref="XRFingerShapeConfigurationState"/>.
        /// </summary>
        public XRFingerShapeConfiguration()
            => m_State = default;

        /// <summary>
        /// The minimum degrees between vectors from the first extension
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumFullCurlDegrees1
        {
            get => m_State.minimumFullCurlDegrees1;
            set => m_State.minimumFullCurlDegrees1 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the first extension
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumFullCurlDegrees1
        {
            get => m_State.maximumFullCurlDegrees1;
            set => m_State.maximumFullCurlDegrees1 = value;
        }

        /// <summary>
        /// The minimum angle between the second extension
        /// joint and its closest neighbors.
        /// </summary>
        public float minimumFullCurlDegrees2
        {
            get => m_State.minimumFullCurlDegrees2;
            set => m_State.minimumFullCurlDegrees2 = value;
        }

        /// <summary>
        /// The maximum angle between the second extension
        /// joint and its closest neighbors.
        /// </summary>
        public float maximumFullCurlDegrees2
        {
            get => m_State.maximumFullCurlDegrees2;
            set => m_State.maximumFullCurlDegrees2 = value;
        }

        /// <summary>
        /// The minimum angle between the third extension
        /// joint and its closest neighbors. Ignored on the thumb.
        /// </summary>
        public float minimumFullCurlDegrees3
        {
            get => m_State.minimumFullCurlDegrees3;
            set => m_State.minimumFullCurlDegrees3 = value;
        }

        /// <summary>
        /// The maximum angle between the third extension
        /// joint and its closest neighbors. Ignored on the thumb.
        /// </summary>
        public float maximumFullCurlDegrees3
        {
            get => m_State.maximumFullCurlDegrees3;
            set => m_State.maximumFullCurlDegrees3 = value;
        }

        /// <summary>
        /// The minimum angle between the central flex joint and
        /// its closest neighbors. When the angle between those two vectors is
        /// less than or equal to this value, the flex value will be <c>1</c>.
        /// </summary>
        public float minimumBaseCurlDegrees
        {
            get => m_State.minimumBaseCurlDegrees;
            set => m_State.minimumBaseCurlDegrees = value;
        }

        /// <summary>
        /// The maximum angle between the central flex joint and
        /// its closest neighbors. When the angle between those two vectors is
        /// greater than or equal to this value, the flex value will be <c>0</c>.
        /// </summary>
        public float maximumBaseCurlDegrees
        {
            get => m_State.maximumBaseCurlDegrees;
            set => m_State.maximumBaseCurlDegrees = value;
        }

        /// <summary>
        /// The minimum degrees between vectors from the first curl
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumTipCurlDegrees1
        {
            get => m_State.minimumTipCurlDegrees1;
            set => m_State.minimumTipCurlDegrees1 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the first curl
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumTipCurlDegrees1
        {
            get => m_State.maximumTipCurlDegrees1;
            set => m_State.maximumTipCurlDegrees1 = value;
        }

        /// <summary>
        /// The minimum degrees between vectors from the second curl
        /// joint to its closest neighbors.
        /// </summary>
        public float minimumTipCurlDegrees2
        {
            get => m_State.minimumTipCurlDegrees2;
            set => m_State.minimumTipCurlDegrees2 = value;
        }

        /// <summary>
        /// The maximum degrees between vectors from the second curl
        /// joint to its closest neighbors.
        /// </summary>
        public float maximumTipCurlDegrees2
        {
            get => m_State.maximumTipCurlDegrees2;
            set => m_State.maximumTipCurlDegrees2 = value;
        }

        /// <summary>
        /// The minimum distance between each finger tip and the thumb tip
        /// to calculate pinch values for. Values below or equal to this will
        /// result in a pinch value of <c>1</c>.
        /// </summary>
        public float minimumPinchDistance
        {
            get => m_State.minimumPinchDistance;
            set => m_State.minimumPinchDistance = value;
        }

        /// <summary>
        /// The maximum distance between each finger tip and the thumb tip
        /// which allows for non-zero pinch values.
        /// </summary>
        public float maximumPinchDistance
        {
            get => m_State.maximumPinchDistance;
            set => m_State.maximumPinchDistance = value;
        }

        /// <summary>
        /// The minimum degrees for splay between this finger and the next.
        /// Not used for the little finger.
        /// </summary>
        public float minimumSpreadDegrees
        {
            get => m_State.minimumSpreadDegrees;
            set => m_State.minimumSpreadDegrees = value;
        }

        /// <summary>
        /// The maximum degrees for splay between this finger and the next.
        /// Not used for the little finger.
        /// </summary>
        public float maximumSpreadDegrees
        {
            get => m_State.maximumSpreadDegrees;
            set => m_State.maximumSpreadDegrees = value;
        }

        /// <summary>
        /// Copies to and from the current state of this <c>XRFingerShapeConfiguration</c>.
        /// </summary>
        public XRFingerShapeConfigurationState state
        {
            get => m_State;
            set => m_State = value;
        }

        [SerializeField]
        XRFingerShapeConfigurationState m_State;
    }
}
