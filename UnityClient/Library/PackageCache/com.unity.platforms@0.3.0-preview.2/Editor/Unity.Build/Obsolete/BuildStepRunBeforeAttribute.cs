using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Remove usage. (RemovedAfter 2020-07-01)", true)]
    public sealed class BuildStepRunBeforeAttribute : BuildStepOrderAttribute
    {
        public BuildStepRunBeforeAttribute(Type type) : base(type) => throw null;
    }
}
