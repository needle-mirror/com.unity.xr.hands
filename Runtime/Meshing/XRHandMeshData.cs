using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands.Meshing
{
    /// <summary>
    /// Low-level meshing data. If the call that retrieved this from
    /// <see cref="XRHandSubsystem.TryGetMeshData"/> was successful,
    /// <see cref="XRHandMeshData.positions"/>, <see cref="XRHandMeshData.normals"/>,
    /// and <see cref="XRHandMeshData.uvs"/> will all have the same length.
    /// </summary>
    public struct XRHandMeshData : IDisposable
    {
        /// <summary>
        /// Dispose of array data in this object.
        /// </summary>
        /// <remarks>
        /// This method is idempotent, meaning it doesn't matter if it gets
        /// duplicate calls. Since <see cref="XRHandMeshDataQueryResult"/>'s
        /// <see cref="XRHandMeshDataQueryResult.Dispose"/> calls this, that
        /// that means <c>Dispose</c> is safe to call on just <see cref="XRHandMeshDataQueryResult.leftHand"/>
        /// and <see cref="XRHandMeshDataQueryResult.rightHand"/>, just the
        /// containing <see cref="XRHandMeshDataQueryResult"/>, or both sets.
        /// </remarks>
        public void Dispose()
        {
            if (m_VertexIndices.IsCreated)
                m_VertexIndices.Dispose();

            if (m_VertexPositions.IsCreated)
                m_VertexPositions.Dispose();

            if (m_VertexNormals.IsCreated)
                m_VertexNormals.Dispose();

            if (m_VertexUVs.IsCreated)
                m_VertexUVs.Dispose();

            if (m_MatchingJointBindPoseIsValid.IsCreated)
                m_MatchingJointBindPoseIsValid.Dispose();

            if (m_JointBindPoses.IsCreated)
                m_JointBindPoses.Dispose();

            if (m_MatchingJointRadiusIsValid.IsCreated)
                m_MatchingJointRadiusIsValid.Dispose();

            if (m_JointRadii.IsCreated)
                m_JointRadii.Dispose();

            if (m_BonesPerVertex.IsCreated)
                m_BonesPerVertex.Dispose();

            if (m_BoneWeights.IsCreated)
                m_BoneWeights.Dispose();
        }

        /// <summary>
        /// Indices into the other arrays in this type for triangle data.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<int> indices
        {
            get => m_VertexIndices;
            internal set
            {
                if (m_VertexIndices.Equals(value))
                    return;

                if (m_VertexIndices.IsCreated)
                    m_VertexIndices.Dispose();

                m_VertexIndices = value;
            }
        }

        /// <summary>
        /// Positions of vertices, in session space.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<Vector3> positions
        {
            get => m_VertexPositions;
            internal set
            {
                if (m_VertexPositions.Equals(value))
                    return;

                if (m_VertexPositions.IsCreated)
                    m_VertexPositions.Dispose();

                m_VertexPositions = value;
            }
        }

        /// <summary>
        /// Normals of vertices.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use. If valid, will have the same
        /// <c>Length</c> as <see cref="positions"/>.
        /// </value>
        public NativeArray<Vector3> normals
        {
            get => m_VertexNormals;
            internal set
            {
                if (m_VertexNormals.Equals(value))
                    return;

                if (m_VertexNormals.IsCreated)
                    m_VertexNormals.Dispose();

                m_VertexNormals = value;
            }
        }

        /// <summary>
        /// Texture UV coordinates of vertices.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use. If valid, will have the same
        /// <c>Length</c> as <see cref="positions"/>.
        /// </value>
        public NativeArray<Vector2> uvs
        {
            get => m_VertexUVs;
            internal set
            {
                if (m_VertexUVs.Equals(value))
                    return;

                if (m_VertexUVs.IsCreated)
                    m_VertexUVs.Dispose();

                m_VertexUVs = value;
            }
        }

        /// <summary>
        /// Bone count for each vertex.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<byte> bonesPerVertex
        {
            get => m_BonesPerVertex;
            internal set
            {
                if (m_BonesPerVertex.Equals(value))
                    return;

                if (m_BonesPerVertex.IsCreated)
                    m_BonesPerVertex.Dispose();

                m_BonesPerVertex = value;
            }
        }

        /// <summary>
        /// Bone weights for each vertex, sorted by vertex index.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<BoneWeight1> boneWeights
        {
            get => m_BoneWeights;
            internal set
            {
                if (m_BoneWeights.Equals(value))
                    return;

                if (m_BoneWeights.IsCreated)
                    m_BoneWeights.Dispose();

                m_BoneWeights = value;
            }
        }

        /// <summary>
        /// Represents which hand this mesh data represents.
        /// </summary>
        /// <value>
        /// Right, left, or invalid.
        /// </value>
        public Handedness handedness { get; internal set; }

        /// <summary>
        /// Retrieves root pose, if this frame's data had one available.
        /// </summary>
        /// <param name="rootPose">
        /// If this function succeeds, this will be filled out with the root
        /// <see cref="Pose"/> for how the mesh data in this object is intended
        /// to be drawn. This pose should not be used otherwise.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the root pose was
        /// filled out, returns <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// If this function fails, you should continue to use the previous pose.
        /// If there has been no successful pose retrieval yet, the closest
        /// approximation would be the <see cref="Pose"/> of the wrist joint.
        /// </remarks>
        public bool TryGetRootPose(out Pose rootPose)
        {
            rootPose = m_IsRootPoseValid ? m_RootPose : Pose.identity;
            return m_IsRootPoseValid;
        }

        /// <summary>
        /// Retrieves joint pose, if this frame's data had one available.
        /// </summary>
        /// <param name="bindPose">
        /// If this function succeeds, this will be filled out with the bind pose
        /// <see cref="Matrix4x4"/> for corresponding joint represented by
        /// <paramref name="jointID"/>.
        /// </param>
        /// <param name="jointID">
        /// ID of joint for the bind pose being requested.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the
        /// <see cref="bindPose"/> was filled out, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// If this function fails, you should instead offset the requested
        /// joint's bind <see cref="Pose"/> by the offset from the previous frame.
        /// </remarks>
        public bool TryGetJointBindPoseMatrix(out Matrix4x4 bindPose, XRHandJointID jointID)
        {
            int jointIndex = jointID.ToIndex();

            bool valid =
                m_MatchingJointBindPoseIsValid.IsCreated &&
                jointIndex >= 0 &&
                m_MatchingJointBindPoseIsValid.Length > jointIndex &&
                m_MatchingJointBindPoseIsValid[jointIndex] &&
                m_JointBindPoses.IsCreated &&
                m_JointBindPoses.Length > jointIndex;

            bindPose = valid ? m_JointBindPoses[jointIndex] : Matrix4x4.identity;
            return valid;
        }

        /// <summary>
        /// Raw joint bind pose matrices. Prefer <see cref="TryGetJointBindPoseMatrix"/>
        /// for safe per-joint access with validity checks.
        /// </summary>
        public NativeArray<Matrix4x4> jointBindPoseMatricesRaw => m_JointBindPoses;

        /// <summary>
        /// Retrieves joint radius, if this frame's data had one available.
        /// </summary>
        /// <param name="radius">
        /// If this function succeeds, this will be filled out with the
        /// radius of corresponding joint represented by
        /// <paramref name="jointID"/>.
        /// </param>
        /// <param name="jointID">
        /// ID of joint for the radius being requested.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the radius was
        /// filled out, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetJointRadius(out float radius, XRHandJointID jointID)
        {
            int jointIndex = jointID.ToIndex();

            bool valid =
                m_MatchingJointRadiusIsValid.IsCreated &&
                jointIndex >= 0 &&
                m_MatchingJointRadiusIsValid.Length > jointIndex &&
                m_MatchingJointRadiusIsValid[jointIndex] &&
                m_JointRadii.IsCreated &&
                m_JointRadii.Length > jointIndex;

            radius = valid ? m_JointRadii[jointIndex] : 0f;
            return valid;
        }

        internal XRHandMeshData(Handedness handedness)
        {
            this.handedness = handedness;

            m_RootPose = Pose.identity;
            m_IsRootPoseValid = false;

            m_VertexIndices = default;
            m_VertexPositions = default;
            m_VertexNormals = default;
            m_VertexUVs = default;

            m_MatchingJointBindPoseIsValid = default;
            m_JointBindPoses = default;
            m_MatchingJointRadiusIsValid = default;
            m_JointRadii = default;

            m_BonesPerVertex = default;
            m_BoneWeights = default;
        }

        internal void SetRootPose(Pose rootPose)
        {
            m_RootPose = rootPose;
            m_IsRootPoseValid = true;
        }

        internal void InvalidateRootPose() => m_IsRootPoseValid = false;

        internal void SetMatchingJointBindPoseValidity(NativeArray<bool> matchingJointPoseIsValid)
        {
            if (m_MatchingJointBindPoseIsValid.Equals(matchingJointPoseIsValid))
                return;

            if (m_MatchingJointBindPoseIsValid.IsCreated)
                m_MatchingJointBindPoseIsValid.Dispose();

            m_MatchingJointBindPoseIsValid = matchingJointPoseIsValid;
        }

        internal void SetJointBindPoses(NativeArray<Matrix4x4> jointBindPoses)
        {
            if (m_JointBindPoses.Equals(jointBindPoses))
                return;

            if (m_JointBindPoses.IsCreated)
                m_JointBindPoses.Dispose();

            m_JointBindPoses = jointBindPoses;
        }

        internal void SetMatchingJointRadiusValidity(NativeArray<bool> matchingJointRadiusIsValid)
        {
            if (m_MatchingJointRadiusIsValid.Equals(matchingJointRadiusIsValid))
                return;

            if (m_MatchingJointRadiusIsValid.IsCreated)
                m_MatchingJointRadiusIsValid.Dispose();

            m_MatchingJointRadiusIsValid = matchingJointRadiusIsValid;
        }

        internal void SetJointRadii(NativeArray<float> jointRadii)
        {
            if (m_JointRadii.Equals(jointRadii))
                return;

            if (m_JointRadii.IsCreated)
                m_JointRadii.Dispose();

            m_JointRadii = jointRadii;
        }

        Pose m_RootPose;
        internal bool m_IsRootPoseValid;
        NativeArray<int> m_VertexIndices;
        NativeArray<Vector3> m_VertexPositions;
        NativeArray<Vector3> m_VertexNormals;
        NativeArray<Vector2> m_VertexUVs;
        NativeArray<bool> m_MatchingJointBindPoseIsValid;
        NativeArray<Matrix4x4> m_JointBindPoses;
        NativeArray<bool> m_MatchingJointRadiusIsValid;
        NativeArray<float> m_JointRadii;
        NativeArray<byte> m_BonesPerVertex;
        NativeArray<BoneWeight1> m_BoneWeights;
    }

    namespace ProviderImplementation
    {
        /// <summary>
        /// Contains extensions to <see cref="XRHandMeshData"/> relevant
        /// to supplying data when <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        public static class XRHandMeshDataExtensions
        {
            /// <summary>
            /// Set the <see cref="XRHandMeshData.indices"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set index data on.
            /// </param>
            /// <param name="indices">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetIndices(this ref XRHandMeshData meshData, NativeArray<int> indices)
                => meshData.indices = indices;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.positions"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set position data on.
            /// </param>
            /// <param name="positions">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetPositions(this ref XRHandMeshData meshData, NativeArray<Vector3> positions)
                => meshData.positions = positions;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.normals"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set normal data on.
            /// </param>
            /// <param name="normals">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetNormals(this ref XRHandMeshData meshData, NativeArray<Vector3> normals)
                => meshData.normals = normals;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.uvs"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set texture coordinate data on.
            /// </param>
            /// <param name="uvs">
            /// Texture coordinate data to set on the invoking mesh data.
            /// </param>
            public static void SetUVs(this ref XRHandMeshData meshData, NativeArray<Vector2> uvs)
                => meshData.uvs = uvs;

            /// <summary>
            /// Set the root <see cref="Pose"/> on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set index data on.
            /// </param>
            /// <param name="rootPose">
            /// Root pose to set on the invoking mesh data.
            /// </param>
            public static void SetRootPose(this ref XRHandMeshData meshData, Pose rootPose)
                => meshData.SetRootPose(rootPose);

            /// <summary>
            /// Invalidates the root <see cref="Pose"/> on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to invalidate the root pose on.
            /// </param>
            public static void InvalidateRootPose(this ref XRHandMeshData meshData)
                => meshData.InvalidateRootPose();

            /// <summary>
            /// Set the joint pose validity on the <see cref="XRHandMeshData"/>.
            /// Must have enough room for each joint up to and including the
            /// last valid one. Must have memory allocated for joints with
            /// lower-integer-value <see cref="XRHandJointID"/>s, even if those
            /// joints are invalid.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set joint data validity flags on.
            /// </param>
            /// <param name="matchingJointBindPoseIsValid">
            /// The value at each index is set to <see langword="true"/> if the
            /// bind <see cref="Pose"/> for that joint is valid. That bind
            /// <see cref="Pose"/> can then be retrieved from
            /// <see cref="XRHandMesh.TryGetJointPose"/>.
            /// </param>
            public static void SetMatchingJointBindPoseValidity(this ref XRHandMeshData meshData, NativeArray<bool> matchingJointBindPoseIsValid)
                => meshData.SetMatchingJointBindPoseValidity(matchingJointBindPoseIsValid);

            /// <summary>
            /// Set the joint bind poses on the <see cref="XRHandMeshData"/>.
            /// Must have enough room for each joint up to and including the
            /// last valid one. Must have memory allocated for joints with
            /// lower-integer-value <see cref="XRHandJointID"/>s, even if those
            /// joints are invalid.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set joint data validity flags on.
            /// </param>
            /// <param name="jointBindPoses">
            /// <see cref="Pose"/> for each joint known with known data this frame.
            /// </param>
            /// <remarks>
            /// The bind <see cref="Pose"/> at each index can only be retrieved
            /// if the value at the matching index in what's supplied to
            /// <see cref="SetMatchingJointBindPoseValidity"/> is set to
            /// <see langword="true"/>.
            /// </remarks>
            public static void SetJointBindPoses(this ref XRHandMeshData meshData, NativeArray<Matrix4x4> jointBindPoses)
                => meshData.SetJointBindPoses(jointBindPoses);

            /// <summary>
            /// Set the joint radius validity on the <see cref="XRHandMeshData"/>.
            /// Must have enough room for each joint up to and including the
            /// last valid one. Must have memory allocated for joints with
            /// lower-integer-value <see cref="XRHandJointID"/>s, even if those
            /// joints are invalid.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set joint data validity flags on.
            /// </param>
            /// <param name="matchingJointRadiusIsValid">
            /// The value at each index is set to <see langword="true"/> if the
            /// radius for that joint is valid. That radius can then be
            /// retrieved from <see cref="XRHandMesh.TryGetJointRadius"/>.
            /// </param>
            public static void SetMatchingJointRadiusValidity(this ref XRHandMeshData meshData, NativeArray<bool> matchingJointRadiusIsValid)
                => meshData.SetMatchingJointRadiusValidity(matchingJointRadiusIsValid);

            /// <summary>
            /// Set the joint radii on the <see cref="XRHandMeshData"/>. Must
            /// have enough room for each joint up to and including the last
            /// valid one. Must have memory allocated for joints with
            /// lower-integer-value <see cref="XRHandJointID"/>s, even if those
            /// joints are invalid.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set joint data validity flags on.
            /// </param>
            /// <param name="jointRadii">
            /// Radius for each joint known with known data this frame.
            /// </param>
            /// <remarks>
            /// The radius at each index can only be retrieved if
            /// the value at the matching index in what's supplied to
            /// <see cref="SetMatchingJointRadiusValidity"/> is set to
            /// <see langword="true"/>.
            /// </remarks>
            public static void SetJointRadii(this ref XRHandMeshData meshData, NativeArray<float> jointRadii)
                => meshData.SetJointRadii(jointRadii);

            /// <summary>
            /// Set bones per vertex on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set bones per vertex on.
            /// </param>
            /// <param name="bonesPerVertex">
            /// Bone count for each vertex in the mesh.
            /// </param>
            public static void SetBonesPerVertex(this ref XRHandMeshData meshData, NativeArray<byte> bonesPerVertex)
                => meshData.bonesPerVertex = bonesPerVertex;

            /// <summary>
            /// Set bone weights on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set bone weights on.
            /// </param>
            /// <param name="boneWeights">
            /// Bone weights for each vertex, sorted by vertex index.
            /// </param>
            public static void SetBoneWeights(this ref XRHandMeshData meshData, NativeArray<BoneWeight1> boneWeights)
                => meshData.boneWeights = boneWeights;
        }
    }
}
