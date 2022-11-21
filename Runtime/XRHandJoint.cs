using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Represents a joint of an <see cref="XRHand"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRHandJoint
    {
        /// <summary>
        /// The ID of this joint.
        /// </summary>
        public XRHandJointID id => m_Id;

        /// <summary>
        /// Represents which tracking data is valid.
        /// </summary>
        public XRHandJointTrackingState trackingState => m_TrackingState;

        /// <summary>
        /// If successful, retrieves the joint radius.
        /// </summary>
        /// <param name="radius">
        /// Will be filled out with the tracked radius of this joint if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the radius was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool TryGetRadius(out float radius)
        {
            if ((m_TrackingState & XRHandJointTrackingState.Radius) == XRHandJointTrackingState.None)
            {
                radius = 0.0f;
                return false;
            }

            radius = m_Radius;
            return true;
        }

        /// <summary>
        /// If successful, retrieves the joint's pose in session space, relative
        /// to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        /// <param name="pose">
        /// Will be filled out with the tracked pose of this joint if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the joint pose was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// To transform to world space, use the <see cref="Pose"/> returned by
        /// <see cref="Pose.GetTransformedBy(Pose)"/> when passing a <c>Pose</c>
        /// populated by the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin)'s
        /// position and rotation.
        /// </remarks>
        public bool TryGetPose(out Pose pose)
        {
            if ((m_TrackingState & XRHandJointTrackingState.Pose) == XRHandJointTrackingState.None)
            {
                pose = Pose.identity;
                return false;
            }

            pose = m_Pose;
            return true;
        }

        /// <summary>
        /// If successful, retrieves the joint linear velocity in session space,
        /// relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        /// <param name="linearVelocity">
        /// Will be filled out with the tracked linear velocity of this joint
        /// (relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin)) if
        /// successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the velocity was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// To transform to world space, rotate this by the rotation of the
        /// [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </remarks>
        public bool TryGetLinearVelocity(out Vector3 linearVelocity)
        {
            if ((m_TrackingState & XRHandJointTrackingState.LinearVelocity) == XRHandJointTrackingState.None)
            {
                linearVelocity = Vector3.zero;
                return false;
            }

            linearVelocity = m_LinearVelocity;
            return true;
        }

        /// <summary>
        /// If successful, retrieves the joint angular velocity in session space
        /// (relative to the device origin at start-up).
        /// </summary>
        /// <param name="angularVelocity">
        /// Will be filled out with the tracked angular velocity of this joint
        /// (relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin)) if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the angular
        /// velocity filled out with valid tracking data, returns
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// To transform to world space, rotate this by the rotation of the
        /// [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </remarks>
        public bool TryGetAngularVelocity(out Vector3 angularVelocity)
        {
            if ((m_TrackingState & XRHandJointTrackingState.AngularVelocity) == XRHandJointTrackingState.None)
            {
                angularVelocity = Vector3.zero;
                return false;
            }

            angularVelocity = m_AngularVelocity;
            return true;
        }

        /// <summary>
        /// Returns a string representation of the XRHandJoint.
        /// </summary>
        /// <returns>
        /// String representation of the value.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "[XRHandJoint ID: {0}] Pose: ({1}) Radius: {2} | Linear Velocity: {3}, Angular Velocity: {4} | Tracking State: {5}",
                m_Id, m_Pose.ToString("F4"), m_Radius.ToString("F4"), m_LinearVelocity.ToString("F4"), m_AngularVelocity.ToString("F4"), m_TrackingState);
        }

        internal XRHandJointID m_Id;
        internal Pose m_Pose;
        internal float m_Radius;
        internal Vector3 m_LinearVelocity;
        internal Vector3 m_AngularVelocity;
        internal XRHandJointTrackingState m_TrackingState;

        internal static readonly XRHandJoint willNeverBeValid;

        static XRHandJoint() => willNeverBeValid = new XRHandJoint
        {
            m_TrackingState = XRHandJointTrackingState.WillNeverBeValid
        };
    }
}
