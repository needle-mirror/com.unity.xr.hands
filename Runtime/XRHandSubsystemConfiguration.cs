using System;

namespace UnityEngine.XR.Hands.Configuration
{
    /// <summary>
    /// Configuration settings for the XR Hand Subsystem.
    /// </summary>
    /// <seealso cref="XRHandSubsystem.UpdateHandsConfiguration"/>
    /// <seealso cref="XRHandSubsystem.handSubsystemConfiguration"/>
    public struct XRHandSubsystemConfiguration : IEquatable<XRHandSubsystemConfiguration>
    {
        /// <summary>
        /// The pose source to be used by <see cref="XRHandDevice"/>. This will take effect in the input device the next
        /// time <see cref="XRHandSubsystem"/> invokes <see cref="XRHandSubsystem.updatedHands"/>.
        /// </summary>
        /// <remarks>
        /// The transition to the new pose source will be immediate. The device will not attempt to transition or cancel
        /// in-flight data streams.
        /// </remarks>
        public XRHandDevicePoseSource xrHandDevicePoseSource { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The object to compare against.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="other"/> has every field equal
        /// to this, otherwise returns <see langword="false"/>.</returns>
        public readonly bool Equals(in XRHandSubsystemConfiguration other)
        {
            return xrHandDevicePoseSource == other.xrHandDevicePoseSource;
        }

        /// <inheritdoc cref="Equals(in XRHandSubsystemConfiguration)"/>
        readonly bool IEquatable<XRHandSubsystemConfiguration>.Equals(XRHandSubsystemConfiguration other) => Equals(in other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="obj"/> is of type <see cref="XRHandSubsystemConfiguration"/>
        /// and has every field equal to this, otherwise returns <see langword="false"/>.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is XRHandSubsystemConfiguration other && Equals(other);
        }

        /// <summary>
        /// Computes a hash code from all fields of this <see cref="XRHandSubsystemConfiguration"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public readonly override int GetHashCode()
        {
            return (int)xrHandDevicePoseSource;
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(in XRHandSubsystemConfiguration)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.</returns>
        public static bool operator ==(in XRHandSubsystemConfiguration lhs, in XRHandSubsystemConfiguration rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests for inequality. Same as <c>!</c><see cref="Equals(in XRHandSubsystemConfiguration)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/> is not equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.</returns>
        public static bool operator !=(in XRHandSubsystemConfiguration lhs, in XRHandSubsystemConfiguration rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Enumeration for controlling how <see cref="XRHandDevice"/> gets data for its public InputControls.
    /// </summary>
    /// <seealso cref="XRHandSubsystemConfiguration"/>
    public enum XRHandDevicePoseSource
    {
        /// <summary>
        /// This is the default and existing behavior for XRHandDevice.
        ///
        /// <see cref="XRHandDevice"/> will continue to report non-OpenXR compliant position and rotation data using
        /// bone joints for poke, pinch, and grip. XRHandDevice's position and rotation will continue to be in the
        /// wrist.
        /// </summary>
        LegacyJointRecognition,

        /// <summary>
        /// <see cref="XRHandDevice"/> will report OpenXR compliant hand interaction poses using <see cref="XRCommonHandGestures"/>
        /// as a data source for each hand. XRHandDevice's <see cref="XRHandDevice.devicePosition"/> and
        /// <see cref="XRHandDevice.deviceRotation"/> will match grip pose to align with other Unity XR input
        /// devices which report 'device' poses where the grip is.
        /// </summary>
        CommonGestures,
    }

    /// <summary>
    /// Payload for when the subsystem updates its configuration.
    /// </summary>
    /// <seealso cref="XRHandSubsystem.configurationUpdated"/>
    readonly struct XRHandSubsystemConfigurationUpdatedEventArgs
    {
        /// <summary>
        /// The hands subsystem that is sending the event.
        /// </summary>
        public XRHandSubsystem subsystem { get; }

        /// <summary>
        /// The new configuration that was set and processed by the subsystem.
        /// </summary>
        public XRHandSubsystemConfiguration newConfiguration { get; }

        internal XRHandSubsystemConfigurationUpdatedEventArgs(XRHandSubsystem subsystem,
            XRHandSubsystemConfiguration newConfiguration)
        {
            this.subsystem = subsystem;
            this.newConfiguration = newConfiguration;
        }
    }
}
