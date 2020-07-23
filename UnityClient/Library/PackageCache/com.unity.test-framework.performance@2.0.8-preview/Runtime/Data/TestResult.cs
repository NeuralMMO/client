using System;
using System.Collections.Generic;

namespace Unity.PerformanceTesting.Data
{
    public class TestResult
    {
        public string Name;
        public string Version;
        public List<string> Categories = new List<string>();
        public List<SampleGroup> SampleGroups = new List<SampleGroup>();
    }
}
