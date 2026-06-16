#if UNITY_EDITOR && UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING

using System.Reflection;
using NUnit.Framework;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.Hands.OpenXR;

namespace UnityEditor.XR.Hands.Tests
{
    class OpenXRHandTrackingExtensionTests
    {
        [Test]
        public void HandTrackingDataSource_EnumValues_MatchOpenXRSpec()
        {
            Assert.AreEqual(1, (int)HandTrackingDataSource.Unobstructed,
                "Unobstructed must equal XR_HAND_TRACKING_DATA_SOURCE_UNOBSTRUCTED_EXT (1).");
            Assert.AreEqual(2, (int)HandTrackingDataSource.Controller,
                "Controller must equal XR_HAND_TRACKING_DATA_SOURCE_CONTROLLER_EXT (2).");
        }

        [Test]
        public void HandJointsMotionRange_EnumValues_MatchOpenXRSpec()
        {
            Assert.AreEqual(1, (int)HandJointsMotionRange.Unobstructed,
                "Unobstructed must equal XR_HAND_JOINTS_MOTION_RANGE_UNOBSTRUCTED_EXT (1).");
            Assert.AreEqual(2, (int)HandJointsMotionRange.ConformingToController,
                "ConformingToController must equal XR_HAND_JOINTS_MOTION_RANGE_CONFORMING_TO_CONTROLLER_EXT (2).");
        }

        [Test]
        public void HandTrackingDataSourceFeature_FeatureId_MatchesOpenXRFeatureAttribute()
        {
            var attr = typeof(HandTrackingDataSourceFeature).GetCustomAttribute<OpenXRFeatureAttribute>();
            Assert.IsNotNull(attr, "HandTrackingDataSourceFeature should have an OpenXRFeature attribute.");
            Assert.AreEqual(HandTrackingDataSourceFeature.featureId, attr.FeatureId,
                "The OpenXRFeature attribute FeatureId should match the feature's featureId constant.");
        }

#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
        [Test]
        public void HandJointsMotionRangeFeature_FeatureId_MatchesOpenXRFeatureAttribute()
        {
            var attr = typeof(HandJointsMotionRangeFeature).GetCustomAttribute<OpenXRFeatureAttribute>();
            Assert.IsNotNull(attr, "HandJointsMotionRangeFeature should have an OpenXRFeature attribute.");
            Assert.AreEqual(HandJointsMotionRangeFeature.featureId, attr.FeatureId,
                "The OpenXRFeature attribute FeatureId should match the feature's featureId constant.");
        }
#endif

        [Test]
        public void HandTrackingDataSourceFeature_DefaultPreference_IsBoth()
        {
            var feature = UnityEngine.ScriptableObject.CreateInstance<HandTrackingDataSourceFeature>();
            try
            {
                Assert.AreEqual(HandTrackingDataSourceFeature.DataSourcePreference.Both, feature.m_LeftHandPreference,
                    "Left hand default should be Both.");
                Assert.AreEqual(HandTrackingDataSourceFeature.DataSourcePreference.Both, feature.m_RightHandPreference,
                    "Right hand default should be Both.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }

#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
        [Test]
        public void HandJointsMotionRangeFeature_DefaultMotionRange_IsUnobstructed()
        {
            var feature = UnityEngine.ScriptableObject.CreateInstance<HandJointsMotionRangeFeature>();
            try
            {
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, feature.m_LeftMotionRange,
                    "Left hand default should be Unobstructed per OpenXR spec.");
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, feature.m_RightMotionRange,
                    "Right hand default should be Unobstructed per OpenXR spec.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }
#endif

        [Test]
        public void HandTrackingDataSourceFeature_TryGetConfiguration_DefaultsToUnobstructedAndController()
        {
            var feature = UnityEngine.ScriptableObject.CreateInstance<HandTrackingDataSourceFeature>();
            try
            {
                Assert.IsTrue(feature.TryGetConfiguration(out var config));
                Assert.IsNotNull(config.leftPreferredSources);
                Assert.AreEqual(2, config.leftPreferredSources.Length);
                Assert.Contains(HandTrackingDataSource.Unobstructed, config.leftPreferredSources);
                Assert.Contains(HandTrackingDataSource.Controller, config.leftPreferredSources);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }

        [Test]
        public void HandTrackingDataSourceFeature_TryUpdateConfiguration_RoundTrips()
        {
            var feature = UnityEngine.ScriptableObject.CreateInstance<HandTrackingDataSourceFeature>();
            try
            {
                var newConfig = new HandTrackingDataSourceConfig
                {
                    leftPreferredSources = new[] { HandTrackingDataSource.Controller },
                    rightPreferredSources = new[] { HandTrackingDataSource.Unobstructed, HandTrackingDataSource.Controller },
                };

                Assert.IsTrue(feature.TryUpdateConfiguration(newConfig));
                Assert.IsTrue(feature.TryGetConfiguration(out var readBack));

                Assert.AreEqual(1, readBack.leftPreferredSources.Length);
                Assert.AreEqual(HandTrackingDataSource.Controller, readBack.leftPreferredSources[0]);

                Assert.AreEqual(2, readBack.rightPreferredSources.Length);
                Assert.Contains(HandTrackingDataSource.Unobstructed, readBack.rightPreferredSources);
                Assert.Contains(HandTrackingDataSource.Controller, readBack.rightPreferredSources);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }

#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
        [Test]
        public void HandJointsMotionRangeFeature_TryUpdateConfiguration_UpdatesEachHandIndependently()
        {
            var feature = UnityEngine.ScriptableObject.CreateInstance<HandJointsMotionRangeFeature>();
            try
            {
                var newConfig = new HandJointsMotionRangeConfig
                {
                    leftMotionRange = HandJointsMotionRange.ConformingToController,
                    rightMotionRange = HandJointsMotionRange.Unobstructed,
                };

                Assert.IsTrue(feature.TryUpdateConfiguration(newConfig));
                Assert.AreEqual(HandJointsMotionRange.ConformingToController, feature.m_LeftMotionRange);
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, feature.m_RightMotionRange);

                Assert.IsTrue(feature.TryGetConfiguration(out var readBack));
                Assert.AreEqual(HandJointsMotionRange.ConformingToController, readBack.leftMotionRange);
                Assert.AreEqual(HandJointsMotionRange.Unobstructed, readBack.rightMotionRange);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }
#endif
    }
}

#endif // UNITY_EDITOR && UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
