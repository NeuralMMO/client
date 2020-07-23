using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Remove usage. (RemovedAfter 2020-07-01)", true)]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RunStepAttribute : Attribute
    {
        public string Name { get => throw null; set => throw null; }
        public string Category { get => throw null; set => throw null; }
    }
}
