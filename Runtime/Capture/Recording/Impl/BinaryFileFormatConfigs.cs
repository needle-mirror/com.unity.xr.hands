namespace UnityEngine.XR.Hands.Capture.Recording
{
    static class XRHandRecordingBinaryFileFormatConfigs
    {
        /***
         * Recording binary file format:
         *
         * Header:
         * - int: version number of the file format                               [4 bytes]
         * - string: recording name string                                        [variable length]
         * - int: XRHandRecordingOptions flags                                    [4 bytes]
         * - int: XRDetectedHandMeshLayout                                        [4 bytes]
         * - int: XRHandJointID count (number of joints per hand)                 [4 bytes]
         * - float: total time of the recording in seconds                        [4 bytes]
         * - int: frame count                                                     [4 bytes]
         *
         * For each frame:
         * - int: FrameFlags (which hands/data are present)                       [4 bytes]
         * - float: timestamp in seconds                                          [4 bytes]
         * - int: InputTrackingState for head pose                                [4 bytes]
         * - Pose: head pose (only if head tracking state != 0):
         *   - float x, y, z (position)                                           [12 bytes]
         *   - float x, y, z, w (rotation)                                        [16 bytes]
         * - For each relevant UpdateType:
         *   - int: XRHandSubsystem.UpdateSuccessFlags                            [4 bytes]
         * - For each hand (Left, Right):
         *   - If snapshot flag set in FrameFlags:
         *     - int: SnapshotFlags                                               [4 bytes]
         *     - For each relevant UpdateType with snapshot data:
         *       - int: HandFlags                                                 [4 bytes]
         *       - For each tracked joint:
         *         - int: XRHandJointTrackingState                                [4 bytes]
         *         - float x, y, z (position)                                     [12 bytes]
         *         - float x, y, z, w (rotation)                                  [16 bytes]
         *   - If common gestures flag set in FrameFlags:
         *     - int: Handedness                                                  [4 bytes]
         *     - int: XRCommonHandGesturesFlags                                   [4 bytes]
         *     - If IsAimPoseValid: Pose (position + rotation)                    [28 bytes]
         *     - If IsAimActivateValueValid: float                                [4 bytes]
         *     - If IsGraspValueValid: float                                      [4 bytes]
         *     - If IsGripPoseValid: Pose                                         [28 bytes]
         *     - If IsPinchPoseValid: Pose                                        [28 bytes]
         *     - If IsPinchValueValid: float                                      [4 bytes]
         *     - If IsPokePoseValid: Pose                                         [28 bytes]
         *     - If IsAimActivatedStateValid: bool                                [1 byte]
         *     - If IsGraspFirmStateValid: bool                                   [1 byte]
         *     - If IsPinchTouchedStateValid: bool                                [1 byte]
         *   - If aim state flag set in FrameFlags:
         *     - int: Handedness                                                  [4 bytes]
         *     - int: AimFlags                                                    [4 bytes]
         *     - int: InputTrackingState                                          [4 bytes]
         *     - int: reserved0                                                   [4 bytes]
         *     - int: reserved1                                                   [4 bytes]
         *     - float: pinchStrengthIndex                                        [4 bytes]
         *     - float: pinchStrengthMiddle                                       [4 bytes]
         *     - float: pinchStrengthRing                                         [4 bytes]
         *     - float: pinchStrengthLittle                                       [4 bytes]
         *     - If IsAimPoseTracked: Pose                                        [28 bytes]
         */

        // Current version of the binary file format, update this when the format changes
        internal const int k_Version = 2;

        // File extension used for recording binary files stored in the device's persistent data directory.
        internal const string k_FileExtension = ".handsbin";
    }
}
