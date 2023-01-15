using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class MetaAimHand : MonoBehaviour
{
    [SerializeField]
    InputActionAsset m_InputActionAsset;

    [SerializeField]
    TextMeshProUGUI m_LeftFlags;

    [SerializeField]
    InputActionReference m_LeftFlagsReference;

    [SerializeField]
    Toggle m_LeftIndexPinched;

    [SerializeField]
    InputActionReference m_LeftIndexPinchedReference;

    [SerializeField]
    Slider m_LeftIndexSlider;

    [SerializeField]
    InputActionReference m_LeftIndexSliderReference;

    [SerializeField]
    Toggle m_LeftMiddlePinched;

    [SerializeField]
    InputActionReference m_LeftMiddlePinchedReference;

    [SerializeField]
    Slider m_LeftMiddleSlider;

    [SerializeField]
    InputActionReference m_LeftMiddleSliderReference;

    [SerializeField]
    Toggle m_LeftRingPinched;

    [SerializeField]
    InputActionReference m_LeftRingPinchedReference;

    [SerializeField]
    Slider m_LeftRingSlider;

    [SerializeField]
    InputActionReference m_LeftRingSliderReference;

    [SerializeField]
    Toggle m_LeftLittlePinched;

    [SerializeField]
    InputActionReference m_LeftLittlePinchedReference;

    [SerializeField]
    Slider m_LeftLittleSlider;

    [SerializeField]
    InputActionReference m_LeftLittleSliderReference;

    [SerializeField]
    TextMeshProUGUI m_RightFlags;

    [SerializeField]
    InputActionReference m_RightFlagsReference;

    [SerializeField]
    Toggle m_RightIndexPinched;

    [SerializeField]
    InputActionReference m_RightIndexPinchedReference;

    [SerializeField]
    Slider m_RightIndexSlider;

    [SerializeField]
    InputActionReference m_RightIndexSliderReference;

    [SerializeField]
    Toggle m_RightMiddlePinched;

    [SerializeField]
    InputActionReference m_RightMiddlePinchedReference;

    [SerializeField]
    Slider m_RightMiddleSlider;

    [SerializeField]
    InputActionReference m_RightMiddleSliderReference;

    [SerializeField]
    Toggle m_RightRingPinched;

    [SerializeField]
    InputActionReference m_RightRingPinchedReference;

    [SerializeField]
    Slider m_RightRingSlider;

    [SerializeField]
    InputActionReference m_RightRingSliderReference;

    [SerializeField]
    Toggle m_RightLittlePinched;

    [SerializeField]
    InputActionReference m_RightLittlePinchedReference;

    [SerializeField]
    Slider m_RightLittleSlider;

    [SerializeField]
    InputActionReference m_RightLittleSliderReference;

    XRHandSubsystem m_Subsystem;

    void Start() => m_InputActionAsset.Enable();

    void Update()
    {
        if (m_Subsystem != null)
            return;

        m_Subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
        if (m_Subsystem != null)
            m_Subsystem.updatedHands += OnUpdatedHands;
    }

    void OnDisable()
    {
        if (m_Subsystem != null)
            m_Subsystem.updatedHands -= OnUpdatedHands;
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        m_LeftFlags.text = ((MetaAimFlags)m_LeftFlagsReference.action.ReadValue<int>()).ToString();

        m_LeftIndexPinched.isOn = m_LeftIndexPinchedReference.action.ReadValue<float>() > 0.5f;
        m_LeftIndexSlider.value = m_LeftIndexSliderReference.action.ReadValue<float>();

        m_LeftMiddlePinched.isOn = m_LeftMiddlePinchedReference.action.ReadValue<float>() > 0.5f;
        m_LeftMiddleSlider.value = m_LeftMiddleSliderReference.action.ReadValue<float>();

        m_LeftRingPinched.isOn = m_LeftRingPinchedReference.action.ReadValue<float>() > 0.5f;
        m_LeftRingSlider.value = m_LeftRingSliderReference.action.ReadValue<float>();

        m_LeftLittlePinched.isOn = m_LeftLittlePinchedReference.action.ReadValue<float>() > 0.5f;
        m_LeftLittleSlider.value = m_LeftLittleSliderReference.action.ReadValue<float>();

        m_RightFlags.text = ((MetaAimFlags)m_RightFlagsReference.action.ReadValue<int>()).ToString();

        m_RightIndexPinched.isOn = m_RightIndexPinchedReference.action.ReadValue<float>() > 0.5f;
        m_RightIndexSlider.value = m_RightIndexSliderReference.action.ReadValue<float>();

        m_RightMiddlePinched.isOn = m_RightMiddlePinchedReference.action.ReadValue<float>() > 0.5f;
        m_RightMiddleSlider.value = m_RightMiddleSliderReference.action.ReadValue<float>();

        m_RightRingPinched.isOn = m_RightRingPinchedReference.action.ReadValue<float>() > 0.5f;
        m_RightRingSlider.value = m_RightRingSliderReference.action.ReadValue<float>();

        m_RightLittlePinched.isOn = m_RightLittlePinchedReference.action.ReadValue<float>() > 0.5f;
        m_RightLittleSlider.value = m_RightLittleSliderReference.action.ReadValue<float>();
    }
}
