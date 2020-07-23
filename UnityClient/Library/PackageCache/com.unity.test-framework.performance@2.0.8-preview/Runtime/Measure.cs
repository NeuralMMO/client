using System;
using System.Diagnostics;
using Unity.PerformanceTesting.Measurements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.PerformanceTesting
{
    public static class Measure
    {
        public static void Custom(SampleGroup sampleGroup, double value)
        {
            if (PerformanceTest.GetSampleGroup(sampleGroup.Name) == null)
            {
                PerformanceTest.Active.SampleGroups.Add(sampleGroup);
            }

            sampleGroup.Samples.Add(value);
        }

        public static void Custom(string name, double value)
        {
            var sg = PerformanceTest.GetSampleGroup(name);
            if (sg == null)
            {
                sg = new SampleGroup(name);
                PerformanceTest.Active.SampleGroups.Add(sg);
            }

            sg.Samples.Add(value);
        }

        public static ScopeMeasurement Scope(string name = "Time")
        {
            return new ScopeMeasurement(name);
        }

        public static ProfilerMeasurement ProfilerMarkers(params string[] profilerMarkerLabels)
        {
            return new ProfilerMeasurement(profilerMarkerLabels);
        }

        public static MethodMeasurement Method(Action action)
        {
            return new MethodMeasurement(action);
        }

        public static FramesMeasurement Frames()
        {
            return new FramesMeasurement();
        }
    }

    public struct ScopeMeasurement : IDisposable
    {
        private readonly SampleGroup m_SampleGroup;
        private readonly long m_StartTicks;

        public ScopeMeasurement(string name)
        {
            m_SampleGroup = PerformanceTest.GetSampleGroup(name);
            if (m_SampleGroup == null)
            {
                m_SampleGroup = new SampleGroup(name);
                PerformanceTest.Active.SampleGroups.Add(m_SampleGroup);
            }

            m_StartTicks = Stopwatch.GetTimestamp();
            PerformanceTest.Disposables.Add(this);
        }

        public void Dispose()
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - m_StartTicks;
            PerformanceTest.Disposables.Remove(this);
            var delta = TimeSpan.FromTicks(elapsedTicks).TotalMilliseconds;

            Measure.Custom(m_SampleGroup, delta);
        }
    }

    public struct ProfilerMeasurement : IDisposable
    {
        private readonly ProfilerMarkerMeasurement m_Test;

        public ProfilerMeasurement(string[] profilerMarkers)
        {
            if (profilerMarkers == null)
            {
                m_Test = null;
                return;
            }

            if (profilerMarkers.Length == 0)
            {
                m_Test = null;
                return;
            }

            var go = new GameObject("Recorder");
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            m_Test = go.AddComponent<ProfilerMarkerMeasurement>();
            m_Test.AddProfilerSample(profilerMarkers);
            PerformanceTest.Disposables.Add(this);
        }

        public void Dispose()
        {
            PerformanceTest.Disposables.Remove(this);
            if (m_Test == null) return;
            m_Test.StopAndSampleRecorders();
            Object.DestroyImmediate(m_Test.gameObject);
        }
    }
}
