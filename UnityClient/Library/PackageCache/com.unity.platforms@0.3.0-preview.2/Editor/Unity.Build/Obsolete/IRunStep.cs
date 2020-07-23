using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Run steps must derive from RunStep instead of IRunStep. (RemovedAfter 2020-05-01) (UnityUpgradable) -> RunStep")]
    public interface IRunStep
    {
    }
}
