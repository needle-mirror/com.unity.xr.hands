#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine.XR.Hands;
using UnityEditor.PackageManager;


#if UNITY_OPENXR_PACKAGE
using System;
using System.Reflection;
using UnityEngine.XR.Hands.OpenXR;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEditor.XR.Hands.Tests
{
    class XRHandsEditorTests
    {
        const string k_PackageName = "com.unity.xr.hands";

        /// <summary>
        /// <see cref="PackageManager.PackageInfo"/>
        /// </summary>
        PackageManager.PackageInfo m_PackageInfo;

        [OneTimeSetUp]
        public void SetUp()
        {
            m_PackageInfo = PackageManager.PackageInfo.FindForPackageName(k_PackageName);
            Assert.That(m_PackageInfo, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Empty);
        }

        [Test]
        public void HelpURLVersionMatchesPackageVersion()
        {
            var majorMinorVersionString = GetMajorMinor(m_PackageInfo.version);
            Assert.AreEqual(majorMinorVersionString, XRHelpURLConstants.currentDocsVersion);
        }

#if UNITY_OPENXR_PACKAGE
        [Test]
        public void HandTracking_FeatureId_IsDeprecatedWithUpgradePath()
        {
            // featureId is obsolete with error, so it cannot be referenced through nameof.
            var field = typeof(HandTracking).GetField("featureId", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, "featureId field should exist on HandTracking.");

            var obsoleteAttr = field.GetCustomAttribute<ObsoleteAttribute>();
            Assert.IsNotNull(obsoleteAttr, "featureId should have an [Obsolete] attribute.");
            Assert.That(obsoleteAttr.Message, Does.Contain(nameof(HandTracking.featureId2)),
                "Obsolete message should reference featureId2 as the upgrade target.");
            Assert.IsTrue(obsoleteAttr.IsError,
                "featureId should produce a compile error now that the migration period has ended.");
        }

        [Test]
        public void HandTracking_FeatureId2_IsNotDeprecated()
        {
            var field = typeof(HandTracking).GetField(nameof(HandTracking.featureId2), BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, "featureId2 field should exist on HandTracking.");

            var obsoleteAttr = field.GetCustomAttribute<ObsoleteAttribute>();
            Assert.IsNull(obsoleteAttr, "featureId2 should not be deprecated.");
        }

        [Test]
        public void HandTracking_FeatureId2_MatchesOpenXRFeatureAttribute()
        {
            var openXRAttr = typeof(HandTracking).GetCustomAttribute<OpenXRFeatureAttribute>();
            Assert.IsNotNull(openXRAttr, "HandTracking should have an OpenXRFeature attribute.");
            Assert.AreEqual(HandTracking.featureId2, openXRAttr.FeatureId,
                "The OpenXRFeature attribute FeatureId should match featureId2.");
        }

        [Test]
        public void OpenXRDocumentationVersion()
        {
            // allow for experimental/pre-release versions to go out without breaking docs links
            if (m_PackageInfo.version.Contains("-"))
                Assert.Pass();

            // We only need the major and minor version from the package, since that's what matters when referencing
            // the docs pages. i.e: 1.3.1 would be referred to as -> 1.3
            var splitVersion = m_PackageInfo.version.Split('.');
            var majorMinorVersion = $"{splitVersion[0]}.{splitVersion[1]}"; // Only use major and minor version

            UnityEngine.Debug.Log(typeof(HandTracking).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink);
            Assert.IsTrue(typeof(HandTracking).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink.Contains(majorMinorVersion));

            UnityEngine.Debug.Log(typeof(MetaHandTrackingAim).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink);
            Assert.IsTrue(typeof(MetaHandTrackingAim).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink.Contains(majorMinorVersion));
        }
#endif

        static string GetMajorMinor(DependencyInfo dependency) => GetMajorMinor(dependency.version);

        static string GetMajorMinor(string version)
        {
            // Return major.minor from the string.
            // For example, "1.2.3" would return "1.2"
            Assert.That(version, Is.Not.Null);
            Assert.That(version, Is.Not.Empty);
            var secondDotIndex = version.IndexOf('.', version.IndexOf('.') + 1);
            Assert.That(secondDotIndex, Is.GreaterThan(0));
            return version.Substring(0, secondDotIndex);
        }
    }
}
#endif
