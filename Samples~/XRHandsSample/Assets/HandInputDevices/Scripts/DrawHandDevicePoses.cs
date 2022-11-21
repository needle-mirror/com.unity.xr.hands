using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Hands;

public class DrawHandDevicePoses : MonoBehaviour
{
    [SerializeField]
    XROrigin m_XROrigin;

    [SerializeField]
    GameObject m_WristPrefab;

    [SerializeField]
    GameObject m_IndexTipPrefab;

    [SerializeField]
    GameObject m_PalmPrefab;

    [SerializeField]
    GameObject m_ThumbTipPrefab;

    [SerializeField]
    InputActionAsset m_InputActionAsset;

    [SerializeField]
    InputActionReference m_LeftWristPositionReference;

    [SerializeField]
    InputActionReference m_LeftWristRotationReference;

    [SerializeField]
    InputActionReference m_LeftIndexTipPositionReference;

    [SerializeField]
    InputActionReference m_LeftIndexTipRotationReference;

    [SerializeField]
    InputActionReference m_LeftPalmPositionReference;

    [SerializeField]
    InputActionReference m_LeftPalmRotationReference;

    [SerializeField]
    InputActionReference m_LeftThumbTipPositionReference;

    [SerializeField]
    InputActionReference m_LeftThumbTipRotationReference;

    [SerializeField]
    InputActionReference m_RightWristPositionReference;

    [SerializeField]
    InputActionReference m_RightWristRotationReference;

    [SerializeField]
    InputActionReference m_RightIndexTipPositionReference;

    [SerializeField]
    InputActionReference m_RightIndexTipRotationReference;

    [SerializeField]
    InputActionReference m_RightPalmPositionReference;

    [SerializeField]
    InputActionReference m_RightPalmRotationReference;

    [SerializeField]
    InputActionReference m_RightThumbTipPositionReference;

    [SerializeField]
    InputActionReference m_RightThumbTipRotationReference;

    void Start()
    {
        m_InputActionAsset.Enable();

        m_LeftHandData = new HandData(
            m_XROrigin,
            m_WristPrefab, m_IndexTipPrefab, m_PalmPrefab, m_ThumbTipPrefab,
            m_LeftWristPositionReference, m_LeftWristRotationReference,
            m_LeftIndexTipPositionReference, m_LeftIndexTipRotationReference,
            m_LeftPalmPositionReference, m_LeftPalmRotationReference,
            m_LeftThumbTipPositionReference, m_LeftThumbTipRotationReference);

        m_RightHandData = new HandData(
            m_XROrigin,
            m_WristPrefab, m_IndexTipPrefab, m_PalmPrefab, m_ThumbTipPrefab,
            m_RightWristPositionReference, m_RightWristRotationReference,
            m_RightIndexTipPositionReference, m_RightIndexTipRotationReference,
            m_RightPalmPositionReference, m_RightPalmRotationReference,
            m_RightThumbTipPositionReference, m_RightThumbTipRotationReference);
    }

    void Update()
    {
        m_LeftHandData.Update();
        m_RightHandData.Update();
    }

    class HandData
    {
        public HandData(
            XROrigin origin,
            GameObject wristPrefab, GameObject indexTipPrefab, GameObject palmPrefab, GameObject thumbTipPrefab,
            InputActionReference wristPositionReference, InputActionReference wristRotationReference,
            InputActionReference indexTipPositionReference, InputActionReference indexTipRotationReference,
            InputActionReference palmPositionReference, InputActionReference palmRotationReference,
            InputActionReference thumbTipPositionReference, InputActionReference thumbTipRotationReference)
        {
            m_Origin = origin;

            m_Wrist = GameObject.Instantiate(wristPrefab);
            m_IndexTip = GameObject.Instantiate(indexTipPrefab);
            m_Palm = GameObject.Instantiate(palmPrefab);
            m_ThumbTip = GameObject.Instantiate(thumbTipPrefab);

            m_WristPositionReference = wristPositionReference;
            m_WristRotationReference = wristRotationReference;

            m_IndexTipPositionReference = indexTipPositionReference;
            m_IndexTipRotationReference = indexTipRotationReference;

            m_PalmPositionReference = palmPositionReference;
            m_PalmRotationReference = palmRotationReference;

            m_ThumbTipPositionReference = thumbTipPositionReference;
            m_ThumbTipRotationReference = thumbTipRotationReference;
        }

        public void Update()
        {
            var originPose = new Pose(
                m_Origin.transform.position,
                m_Origin.transform.rotation);

            UpdatePose(
                m_WristPositionReference,
                m_WristRotationReference,
                originPose,
                m_Wrist.transform);

            UpdatePose(
                m_IndexTipPositionReference,
                m_IndexTipRotationReference,
                originPose,
                m_IndexTip.transform);

            UpdatePose(
                m_PalmPositionReference,
                m_PalmRotationReference,
                originPose,
                m_Palm.transform);

            UpdatePose(
                m_ThumbTipPositionReference,
                m_ThumbTipRotationReference,
                originPose,
                m_ThumbTip.transform);
        }

        void UpdatePose(
            InputActionReference positionReference,
            InputActionReference rotationReference,
            Pose originPose,
            Transform xform)
        {
            var pose = new Pose(positionReference.action.ReadValue<Vector3>(), rotationReference.action.ReadValue<Quaternion>());
            pose = pose.GetTransformedBy(originPose);
            xform.position = pose.position;
            xform.rotation = pose.rotation;
        }

        XROrigin m_Origin;
        GameObject m_Wrist, m_IndexTip, m_Palm, m_ThumbTip;
        InputActionReference m_WristPositionReference, m_WristRotationReference;
        InputActionReference m_IndexTipPositionReference, m_IndexTipRotationReference;
        InputActionReference m_PalmPositionReference, m_PalmRotationReference;
        InputActionReference m_ThumbTipPositionReference, m_ThumbTipRotationReference;
    }

    HandData m_LeftHandData, m_RightHandData;
}
