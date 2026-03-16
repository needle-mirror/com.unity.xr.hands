namespace UnityEngine.XR.Hands
{
    static class Constants
    {
        // loop constraints / array sizes / etc.
        internal const int k_NumHands = 2; // see enum Handedness
        internal const int k_NumUpdateTypes = 2; // see enum XRHandSubsystem.UpdateType
        internal const int k_NumFingersPerHand = 5; // see enum XRHandFingerID
        internal const int k_NumDelegationTypes = 2; // see enum ProviderDelegationType
        internal const int k_NumJointsPerHand = 26; // see enum XRHandJointID
        internal const int k_NumBitsInByte = 8;

        // sentinel values
        internal const int k_InvalidIndex = -1;
        internal const float k_InvalidTime = -1f;

        internal const float k_Epsilon = 0.00001f;
        internal const string k_PlaybackDescriptorID = "Hand-Capture-Playback";
    }
}
