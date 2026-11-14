using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

[Category("Level.Integration")]
[NUnit.Framework.Property("Covers", "Input")]
class XRHandMicrogestureTests
{
    class BaseProvider : XRHandSubsystemProvider
    {
        public override void GetHandLayout(NativeArray<bool> handJointsInLayout) { }

        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints)
            => XRHandSubsystem.UpdateSuccessFlags.None;

        public override void Start() { }

        public override void Stop() { }

        public override void Destroy() { }
    }

    static XRHandSubsystem CreateMicrogestureCapableSubsystem()
    {
        var cinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "TestHandProviderMicrogestures",
            providerType = typeof(TestHandProvider),
            supportsMicrogestures = true,
        };
        return TestHandUtils.CreateTestSubsystem(cinfo);
    }

    static TestHandProvider StartWithMicrogestures(
        XRHandSubsystem subsystem,
        bool useFastPath,
        TestHandProvider.CommonGestureBehavior commonGestureBehavior = TestHandProvider.CommonGestureBehavior.CorePosesOnly)
    {
        subsystem.Start();

        var provider = (TestHandProvider)subsystem.GetProvider();
        provider.commonGestureBehavior = commonGestureBehavior;
        provider.commonGesturesFastPathEnabled = useFastPath;
        provider.microgesturesEnabled = true;
        return provider;
    }

    static readonly XRHandMicrogesture[] k_Gestures =
    {
        XRHandMicrogesture.SwipeLeft,
        XRHandMicrogesture.SwipeRight,
        XRHandMicrogesture.SwipeForward,
        XRHandMicrogesture.SwipeBackward,
        XRHandMicrogesture.TapThumb,
    };

    static readonly bool[] k_BothActiveStates = { true, false };

    const string k_StepAfterDynamic = "after dynamic update";
    const string k_StepRepeatedUpdate = "after repeated update";
    const string k_StepInactive = "inactive";
    const string k_StepAfterStop = "after provider stop";

    static string Describe(XRHandMicrogesture gesture, string step)
    {
        return gesture + ": " + step;
    }

    static XRHandMicrogesture OtherThan(XRHandMicrogesture gesture)
    {
        return k_Gestures[0] == gesture ? k_Gestures[1] : k_Gestures[0];
    }

    struct UpdatedRecord
    {
        public Handedness handedness;
        public XRCommonHandGestures commonGestures;
        public bool[] active;
        public bool[] performed;
        public bool[] completed;
    }

    static bool ArgumentsActive(XRCommonHandGestures.MicrogestureUpdatedEventArgs args, XRHandMicrogesture gesture)
    {
        switch (gesture)
        {
            case XRHandMicrogesture.SwipeLeft: return args.swipeLeftActive;
            case XRHandMicrogesture.SwipeRight: return args.swipeRightActive;
            case XRHandMicrogesture.SwipeForward: return args.swipeForwardActive;
            case XRHandMicrogesture.SwipeBackward: return args.swipeBackwardActive;
            case XRHandMicrogesture.TapThumb: return args.tapThumbActive;
            default: return false;
        }
    }

    static bool ArgumentsPerformed(XRCommonHandGestures.MicrogestureUpdatedEventArgs args, XRHandMicrogesture gesture)
    {
        switch (gesture)
        {
            case XRHandMicrogesture.SwipeLeft: return args.swipeLeftPerformedThisFrame;
            case XRHandMicrogesture.SwipeRight: return args.swipeRightPerformedThisFrame;
            case XRHandMicrogesture.SwipeForward: return args.swipeForwardPerformedThisFrame;
            case XRHandMicrogesture.SwipeBackward: return args.swipeBackwardPerformedThisFrame;
            case XRHandMicrogesture.TapThumb: return args.tapThumbPerformedThisFrame;
            default: return false;
        }
    }

    static bool ArgumentsCompleted(XRCommonHandGestures.MicrogestureUpdatedEventArgs args, XRHandMicrogesture gesture)
    {
        switch (gesture)
        {
            case XRHandMicrogesture.SwipeLeft: return args.swipeLeftCompletedThisFrame;
            case XRHandMicrogesture.SwipeRight: return args.swipeRightCompletedThisFrame;
            case XRHandMicrogesture.SwipeForward: return args.swipeForwardCompletedThisFrame;
            case XRHandMicrogesture.SwipeBackward: return args.swipeBackwardCompletedThisFrame;
            case XRHandMicrogesture.TapThumb: return args.tapThumbCompletedThisFrame;
            default: return false;
        }
    }

    class UpdatedRecorder
    {
        readonly List<UpdatedRecord> m_Records = new List<UpdatedRecord>();
        readonly XRCommonHandGestures m_CommonGestures;

        public int count => m_Records.Count;

        public UpdatedRecorder(XRCommonHandGestures commonGestures)
        {
            m_CommonGestures = commonGestures;
            m_CommonGestures.microgestureUpdated += OnMicrogestureUpdated;
        }

        public void Dispose() => m_CommonGestures.microgestureUpdated -= OnMicrogestureUpdated;

        public UpdatedRecord GetRecord(int recordIdx) => m_Records[recordIdx];

        public void Reset() => m_Records.Clear();

        void OnMicrogestureUpdated(XRCommonHandGestures.MicrogestureUpdatedEventArgs args)
        {
            var record = new UpdatedRecord
            {
                handedness = args.handedness,
                commonGestures = args.commonGestures,
                active = new bool[k_Gestures.Length],
                performed = new bool[k_Gestures.Length],
                completed = new bool[k_Gestures.Length],
            };
            for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
            {
                record.active[gestureIdx] = ArgumentsActive(args, k_Gestures[gestureIdx]);
                record.performed[gestureIdx] = ArgumentsPerformed(args, k_Gestures[gestureIdx]);
                record.completed[gestureIdx] = ArgumentsCompleted(args, k_Gestures[gestureIdx]);
            }

            m_Records.Add(record);
        }
    }

    static int IndexOf(XRHandMicrogesture gesture)
    {
        return System.Array.IndexOf(k_Gestures, gesture);
    }

    [Test]
    [Description("A dynamic update surfaces each hand's reported microgestures as common gestures on both the fast " +
                 "and slow paths, for each of the five gestures: the gesture is active only on the hand " +
                 "reporting it, and inactive on a hand reporting a different gesture.")]
    public void DynamicUpdateSurfacesGestureOnlyOnReportingHand([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag() | other.ToMicrogestureFlag();
                provider.rightHandMicrogesturesToReport = other.ToMicrogestureFlag();

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.True, Describe(gesture, "left active"));
                Assert.That(subsystem.rightHandCommonGestures.IsMicrogestureActive(gesture), Is.False, Describe(gesture, "right inactive"));
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("When the provider reports a single microgesture, only that microgesture is active on that hand and " +
                 "the other four are inactive, on both the fast and slow paths.")]
    public void GestureActivatesOnlyItself([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            foreach (var gesture in k_Gestures)
            {
                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
                {
                    var candidate = k_Gestures[gestureIdx];
                    Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(candidate), Is.EqualTo(candidate == gesture), Describe(gesture, "candidate " + candidate));
                }
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("A before-render update leaves the microgestures unchanged on both the fast and slow paths: a " +
                 "set gesture stays set after the provider stops reporting it.")]
    public void BeforeRenderUpdateDoesNotRefreshGesture([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            foreach (var gesture in k_Gestures)
            {
                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.True, Describe(gesture, k_StepAfterDynamic));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.True, Describe(gesture, k_StepAfterDynamic + " performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.False, Describe(gesture, k_StepAfterDynamic + " completed"));

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);

                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.True, Describe(gesture, "after before-render update"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.True, Describe(gesture, "after before-render update performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.False, Describe(gesture, "after before-render update completed"));

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.False, Describe(gesture, "after falling dynamic update"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.False, Describe(gesture, "after falling dynamic update performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.True, Describe(gesture, "after falling dynamic update completed"));

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);
                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.False, Describe(gesture, "after falling before-render update"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.False, Describe(gesture, "after falling before-render update performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.True, Describe(gesture, "after falling before-render update completed"));
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("On the slow path, the microgestures stay inactive when the subsystem descriptor does not " +
                 "declare supportsMicrogestures, even if the provider reports a gesture.")]
    public void GestureStaysInactiveWhenDescriptorDoesNotSupportMicrogesturesOnSlowPath()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, false);
            foreach (var gesture in k_Gestures)
            {
                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.False, Describe(gesture, k_StepInactive));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.False, Describe(gesture, k_StepInactive + " performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.False, Describe(gesture, k_StepInactive + " completed"));
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("With common gesture updates disabled on the provider, the microgestures stay inactive " +
                 "on both the fast and slow paths, even if the provider reports a gesture.")]
    public void GestureStaysInactiveWhenPumpGateIsOff([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath, TestHandProvider.CommonGestureBehavior.Disabled);
            foreach (var gesture in k_Gestures)
            {
                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                Assert.That(subsystem.leftHandCommonGestures.IsMicrogestureActive(gesture), Is.False, Describe(gesture, k_StepInactive));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.False, Describe(gesture, k_StepInactive + " performed"));
                Assert.That(subsystem.leftHandCommonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.False, Describe(gesture, k_StepInactive + " completed"));
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("microgestureUpdated fires once per dynamic update that changes any gesture, with both gestures " +
                 "reporting performed when two become active, the dropped gesture reporting completed when one is " +
                 "dropped, and no invocation when nothing changes, on both the fast and slow paths.")]
    public void MicrogestureUpdatedFiresOncePerUpdate([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        var recorder = new UpdatedRecorder(subsystem.leftHandCommonGestures);
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                var gestureIdx = IndexOf(gesture);
                var otherIdx = IndexOf(other);
                var gestureType = gesture.ToMicrogestureFlag();
                var otherType = other.ToMicrogestureFlag();

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                recorder.Reset();

                provider.leftHandMicrogesturesToReport = gestureType | otherType;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(1), Describe(gesture, "union count"));
                Assert.That(recorder.GetRecord(0).performed[gestureIdx], Is.True, Describe(gesture, "union gesture performed"));
                Assert.That(recorder.GetRecord(0).performed[otherIdx], Is.True, Describe(gesture, "union other performed"));

                recorder.Reset();
                provider.leftHandMicrogesturesToReport = otherType;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(1), Describe(gesture, "single count"));
                Assert.That(recorder.GetRecord(0).completed[gestureIdx], Is.True, Describe(gesture, "single gesture completed"));
                Assert.That(recorder.GetRecord(0).active[otherIdx], Is.True, Describe(gesture, "single other active"));
                Assert.That(recorder.GetRecord(0).performed[otherIdx], Is.False, Describe(gesture, "single other performed"));

                recorder.Reset();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(0), Describe(gesture, "unchanged count"));
            }
        }
        finally
        {
            recorder.Dispose();
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("microgestureUpdated fires when a gesture becomes active and when it becomes inactive, but not on " +
                 "hold updates, unchanged updates, or before-render updates, on both the fast and slow paths.")]
    public void MicrogestureUpdatedFiresOnRiseAndFallOnly([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        var recorder = new UpdatedRecorder(subsystem.leftHandCommonGestures);
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
            subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

            foreach (var gesture in k_Gestures)
            {
                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                recorder.Reset();

                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(1), Describe(gesture, "rise count"));

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(1), Describe(gesture, "hold count"));

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);
                Assert.That(recorder.count, Is.EqualTo(1), Describe(gesture, "before-render count"));

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(recorder.count, Is.EqualTo(2), Describe(gesture, "fall count"));

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);
                Assert.That(recorder.count, Is.EqualTo(2), Describe(gesture, "after fall count"));
            }
        }
        finally
        {
            recorder.Dispose();
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("Each of the event args' active, performed, and completed properties equals the matching query on " +
                 "args.commonGestures for every gesture, and handedness and commonGestures are correct.")]
    public void MicrogestureUpdatedArgsMatchCommonGestures([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        var commonGestures = subsystem.leftHandCommonGestures;
        var invocations = 0;
        var handednessCorrect = true;
        var sameReference = true;
        var propsMatch = true;
        void Listener(XRCommonHandGestures.MicrogestureUpdatedEventArgs args)
        {
            invocations++;
            handednessCorrect &= args.handedness == Handedness.Left;
            sameReference &= ReferenceEquals(args.commonGestures, commonGestures);
            for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
            {
                var candidate = k_Gestures[gestureIdx];
                propsMatch &= ArgumentsActive(args, candidate) == args.commonGestures.IsMicrogestureActive(candidate);
                propsMatch &= ArgumentsPerformed(args, candidate) == args.commonGestures.WasMicrogesturePerformedThisFrame(candidate);
                propsMatch &= ArgumentsCompleted(args, candidate) == args.commonGestures.WasMicrogestureCompletedThisFrame(candidate);
            }
        }

        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
            subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
            commonGestures.microgestureUpdated += Listener;

            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                invocations = 0;

                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag() | other.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(invocations, Is.EqualTo(1), Describe(gesture, "active invocations"));

                provider.leftHandMicrogesturesToReport = other.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(invocations, Is.EqualTo(2), Describe(gesture, "partial invocations"));

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                Assert.That(invocations, Is.EqualTo(3), Describe(gesture, "inactive invocations"));

                Assert.That(handednessCorrect, Is.True, Describe(gesture, "handedness"));
                Assert.That(sameReference, Is.True, Describe(gesture, "commonGestures reference"));
                Assert.That(propsMatch, Is.True, Describe(gesture, "args match commonGestures"));
            }
        }
        finally
        {
            commonGestures.microgestureUpdated -= Listener;
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("When the provider stops reporting microgestures, microgestureUpdated fires once with both active " +
                 "gestures reporting completed when any were active and not at all when none were; afterwards " +
                 "every gesture is inactive and later updates do not fire the event again.")]
    public void ProviderStoppingMicrogesturesDeactivatesActiveGestures([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        var commonGestures = subsystem.leftHandCommonGestures;
        var recorder = new UpdatedRecorder(commonGestures);
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                var activeMask = gesture.ToMicrogestureFlag() | other.ToMicrogestureFlag();
                foreach (var bothActive in k_BothActiveStates)
                {
                    var label = Describe(gesture, "bothActive=" + bothActive);
                    provider.microgesturesEnabled = true;
                    provider.leftHandMicrogesturesToReport = bothActive ? activeMask : XRHandMicrogestureFlags.None;
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                    Assert.That(commonGestures.IsMicrogestureActive(gesture), Is.EqualTo(bothActive), label + ", before stop");
                    Assert.That(commonGestures.IsMicrogestureActive(other), Is.EqualTo(bothActive), label + ", before stop other");

                    recorder.Reset();

                    provider.microgesturesEnabled = false;
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                    Assert.That(recorder.count, Is.EqualTo(bothActive ? 1 : 0), label + ", " + k_StepAfterStop + " count");
                    if (bothActive)
                    {
                        Assert.That(recorder.GetRecord(0).completed[IndexOf(gesture)], Is.True, label + ", " + k_StepAfterStop + " gesture completed");
                        Assert.That(recorder.GetRecord(0).completed[IndexOf(other)], Is.True, label + ", " + k_StepAfterStop + " other completed");
                    }

                    for (var gestureIdx = 0; gestureIdx < k_Gestures.Length; gestureIdx++)
                    {
                        Assert.That(commonGestures.IsMicrogestureActive(k_Gestures[gestureIdx]), Is.False, label + ", " + k_StepAfterStop + " " + k_Gestures[gestureIdx]);
                    }

                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                    Assert.That(recorder.count, Is.EqualTo(bothActive ? 1 : 0), label + ", " + k_StepRepeatedUpdate);
                }
            }
        }
        finally
        {
            recorder.Dispose();
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("The edge queries track a gesture through rise, hold, fall, and a further unchanged update on both " +
                 "the fast and slow paths: performed is true only on the rising update and completed only on the " +
                 "falling update, and another gesture never shows an edge.")]
    public void MicrogestureEdgesTrackRiseHoldAndFall([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            var commonGestures = subsystem.leftHandCommonGestures;
            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                AssertEdges(commonGestures, gesture, true, true, false, "rise");
                AssertEdges(commonGestures, other, false, false, false, "rise other");

                for (var holdIdx = 0; holdIdx < 2; holdIdx++)
                {
                    subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                    AssertEdges(commonGestures, gesture, true, false, false, "hold " + holdIdx);
                    AssertEdges(commonGestures, other, false, false, false, "hold other " + holdIdx);
                }

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                AssertEdges(commonGestures, gesture, false, false, true, "fall");
                AssertEdges(commonGestures, other, false, false, false, "fall other");

                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                AssertEdges(commonGestures, gesture, false, false, false, "after fall");
                AssertEdges(commonGestures, other, false, false, false, "after fall other");
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("Edges are tracked per gesture: when one update drops an active gesture and reports another, the " +
                 "dropped gesture shows only completed and the new gesture shows only performed, on both the fast " +
                 "and slow paths.")]
    public void MicrogestureEdgesAreIndependentPerGesture([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            var commonGestures = subsystem.leftHandCommonGestures;
            foreach (var gesture in k_Gestures)
            {
                var other = OtherThan(gesture);
                provider.leftHandMicrogesturesToReport = gesture.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                provider.leftHandMicrogesturesToReport = other.ToMicrogestureFlag();
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
                AssertEdges(commonGestures, gesture, false, false, true, "dropped");
                AssertEdges(commonGestures, other, true, true, false, "reported");

                provider.leftHandMicrogesturesToReport = XRHandMicrogestureFlags.None;
                subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    static void AssertEdges(XRCommonHandGestures commonGestures, XRHandMicrogesture gesture, bool active, bool performed, bool completed, string step)
    {
        Assert.That(commonGestures.IsMicrogestureActive(gesture), Is.EqualTo(active), Describe(gesture, step + " active"));
        Assert.That(commonGestures.WasMicrogesturePerformedThisFrame(gesture), Is.EqualTo(performed), Describe(gesture, step + " performed"));
        Assert.That(commonGestures.WasMicrogestureCompletedThisFrame(gesture), Is.EqualTo(completed), Describe(gesture, step + " completed"));
    }

    [Test]
    [Description("The active, performed, and completed queries return false for None and for undefined enum values, even with all " +
                 "five microgestures performed in the same update, on both the fast and slow paths.")]
    public void MicrogestureQueriesReturnFalseForNoneAndUndefinedValues([Values] bool useFastPath)
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, useFastPath);
            provider.leftHandMicrogesturesToReport = XRHandMicrogestureUtility.k_AllMicrogestures;
            subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

            var commonGestures = subsystem.leftHandCommonGestures;
            var undefinedGestures = new[]
            {
                XRHandMicrogesture.None,
                (XRHandMicrogesture)0x06,
                (XRHandMicrogesture)0xFF,
            };
            foreach (var undefinedGesture in undefinedGestures)
            {
                Assert.That(commonGestures.IsMicrogestureActive(undefinedGesture), Is.False, "active " + undefinedGesture);
                Assert.That(commonGestures.WasMicrogesturePerformedThisFrame(undefinedGesture), Is.False, "performed " + undefinedGesture);
                Assert.That(commonGestures.WasMicrogestureCompletedThisFrame(undefinedGesture), Is.False, "completed " + undefinedGesture);
            }
        }
        finally
        {
            subsystem.Destroy();
        }
    }

    [Test]
    [Description("The provider's IsMicrogestureActive returns false for None and for undefined enum values on a hand " +
                 "reporting all five microgestures, and the base provider returns false for every value.")]
    public void ProviderIsMicrogestureActiveReturnsFalseForNoneAndUndefinedValues()
    {
        var subsystem = CreateMicrogestureCapableSubsystem();
        try
        {
            var provider = StartWithMicrogestures(subsystem, true);
            provider.leftHandMicrogesturesToReport = XRHandMicrogestureUtility.k_AllMicrogestures;

            var baseProvider = new BaseProvider();
            Assert.That(provider.IsMicrogestureActive(Handedness.Left, XRHandMicrogesture.TapThumb), Is.True);
            Assert.That(provider.IsMicrogestureActive(Handedness.Left, XRHandMicrogesture.None), Is.False);
            Assert.That(provider.IsMicrogestureActive(Handedness.Left, (XRHandMicrogesture)0x06), Is.False);
            Assert.That(provider.IsMicrogestureActive(Handedness.Left, (XRHandMicrogesture)0xFF), Is.False);

            Assert.That(baseProvider.IsMicrogestureActive(Handedness.Left, XRHandMicrogesture.TapThumb), Is.False);
            Assert.That(baseProvider.IsMicrogestureActive(Handedness.Left, (XRHandMicrogesture)0x06), Is.False);
            Assert.That(baseProvider.IsMicrogestureActive(Handedness.Left, (XRHandMicrogesture)0xFF), Is.False);
        }
        finally
        {
            subsystem.Destroy();
        }
    }
}
