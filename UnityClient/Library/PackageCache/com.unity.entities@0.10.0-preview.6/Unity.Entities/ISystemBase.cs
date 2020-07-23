using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Jobs;

namespace Unity.Entities
{
    // This is internal for now to prevent users from accessing the functionality. We need to land this first PR as a stop gap to get features into CI and get coverage on stuff.

    /// <summary>
    /// Interface implemented by unmanaged component systems.
    /// </summary>
    internal interface ISystemBase
    {
        void OnCreate(ref SystemState state);
        void OnDestroy(ref SystemState state);
        void OnUpdate(ref SystemState state);
    }
}
