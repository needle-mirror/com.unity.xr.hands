#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.OpenXR.NativeInterop
{
    /// <summary>
    /// Base class for nodes in an OpenXR structure chain. Each node must wrap a
    /// blittable struct in native memory so its address stays stable for the
    /// lifetime of the node — a requirement for passing structure chains to
    /// OpenXR APIs via P/Invoke.
    /// </summary>
    abstract unsafe class XrStructureNodeBase : IDisposable
    {
        /// <summary>
        /// Returns a pointer to the node's data reinterpreted as an
        /// <see cref="XrBaseInStructure"/> header. This is valid because every
        /// OpenXR struct begins with a <c>type</c> / <c>next</c> pair that
        /// matches the <see cref="XrBaseInStructure"/> layout.
        /// </summary>
        /// <returns>
        /// Pointer to the structure header, or <c>null</c> if the node has
        /// been disposed.
        /// </returns>
        public abstract XrBaseInStructure* GetAsXrBaseInStructure();

        /// <summary>
        /// Releases the native memory owned by this node.
        /// </summary>
        public abstract void Dispose();
    }

    /// <summary>
    /// A node that owns a single <typeparamref name="TData"/> instance in
    /// persistent native memory. The struct must be blittable and must begin
    /// with an <see cref="XrBaseInStructure"/>-compatible header
    /// (<c>XrStructureType type</c>, <c>void* next</c>).
    /// </summary>
    /// <typeparam name="TData">
    /// The OpenXR structure type stored in this node. Must be an unmanaged
    /// struct whose first two fields are <c>XrStructureType type</c> and
    /// <c>void* next</c>.
    /// </typeparam>
    sealed unsafe class XrStructureNode<TData> : XrStructureNodeBase
        where TData : unmanaged
    {
        NativeReference<TData> m_Data;
        XrBaseInStructure* m_BasePtr;
        bool m_Disposed;

        /// <summary>
        /// Creates a new node, copying <paramref name="data"/> into a
        /// persistently-allocated <see cref="NativeReference{T}"/>. The
        /// <c>next</c> pointer is preserved as-is from <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The structure data to store.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TData"/> is smaller than
        /// <see cref="XrBaseInStructure"/>, or if the data does not contain a
        /// valid <see cref="XrStructureType"/> at offset 0.
        /// </exception>
        public XrStructureNode(TData data)
        {
            if (sizeof(TData) < sizeof(XrBaseInStructure))
                throw new ArgumentException(
                    $"{typeof(TData).Name} ({sizeof(TData)} bytes) is too small to contain " +
                    $"an XrBaseInStructure header ({sizeof(XrBaseInStructure)} bytes).");

            if (((XrBaseInStructure*)&data)->type == XrStructureType.Unknown)
                throw new ArgumentException(
                    $"{typeof(TData).Name} has XrStructureType.Unknown at offset 0. " +
                    $"Ensure the struct is constructed with a valid XrStructureType.");

            m_Data = new NativeReference<TData>(data, Allocator.Persistent);
            m_BasePtr = (XrBaseInStructure*)m_Data.GetUnsafePtr();
        }

        /// <inheritdoc/>
        public override XrBaseInStructure* GetAsXrBaseInStructure() => m_BasePtr;

        /// <summary>
        /// Copies the stored data into <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Receives the stored structure data.</param>
        /// <returns>
        /// <c>true</c> if the node has not been disposed and data was copied;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryGetData(out TData data)
        {
            ThrowIfDisposed();

            if (m_Data.IsCreated)
            {
                data = m_Data.Value;
                return true;
            }

            data = default;
            return false;
        }

        /// <summary>
        /// Attempts to overwrite the stored data with <paramref name="data"/>.
        /// Validates that the <see cref="XrStructureType"/> in
        /// <paramref name="data"/> matches the type stored in the node.
        /// The caller is responsible for preserving the <c>next</c> pointer
        /// if chain linkage must be maintained.
        /// </summary>
        /// <param name="data">The new data to write.</param>
        /// <returns>
        /// <c>true</c> if the data was written; <c>false</c> if the
        /// <see cref="XrStructureType"/> does not match.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the node has been disposed.
        /// </exception>
        internal bool TrySetData(TData data)
        {
            ThrowIfDisposed();

            if (!m_Data.IsCreated)
                return false;

            var incomingType = ((XrBaseInStructure*)&data)->type;
            if (m_BasePtr == null || m_BasePtr->type != incomingType)
                return false;

            m_Data.Value = data;
            return true;
        }

        ~XrStructureNode()
        {
            FinalizerImpl();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (m_Disposed)
                return;

            if (m_Data.IsCreated)
                m_Data.Dispose();

            m_BasePtr = null;
            m_Disposed = true;
            GC.SuppressFinalize(this);
        }

        internal void FinalizerImpl()
        {
            if (!m_Disposed && m_Data.IsCreated)
                Debug.LogError(
                    $"XrStructureNode<{typeof(TData).Name}> was not disposed. " +
                    $"Call Dispose() explicitly before the node is garbage collected.");
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(XrStructureNode<TData>));
        }
    }
}
#endif
