using System;

namespace Unity.Build
{
    public interface IRunInstance : IDisposable
    {
        bool IsRunning { get; }
    }
}
