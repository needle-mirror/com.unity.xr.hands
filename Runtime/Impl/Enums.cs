using System;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands.Capture.Recording;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Internal flags that represent a subset of <see cref="MetaAimFlags"/>
    /// and additional fields needed for serialization/deserialization signalling.
    /// </summary>
    /// <seealso cref="MetaAimHandState"/>
    /// <seealso cref="XRHandAimState"/>
    [Flags]
    enum AimStateFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Whether the pose represents an actively tracked position and orientation.
        /// This represents the <see langword="bool"/> that corresponds with <see cref="TrackedDevice.isTracked"/>.
        /// </summary>
        /// <seealso cref="TrackedDevice.isTracked"/>
        IsTracked = 1 << 0,

        /// <summary>
        /// Whether the index finger and thumb are currently pressed together.
        /// </summary>
        /// <seealso cref="MetaAimFlags.IndexPinching"/>
        IsIndexPressed = 1 << 1,

        /// <summary>
        /// Whether the middle finger and thumb are currently pressed together.
        /// </summary>
        /// <seealso cref="MetaAimFlags.MiddlePinching"/>
        IsMiddlePressed = 1 << 2,

        /// <summary>
        /// Whether the ring finger and thumb are currently pressed together.
        /// </summary>
        /// <seealso cref="MetaAimFlags.RingPinching"/>
        IsRingPressed = 1 << 3,

        /// <summary>
        /// Whether the little finger and thumb are currently pressed together.
        /// </summary>
        /// <seealso cref="MetaAimFlags.LittlePinching"/>
        IsLittlePressed = 1 << 4,

        /// <summary>
        /// Whether the serialized stream has a <see cref="Pose"/> struct to read
        /// or whether the aim pose is a valid <see cref="Pose"/> that can be used.
        /// When this flag is not set, the pose is either uninitialized and should not be used
        /// (likely due to the feature not being supported), or you should not attempt to read
        /// a <see cref="Pose"/> from the serialized stream as it is not present.
        /// This flag is not to be confused with the concept of the <see cref="MetaAimFlags.Valid"/> flag.
        /// </summary>
        /// <seealso cref="MetaAimHandState.TryGetAimPose"/>
        /// <seealso cref="XRHandAimState.TryGetAimPose"/>
        /// <seealso cref="SerializationExtensions.ReadAimState"/>
        /// <seealso cref="SerializationExtensions.Write(BinaryWriter, in XRHandAimState)"/>
        HasAimPose = 1 << 5,
    }
}
