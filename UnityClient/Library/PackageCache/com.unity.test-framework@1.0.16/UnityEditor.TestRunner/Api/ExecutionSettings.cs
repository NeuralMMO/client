using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class ExecutionSettings
    {
        public BuildTarget? targetPlatform;
        public ITestRunSettings overloadTestRunSettings;
        internal Filter filter;
        public Filter[] filters;
    }
}
