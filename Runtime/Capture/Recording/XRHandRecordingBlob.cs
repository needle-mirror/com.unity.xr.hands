using System;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Manages the state and data for a single XR hand recording session.
    /// This class manages the lifecycle of a recording,
    /// including capturing data from the <see cref="XRHandSubsystem"/>, saving and deleting the data.
    /// </summary>
    public class XRHandRecordingBlob : XRHandRecordingBase, IDisposable
    {
        /// <summary>
        /// Called when the status of the recording changes.
        /// </summary>
        public event Action<XRHandRecordingStatusChangedEventArgs> statusChanged;

        /// <summary>
        /// Called when a new frame is captured during a recording.
        /// </summary>
        public event Action<XRHandRecordingFrameCapturedEventArgs> frameCaptured;

        /// <summary>
        /// Creates a new instance of the <see cref="XRHandRecordingBlob"/> class.
        /// </summary>
        public XRHandRecordingBlob()
        {
            m_Frames = new List<byte[]>();
            m_FrameStream = new MemoryStream();
            m_FrameWriter = new BinaryWriter(m_FrameStream);
            Reset();
        }

        /// <summary>
        /// Disposes any captured in-memory data. All allocated resources will be freed.
        /// </summary>
        /// <remarks>
        /// Any unsaved data is lost.
        /// </remarks>
        public void Dispose()
        {
            Stop();
            m_Frames.Clear();
            m_FrameWriter?.Dispose();
            m_FrameStream?.Dispose();
            CleanupSubscription();
        }

        /// <summary>
        /// Resets the recording to its initial state and dispose any allocated resources.
        /// </summary>
        /// <remarks>
        /// Resetting this recording allows it to be reused for a new recording session.
        /// This method clears all captured data and unsubscribes from hand tracking updates,
        /// while preserving metadata like the asset name and elapsed time.
        /// </remarks>
        public void Reset()
        {
            Clear();
            m_FrameStream = new MemoryStream();
            m_FrameWriter = new BinaryWriter(m_FrameStream);
            m_DurationInSeconds = 0f;
            m_StartTime = 0f;
            m_AssetName = null;
            m_UniqueID = null;
            m_IsRecording = false;
            m_CurrentStatusChangedEventArgs = new XRHandRecordingStatusChangedEventArgs();
        }

        /// <summary>
        /// Clears the recording blob and release the memory used by the captured data.
        /// </summary>
        /// <remarks>
        /// This method clears all captured data and unsubscribes from hand tracking updates,
        /// while preserving metadata like the asset name and elapsed time.
        /// To make the object ready for a completely new recording, call <see cref="Reset()"/> instead.
        /// </remarks>
        public void Clear()
        {
            if (m_IsRecording)
                Stop();

            Dispose();
            CleanupSubscription();
            m_SequenceFlags = SequenceFlags.None;
            m_FrameBuffer.Dispose();
            m_FrameBuffer = default;
        }

        /// <summary>
        /// Initializes a new recording and prepares for capturing hand data, if possible.
        /// </summary>
        /// <param name="args">The initialization arguments <see cref="XRHandRecordingInitArgs"/>.</param>
        /// <returns><c>true</c> if the recording was successfully initialized; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// If successful, the recording begins on the subsequent frame update.
        /// Initializing a new recording can fail if a recording is already in progress or the
        /// <c>XRHandSubsystem</c> is null.
        /// It can also fail for other reasons. Refer to the player log
        /// for more detailed information about the cause of a failure.
        /// </remarks>
        public bool TryInitialize(XRHandRecordingInitializeArgs args)
        {
            if (args.subsystem == null)
            {
                Debug.LogError("Cannot start recording: XRHandSubsystem is null.");
                return false;
            }

            if (m_IsRecording)
            {
                Debug.LogWarning("Cannot start recording: a recording session is already in progress.");
                return false;
            }

            if (m_Frames.Count != 0)
            {
                Debug.LogWarning("Cannot start recording: would stomp over previously recorded data. If you want to re-use a recording, either call Reset() or get a successful call to TrySave() first.");
                return false;
            }

            try
            {
                CleanupSubscription();
                m_Subsystem = args.subsystem;

                m_SequenceFlags = SequenceFlags.None;
                var descriptor = m_Subsystem.subsystemDescriptor;
                if (descriptor.supportsAimPose)
                    m_SequenceFlags |= SequenceFlags.SupportsAimPose;
                if (descriptor.supportsAimActivateValue)
                    m_SequenceFlags |= SequenceFlags.SupportsAimActivateValue;
                if (descriptor.supportsGraspValue)
                    m_SequenceFlags |= SequenceFlags.SupportsGraspValue;
                if (descriptor.supportsGripPose)
                    m_SequenceFlags |= SequenceFlags.SupportsGripPose;
                if (descriptor.supportsPinchPose)
                    m_SequenceFlags |= SequenceFlags.SupportsPinchPose;
                if (descriptor.supportsPinchValue)
                    m_SequenceFlags |= SequenceFlags.SupportsPinchValue;
                if (descriptor.supportsPokePose)
                    m_SequenceFlags |= SequenceFlags.SupportsPokePose;

                // this one might not be immediately known, so we keep checking in OnUpdatedHands as well
                if (m_Subsystem.GetProvider().canSurfaceCommonPoseData)
                    m_SequenceFlags |= SequenceFlags.CanSurfaceCommonPoseData;

                m_RecordingOptions = args.recordingOptions;

                m_Subsystem.updatedHands += OnUpdatedHands;

                m_IsRecording = true;
                m_DurationInSeconds = 0f;
                m_StartTime = Constants.k_InvalidTime;
                m_UniqueID = GenerateUniqueID();

                NotifyStatusChanged(XRHandRecordingStatus.Ready);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to start recording: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the current recording session. The captured data remains in memory
        /// until it is saved, cleared or disposed.
        /// </summary>
        public void Stop()
        {
            StopRecordingInternal(XRHandRecordingStatus.StoppedManually);
        }

        /// <summary>
        /// Attempts to save the captured hand data to disk.
        /// </summary>
        /// <param name="args">The save arguments <see cref="XRHandRecordingSaveArgs"/>.</param>
        /// <returns>Return <c>true</c> if the data was saved successfully, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// If successful, the in-memory data is disposed to free up resources.
        /// Otherwise, the data remains in memory and should be managed accordingly.
        /// </remarks>
        public bool TrySave(XRHandRecordingSaveArgs args)
        {
            // Validate recording name
            if (string.IsNullOrEmpty(args.recordingName))
            {
                Debug.LogError("Recording name cannot be null or empty");
                return false;
            }

            m_AssetName = args.recordingName;

            try
            {
                var success = TryWriteRecordedDataToDisk(m_Subsystem);

                if (success)
                {
                    NotifyStatusChanged(XRHandRecordingStatus.Saved);

                    // Only dispose the data if saving was successful
                    Dispose();
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save recording: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the saved file of this recording from the device's storage.
        /// </summary>
        /// <remarks>
        /// Only call this method after the recording has been successfully saved.
        /// To dispose an unsaved recording, use <see cref="Dispose"/> instead.
        /// </remarks>
        public override void Delete()
        {
            if (m_IsRecording)
                Stop();

            Dispose();
            DeleteFileFromDisk(internalBinaryFileName);
        }

        /// <summary>
        /// The recording options supplied in the most recent successful call to
        /// <see cref="TryInitialize"/> in <see cref="XRHandRecordingInitializeArgs"/>.
        /// </summary>
        /// <remarks>
        /// This will be available in the resulting <see cref="XRHandCaptureSequence"/>
        /// through <see cref="XRHandCaptureSequence.optionsRecordedWith"/>.
        /// </remarks>
        internal XRHandRecordingOptions recordingOptions => m_RecordingOptions;

        void OnUpdatedHands(
            XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (!m_IsRecording || updateType == XRHandSubsystem.UpdateType.BeforeRender && !m_FrameBuffer.m_MarkedAsValid)
                return;

            if (m_StartTime == Constants.k_InvalidTime)
                m_StartTime = Time.timeSinceLevelLoad;

            OnUpdatedHandsImpl(subsystem, updateSuccessFlags, updateType);

            // OnUpdatedHandsImpl may stop the recording (e.g. when the time limit is reached) before a new m_FrameBuffer
            // has been created for the current frame, so guard against accessing it in AddRawFrameAndEmitEvents.
            if (!m_IsRecording)
            {
                m_FrameBuffer.Dispose();
                m_FrameBuffer = default;
                return;
            }

            if (XRHandCaptureSequence.IsLastRelevantUpdateTypeThisFrame(updateType, m_RecordingOptions))
                AddRawFrameAndEmitEvents();
        }

        void OnUpdatedHandsImpl(
            XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (subsystem.GetProvider().canSurfaceCommonPoseData)
                m_SequenceFlags |= SequenceFlags.CanSurfaceCommonPoseData;

            if (!m_IsRecording || !XRHandCaptureSequence.IsUpdateTypeRelevant(updateType, m_RecordingOptions))
                return;

            float durationIncludingThisFrame = Time.timeSinceLevelLoad - m_StartTime;
            if (durationIncludingThisFrame >= XRHandRecordingSettings.timeLimitInSeconds)
            {
                StopRecordingInternal(XRHandRecordingStatus.StoppedAtTimeLimit);
                return;
            }

            // skip over the before-render when the option is disabled, as well as the first before-render step of a recording if we missed the Dynamic step
            if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                m_FrameBuffer = new FrameBuffer(m_DurationInSeconds, m_RecordingOptions);

            CaptureUpdateStep(subsystem, updateSuccessFlags, updateType);
        }

        void AddRawFrameAndEmitEvents()
        {
            try
            {
                AddRawFrameAndEmitEventsImpl();
            }
            catch (Exception ex)
            {
                m_FrameBuffer.m_MarkedAsValid = false;
                StopRecordingInternal(XRHandRecordingStatus.StoppedWithError, ex.Message);
            }
            finally
            {
                m_FrameBuffer.Dispose();
                m_FrameBuffer = default;
            }
        }

        void CaptureUpdateStep(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            try
            {
                m_FrameBuffer.CaptureSingleUpdateStep(
                    subsystem,
                    updateSuccessFlags,
                    updateType);

                if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                    m_FrameBuffer.CaptureUpdateTypeAgnosticData(subsystem);

                m_DurationInSeconds = Time.timeSinceLevelLoad - m_StartTime;
            }
            catch (Exception ex)
            {
                m_FrameBuffer.m_MarkedAsValid = false;
                StopRecordingInternal(XRHandRecordingStatus.StoppedWithError, ex.Message);
            }
        }

        void AddRawFrameAndEmitEventsImpl()
        {
            m_Frames.Add(m_FrameBuffer.ConvertToRawFrame(m_FrameStream, m_FrameWriter));

            // Notify that recording has started on the first captured frame
            if (m_Frames.Count == 1)
            {
                m_StartTime = Time.timeSinceLevelLoad;
                NotifyStatusChanged(XRHandRecordingStatus.Recording);
            }

            var beforeRenderResult =
                XRHandCaptureSequence.IsBeforeRenderUpdateTypeRelevant(m_RecordingOptions)
                ? m_FrameBuffer.m_UpdateSuccessFlags[XRHandSubsystem.UpdateType.BeforeRender.ToIndex()]
                : XRHandSubsystem.UpdateSuccessFlags.None;

            NotifyFrameCaptured(
                m_FrameBuffer.m_UpdateSuccessFlags[XRHandSubsystem.UpdateType.Dynamic.ToIndex()],
                beforeRenderResult);
        }

        void StopRecordingInternal(XRHandRecordingStatus status, string errorMessage = null)
        {
            if (!m_IsRecording)
                return;

            m_IsRecording = false;
            CleanupSubscription();
            NotifyStatusChanged(status, errorMessage);
        }

        void NotifyStatusChanged(XRHandRecordingStatus status, string errorMessage = null)
        {
            try
            {
                var args = new XRHandRecordingStatusChangedEventArgs
                {
                    blob = this,
                    status = status,
                    elapsedTime = m_DurationInSeconds,
                    recordingName = m_AssetName,
                    subsystem = m_Subsystem,
                    errorMessage = errorMessage
                };

                if (args == m_CurrentStatusChangedEventArgs)
                    return;

                statusChanged?.Invoke(args);
                m_CurrentStatusChangedEventArgs = args;
            }
            catch
            {
                Debug.LogWarning($"Exception encountered when notifying of status change to '{status}' - there is no guarantee everything that subscribed to XRHandRecordingBlob.statusChanged has been called.");
            }
        }

        void NotifyFrameCaptured(
            XRHandSubsystem.UpdateSuccessFlags dynamicSuccess,
            XRHandSubsystem.UpdateSuccessFlags beforeRenderSuccess)
        {
            int frameIndex = -1;
            try
            {
                var args = new XRHandRecordingFrameCapturedEventArgs
                {
                    elapsedTime = m_DurationInSeconds,
                    subsystem = m_Subsystem,
                    updateSuccessFlags = dynamicSuccess,
                    updateSuccessFlagsBeforeRender = beforeRenderSuccess,
                    frameIndex = m_Frames.Count - 1
                };

                frameIndex = args.frameIndex;
                frameCaptured?.Invoke(args);
            }
            catch
            {
                if (frameIndex < 0)
                    Debug.LogWarning("Unknown exception encountered, unable to notify of captured frame.");
                else
                    Debug.LogWarning($"Exception encountered when notifying of captured frame #{frameIndex} - there is no guarantee everything that subscribed to XRHandRecordingBlob.frameCaptured has been called.");
            }
        }

        void CleanupSubscription()
        {
            if (m_Subsystem != null)
                m_Subsystem.updatedHands -= OnUpdatedHands;
        }

        struct WriteRecordingContext : IDisposable
        {
            public void Dispose()
            {
                m_Stream?.Dispose();
                m_Writer?.Dispose();
            }

            internal WriteRecordingContext(string savePath)
            {
                m_Stream = File.Open(savePath, FileMode.Create);
                m_Writer = new BinaryWriter(m_Stream);
            }

            internal BinaryWriter writer => m_Writer;

            FileStream m_Stream;
            BinaryWriter m_Writer;
        }

        [BurstDiscard]
        bool TryWriteRecordedDataToDisk(XRHandSubsystem subsystem)
        {
            try
            {
                var saveDirectory = GetDeviceStoragePath();
                if (!Directory.Exists(saveDirectory))
                    Directory.CreateDirectory(saveDirectory);

                var fullPath = Path.Combine(saveDirectory, internalBinaryFileName);
                using (var context = new WriteRecordingContext(fullPath))
                {
                    var writer = context.writer;

                    // Write the version number
                    writer.Write(XRHandRecordingBinaryFileFormatConfigs.k_Version);

                    // Write flags pertaining to the asset as a whole (not frame-specific)
                    writer.Write(m_SequenceFlags);

                    // Write the options we recorded with
                    writer.Write(m_RecordingOptions);

                    // Write the recording name string
                    writer.Write(m_AssetName);

                    // Write the total time of the recording
                    writer.Write(m_DurationInSeconds);

                    // Write the effectively constant data
                    writer.WriteHandJointLayout(subsystem.jointsInLayout);
                    writer.Write(subsystem.detectedHandMeshLayout);

                    // Write the gesture-related data (finger shape configurations)
                    foreach (var fingerID in HandsUtility.validFingerIDs)
                        writer.Write(XRFingerShapeMath.GetActiveConfiguration(fingerID));

                    // Write the frame count
                    writer.Write(m_Frames.Count);

                    foreach (var frame in m_Frames)
                        writer.Write(frame);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {internalBinaryFileName}: {e.Message}");
                return false;
            }
        }

        struct ReadRecordingContext : IDisposable
        {
            public void Dispose()
            {
                m_Stream?.Dispose();
                m_Reader?.Dispose();
            }

            internal ReadRecordingContext(string filePath)
            {
                m_Stream = File.Open(filePath, FileMode.Open);
                m_Reader = new BinaryReader(m_Stream);
            }

            internal BinaryReader reader => m_Reader;

            FileStream m_Stream;
            BinaryReader m_Reader;
        }

        /// <summary>
        /// Attempts to read an <see cref="XRHandCaptureSequence"/> from disk at the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the file containing the captured sequence data.</param>
        /// <param name="capturedData">
        /// When this method returns, contains the loaded <see cref="XRHandCaptureSequence"/> if successful;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the sequence was successfully read and loaded; <c>false</c> otherwise.
        /// </returns>
        [BurstDiscard]
        internal static bool TryReadCaptureSequenceFromDisk(string filePath, out XRHandCaptureSequence captureData)
        {
            captureData = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"File does not exist: {filePath}");
                    return false;
                }

                using (var context = new ReadRecordingContext(filePath))
                {
                    var reader = context.reader;
                    captureData = ScriptableObject.CreateInstance<XRHandCaptureSequence>();
                    captureData.InitializeBeforeRecordingImport();

                    // Read the version number
                    int version = reader.ReadInt32();
                    if (version != XRHandRecordingBinaryFileFormatConfigs.k_Version)
                    {
                        Debug.LogError($"XR Hand Capture data format version mismatch. File uses v{version}, " +
                            $"but this application requires v{XRHandRecordingBinaryFileFormatConfigs.k_Version}." +
                            $" Please use a compatible recording file or update the application.");
                        Object.DestroyImmediate(captureData);
                        captureData = null;
                        return false;
                    }

                    // Read flags pertaining to the asset as a whole (not frame-specific)
                    captureData.flags = reader.ReadSequenceFlags();

                    // Read the options that were recorded with
                    captureData.optionsRecordedWith = reader.ReadRecordingOptions();

                    // Read the recording name
                    captureData.name = reader.ReadString();

                    // Read the total time of the recording
                    captureData.durationInSeconds = reader.ReadSingle();

                    captureData.ReadJointsInLayoutPacked(reader);
                    captureData.ReadDetectedMeshLayout(reader);

                    foreach (var fingerID in HandsUtility.validFingerIDs)
                    {
                        reader.ReadFingerShapeConfiguration(out var defaultFingerShapeConfiguration);
                        captureData.SetFingerShapeConfigurationState(fingerID, defaultFingerShapeConfiguration);
                    }

                    // Read the frame count
                    int frameCount = reader.ReadInt32();
                    for (var i = 0; i < frameCount; ++i)
                    {
                        reader.ReadFrameBuffer(captureData.optionsRecordedWith, out var frameBuffer);
                        captureData.AddFrame(frameBuffer);
                        frameBuffer.Dispose();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to retrieve captured data from file {filePath}: {e.Message}");
                if (captureData != null)
                {
                    if (Application.isEditor)
                        Object.DestroyImmediate(captureData);
                    else
                        Object.Destroy(captureData);
                    captureData = null;
                }
                return false;
            }
        }

        float m_StartTime;
        bool m_IsRecording;
        List<byte[]> m_Frames;
        MemoryStream m_FrameStream;
        BinaryWriter m_FrameWriter;
        XRHandSubsystem m_Subsystem;
        SequenceFlags m_SequenceFlags;
        XRHandRecordingOptions m_RecordingOptions;
        XRHandRecordingStatusChangedEventArgs m_CurrentStatusChangedEventArgs;

        // only useful/useable when recording options allow for also capturing during on-before-render
        FrameBuffer m_FrameBuffer;

        static readonly List<XRHandSubsystem> s_SubsystemReuse;

        /***
         * Recording binary file format:
         *
         * [        4 bytes] FrameFlags
         *                    - IsLeftSnapshotValid and IsRightSnapshotValid: whether any associated XRHandCaptureSnapshot data was valid and included in file
         *                    - CanLeftCommonGesturesBeValid and CanRightCommonGesturesBeValid: whether any associated XRCommonHandGestures(State) data was valid and included in file
         *                    - CanLeftAimStateBeValid and CanRightAimStateBeValid: whether any associated XRHandAimState data was valid and included in file
         *                    - MarkedAsValid: this will be cleared is there was an exception encountered when recording this frame
         * [        4 bytes] (float) timestamp
         * [        4 bytes] InputTrackingState for head pose
         * [       28 bytes][Optional] head pose, if the above InputTrackingState was not InputTrackingState.None
         * [        4 bytes] XRHandSubsystem.UpdateSucessFlags for XRHandSubsystem.UpdateType.Dynamic step
         * [        4 bytes][Optional] XRHandSubsystem.UpdateSucessFlags for XRHandSubsystem.UpdateType.Before step (only present if XRHandRecordingOptions.AlsoCaptureBeforeRender was enabled before recording started)
         * [36 - 1532 bytes][Optional] left-hand XRHandCaptureSnapshot data, if there was any valid left-hand data for either udpate step this frame
         *                  [       4 bytes] SnapshotFlags
         *                                    - IsDynamicHandValid: whether any left-hand data was valid during the XRHandSubsystem.UpdateType.Dynamic step this frame
         *                                    - IsBeforeRenderHandValid: whether left-hand data was valid during the XRHandSubsystem.UpdateType.BeforeRender step this frame (was only attempted if the XRHandRecordingOptions.AlsoCaptureBeforeRender option was enabled)
         *                  [32 - 764 bytes][Optional] left-hand XRHand data associated with the XRHandSubsystem.UpdateType.Dynamic step, if any such data was available this frame
         *                                  [  4 bytes] HandFlags
         *                                              - AreAllJointPosesValid: whether the left-hand joint pose data during the XRHandSubsystem.UpdateType.Dynamic step is valid
         *                                              - WasHandTrackedDuringCapture: whether the left XRHand reported isTracked as true during the XRHandSubsystem.UpdateType.Dynamic step
         *                                  [ 28 bytes] left-hand XRHand.rootPose data during the XRHandSubsystem.UpdateType.Dynamic step
         *                                  [  4 bytes][Optional] number of joints written (if the joint data was valid)
         *                                  [728 bytes][Optional] 26 joint poses, 28 bytes per pose (if the joint data was valid)
         *                  [32 - 764 bytes][Optional] left-hand XRHand data associated with the XRHandSubsystem.UpdateType.BeforeRender step, if any such data was available this frame
         *                                  [  4 bytes] HandFlags
         *                                              - AreAllJointPosesValid: whether the left-hand joint pose data during the XRHandSubsystem.UpdateType.BeforeRender step is valid
         *                                              - WasHandTrackedDuringCapture: whether the left XRHand reported isTracked as true during the XRHandSubsystem.UpdateType.BeforeRender step
         *                                  [ 28 bytes] left-hand XRHand.rootPose data during the XRHandSubsystem.UpdateType.BeforeRender step
         *                                  [  4 bytes][Optional] number of joints written (if the joint data was valid)
         *                                  [728 bytes][Optional] 26 joint poses, 28 bytes per pose (if the joint data was valid)
         * [ 8 -  128 bytes][Optional] left-hand XRCommonHandGesturesState data, if there was any valid data this frame
         *                  [       4 bytes] XRCommonHandGesturesFlags
         *                                   - IsAimPoseValid: whether the left-hand aim pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsAimActivateValueValid: whether the left-hand aim activate value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsGraspValueValid: whether the left-hand grasp value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsGripPoseValid: whether the left-hand grip pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPinchPoseValid: whether the left-hand pinch pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPinchValueValid: whether the left-hand pinch value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPokePoseValid: whether the left-hand poke pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                  [      28 bytes][Optional] left-hand aim pose, if valid
         *                  [       4 bytes][Optional] left-hand aim activate value, if valid
         *                  [       4 bytes][Optional] left-hand grasp value, if valid
         *                  [      28 bytes][Optional] left-hand grip pose, if valid
         *                  [      28 bytes][Optional] left-hand pinch pose, if valid
         *                  [       4 bytes][Optional] left-hand pinch value, if valid
         *                  [      28 bytes][Optional] left-hand poke pose, if valid
         * [36 -   64 bytes][Optional] left-hand XRHandAimState data, if there was any valid data this frame
         *                  [       4 bytes] Handedness (Left or Right)
         *                  [       4 bytes] AimFlags
         *                                    - IsTracked: whether the left-hand aim data reported as tracked
         *                                    - IsIndexPressed: whether the left aim data reported with index pressing
         *                                    - IsMiddlePressed: whether the left aim data reported with middle pressing
         *                                    - IsRingPressed: whether the left aim data reported with ring pressing
         *                                    - IsLittlePressed: whether the left aim data reported with little pressing
         *                                    - IsAimPoseValid: whether the left aim pose is valid
         *                  [       4 bytes] InputTrackingState (as we normally build it based on platform-surfaced data)
         *                  [       4 bytes] reserved (for Meta Aim, used for most significant four bytes of their XrFlags)
         *                  [       4 bytes] reserved (for Meta Aim, used for least significant four bytes of their XrFlags)
         *                  [       4 bytes] (float) pinch strength: index
         *                  [       4 bytes] (float) pinch strength: middle
         *                  [       4 bytes] (float) pinch strength: ring
         *                  [       4 bytes] (float) pinch strength: little
         *                  [      28 bytes][Optional] aim pose, if valid (IsAimPoseValid flag will be set)
         * [36 - 1532 bytes][Optional] right-hand XRHandCaptureSnapshot data, if there was any valid right-hand data for either udpate step this frame
         *                  [       4 bytes] SnapshotFlags
         *                                    - IsDynamicHandValid: whether any right-hand data was valid during the XRHandSubsystem.UpdateType.Dynamic step this frame
         *                                    - IsBeforeRenderHandValid: whether right-hand data was valid during the XRHandSubsystem.UpdateType.BeforeRender step this frame (was only attempted if the XRHandRecordingOptions.AlsoCaptureBeforeRender option was enabled)
         *                  [32 - 764 bytes][Optional] right-hand XRHand data associated with the XRHandSubsystem.UpdateType.Dynamic step, if any such data was available this frame
         *                                  [  4 bytes] HandFlags
         *                                              - AreAllJointPosesValid: whether the right-hand joint pose data during the XRHandSubsystem.UpdateType.Dynamic step is valid
         *                                              - WasHandTrackedDuringCapture: whether the right XRHand reported isTracked as true during the XRHandSubsystem.UpdateType.Dynamic step
         *                                  [ 28 bytes] right-hand XRHand.rootPose data during the XRHandSubsystem.UpdateType.Dynamic step
         *                                  [  4 bytes][Optional] number of joints written (if the joint data was valid)
         *                                  [728 bytes][Optional] 26 joint poses, 28 bytes per pose (if the joint data was valid)
         *                  [32 - 764 bytes][Optional] right-hand XRHand data associated with the XRHandSubsystem.UpdateType.BeforeRender step, if any such data was available this frame
         *                                  [  4 bytes] HandFlags
         *                                              - AreAllJointPosesValid: whether the right-hand joint pose data during the XRHandSubsystem.UpdateType.BeforeRender step is valid
         *                                              - WasHandTrackedDuringCapture: whether the right XRHand reported isTracked as true during the XRHandSubsystem.UpdateType.BeforeRender step
         *                                  [ 28 bytes] right-hand XRHand.rootPose data during the XRHandSubsystem.UpdateType.BeforeRender step
         *                                  [  4 bytes][Optional] number of joints written (if the joint data was valid)
         *                                  [728 bytes][Optional] 26 joint poses, 28 bytes per pose (if the joint data was valid)
         * [ 8 -  128 bytes][Optional] right-hand XRCommonHandGesturesState data, if there was any valid data this frame
         *                  [       4 bytes] XRCommonHandGesturesFlags
         *                                   - IsAimPoseValid: whether the right-hand aim pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsAimActivateValueValid: whether the right-hand aim activate value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsGraspValueValid: whether the right-hand grasp value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsGripPoseValid: whether the right-hand grip pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPinchPoseValid: whether the right-hand pinch pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPinchValueValid: whether the right-hand pinch value was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                                   - IsPokePoseValid: whether the right-hand poke pose was valid during the XRHandSubsystem.UpdateType.Dynamic step
         *                  [      28 bytes][Optional] right-hand aim pose, if valid
         *                  [       4 bytes][Optional] right-hand aim activate value, if valid
         *                  [       4 bytes][Optional] right-hand grasp value, if valid
         *                  [      28 bytes][Optional] right-hand grip pose, if valid
         *                  [      28 bytes][Optional] right-hand pinch pose, if valid
         *                  [       4 bytes][Optional] right-hand pinch value, if valid
         *                  [      28 bytes][Optional] right-hand poke pose, if valid
         * [36 -   64 bytes][Optional] right-hand XRHandAimState data, if there was any valid data this frame
         *                  [       4 bytes] Handedness (Right or Right)
         *                  [       4 bytes] AimFlags
         *                                    - IsTracked: whether the right-hand aim data reported as tracked
         *                                    - IsIndexPressed: whether the right aim data reported with index pressing
         *                                    - IsMiddlePressed: whether the right aim data reported with middle pressing
         *                                    - IsRingPressed: whether the right aim data reported with ring pressing
         *                                    - IsLittlePressed: whether the right aim data reported with little pressing
         *                                    - IsAimPoseValid: whether the right aim pose is valid
         *                  [       4 bytes] InputTrackingState (as we normally build it based on platform-surfaced data)
         *                  [       4 bytes] reserved (for Meta Aim, used for most significant four bytes of their XrFlags)
         *                  [       4 bytes] reserved (for Meta Aim, used for least significant four bytes of their XrFlags)
         *                  [       4 bytes] (float) pinch strength: index
         *                  [       4 bytes] (float) pinch strength: middle
         *                  [       4 bytes] (float) pinch strength: ring
         *                  [       4 bytes] (float) pinch strength: little
         *                  [      28 bytes][Optional] aim pose, if valid (IsAimPoseValid flag will be set)
         */
    }
}
