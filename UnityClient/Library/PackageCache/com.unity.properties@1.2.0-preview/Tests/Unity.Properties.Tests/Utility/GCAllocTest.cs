using System;
using NUnit.Framework;
using UnityEngine.Profiling;

namespace Unity.Properties.Tests
{
    static class GCAllocTest
    {
        public class MethodMeasurement
        {
            readonly Recorder m_GCAllocRecorder;
            
            bool m_Warmup;
            Action m_SetUp;
            Action m_Action;
            int m_Expected;

            public MethodMeasurement(Action action)
            {
                m_Action = action;
                m_GCAllocRecorder = Recorder.Get("GC.Alloc");
            }

            public MethodMeasurement SetUp(Action action)
            {
                m_SetUp = action;
                return this;
            }

            public MethodMeasurement Warmup()
            {
                m_Warmup = true;
                return this;
            }

            public MethodMeasurement ExpectedCount(int expected)
            {
                m_Expected = expected;
                return this;
            }

            public void Run()
            {
                if (m_Warmup)
                {
                    CountGCAllocs();
                }

                var count = CountGCAllocs();
                Assert.That(count, Is.EqualTo(m_Expected));
            }

            int CountGCAllocs()
            {
                m_SetUp?.Invoke();
                
                m_GCAllocRecorder.FilterToCurrentThread();
                m_GCAllocRecorder.enabled = false;
                m_GCAllocRecorder.enabled = true;

                m_Action();

                m_GCAllocRecorder.enabled = false;
                return m_GCAllocRecorder.sampleBlockCount;
            }
        }

        public static MethodMeasurement Method(Action action)
        {
            return new MethodMeasurement(action);
        }
    }
}