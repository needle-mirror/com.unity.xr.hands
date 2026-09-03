#if OPENXR_1_19_OR_NEWER
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    public class MetaHandTrackingWideMotionModeTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;
        HandTrackingDataSourceFeature m_DataSourceFeature;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.Environment.AddSupportedExtension("XR_EXT_hand_tracking_data_source", 1);
            m_MockEnvironment.Environment.AddSupportedExtension(MetaHandTrackingWideMotionMode.extensionString, 1);
            m_MockEnvironment.Environment.Settings.EnableFeature<HandTrackingDataSourceFeature>(true);
            m_MockEnvironment.Environment.Settings.EnableFeature<MetaHandTrackingWideMotionMode>(true);
            m_DataSourceFeature = m_MockEnvironment.Environment.Settings.GetFeature<HandTrackingDataSourceFeature>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_MockEnvironment?.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            m_MockEnvironment?.Stop();
            if (m_MockEnvironment != null)
            {
                m_MockEnvironment.createHandTracker.mock = null;
                m_MockEnvironment.locateHandJoints.mock = null;
                m_MockEnvironment.destroyHandTracker.mock = null;
            }
        }

        [UnityTest]
        public IEnumerator OnInstanceCreate_ConfiguresDataSourceWithWideMotion()
        {
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJointsActive;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            Assert.IsTrue(m_DataSourceFeature.TryGetConfiguration(out var config),
                "TryGetConfiguration should return true after environment starts.");

            Assert.Contains(HandTrackingDataSource.Unobstructed, config.leftPreferredSources,
                "Left hand should include Unobstructed.");
            Assert.Contains(HandTrackingDataSource.UnobstructedWideMotion, config.leftPreferredSources,
                "Left hand should include UnobstructedWideMotion.");

            Assert.Contains(HandTrackingDataSource.Unobstructed, config.rightPreferredSources,
                "Right hand should include Unobstructed.");
            Assert.Contains(HandTrackingDataSource.UnobstructedWideMotion, config.rightPreferredSources,
                "Right hand should include UnobstructedWideMotion.");
        }

        static unsafe XrResult MockLocateHandJointsActive(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);
            return XrResult.Success;
        }
    }
}
#endif
