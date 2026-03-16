#if UNITY_OPENXR_PACKAGE

using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;

/// <summary>
/// A sample demonstrating how to use the <see cref="OpenXRHandSubsystemManager"/>
/// to start and stop hand tracking based on platform-specific permission requests.
/// </summary>
public class SubsystemManagerSample : MonoBehaviour
{
    [SerializeField]
    OpenXRHandSubsystemManager handSubsystemManager;

    void Awake()
    {
        if (handSubsystemManager == null)
        {
            handSubsystemManager =
                FindAnyObjectByType<OpenXRHandSubsystemManager>();
        }
    }

    void Start()
    {
        // Make some platform-specific permission requests
    }

    /// <summary>
    /// Called when permission for hand tracking is granted.
    /// </summary>
    public void OnPermissionGranted()
    {
        // Start hand tracking
        handSubsystemManager.enabled = true;
    }

    /// <summary>
    /// Called when permission for hand tracking is denied.
    /// </summary>
    public void OnPermissionDenied()
    {
        // Stop hand tracking
        handSubsystemManager.enabled = false;
    }
}
#endif // UNITY_OPENXR_PACKAGE
