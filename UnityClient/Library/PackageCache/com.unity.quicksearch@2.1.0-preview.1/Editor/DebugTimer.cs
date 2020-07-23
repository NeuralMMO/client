#if UNITY_2020_1_OR_NEWER && PACKAGE_PERFORMANCE_TRACKING
#define USE_DEBUG_PERFORMANCE_TRACKER
#endif

using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if USE_DEBUG_PERFORMANCE_TRACKER
using Unity.PerformanceTracking;
#endif

namespace Unity.QuickSearch
{
    internal struct DebugTimer : IDisposable
    {
        private bool m_Disposed;
        private string m_Name;
        private Stopwatch m_Timer;
        private double m_MinTime;

        public double timeMs => m_Timer.Elapsed.TotalMilliseconds;

        #if USE_DEBUG_PERFORMANCE_TRACKER
        private PerformanceTracker? m_Tracker;
        #endif

        public DebugTimer(string name, bool useTracker = false, double minTimeMs = 0)
        {
            m_Disposed = false;
            m_Name = name;
            m_Timer = Stopwatch.StartNew();
            m_MinTime = minTimeMs;

            #if USE_DEBUG_PERFORMANCE_TRACKER
            if (useTracker)
                m_Tracker = new PerformanceTracker(name);
            else
                m_Tracker = null;
            #endif
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;
            m_Disposed = true;

            #if USE_DEBUG_PERFORMANCE_TRACKER
            if (m_Tracker.HasValue)
            {
                m_Tracker?.Dispose();
                m_Tracker = null;
            }
            #endif

            m_Timer.Stop();
            if (!String.IsNullOrEmpty(m_Name) && timeMs >= m_MinTime)
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"{m_Name} took {timeMs:F2} ms");
        }
    }
}