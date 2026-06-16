#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System;
using System.Collections.Generic;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.Hands.OpenXR.NativeInterop
{
    /// <summary>
    /// Manages a linked list of OpenXR extension structures with stable native
    /// memory pointers suitable for passing to OpenXR APIs via P/Invoke.
    /// </summary>
    /// <remarks>
    /// Append nodes with <see cref="TryAddNode{TNodeType}"/>, mutate them in
    /// place with <see cref="TryUpdateNode{TData}"/>, and read data populated
    /// by the OpenXR runtime with <see cref="TryGetNode{TData}"/>. The chain
    /// owns the native memory of every node it contains. Call
    /// <see cref="Dispose"/> before the chain is garbage collected.
    /// </remarks>
    public sealed unsafe class XrStructureChain : IDisposable
    {
        readonly List<XrStructureNodeBase> m_Nodes = new();
        bool m_HasActiveResources;

        bool m_Disposed;

        /// <summary>
        /// The number of nodes in the chain.
        /// </summary>
        public int count => m_Nodes.Count;

        /// <summary>
        /// Returns a pointer to the head of the structure chain.
        /// </summary>
        /// <returns>
        /// A pointer to the first node in the chain, or <c>null</c> when the
        /// chain is empty.
        /// </returns>
        /// <remarks>
        /// The returned pointer is invalidated when the chain is cleared or
        /// disposed, or when the head node is replaced. Fetch the head pointer
        /// again after any structural change before passing it to OpenXR.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public XrBaseInStructure* GetHeadPointer()
        {
            ThrowIfDisposed();

            if (m_Nodes.Count == 0)
                return null;

            return m_Nodes[0].GetAsXrBaseInStructure();
        }

        /// <summary>
        /// Tries to append a new structure node to the tail of the chain.
        /// </summary>
        /// <typeparam name="TNodeType">
        /// The unmanaged structure type to add. Must begin with the same header
        /// layout as <see cref="XrBaseInStructure"/>.
        /// </typeparam>
        /// <param name="data">The structure data to copy into the new node.</param>
        /// <returns>
        /// <c>true</c> if the node was added; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Returns <c>false</c> when a node with the same
        /// <see cref="XrStructureType"/> already exists in the chain. Each
        /// <see cref="XrStructureType"/> may appear at most once. The new node
        /// owns a native memory copy of <paramref name="data"/> for the
        /// lifetime of the node.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TNodeType"/> is smaller than
        /// <see cref="XrBaseInStructure"/>, or if <paramref name="data"/> has
        /// <see cref="XrStructureType.Unknown"/> at offset 0.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public bool TryAddNode<TNodeType>(TNodeType data) where TNodeType : unmanaged
        {
            ThrowIfDisposed();

            ValidateData(data);

            var type = ((XrBaseInStructure*)&data)->type;
            if (ContainsNode(type))
                return false;

            var node = new XrStructureNode<TNodeType>(data);
            var newBasePtr = node.GetAsXrBaseInStructure();
            newBasePtr->next = null;
            m_HasActiveResources = true;

            UpdateTail(newBasePtr);
            m_Nodes.Add(node);

            return true;
        }

        /// <summary>
        /// Checks whether a node with a given <see cref="XrStructureType"/>
        /// exists in the chain.
        /// </summary>
        /// <param name="type">The <see cref="XrStructureType"/> to search for in the chain.</param>
        /// <returns>
        /// <c>true</c> if the chain contains a node of the given type;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Use this to check for an existing node before calling
        /// <see cref="TryAddNode{TNodeType}"/>, since each
        /// <see cref="XrStructureType"/> may appear at most once in the chain.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public bool ContainsNode(XrStructureType type)
        {
            ThrowIfDisposed();

            foreach (var node in m_Nodes)
            {
                var basePtr = node.GetAsXrBaseInStructure();
                if (basePtr != null && basePtr->type == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the data from the node whose
        /// <see cref="XrStructureType"/> matches <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TData">
        /// The expected structure type stored in the matching node.
        /// </typeparam>
        /// <param name="type">The <see cref="XrStructureType"/> to search for in the chain.</param>
        /// <param name="data">
        /// When this method returns <c>true</c>, contains the data from the
        /// matching node; otherwise, the default value of
        /// <typeparamref name="TData"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a node with the requested type was found and its data
        /// was retrieved; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Use this method to read data that the OpenXR runtime populated in
        /// the chain, such as locate output structures filled by
        /// <c>xrLocateHandJointsEXT</c>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public bool TryGetNode<TData>(XrStructureType type, out TData data) where TData : unmanaged
        {
            ThrowIfDisposed();

            foreach (var node in m_Nodes)
            {
                var basePtr = node.GetAsXrBaseInStructure();
                if (basePtr != null && basePtr->type == type)
                {
                    if (node is XrStructureNode<TData> typedNode)
                        return typedNode.TryGetData(out data);
                }
            }

            data = default;
            return false;
        }

        /// <summary>
        /// Overwrites the data of an existing node in place, preserving the
        /// chain's <c>next</c> linkage.
        /// </summary>
        /// <typeparam name="TData">
        /// The unmanaged structure type to update. Must match the type stored
        /// in an existing node.
        /// </typeparam>
        /// <param name="data">
        /// The new data to write into the matching node. The <c>next</c> field
        /// inside <paramref name="data"/> is ignored. The chain restores its
        /// own linkage pointer after the write.
        /// </param>
        /// <returns>
        /// <c>true</c> if a matching node was found and updated; otherwise,
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Locates the node by the <see cref="XrStructureType"/> field of
        /// <paramref name="data"/>. Use this method to update input structures
        /// between OpenXR calls without allocating new native memory.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TData"/> is smaller than
        /// <see cref="XrBaseInStructure"/>, or if <paramref name="data"/> has
        /// <see cref="XrStructureType.Unknown"/> at offset 0.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public bool TryUpdateNode<TData>(TData data) where TData : unmanaged
        {
            ThrowIfDisposed();

            ValidateData(data);
            var type = ((XrBaseInStructure*)&data)->type;

            foreach (var node in m_Nodes)
            {
                var basePtr = node.GetAsXrBaseInStructure();
                if (basePtr != null && basePtr->type == type)
                {
                    if (node is XrStructureNode<TData> typedNode)
                    {
                        var savedNext = basePtr->next;
                        if (typedNode.TrySetData(data))
                        {
                            basePtr->next = savedNext;
                            return true;
                        }
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Disposes all nodes and resets the chain to an empty state.
        /// </summary>
        /// <remarks>
        /// The chain instance can be reused after clearing. Any pointer
        /// returned by <see cref="GetHeadPointer"/> before this call is
        /// invalidated.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the chain has been disposed.
        /// </exception>
        public void Clear()
        {
            ThrowIfDisposed();

            foreach (var node in m_Nodes)
                node.Dispose();

            m_Nodes.Clear();
            m_HasActiveResources = false;
        }

        ~XrStructureChain()
        {
            FinalizerImpl();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            Clear();
            m_Disposed = true;
            GC.SuppressFinalize(this);
        }

        static void ValidateData<TData>(TData data) where TData : unmanaged
        {
            if (sizeof(TData) < sizeof(XrBaseInStructure))
                throw new ArgumentException(
                    $"{typeof(TData).Name} ({sizeof(TData)} bytes) is too small to contain " +
                    $"an XrBaseInStructure header ({sizeof(XrBaseInStructure)} bytes).");

            if (((XrBaseInStructure*)&data)->type == XrStructureType.Unknown)
                throw new ArgumentException(
                    $"{typeof(TData).Name} has XrStructureType.Unknown at offset 0. " +
                    $"Ensure the struct is constructed with a valid XrStructureType.");
        }

        void UpdateTail(XrBaseInStructure* newNodePtr)
        {
            if (m_Nodes.Count > 0)
            {
                var tailPtr = m_Nodes[m_Nodes.Count - 1].GetAsXrBaseInStructure();
                if (tailPtr != null)
                    tailPtr->next = newNodePtr;
            }
        }

        internal void FinalizerImpl()
        {
            if (m_HasActiveResources && !m_Disposed)
                Debug.LogError(
                    "XrStructureChain was not disposed. Call Dispose() explicitly before the chain is garbage collected.");
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(XrStructureChain));
        }
    }
}
#endif
