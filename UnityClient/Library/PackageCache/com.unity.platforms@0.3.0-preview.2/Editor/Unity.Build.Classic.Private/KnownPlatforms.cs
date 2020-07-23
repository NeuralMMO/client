using System;
using System.Collections.Generic;
using Unity.BuildSystem.NativeProgramSupport;

namespace Unity.Build.Classic.Private
{
    static class KnownPlatforms
    {
        public static Dictionary<Type, string> All { get; } = new Dictionary<Type, string>
        {
            {typeof(WindowsPlatform), "com.platforms.windows"},
            {typeof(MacOSXPlatform), "com.platforms.macos"},
            {typeof(LinuxPlatform), "com.platforms.linux"},
            {typeof(IosPlatform), "com.platforms.ios"},
            {typeof(AndroidPlatform), "com.platforms.android"}
        };
    }
}
