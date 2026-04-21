using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine.XR.Hands.Capture.Recording;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// A sequence of captured frames, providing data for hand tracking over a period of time.
    /// </summary>
    public class XRHandCaptureSequence : ScriptableObject
    {
        /// <summary>
        /// The duration of the entire captured sequence in seconds.
        /// </summary>
        /// <value> The duration in seconds.</value>
        public float durationInSeconds
        {
            get => m_DurationInSeconds;
            internal set => m_DurationInSeconds = value;
        }

        /// <summary>
        /// The captured frames in this sequence.
        /// </summary>
        /// <value> A read-only list of <see cref="XRHandCaptureFrame"/> objects. </value>
        public IReadOnlyList<XRHandCaptureFrame> frames => m_Frames;

        /// <summary>
        /// Gets the same joint layout that was retrieved during capture from
        /// <see cref="XRHandSubsystem"/><c>.</c><see cref="XRHandSubsystem.jointsInLayout"/>.
        /// </summary>
        /// <param name="allocator">
        /// This will be passed to the <see cref="NativeArray{bool}"/> created and returned from
        /// <c>GetHandJointsInLayout</c>.
        /// </param>
        /// <returns>
        /// The same layout reported during capture for <see cref="XRHandSubsystem.jointsInLayout"/>.
        /// Each index will have a value of <see langword="true"/> if the capture can ever
        /// report valid <see cref="XRHandJoint"/>s in captured <see cref="XRHand"/>s
        /// if that <c>XRHandJoint</c> has an <see cref="XRHandJointID"/> that you can call
        /// <c>XRHandJointID.</c><see cref="XRHandJointIDUtility.ToIndex"/>.
        /// </returns>
        public NativeArray<bool> GetHandJointsInLayout(Allocator allocator)
        {
            var jointsInLayoutExtracted = new NativeArray<bool>(Constants.k_NumJointsPerHand, allocator);
            if (m_JointsInLayoutPacked != null)
            {
                int totalJoints = jointsInLayoutExtracted.Length;
                int numFullBytes = totalJoints / Constants.k_NumBitsInByte;
                int numRemainingBits = totalJoints % Constants.k_NumBitsInByte;

                for (int packedByteIndex = 0; packedByteIndex < numFullBytes; ++packedByteIndex)
                {
                    byte packedByte = m_JointsInLayoutPacked[packedByteIndex];
                    for (int bit = 0, extractedIndex = Constants.k_NumBitsInByte * packedByteIndex;
                         bit < Constants.k_NumBitsInByte;
                         ++bit, ++extractedIndex)
                        jointsInLayoutExtracted[extractedIndex] = (packedByte & 1 << (Constants.k_NumBitsInByte - 1 - bit)) != 0;
                }

                if (numRemainingBits > 0)
                {
                    byte packedByte = m_JointsInLayoutPacked[numFullBytes];
                    for (int bit = 0, extractedIndex = Constants.k_NumBitsInByte * numFullBytes;
                         bit < numRemainingBits;
                         ++bit, ++extractedIndex)
                        jointsInLayoutExtracted[extractedIndex] = (packedByte & 1 << (numRemainingBits - 1 - bit)) != 0;
                }
            }
            else
            {
                for (int jointIndex = 0; jointIndex < jointsInLayoutExtracted.Length; ++jointIndex)
                    jointsInLayoutExtracted[jointIndex] = true;
            }

            return jointsInLayoutExtracted;
        }

        /// <summary>
        /// Reports what <see cref="XRHandSubsystem"/><c>.</c><see cref="canSurfaceCommonPoseData"/>
        /// returned during capture.
        /// </summary>
        /// <value>
        /// If <c>canSurfaceCommonPoseData</c> is <see langword="false"/>, no call to
        /// <see cref="XRHandCaptureFrame"/><c>.</c><see cref="XRHandCaptureFrame.TryGetCommonGestures"/>.
        /// will ever succeed for frames retrieved from this asset's <see cref="frames"/>.
        /// </value>
        public bool canSurfaceCommonPoseData => (m_SequenceFlags & SequenceFlags.CanSurfaceCommonPoseData) != 0;

        /// <summary>
        /// Reports what <see cref="XRHandSubsystem"/><c>.</c><see cref="detectedHandMeshLayout"/>
        /// returned during capture.
        /// </summary>
        public XRDetectedHandMeshLayout detectedHandMeshLayout => m_DetectedHandMeshLayout;

        /// <summary>
        /// Gets the <see cref="XRFingerShapeConfiguration"/> data for the given
        /// <see cref="XRHandFingerID"/> that was active at the end of capture
        /// that resulted in this asset.
        /// </summary>
        /// <param name="fingerID">
        /// The finger to get captured finger shape configuration for.
        /// </param>
        /// <param name="state">
        /// Will be filled out with state representing the active <see cref="XRFingerShapeConfigurationState"/>
        /// at the end of when the tracking data was captured.
        /// </param>
        public void GetLastActiveFingerShapeConfigurationState(XRHandFingerID fingerID, out XRFingerShapeConfigurationState state)
            => GetFingerShapeConfigurationState(fingerID, out state);

        /// <summary>
        /// The options that the tracking data was captured with.
        /// </summary>
        /// <value>
        /// This is the same as what was supplied to <see cref="XRHandRecordingBlob"/><c>.</c><see cref="XRHandRecordingBlob.TryInitialize"/>
        /// through <see cref="XRHandRecordingInitializeArgs"/><c>.</c><see cref="XRHandRecordingInitializeArgs.recordingOptions"/>
        /// when the data in this asset was captured.
        /// </value>
        internal XRHandRecordingOptions optionsRecordedWith
        {
            get => m_OptionsRecordedWith;
            set => m_OptionsRecordedWith = value;
        }

        internal void SetFingerShapeConfigurationState(XRHandFingerID fingerID, in XRFingerShapeConfigurationState state)
            => m_FingerShapeConfigurationStates[fingerID.ToIndex()] = state;

        internal void AddFrame(in FrameBuffer frameBuffer)
            => m_Frames.Add(new XRHandCaptureFrame(this, frameBuffer));

        internal void InitializeBeforeRecordingImport()
        {
            hideFlags = hideFlags | HideFlags.HideInInspector;
            m_InternalLayoutVersion = k_LayoutVersion_1_8_0;
            m_FingerShapeConfigurationStates = new XRFingerShapeConfigurationState[Constants.k_NumFingersPerHand];
            m_Frames = new List<XRHandCaptureFrame>();
            m_FlattenedUpdateSuccessFlags = new List<XRHandSubsystem.UpdateSuccessFlags>();
            m_FlattenedHandSnapshots = new List<XRHandCaptureSnapshot>();
            m_FlattenedHandFlags = new List<HandFlags>();
            m_FlattenedHandJoints = new List<XRHandJoint>();
            m_FlattenedHandRootPoses = new List<Pose>();
            m_FlattenedCommonGestures = new List<XRCommonHandGesturesState>();
            m_FlattenedAimStates = new List<XRHandAimState>();
        }

        internal SequenceFlags flags
        {
            get => m_SequenceFlags;
            set => m_SequenceFlags = value;
        }

        internal List<XRHandSubsystem.UpdateSuccessFlags> flattenedUpdateSuccessFlags => m_FlattenedUpdateSuccessFlags;
        internal List<XRHandCaptureSnapshot> flattenedHandSnapshots => m_FlattenedHandSnapshots;
        internal List<HandFlags> flattenedHandFlags => m_FlattenedHandFlags;
        internal List<XRHandJoint> flattenedHandJoints => m_FlattenedHandJoints;
        internal List<Pose> flattenedHandRootPoses => m_FlattenedHandRootPoses;
        internal List<XRCommonHandGesturesState> flattenedCommonGestures => m_FlattenedCommonGestures;
        internal List<XRHandAimState> flattenedAimStates => m_FlattenedAimStates;

        internal int GetClosestFrameIndex(float elapsedTime)
        {
            int TimeBasedSearchForClosestFrameIndex(List<XRHandCaptureFrame> frames, float requestedTime, int begin, int end)
            {
                if (begin + 1 == end)
                    return begin;

                int middle = (begin + end) / 2;
                return requestedTime < frames[middle].timestamp
                    ? TimeBasedSearchForClosestFrameIndex(frames, requestedTime, begin, middle)
                    : TimeBasedSearchForClosestFrameIndex(frames, requestedTime, middle, end);
            }

            if (elapsedTime < 0f || m_Frames == null || m_Frames.Count == 0)
                return 0;

            if (elapsedTime > m_DurationInSeconds)
                return m_Frames.Count - 1;

            return TimeBasedSearchForClosestFrameIndex(m_Frames, elapsedTime, 0, m_Frames.Count);
        }

        internal void ReadJointsInLayoutPacked(BinaryReader reader)
            => m_JointsInLayoutPacked = reader.ReadHandJointLayout();
        internal void ReadDetectedMeshLayout(BinaryReader reader)
            => m_DetectedHandMeshLayout = reader.ReadDetectedMeshLayout();

        internal static int GetNumRelevantUpdateTypes(XRHandRecordingOptions recordingOptions)
            => IsBeforeRenderUpdateTypeRelevant(recordingOptions) ? Constants.k_NumUpdateTypes : 1;
        internal static bool IsDynamicUpdateTypeRelevant(XRHandRecordingOptions recordingOptions)
            => IsUpdateTypeRelevant(XRHandSubsystem.UpdateType.Dynamic, recordingOptions);
        internal static bool IsBeforeRenderUpdateTypeRelevant(XRHandRecordingOptions recordingOptions)
            => IsUpdateTypeRelevant(XRHandSubsystem.UpdateType.BeforeRender, recordingOptions);
        internal static bool IsUpdateTypeRelevant(XRHandSubsystem.UpdateType updateType, XRHandRecordingOptions recordingOptions)
            => updateType == XRHandSubsystem.UpdateType.Dynamic || recordingOptions.IsAnyFlagEnabled(XRHandRecordingOptions.AlsoCaptureBeforeRender);
        internal static bool IsLastRelevantUpdateTypeThisFrame(XRHandSubsystem.UpdateType updateType, XRHandRecordingOptions recordingOptions)
            => (updateType == XRHandSubsystem.UpdateType.BeforeRender) == IsBeforeRenderUpdateTypeRelevant(recordingOptions);

        void Awake() => MigrateFromUnflattenedRepresentationIfNeeded();

        void MigrateFromUnflattenedRepresentationIfNeeded()
        {
            if (m_InternalLayoutVersion == k_LayoutVersion_1_8_0)
                return;

            if (m_Frames != null && m_Frames.Count != 0)
            {
                for (int frameIndex = 0; frameIndex < m_Frames.Count; ++frameIndex)
                    m_Frames[frameIndex] = new XRHandCaptureFrame(this, m_Frames[frameIndex], frameIndex);
            }

            m_InternalLayoutVersion = k_LayoutVersion_1_8_0;
        }

        void GetFingerShapeConfigurationState(XRHandFingerID fingerID, out XRFingerShapeConfigurationState state)
            => state = m_FingerShapeConfigurationStates[fingerID.ToIndex()];

        // XRHandCaptureSequence (simplified version unflattened structure)
        // +--List<XRHandCaptureFrame>
        //    +--XRHandCaptureSnapshot[] (up to two, depending on whether that hand was visible during either/one update step when the frame was recorded)
        //       +--SerializedHand[] (up to two, depending on whether before-render was enabled during recording and whether either/one update step had any visible hand(s))
        //          +--XRHandJoint[] (enough for every joint, since we currently assume every joint is available during recording to be valid for capture)
        //    +--XRCommonGesturesState[] (up to two, depending on whether the data was enabled/available and the hand was visible when the frame was captured)
        //    +--XRHandAimState[] (up to two, depending on whether the data was enabled/available and the hand was visible when the frame was captured)

        // XRHandCaptureSequence (unflattened structure)
        // +--SequenceFlags
        // +--XRHandRecordingOptions
        // +--XRDetectedHandMeshLayout
        // +--XRFingerShapeConfigurationState[]
        // +--byte[] (support for GetHandLayout)
        // +--List<XRHandCaptureFrame>

        // unflattened structure (XRHandCaptureFrame in detail)
        // +--FrameFlags
        // +--timestamp (float)
        // +--head pose data (InputTrackingState and, if position or rotation is valid, the pose itself)
        // +--XRHandCaptureSnapshot[] (up to two, depending on whether that hand was visible during either/one update step when the frame was recorded)
        //    +--Handedness
        //    +--SnapshotFlags
        //    +--SerializedHand[] (up to two, depending on whether before-render was enabled during recording and whether either/one update step had any visible hand(s))
        //       +--HandFlags
        //       +--XRHandJoint[] (enough for every joint, since we currently assume every joint is available during recording to be valid for capture)
        // +--XRCommonGesturesState[] (up to two, depending on whether the data was enabled/available and the hand was visible when the frame was captured)
        //    (nothing specific to capture here, this is just a mirror of XRHandCommonGestures)
        // +--XRHandAimState[] (up to two, depending on whether the data was enabled/available and the hand was visible when the frame was captured)
        //    (nothing specific to capture here, this is just a mirror of per-hand Meta aim data)

        [SerializeField]
        SequenceFlags m_SequenceFlags;

        [SerializeField]
        [Tooltip("The duration of the entire captured sequence in seconds.")]
        float m_DurationInSeconds;

        [SerializeField]
        XRHandRecordingOptions m_OptionsRecordedWith;

        [SerializeField]
        XRDetectedHandMeshLayout m_DetectedHandMeshLayout;

        [SerializeField]
        XRFingerShapeConfigurationState[] m_FingerShapeConfigurationStates;

        [SerializeField]
        byte[] m_JointsInLayoutPacked;

        [SerializeField]
        List<XRHandCaptureFrame> m_Frames;

        [SerializeField]
        List<XRHandSubsystem.UpdateSuccessFlags> m_FlattenedUpdateSuccessFlags;

        [SerializeField]
        List<XRHandCaptureSnapshot> m_FlattenedHandSnapshots;

        [SerializeField]
        List<HandFlags> m_FlattenedHandFlags;

        [SerializeField]
        List<XRHandJoint> m_FlattenedHandJoints;

        [SerializeField]
        List<Pose> m_FlattenedHandRootPoses;

        [SerializeField]
        List<XRCommonHandGesturesState> m_FlattenedCommonGestures;

        [SerializeField]
        List<XRHandAimState> m_FlattenedAimStates;

        [SerializeField]
        int m_InternalLayoutVersion;

        const int k_LayoutVersion_1_7_0 = 0;
        const int k_LayoutVersion_1_8_0 = 1;
    }
}
