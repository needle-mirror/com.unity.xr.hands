#if UNITY_OPENXR_HAS_EXTENSIBLE_HAND_TRACKING
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.OpenXR.NativeInterop;
using UnityEngine.XR.Hands.Tests.OpenXR.MockHandsRuntime;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrHandTrackerEXT = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.Hands.Tests.OpenXR
{
    /// <summary>
    /// Integration tests that verify HandExtensibilityManager lifecycle and
    /// chain accessor wiring via TestHandTrackingExtensionFeature.
    /// </summary>
    public class HandExtensibilityManagerTests : OpenXRHandTrackingTestFixture
    {
        MockHandsEnvironment m_MockEnvironment;
        TestHandTrackingExtensionFeature m_TestFeature;

        OpenXRSettings.LatencyOptimization m_PreviousLatencyOptimization;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_MockEnvironment = new MockHandsEnvironment();
            m_MockEnvironment.SetUpDefaultHandTrackingEnvironment();
            m_MockEnvironment.Environment.Settings.EnableFeature<TestHandTrackingExtensionFeature>(true);
            m_TestFeature = m_MockEnvironment.Environment.Settings.GetFeature<TestHandTrackingExtensionFeature>();

            // Pin xrWaitFrame to before EarlyUpdate so the Dynamic phase always
            // has a valid predicted display time. Eliminates the first-frame
            // Dynamic miss and decouples dispatch timing from project setting.
            m_PreviousLatencyOptimization = OpenXRSettings.Instance.latencyOptimization;
            OpenXRSettings.Instance.latencyOptimization =
                OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            OpenXRSettings.Instance.latencyOptimization = m_PreviousLatencyOptimization;
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
            m_TestFeature?.ResetCallbacks();
            s_MockLocateIsActive = true;
            s_MockLocateResult = XrResult.Success;
        }

        [Test]
        public void ChainAccessors_ReturnNonNull_AfterConstruction()
        {
            using var manager = new HandExtensibilityManager();

            Assert.That(manager.GetCreateChain(XrHandEXT.Left), Is.Not.Null);
            Assert.That(manager.GetLocateInputChain(XrHandEXT.Left), Is.Not.Null);
            Assert.That(manager.GetLocateOutputChain(XrHandEXT.Left), Is.Not.Null);
            Assert.That(manager.GetCreateChain(XrHandEXT.Right), Is.Not.Null);
            Assert.That(manager.GetLocateInputChain(XrHandEXT.Right), Is.Not.Null);
            Assert.That(manager.GetLocateOutputChain(XrHandEXT.Right), Is.Not.Null);
        }

        [Test]
        public void OnSessionEnd_ClearsChainContents()
        {
            using var manager = new HandExtensibilityManager();
            manager.GetLocateInputChain(XrHandEXT.Left)
                .TryAddNode(new XrMockChainTestInfoEXT(0x31415));

            manager.OnSessionEnd();

            Assert.That(manager.GetLocateInputChain(XrHandEXT.Left).count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator ChainAccessors_ReturnNonNull_DuringSession()
        {
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(m_TestFeature.GetInputChain(XrHandEXT.Left), Is.Not.Null);
            Assert.That(m_TestFeature.GetInputChain(XrHandEXT.Right), Is.Not.Null);
            Assert.That(m_TestFeature.GetOutputChain(XrHandEXT.Left), Is.Not.Null);
            Assert.That(m_TestFeature.GetOutputChain(XrHandEXT.Right), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ChainAccessors_ReturnNull_AfterSessionStop()
        {
            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            m_MockEnvironment.Stop();
            yield return null;

            Assert.That(m_TestFeature.GetInputChain(XrHandEXT.Left), Is.Null);
            Assert.That(m_TestFeature.GetOutputChain(XrHandEXT.Left), Is.Null);
        }

        [UnityTest]
        public IEnumerator ChainState_PersistsAcrossFrames()
        {
            const uint expectedValue = 0x31415;
            m_TestFeature.sessionCreate = _ =>
            {
                m_TestFeature.GetInputChain(XrHandEXT.Left)
                    ?.TryAddNode(new XrMockChainTestInfoEXT(expectedValue));
                m_TestFeature.GetInputChain(XrHandEXT.Right)
                    ?.TryAddNode(new XrMockChainTestInfoEXT(expectedValue));
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            var chain = m_TestFeature.GetInputChain(XrHandEXT.Left);
            Assert.That(chain, Is.Not.Null);
            Assert.That(
                chain.TryGetNode<XrMockChainTestInfoEXT>(XrMockChainTestInfoEXT.k_Type, out var actual),
                Is.True);
            Assert.That(actual.value, Is.EqualTo(expectedValue));

            yield return new WaitForXrFrame(3);

            Assert.That(
                chain.TryGetNode<XrMockChainTestInfoEXT>(XrMockChainTestInfoEXT.k_Type, out var afterFrames),
                Is.True);
            Assert.That(afterFrames.value, Is.EqualTo(expectedValue));
        }

        [UnityTest]
        public IEnumerator ChainState_UpdatePersistsAcrossFrames()
        {
            const uint initialValue = 0x31415;
            const uint updatedValue = 0x27182;
            m_TestFeature.sessionCreate = _ =>
            {
                m_TestFeature.GetInputChain(XrHandEXT.Left)
                    ?.TryAddNode(new XrMockChainTestInfoEXT(initialValue));
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            var chain = m_TestFeature.GetInputChain(XrHandEXT.Left);
            Assert.That(chain.TryUpdateNode(new XrMockChainTestInfoEXT(updatedValue)), Is.True);

            yield return new WaitForXrFrame(3);

            Assert.That(
                chain.TryGetNode<XrMockChainTestInfoEXT>(XrMockChainTestInfoEXT.k_Type, out var actual),
                Is.True);
            Assert.That(actual.value, Is.EqualTo(updatedValue));
        }

        [UnityTest]
        public IEnumerator ChainState_ClearedBetweenSessions()
        {
            const uint expectedValue = 0x31415;
            m_TestFeature.sessionCreate = _ =>
            {
                m_TestFeature.GetInputChain(XrHandEXT.Left)
                    ?.TryAddNode(new XrMockChainTestInfoEXT(expectedValue));
            };

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            var chain = m_TestFeature.GetInputChain(XrHandEXT.Left);
            Assert.That(
                chain.TryGetNode<XrMockChainTestInfoEXT>(XrMockChainTestInfoEXT.k_Type, out _),
                Is.True);

            m_MockEnvironment.Stop();
            yield return null;

            m_MockEnvironment.Start();
            yield return new WaitForXrFrame();

            var newChain = m_TestFeature.GetInputChain(XrHandEXT.Left);
            Assert.That(newChain, Is.Not.Null);
            Assert.That(
                newChain.TryGetNode<XrMockChainTestInfoEXT>(XrMockChainTestInfoEXT.k_Type, out var actual),
                Is.True);
            Assert.That(actual.value, Is.EqualTo(expectedValue));
        }

        [UnityTest]
        public IEnumerator CreateCallbackFlow_FeaturesReceiveCreateRequest()
        {
            int createRequestCount = 0;
            m_TestFeature.handTrackingCreateRequest = (hand, chain) => createRequestCount++;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(createRequestCount, Is.GreaterThanOrEqualTo(2));
        }

        [UnityTest]
        public IEnumerator HandTrackerLifecycle_FeaturesReceiveNotifications()
        {
            int createdCount = 0;
            int destroyedCount = 0;
            XrResult lastCreatedResult = default;
            m_TestFeature.handTrackerCreated = (hand, result) =>
            {
                createdCount++;
                lastCreatedResult = result;
            };
            m_TestFeature.handTrackerDestroyed = (hand, result) => destroyedCount++;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(createdCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(lastCreatedResult, Is.EqualTo(XrResult.Success));

            m_MockEnvironment.Stop();
            yield return null;

            Assert.That(destroyedCount, Is.GreaterThanOrEqualTo(2));
        }

        static unsafe bool VerifyCreateChainContainsMockExtension(
            in XrHandTrackerCreateInfoEXT info, uint expectedTestValue)
        {
            var current = (XrBaseInStructure*)info.next;
            while (current != null)
            {
                if (current->type == XrMockCreateExtensionInfoEXT.k_Type)
                {
                    var mockExt = (XrMockCreateExtensionInfoEXT*)current;
                    Assert.That(mockExt->testValue, Is.EqualTo(expectedTestValue));
                    return true;
                }
                current = (XrBaseInStructure*)current->next;
            }
            return false;
        }

        [UnityTest]
        public IEnumerator CreateCallbackFlow_ExtensionChainData_PassedToNative()
        {
            const uint expectedTestValue = 0x31415;
            m_TestFeature.handTrackingCreateRequest = (hand, chain) =>
                chain.TryAddNode(new XrMockCreateExtensionInfoEXT(expectedTestValue));

            bool chainVerified = false;

            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = info.hand == XrHandEXT.Left ? MockHandsEnvironment.k_LeftTrackerHandle : MockHandsEnvironment.k_RightTrackerHandle;
                    if (VerifyCreateChainContainsMockExtension(in info, expectedTestValue))
                        chainVerified = true;
                    return XrResult.Success;
                };

            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(chainVerified, Is.True, "Mock extension struct was not found in createInfo.next chain");
        }

        [UnityTest]
        public IEnumerator HandTrackerLifecycle_CreateFailure_PassesResultToFeature()
        {
            XrResult lastResult = default;
            m_TestFeature.handTrackerCreated = (hand, result) => lastResult = result;

            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = 0;
                    return XrResult.RuntimeFailure;
                };

            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(lastResult, Is.EqualTo(XrResult.RuntimeFailure));
        }

        [UnityTest]
        public IEnumerator CreateCallbackFlow_FeatureException_DoesNotCrash()
        {
            m_TestFeature.handTrackingCreateRequest = (hand, chain) =>
                throw new System.InvalidOperationException("Test exception in OnHandTrackingCreateRequest");

            bool createCalled = false;

            m_MockEnvironment.createHandTracker.mock =
                (XrSession session, in XrHandTrackerCreateInfoEXT info, out XrHandTrackerEXT tracker) =>
                {
                    tracker = info.hand == XrHandEXT.Left ? MockHandsEnvironment.k_LeftTrackerHandle : MockHandsEnvironment.k_RightTrackerHandle;
                    createCalled = true;
                    return XrResult.Success;
                };

            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            // Expect one error per hand (left + right)
            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));
            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(createCalled, Is.True, "xrCreateHandTrackerEXT should still be called despite feature exception");
        }

        [UnityTest]
        public IEnumerator HandTrackerLifecycle_CallbackException_DoesNotCrash()
        {
            m_TestFeature.handTrackerCreated = (hand, result) =>
                throw new System.InvalidOperationException("Test exception in OnHandTrackerCreated");
            m_TestFeature.handTrackerDestroyed = (hand, result) =>
                throw new System.InvalidOperationException("Test exception in OnHandTrackerDestroyed");

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));
            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));
            LogAssert.Expect(LogType.Error, new Regex("TestHandTrackingExtensionFeature threw"));

            m_MockEnvironment.Stop();
            yield return null;
        }

        // Per Unity frame, the hand subsystem invokes
        // xrLocateHandJointsEXT once per hand per XRHandSubsystem.UpdateType
        // (BeforeRender + Dynamic). The feature's locateResult is dispatched
        // for each invocation, so this is also the per-frame feature dispatch
        // count. This is a structural contract of the subsystem; if Unity
        // adds a third update type or the subsystem stops invoking per
        // update type, the contract has changed and these tests should fail.
        //
        // A "warm" WaitForXrFrame spans exactly one frame's worth of these
        // dispatches, so the delta in callCount across a warm wait equals
        // k_LocateHandJointsCallsPerFrame. The first wait of a session is
        // "cold" and spans more dispatches due to session bootstrap; tests
        // must perform one warm-up wait after m_MockEnvironment.Start()
        // before asserting on this delta.
        const int k_UpdateTypesPerFrame = 2;
        const int k_HandsPerUpdateType = 2;
        const int k_LocateHandJointsCallsPerFrame = k_HandsPerUpdateType * k_UpdateTypesPerFrame;

        static bool s_MockLocateIsActive = true;
        static XrResult s_MockLocateResult = XrResult.Success;

        static unsafe XrResult MockLocateHandJoints(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, s_MockLocateIsActive, 26, locations.jointLocations);
            return s_MockLocateResult;
        }

        static unsafe bool VerifyLocateInputChainContainsMockExtension(
            in XrHandJointsLocateInfoEXT locateInfo, uint expectedInputValue)
        {
            var current = (XrBaseInStructure*)locateInfo.next;
            while (current != null)
            {
                if (current->type == XrMockLocateInputInfoEXT.k_Type)
                {
                    var mockExt = (XrMockLocateInputInfoEXT*)current;
                    Assert.That(mockExt->inputValue, Is.EqualTo(expectedInputValue));
                    return true;
                }
                current = (XrBaseInStructure*)current->next;
            }
            return false;
        }

        static unsafe void WriteToOutputChain(
            void* next, uint isActive, uint dataSource, float confidence)
        {
            var current = (XrBaseInStructure*)next;
            while (current != null)
            {
                if (current->type == XrMockLocateOutputStateEXT.k_Type)
                {
                    var mockOutput = (XrMockLocateOutputStateEXT*)current;
                    mockOutput->isActive = isActive;
                    mockOutput->dataSource = dataSource;
                    mockOutput->confidence = confidence;
                    return;
                }
                current = (XrBaseInStructure*)current->next;
            }
        }

        static unsafe bool VerifyLocateOutputChainContainsMockExtension(
            ref XrHandJointLocationsEXT locations)
        {
            var current = (XrBaseInStructure*)locations.next;
            while (current != null)
            {
                if (current->type == XrMockLocateOutputStateEXT.k_Type)
                    return true;
                current = (XrBaseInStructure*)current->next;
            }
            return false;
        }

        static bool s_InputChainVerified;
        static bool s_OutputChainVerified;
        static uint s_ExpectedInputValue;

        static unsafe XrResult MockLocateWithChainVerification(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            if (VerifyLocateInputChainContainsMockExtension(in locateInfo, s_ExpectedInputValue))
                s_InputChainVerified = true;
            if (VerifyLocateOutputChainContainsMockExtension(ref locations))
                s_OutputChainVerified = true;
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);
            return XrResult.Success;
        }

        static uint s_OutputIsActive;
        static uint s_OutputDataSource;
        static float s_OutputConfidence;

        static unsafe XrResult MockLocateWithOutputWrite(
            XrHandTrackerEXT tracker,
            in XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations)
        {
            locations = new XrHandJointLocationsEXT(locations.next, true, 26, locations.jointLocations);
            WriteToOutputChain(locations.next, s_OutputIsActive, s_OutputDataSource, s_OutputConfidence);
            return XrResult.Success;
        }

        [UnityTest]
        public IEnumerator LocateCallbackFlow_StructureChainData_PassedToNative()
        {
            const uint expectedInputValue = 0x27182;
            s_ExpectedInputValue = expectedInputValue;
            s_InputChainVerified = false;
            s_OutputChainVerified = false;

            m_TestFeature.sessionCreate = _ =>
            {
                m_TestFeature.GetInputChain(XrHandEXT.Left)
                    ?.TryAddNode(new XrMockLocateInputInfoEXT(expectedInputValue));
                m_TestFeature.GetInputChain(XrHandEXT.Right)
                    ?.TryAddNode(new XrMockLocateInputInfoEXT(expectedInputValue));
                m_TestFeature.GetOutputChain(XrHandEXT.Left)
                    ?.TryAddNode(XrMockLocateOutputStateEXT.defaultValue);
                m_TestFeature.GetOutputChain(XrHandEXT.Right)
                    ?.TryAddNode(XrMockLocateOutputStateEXT.defaultValue);
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithChainVerification;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(s_InputChainVerified, Is.True, "Mock input struct not found in locateInfo.next chain");
            Assert.That(s_OutputChainVerified, Is.True, "Mock output struct not found in locations.next chain");
        }

        [UnityTest]
        public IEnumerator LocateCallbackFlow_OutputChainData_ReadByFeature()
        {
            const uint expectedIsActive = 0;
            const uint expectedDataSource = 2;
            const float expectedConfidence = 0.9876f;

            s_OutputIsActive = expectedIsActive;
            s_OutputDataSource = expectedDataSource;
            s_OutputConfidence = expectedConfidence;

            uint lastOutputIsActive = 0;
            uint lastOutputDataSource = 0;
            float lastOutputConfidence = 0f;

            m_TestFeature.sessionCreate = _ =>
            {
                m_TestFeature.GetOutputChain(XrHandEXT.Left)
                    ?.TryAddNode(XrMockLocateOutputStateEXT.defaultValue);
                m_TestFeature.GetOutputChain(XrHandEXT.Right)
                    ?.TryAddNode(XrMockLocateOutputStateEXT.defaultValue);
            };

            m_TestFeature.locateResult = (hand, outputChain, result, isActive) =>
            {
                if (outputChain != null &&
                    outputChain.TryGetNode<XrMockLocateOutputStateEXT>(XrMockLocateOutputStateEXT.k_Type, out var state))
                {
                    lastOutputIsActive = state.isActive;
                    lastOutputDataSource = state.dataSource;
                    lastOutputConfidence = state.confidence;
                }
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateWithOutputWrite;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            yield return new WaitForXrFrame();

            Assert.That(lastOutputIsActive, Is.EqualTo(expectedIsActive));
            Assert.That(lastOutputDataSource, Is.EqualTo(expectedDataSource));
            Assert.That(lastOutputConfidence, Is.EqualTo(expectedConfidence));
        }

        [UnityTest]
        public IEnumerator LocateCallbackFlow_FeaturesReceiveLocateResult()
        {
            int callCount = 0;
            XrResult lastResult = default;
            bool lastIsActive = false;

            m_TestFeature.locateResult = (hand, outputChain, result, isActive) =>
            {
                callCount++;
                lastResult = result;
                lastIsActive = isActive;
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJoints;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            // Warm up — the first wait of a session is "cold" and spans more
            // calls than k_LocateHandJointsCallsPerFrame. Discard its count.
            yield return new WaitForXrFrame();

            int baseline = callCount;
            yield return new WaitForXrFrame();

            Assert.That(callCount - baseline, Is.EqualTo(k_LocateHandJointsCallsPerFrame));
            Assert.That(lastResult, Is.EqualTo(XrResult.Success));
            Assert.That(lastIsActive, Is.True);

            s_MockLocateIsActive = false;
            baseline = callCount;
            yield return new WaitForXrFrame();

            Assert.That(callCount - baseline, Is.EqualTo(k_LocateHandJointsCallsPerFrame));
            Assert.That(lastResult, Is.EqualTo(XrResult.Success));
            Assert.That(lastIsActive, Is.False);

            s_MockLocateIsActive = true;
            baseline = callCount;
            yield return new WaitForXrFrame();

            Assert.That(callCount - baseline, Is.EqualTo(k_LocateHandJointsCallsPerFrame));
            Assert.That(lastResult, Is.EqualTo(XrResult.Success));
            Assert.That(lastIsActive, Is.True);
        }

        [UnityTest]
        public IEnumerator LocateCallbackFlow_LocateFailure_StillDispatchedToFeature()
        {
            XrResult lastResult = default;
            bool lastIsActive = false;
            int callCount = 0;

            m_TestFeature.locateResult = (hand, outputChain, result, isActive) =>
            {
                callCount++;
                lastResult = result;
                lastIsActive = isActive;
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJoints;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            s_MockLocateResult = XrResult.RuntimeFailure;
            s_MockLocateIsActive = false;

            m_MockEnvironment.Start();

            // Warm up — first wait of session is cold; discard.
            yield return new WaitForXrFrame();

            int baseline = callCount;
            yield return new WaitForXrFrame();

            Assert.That(callCount - baseline, Is.EqualTo(k_LocateHandJointsCallsPerFrame));
            Assert.That(lastResult, Is.EqualTo(XrResult.RuntimeFailure));
            Assert.That(lastIsActive, Is.False);
        }

        [UnityTest]
        public IEnumerator RequestRestart_DispatchesDestroyThenCreateOnNextDynamic()
        {
            int createdCount = 0;
            int destroyedCount = 0;
            XrResult lastCreatedResult = default;
            m_TestFeature.handTrackerCreated = (hand, result) =>
            {
                createdCount++;
                lastCreatedResult = result;
            };
            m_TestFeature.handTrackerDestroyed = (hand, result) => destroyedCount++;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            // Warm up — initial creates fire during session bootstrap.
            yield return new WaitForXrFrame();

            createdCount = 0;
            destroyedCount = 0;

            HandTracking.extensibilityManager.RequestRestart();

            yield return new WaitForXrFrame();

            Assert.That(destroyedCount, Is.EqualTo(2));
            Assert.That(createdCount, Is.EqualTo(2));
            Assert.That(lastCreatedResult, Is.EqualTo(XrResult.Success));
        }

        [UnityTest]
        public IEnumerator RequestRestart_CoalescesMultipleCallsPerFrame()
        {
            int createdCount = 0;
            int destroyedCount = 0;
            m_TestFeature.handTrackerCreated = (hand, result) => createdCount++;
            m_TestFeature.handTrackerDestroyed = (hand, result) => destroyedCount++;

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            // Warm up — initial creates fire during session bootstrap.
            yield return new WaitForXrFrame();

            createdCount = 0;
            destroyedCount = 0;

            HandTracking.extensibilityManager.RequestRestart();
            HandTracking.extensibilityManager.RequestRestart();
            HandTracking.extensibilityManager.RequestRestart();
            HandTracking.extensibilityManager.RequestRestart();
            HandTracking.extensibilityManager.RequestRestart();

            yield return new WaitForXrFrame();

            Assert.That(destroyedCount, Is.EqualTo(2));
            Assert.That(createdCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator RequestHandTrackerRestart_OnFeature_TriggersRestart()
        {
            int createdCount = 0;
            int destroyedCount = 0;
            XrResult lastCreatedResult = default;
            m_TestFeature.handTrackerCreated = (hand, result) =>
            {
                createdCount++;
                lastCreatedResult = result;
            };
            m_TestFeature.handTrackerDestroyed = (hand, result) => destroyedCount++;

            bool restartFired = false;
            m_TestFeature.locateResult = (hand, outputChain, result, isActive) =>
            {
                if (!restartFired)
                {
                    restartFired = true;
                    m_TestFeature.TriggerRestartRequest();
                }
            };

            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJoints;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            // Warm up — initial creates fire during session bootstrap.
            yield return new WaitForXrFrame();

            createdCount = 0;
            destroyedCount = 0;

            // The one-shot fired during warmup (restartFired is true). Re-arm
            // it for this observation: the next WaitForXrFrame fires locate
            // again, which now calls TriggerRestartRequest. The Dynamic tick
            // following that consumes the flag and executes the restart.
            restartFired = false;
            yield return new WaitForXrFrame();
            yield return new WaitForXrFrame();

            Assert.That(destroyedCount, Is.EqualTo(2));
            Assert.That(createdCount, Is.EqualTo(2));
            Assert.That(lastCreatedResult, Is.EqualTo(XrResult.Success));
        }

        [UnityTest]
        public IEnumerator LocateCallbackFlow_FeatureException_DoesNotCrash()
        {
            m_MockEnvironment.createHandTracker.mock = MockHandsEnvironment.SuccessfulCreateHandTracker;
            m_MockEnvironment.locateHandJoints.mock = MockLocateHandJoints;
            m_MockEnvironment.destroyHandTracker.mock = MockHandsEnvironment.SuccessfulDestroyHandTracker;

            m_MockEnvironment.Start();

            int invocationCount = 0;
            m_TestFeature.locateResult = (hand, outputChain, result, isActive) =>
            {
                invocationCount++;
                throw new System.InvalidOperationException("Test exception in OnLocateHandJointsResult");
            };

            LogAssert.ignoreFailingMessages = true;

            yield return new WaitForXrFrame();

            LogAssert.ignoreFailingMessages = false;

            Assert.That(invocationCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(invocationCount % 2, Is.EqualTo(0));
        }
    }
}

#endif
