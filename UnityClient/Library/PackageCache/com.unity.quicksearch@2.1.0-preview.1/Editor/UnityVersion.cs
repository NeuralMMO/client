using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    internal static class UnityVersion
    {
        enum Candidate
        {
            Dev = 0,
            Alpha = 1 << 8,
            Beta = 1 << 16,
            Final = 1 << 24
        }

        static UnityVersion()
        {
            var version = Application.unityVersion.Split('.');

            if (version.Length < 2)
            {
                Console.WriteLine("Could not parse current Unity version '" + Application.unityVersion + "'; not enough version elements.");
                return;
            }

            if (int.TryParse(version[0], out Major) == false)
            {
                Console.WriteLine("Could not parse major part '" + version[0] + "' of Unity version '" + Application.unityVersion + "'.");
            }

            if (int.TryParse(version[1], out Minor) == false)
            {
                Console.WriteLine("Could not parse minor part '" + version[1] + "' of Unity version '" + Application.unityVersion + "'.");
            }

            if (version.Length >= 3)
            {
                try
                {
                    Build = ParseBuild(version[2]);
                }
                catch
                {
                    Console.WriteLine("Could not parse minor part '" + version[1] + "' of Unity version '" + Application.unityVersion + "'.");
                }
            }
        }

        public static int ParseBuild(string build)
        {
            var rev = 0;
            if (build.Contains("a"))
                rev = (int)Candidate.Alpha;
            else if (build.Contains("b"))
                rev = (int)Candidate.Beta;
            if (build.Contains("f"))
                rev = (int)Candidate.Final;
            var tags = build.Split('a', 'b', 'f', 'p', 'x');
            if (tags.Length == 2)
            {
                rev += Convert.ToInt32(tags[0], 10) << 4;
                rev += Convert.ToInt32(tags[1], 10);
            }
            return rev;
        }

        [UsedImplicitly, RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLoaded()
        {
            // This method ensures that this type has been initialized before any loading of objects occurs.
            // If this isn't done, the static constructor may be invoked at an illegal time that is not
            // allowed by Unity. During scene deserialization, off the main thread, is an example.
        }

        public static bool IsVersionGreaterOrEqual(int major, int minor)
        {
            if (Major > major)
                return true;
            if (Major == major)
            {
                if (Minor >= minor)
                    return true;
            }

            return false;
        }

        public static bool IsVersionGreaterOrEqual(int major, int minor, int build)
        {
            if (Major > major)
                return true;
            if (Major == major)
            {
                if (Minor > minor)
                    return true;

                if (Minor == minor)
                {
                    if (Build >= build)
                        return true;
                }
            }

            return false;
        }

        public static readonly int Major;
        public static readonly int Minor;
        public static readonly int Build;
    }
}