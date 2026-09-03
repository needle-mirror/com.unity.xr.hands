using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Configuration;

/// <summary>
/// A component that automatically sets the hand subsystem configuration
/// to use the Common Gestures mode instead of Legacy.
/// </summary>
class UpdateHandsConfigurationSample : MonoBehaviour
{
    List<XRHandSubsystem> m_HandSubsystems;

    void Start()
    {
        if (!TryGetHandSubsystem(out var handSubsystem))
        {
            Debug.LogWarning("Hand Tracking Subsystem not found or not running," +
                " can't update its config.", this);
            return;
        }

        var config = handSubsystem.handSubsystemConfiguration;
        config.xrHandDevicePoseSource = XRHandDevicePoseSource.CommonGestures;

        handSubsystem.UpdateHandsConfiguration(config);
    }

    // Gets the first hand subsystem. If there are multiple,
    // returns the first running subsystem.
    bool TryGetHandSubsystem(out XRHandSubsystem handSubsystem)
    {
        m_HandSubsystems ??= new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(m_HandSubsystems);
        if (m_HandSubsystems.Count == 0)
        {
            handSubsystem = default;
            return false;
        }

        if (m_HandSubsystems.Count > 1)
        {
            for (var i = 0; i < m_HandSubsystems.Count; ++i)
            {
                handSubsystem = m_HandSubsystems[i];
                if (handSubsystem.running)
                    return true;
            }
        }

        handSubsystem = m_HandSubsystems[0];
        return true;
    }
}
