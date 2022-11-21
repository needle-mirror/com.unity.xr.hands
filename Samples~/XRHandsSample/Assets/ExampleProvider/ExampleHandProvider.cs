using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Example.ProviderImplementation
{
    /// <summary>
    /// Example hand tracking provider for the Oculus platform.
    /// </summary>
    public class ExampleHandProvider : XRHandSubsystemProvider
    {
        /// <inheritdoc/>
        public override void Start()
        {
        }

        /// <inheritdoc/>
        public override void Stop()
        {
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            if (m_LeftHandOpenPoses.IsCreated)
                m_LeftHandOpenPoses.Dispose();

            if (m_RightHandOpenPoses.IsCreated)
                m_RightHandOpenPoses.Dispose();

            if (m_LeftHandClosedPoses.IsCreated)
                m_LeftHandClosedPoses.Dispose();

            if (m_RightHandClosedPoses.IsCreated)
                m_RightHandClosedPoses.Dispose();

            ExampleHandExtensions.subsystem = null;
        }

        /// <inheritdoc/>
        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            // this uses data captured from a run on OpenXR, so the data layout matches that
            handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;

            m_LeftHandOpenRootPose = new Pose(
                new Vector3(-0.05319f, 1.14112f, 0.08206f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);
            m_LeftHandOpenPoses[XRHandJointID.Wrist.ToIndex()] = new Pose(
                new Vector3(-0.08227f, 1.09858f, 0.06240f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses[XRHandJointID.Palm.ToIndex()] = new Pose(
                new Vector3(-0.05319f, 1.14112f, 0.08206f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses[XRHandJointID.ThumbMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.03098f, 1.12033f, 0.05497f),
                new Quaternion(0.62813f, -0.40647f, 0.45152f, -0.48617f));
            m_LeftHandOpenPoses[XRHandJointID.ThumbProximal.ToIndex()] = new Pose(
                new Vector3(0.00508f, 1.12946f, 0.05049f),
                new Quaternion(0.56658f, -0.37898f, 0.40879f, -0.60683f));
            m_LeftHandOpenPoses[XRHandJointID.ThumbDistal.ToIndex()] = new Pose(
                new Vector3(0.04104f, 1.14418f, 0.05325f),
                new Quaternion(0.50136f, -0.31467f, 0.52384f, -0.61255f));
            m_LeftHandOpenPoses[XRHandJointID.ThumbTip.ToIndex()] = new Pose(
                new Vector3(0.06704f, 1.15292f, 0.06050f),
                new Quaternion(0.50136f, -0.31467f, 0.52384f, -0.61255f));
            m_LeftHandOpenPoses[XRHandJointID.IndexMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.04156f, 1.12615f, 0.06273f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses[XRHandJointID.IndexProximal.ToIndex()] = new Pose(
                new Vector3(-0.00694f, 1.18204f, 0.08261f),
                new Quaternion(0.33862f, -0.46716f, -0.16301f, -0.80033f));
            m_LeftHandOpenPoses[XRHandJointID.IndexIntermediate.ToIndex()] = new Pose(
                new Vector3(0.02092f, 1.21239f, 0.09721f),
                new Quaternion(0.20645f, -0.49184f, -0.11594f, -0.83787f));
            m_LeftHandOpenPoses[XRHandJointID.IndexDistal.ToIndex()] = new Pose(
                new Vector3(0.04267f, 1.22527f, 0.10928f),
                new Quaternion(0.19916f, -0.47258f, -0.13734f, -0.84743f));
            m_LeftHandOpenPoses[XRHandJointID.IndexTip.ToIndex()] = new Pose(
                new Vector3(0.06123f, 1.23835f, 0.12155f),
                new Quaternion(0.19916f, -0.47258f, -0.13734f, -0.84743f));
            m_LeftHandOpenPoses[XRHandJointID.MiddleMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.05418f, 1.12452f, 0.07682f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses[XRHandJointID.MiddleProximal.ToIndex()] = new Pose(
                new Vector3(-0.02412f, 1.18365f, 0.10172f),
                new Quaternion(0.29654f, -0.42389f, -0.22072f, -0.82684f));
            m_LeftHandOpenPoses[XRHandJointID.MiddleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.00409f, 1.21717f, 0.12471f),
                new Quaternion(0.07485f, -0.46811f, -0.11850f, -0.87249f));
            m_LeftHandOpenPoses[XRHandJointID.MiddleDistal.ToIndex()] = new Pose(
                new Vector3(0.02946f, 1.22484f, 0.14219f),
                new Quaternion(0.01800f, -0.47650f, -0.14103f, -0.86761f));
            m_LeftHandOpenPoses[XRHandJointID.MiddleTip.ToIndex()] = new Pose(
                new Vector3(0.05258f, 1.23078f, 0.15831f),
                new Quaternion(0.01800f, -0.47650f, -0.14103f, -0.86761f));
            m_LeftHandOpenPoses[XRHandJointID.RingMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.06652f, 1.12487f, 0.09424f),
                new Quaternion(0.37018f, -0.42667f, -0.23629f, -0.79063f));
            m_LeftHandOpenPoses[XRHandJointID.RingProximal.ToIndex()] = new Pose(
                new Vector3(-0.03655f, 1.17324f, 0.11937f),
                new Quaternion(0.28613f, -0.37813f, -0.29259f, -0.83039f));
            m_LeftHandOpenPoses[XRHandJointID.RingIntermediate.ToIndex()] = new Pose(
                new Vector3(-0.01585f, 1.20454f, 0.14410f),
                new Quaternion(0.05979f, -0.45370f, -0.22094f, -0.86126f));
            m_LeftHandOpenPoses[XRHandJointID.RingDistal.ToIndex()] = new Pose(
                new Vector3(0.00728f, 1.21384f, 0.16190f),
                new Quaternion(-0.02753f, -0.50001f, -0.17905f, -0.84686f));
            m_LeftHandOpenPoses[XRHandJointID.RingTip.ToIndex()] = new Pose(
                new Vector3(0.03065f, 1.21918f, 0.17654f),
                new Quaternion(-0.02753f, -0.50001f, -0.17905f, -0.84686f));
            m_LeftHandOpenPoses[XRHandJointID.LittleMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.06905f, 1.12129f, 0.10329f),
                new Quaternion(0.22237f, -0.38305f, -0.43656f, -0.78310f));
            m_LeftHandOpenPoses[XRHandJointID.LittleProximal.ToIndex()] = new Pose(
                new Vector3(-0.04770f, 1.15721f, 0.13526f),
                new Quaternion(0.23079f, -0.30136f, -0.37872f, -0.84409f));
            m_LeftHandOpenPoses[XRHandJointID.LittleIntermediate.ToIndex()] = new Pose(
                new Vector3(-0.03588f, 1.17908f, 0.16046f),
                new Quaternion(-0.02823f, -0.37711f, -0.34394f, -0.85947f));
            m_LeftHandOpenPoses[XRHandJointID.LittleDistal.ToIndex()] = new Pose(
                new Vector3(-0.02025f, 1.18402f, 0.17718f),
                new Quaternion(-0.08543f, -0.44714f, -0.31003f, -0.83466f));
            m_LeftHandOpenPoses[XRHandJointID.LittleTip.ToIndex()] = new Pose(
                new Vector3(-0.00056f, 1.18870f, 0.19236f),
                new Quaternion(-0.08543f, -0.44714f, -0.31003f, -0.83466f));

            m_RightHandOpenRootPose = new Pose(
                new Vector3(0.26061f, 1.04213f, -0.12217f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);
            m_RightHandOpenPoses[XRHandJointID.Wrist.ToIndex()] = new Pose(
                new Vector3(0.25036f, 1.00710f, -0.16350f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses[XRHandJointID.Palm.ToIndex()] = new Pose(
                new Vector3(0.26061f, 1.04213f, -0.12217f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses[XRHandJointID.ThumbMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.22277f, 1.02779f, -0.11895f),
                new Quaternion(-0.29443f, -0.09961f, 0.61688f, 0.72309f));
            m_RightHandOpenPoses[XRHandJointID.ThumbProximal.ToIndex()] = new Pose(
                new Vector3(0.20376f, 1.03914f, -0.08872f),
                new Quaternion(-0.17373f, 0.03486f, 0.54658f, 0.81844f));
            m_RightHandOpenPoses[XRHandJointID.ThumbDistal.ToIndex()] = new Pose(
                new Vector3(0.19858f, 1.05170f, -0.05222f),
                new Quaternion(-0.08241f, 0.04251f, 0.61551f, 0.78265f));
            m_RightHandOpenPoses[XRHandJointID.ThumbTip.ToIndex()] = new Pose(
                new Vector3(0.19663f, 1.05785f, -0.02459f),
                new Quaternion(-0.08241f, 0.04251f, 0.61551f, 0.78265f));
            m_RightHandOpenPoses[XRHandJointID.IndexMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.23565f, 1.03198f, -0.12363f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses[XRHandJointID.IndexProximal.ToIndex()] = new Pose(
                new Vector3(0.24581f, 1.07980f, -0.07552f),
                new Quaternion(-0.26745f, -0.00230f, -0.05316f, 0.96210f));
            m_RightHandOpenPoses[XRHandJointID.IndexIntermediate.ToIndex()] = new Pose(
                new Vector3(0.24686f, 1.10231f, -0.03806f),
                new Quaternion(-0.08818f, -0.01665f, -0.07783f, 0.99292f));
            m_RightHandOpenPoses[XRHandJointID.IndexDistal.ToIndex()] = new Pose(
                new Vector3(0.24632f, 1.10729f, -0.01050f),
                new Quaternion(-0.08070f, 0.00726f, -0.09635f, 0.99204f));
            m_RightHandOpenPoses[XRHandJointID.IndexTip.ToIndex()] = new Pose(
                new Vector3(0.24765f, 1.11246f, 0.01474f),
                new Quaternion(-0.08070f, 0.00726f, -0.09635f, 0.99204f));
            m_RightHandOpenPoses[XRHandJointID.MiddleMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.25356f, 1.02741f, -0.12805f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses[XRHandJointID.MiddleProximal.ToIndex()] = new Pose(
                new Vector3(0.27087f, 1.07717f, -0.08084f),
                new Quaternion(-0.25376f, 0.06876f, -0.13017f, 0.95600f));
            m_RightHandOpenPoses[XRHandJointID.MiddleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.28064f, 1.10029f, -0.03821f),
                new Quaternion(0.00277f, 0.03060f, -0.15586f, 0.98730f));
            m_RightHandOpenPoses[XRHandJointID.MiddleDistal.ToIndex()] = new Pose(
                new Vector3(0.28253f, 1.09981f, -0.00652f),
                new Quaternion(0.03529f, 0.02435f, -0.20304f, 0.97823f));
            m_RightHandOpenPoses[XRHandJointID.MiddleTip.ToIndex()] = new Pose(
                new Vector3(0.28434f, 1.09860f, 0.02220f),
                new Quaternion(0.03529f, 0.02435f, -0.20304f, 0.97823f));
            m_RightHandOpenPoses[XRHandJointID.RingMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.27434f, 1.02385f, -0.13037f),
                new Quaternion(-0.36011f, 0.06995f, -0.10860f, 0.92392f));
            m_RightHandOpenPoses[XRHandJointID.RingProximal.ToIndex()] = new Pose(
                new Vector3(0.28989f, 1.06311f, -0.08470f),
                new Quaternion(-0.30300f, 0.12805f, -0.19366f, 0.92428f));
            m_RightHandOpenPoses[XRHandJointID.RingIntermediate.ToIndex()] = new Pose(
                new Vector3(0.30581f, 1.08606f, -0.04948f),
                new Quaternion(-0.02105f, 0.04613f, -0.25712f, 0.96505f));
            m_RightHandOpenPoses[XRHandJointID.RingDistal.ToIndex()] = new Pose(
                new Vector3(0.30886f, 1.08658f, -0.01902f),
                new Quaternion(0.09422f, -0.01594f, -0.27039f, 0.95800f));
            m_RightHandOpenPoses[XRHandJointID.RingTip.ToIndex()] = new Pose(
                new Vector3(0.30779f, 1.08315f, 0.00885f),
                new Quaternion(0.09422f, -0.01594f, -0.27039f, 0.95800f));
            m_RightHandOpenPoses[XRHandJointID.LittleMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.28250f, 1.01819f, -0.12876f),
                new Quaternion(-0.33093f, 0.12084f, -0.34823f, 0.86869f));
            m_RightHandOpenPoses[XRHandJointID.LittleProximal.ToIndex()] = new Pose(
                new Vector3(0.30567f, 1.04402f, -0.08921f),
                new Quaternion(-0.31247f, 0.21947f, -0.28321f, 0.87977f));
            m_RightHandOpenPoses[XRHandJointID.LittleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.32560f, 1.05908f, -0.06413f),
                new Quaternion(-0.02639f, 0.12369f, -0.40234f, 0.90671f));
            m_RightHandOpenPoses[XRHandJointID.LittleDistal.ToIndex()] = new Pose(
                new Vector3(0.33135f, 1.05787f, -0.04147f),
                new Quaternion(0.06308f, 0.03026f, -0.41147f, 0.90874f));
            m_RightHandOpenPoses[XRHandJointID.LittleTip.ToIndex()] = new Pose(
                new Vector3(0.33230f, 1.05547f, -0.01630f),
                new Quaternion(0.06308f, 0.03026f, -0.41147f, 0.90874f));

            m_LeftHandClosedRootPose = new Pose(
                new Vector3(-0.05765f, 1.13135f, 0.07474f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);
            m_LeftHandClosedPoses[XRHandJointID.Wrist.ToIndex()] = new Pose(
                new Vector3(-0.08861f, 1.08868f, 0.05857f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses[XRHandJointID.Palm.ToIndex()] = new Pose(
                new Vector3(-0.05765f, 1.13135f, 0.07474f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses[XRHandJointID.ThumbMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.03546f, 1.10900f, 0.05120f),
                new Quaternion(0.59983f, -0.37735f, 0.50003f, -0.49777f));
            m_LeftHandClosedPoses[XRHandJointID.ThumbProximal.ToIndex()] = new Pose(
                new Vector3(0.00110f, 1.11724f, 0.05104f),
                new Quaternion(0.49478f, -0.30279f, 0.48325f, -0.65573f));
            m_LeftHandClosedPoses[XRHandJointID.ThumbDistal.ToIndex()] = new Pose(
                new Vector3(0.03519f, 1.13111f, 0.06377f),
                new Quaternion(0.14385f, -0.02208f, 0.61571f, -0.77442f));
            m_LeftHandClosedPoses[XRHandJointID.ThumbTip.ToIndex()] = new Pose(
                new Vector3(0.04211f, 1.13763f, 0.09051f),
                new Quaternion(0.14385f, -0.02208f, 0.61571f, -0.77442f));
            m_LeftHandClosedPoses[XRHandJointID.IndexMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.04663f, 1.11556f, 0.05720f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses[XRHandJointID.IndexProximal.ToIndex()] = new Pose(
                new Vector3(-0.01160f, 1.17225f, 0.07013f),
                new Quaternion(-0.00409f, -0.46362f, -0.05542f, -0.88429f));
            m_LeftHandClosedPoses[XRHandJointID.IndexIntermediate.ToIndex()] = new Pose(
                new Vector3(0.02426f, 1.17418f, 0.09505f),
                new Quaternion(-0.69617f, -0.34948f, 0.30606f, -0.54731f));
            m_LeftHandClosedPoses[XRHandJointID.IndexDistal.ToIndex()] = new Pose(
                new Vector3(0.02304f, 1.14684f, 0.08907f),
                new Quaternion(-0.86100f, -0.16993f, 0.42576f, -0.22028f));
            m_LeftHandClosedPoses[XRHandJointID.IndexTip.ToIndex()] = new Pose(
                new Vector3(0.00645f, 1.13230f, 0.07569f),
                new Quaternion(-0.86100f, -0.16993f, 0.42576f, -0.22028f));
            m_LeftHandClosedPoses[XRHandJointID.MiddleMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.05856f, 1.11476f, 0.07217f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses[XRHandJointID.MiddleProximal.ToIndex()] = new Pose(
                new Vector3(-0.02669f, 1.17402f, 0.09092f),
                new Quaternion(-0.12944f, -0.48207f, -0.05105f, -0.86501f));
            m_LeftHandClosedPoses[XRHandJointID.MiddleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.01523f, 1.16538f, 0.11574f),
                new Quaternion(-0.77011f, -0.33807f, 0.33994f, -0.42081f));
            m_LeftHandClosedPoses[XRHandJointID.MiddleDistal.ToIndex()] = new Pose(
                new Vector3(0.00764f, 1.13750f, 0.10257f),
                new Quaternion(-0.90967f, -0.07972f, 0.40663f, -0.02820f));
            m_LeftHandClosedPoses[XRHandJointID.MiddleTip.ToIndex()] = new Pose(
                new Vector3(-0.01353f, 1.13282f, 0.08361f),
                new Quaternion(-0.90967f, -0.07972f, 0.40663f, -0.02820f));
            m_LeftHandClosedPoses[XRHandJointID.RingMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.06963f, 1.11521f, 0.08838f),
                new Quaternion(0.36043f, -0.46840f, -0.25351f, -0.76578f));
            m_LeftHandClosedPoses[XRHandJointID.RingProximal.ToIndex()] = new Pose(
                new Vector3(-0.03721f, 1.16377f, 0.10986f),
                new Quaternion(-0.23550f, -0.48960f, -0.06269f, -0.83720f));
            m_LeftHandClosedPoses[XRHandJointID.RingIntermediate.ToIndex()] = new Pose(
                new Vector3(0.00096f, 1.14880f, 0.12827f),
                new Quaternion(-0.80554f, -0.37674f, 0.28964f, -0.35395f));
            m_LeftHandClosedPoses[XRHandJointID.RingDistal.ToIndex()] = new Pose(
                new Vector3(-0.00516f, 1.12465f, 0.11045f),
                new Quaternion(-0.89322f, -0.17439f, 0.40480f, 0.08882f));
            m_LeftHandClosedPoses[XRHandJointID.RingTip.ToIndex()] = new Pose(
                new Vector3(-0.02604f, 1.12332f, 0.09170f),
                new Quaternion(-0.89322f, -0.17439f, 0.40480f, 0.08882f));
            m_LeftHandClosedPoses[XRHandJointID.LittleMetacarpal.ToIndex()] = new Pose(
                new Vector3(-0.07120f, 1.11171f, 0.09767f),
                new Quaternion(0.20234f, -0.42523f, -0.44596f, -0.76116f));
            m_LeftHandClosedPoses[XRHandJointID.LittleProximal.ToIndex()] = new Pose(
                new Vector3(-0.04664f, 1.14787f, 0.12695f),
                new Quaternion(-0.33363f, -0.54099f, -0.08660f, -0.76715f));
            m_LeftHandClosedPoses[XRHandJointID.LittleIntermediate.ToIndex()] = new Pose(
                new Vector3(-0.01521f, 1.13306f, 0.13375f),
                new Quaternion(-0.80892f, -0.42266f, 0.23467f, -0.33457f));
            m_LeftHandClosedPoses[XRHandJointID.LittleDistal.ToIndex()] = new Pose(
                new Vector3(-0.01747f, 1.11575f, 0.11816f),
                new Quaternion(-0.87711f, -0.26861f, 0.37815f, 0.12461f));
            m_LeftHandClosedPoses[XRHandJointID.LittleTip.ToIndex()] = new Pose(
                new Vector3(-0.03523f, 1.11514f, 0.10014f),
                new Quaternion(-0.87711f, -0.26861f, 0.37815f, 0.12461f));

            m_RightHandClosedRootPose = new Pose(
                new Vector3(0.25479f, 1.03223f, -0.13096f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);
            m_RightHandClosedPoses[XRHandJointID.Wrist.ToIndex()] = new Pose(
                new Vector3(0.24707f, 0.99063f, -0.16631f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses[XRHandJointID.Palm.ToIndex()] = new Pose(
                new Vector3(0.25479f, 1.03223f, -0.13096f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses[XRHandJointID.ThumbMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.21906f, 1.01687f, -0.12384f),
                new Quaternion(-0.28182f, -0.05450f, 0.66163f, 0.69272f));
            m_RightHandClosedPoses[XRHandJointID.ThumbProximal.ToIndex()] = new Pose(
                new Vector3(0.20226f, 1.02879f, -0.09254f),
                new Quaternion(-0.14498f, 0.10806f, 0.58328f, 0.79189f));
            m_RightHandClosedPoses[XRHandJointID.ThumbDistal.ToIndex()] = new Pose(
                new Vector3(0.20234f, 1.04265f, -0.05614f),
                new Quaternion(0.25174f, 0.36066f, 0.51855f, 0.73325f));
            m_RightHandClosedPoses[XRHandJointID.ThumbTip.ToIndex()] = new Pose(
                new Vector3(0.22420f, 1.04391f, -0.03809f),
                new Quaternion(0.25174f, 0.36066f, 0.51855f, 0.73325f));
            m_RightHandClosedPoses[XRHandJointID.IndexMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.23109f, 1.02106f, -0.13030f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses[XRHandJointID.IndexProximal.ToIndex()] = new Pose(
                new Vector3(0.23717f, 1.07719f, -0.09244f),
                new Quaternion(-0.08083f, 0.09967f, -0.18174f, 0.97494f));
            m_RightHandClosedPoses[XRHandJointID.IndexIntermediate.ToIndex()] = new Pose(
                new Vector3(0.24695f, 1.08249f, -0.05017f),
                new Quaternion(0.58029f, -0.05886f, -0.22340f, 0.78096f));
            m_RightHandClosedPoses[XRHandJointID.IndexDistal.ToIndex()] = new Pose(
                new Vector3(0.23712f, 1.05784f, -0.04122f),
                new Quaternion(0.81297f, -0.10530f, -0.19315f, 0.53916f));
            m_RightHandClosedPoses[XRHandJointID.IndexTip.ToIndex()] = new Pose(
                new Vector3(0.22645f, 1.03570f, -0.04907f),
                new Quaternion(0.81297f, -0.10530f, -0.19315f, 0.53916f));
            m_RightHandClosedPoses[XRHandJointID.MiddleMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.24939f, 1.01643f, -0.13340f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses[XRHandJointID.MiddleProximal.ToIndex()] = new Pose(
                new Vector3(0.26251f, 1.07384f, -0.09560f),
                new Quaternion(-0.03191f, 0.05776f, -0.22096f, 0.97305f));
            m_RightHandClosedPoses[XRHandJointID.MiddleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.26877f, 1.07565f, -0.04656f),
                new Quaternion(0.73256f, -0.14112f, -0.19581f, 0.63647f));
            m_RightHandClosedPoses[XRHandJointID.MiddleDistal.ToIndex()] = new Pose(
                new Vector3(0.25396f, 1.04780f, -0.05015f),
                new Quaternion(0.95114f, -0.16705f, -0.17376f, 0.19297f));
            m_RightHandClosedPoses[XRHandJointID.MiddleTip.ToIndex()] = new Pose(
                new Vector3(0.24258f, 1.03763f, -0.07458f),
                new Quaternion(0.95114f, -0.16705f, -0.17376f, 0.19297f));
            m_RightHandClosedPoses[XRHandJointID.RingMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.26892f, 1.01290f, -0.13502f),
                new Quaternion(-0.43384f, 0.03012f, -0.12568f, 0.89167f));
            m_RightHandClosedPoses[XRHandJointID.RingProximal.ToIndex()] = new Pose(
                new Vector3(0.28167f, 1.05945f, -0.09577f),
                new Quaternion(0.00147f, 0.04437f, -0.27763f, 0.95966f));
            m_RightHandClosedPoses[XRHandJointID.RingIntermediate.ToIndex()] = new Pose(
                new Vector3(0.28546f, 1.05821f, -0.05100f),
                new Quaternion(0.72242f, -0.20494f, -0.25286f, 0.61005f));
            m_RightHandClosedPoses[XRHandJointID.RingDistal.ToIndex()] = new Pose(
                new Vector3(0.26662f, 1.03439f, -0.05492f),
                new Quaternion(0.92979f, -0.30130f, -0.15118f, 0.14783f));
            m_RightHandClosedPoses[XRHandJointID.RingTip.ToIndex()] = new Pose(
                new Vector3(0.25551f, 1.02762f, -0.07982f),
                new Quaternion(0.92979f, -0.30130f, -0.15118f, 0.14783f));
            m_RightHandClosedPoses[XRHandJointID.LittleMetacarpal.ToIndex()] = new Pose(
                new Vector3(0.27693f, 1.00765f, -0.13195f),
                new Quaternion(-0.39225f, 0.06217f, -0.36771f, 0.84087f));
            m_RightHandClosedPoses[XRHandJointID.LittleProximal.ToIndex()] = new Pose(
                new Vector3(0.29761f, 1.03995f, -0.09594f),
                new Quaternion(0.00464f, -0.00316f, -0.35123f, 0.93627f));
            m_RightHandClosedPoses[XRHandJointID.LittleIntermediate.ToIndex()] = new Pose(
                new Vector3(0.29728f, 1.03973f, -0.06053f),
                new Quaternion(0.68428f, -0.25641f, -0.32133f, 0.60229f));
            m_RightHandClosedPoses[XRHandJointID.LittleDistal.ToIndex()] = new Pose(
                new Vector3(0.27976f, 1.02429f, -0.06212f),
                new Quaternion(0.88115f, -0.41117f, -0.17219f, 0.15768f));
            m_RightHandClosedPoses[XRHandJointID.LittleTip.ToIndex()] = new Pose(
                new Vector3(0.26771f, 1.02021f, -0.08400f),
                new Quaternion(0.88115f, -0.41117f, -0.17219f, 0.15768f));
        }

        public override unsafe XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints)
        {
            if (updateType != XRHandSubsystem.UpdateType.Dynamic && m_FirstReported)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;
            if (Input.GetKeyDown(KeyCode.LeftControl) || !m_FirstReported)
            {
                m_LeftHasEventToFire = m_FirstReported;
                m_IsLeftOpen = !m_IsLeftOpen;

                var leftHandPoses = m_IsLeftOpen ? m_LeftHandOpenPoses : m_LeftHandClosedPoses;
                for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
                {
                    leftHandJoints[jointIndex] = XRHandProviderUtility.CreateJoint(
                        XRHandJointTrackingState.Pose,
                        XRHandJointIDUtility.FromIndex(jointIndex),
                        leftHandPoses[jointIndex]);
                }

                leftHandRootPose = m_IsLeftOpen ? m_LeftHandOpenRootPose : m_LeftHandClosedRootPose;
                successFlags |= XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt) || !m_FirstReported)
            {
                m_RightHasEventToFire = m_FirstReported;
                m_IsRightOpen = !m_IsRightOpen;

                var rightHandPoses = m_IsRightOpen ? m_RightHandOpenPoses : m_RightHandClosedPoses;
                for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
                {
                    rightHandJoints[jointIndex] = XRHandProviderUtility.CreateJoint(
                        XRHandJointTrackingState.Pose,
                        XRHandJointIDUtility.FromIndex(jointIndex),
                        rightHandPoses[jointIndex]);
                }

                rightHandRootPose = m_IsRightOpen ? m_RightHandOpenRootPose : m_RightHandClosedRootPose;
                successFlags |= XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
            }

            m_FirstReported = true;
            return successFlags;
        }

        internal void GetEventFireBools(
            out bool isLeftClosed, out bool leftHasEventToFire,
            out bool isRightClosed, out bool rightHasEventToFire)
        {
            isLeftClosed = !m_IsLeftOpen;
            isRightClosed = !m_IsRightOpen;

            leftHasEventToFire = m_LeftHasEventToFire;
            m_LeftHasEventToFire = false;

            rightHasEventToFire = m_RightHasEventToFire;
            m_RightHasEventToFire = false;
        }

        internal bool IsFistClosed(Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return !m_IsLeftOpen;

                case Handedness.Right:
                    return !m_IsRightOpen;

                default:
                    throw new ArgumentException("Handedness must be Left or Right! Only use hands provided by the subsystem, do not create your own.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = "Example-Hands",
                providerType = typeof(ExampleHandProvider),
                subsystemTypeOverride = typeof(ExampleHandSubsystem)
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }

        bool m_IsLeftOpen, m_IsRightOpen, m_LeftHasEventToFire, m_RightHasEventToFire, m_FirstReported;
        Pose m_LeftHandOpenRootPose, m_LeftHandClosedRootPose, m_RightHandOpenRootPose, m_RightHandClosedRootPose;
        NativeArray<Pose> m_LeftHandOpenPoses, m_LeftHandClosedPoses, m_RightHandOpenPoses, m_RightHandClosedPoses;
    }
}
