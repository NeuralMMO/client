using UnityEngine.Profiling;

namespace Unity.PerformanceTesting
{
    public class SampleGroup : Unity.PerformanceTesting.Data.SampleGroup
    {
        internal Recorder Recorder;

        public SampleGroup(string name, SampleUnit unit = SampleUnit.Millisecond, bool increaseIsBetter = false)
            : base(name, unit, increaseIsBetter) { }

        public Recorder GetRecorder()
        {
            return Recorder ?? (Recorder = Recorder.Get(Name));
        }
    }
}
