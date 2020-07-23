using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Remove usage. (RemovedAfter 2020-07-01)", true)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class BuildStepOrderAttribute : Attribute
    {
        public Type DependentStep { get => throw null; private set => throw null; }
        public BuildStepOrderAttribute(Type type) => throw null;
    }
}
