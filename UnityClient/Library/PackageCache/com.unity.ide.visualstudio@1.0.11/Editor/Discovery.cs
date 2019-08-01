using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Unity.CodeEditor;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Microsoft.Win32;

namespace VisualStudioEditor
{
    public interface IDiscovery {
        CodeEditor.Installation[] PathCallback();
    }

    public class Discovery : IDiscovery {
        internal static string VisualStudioVersionToNiceName(VisualStudioVersion version)
        {
            switch (version)
            {
                case VisualStudioVersion.Invalid: return "Invalid Version";
                case VisualStudioVersion.VisualStudio2008: return "Visual Studio 2008";
                case VisualStudioVersion.VisualStudio2010: return "Visual Studio 2010";
                case VisualStudioVersion.VisualStudio2012: return "Visual Studio 2012";
                case VisualStudioVersion.VisualStudio2013: return "Visual Studio 2013";
                case VisualStudioVersion.VisualStudio2015: return "Visual Studio 2015";
                case VisualStudioVersion.VisualStudio2017: return "Visual Studio 2017";
                case VisualStudioVersion.VisualStudio2019: return "Visual Studio 2019";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public CodeEditor.Installation[] PathCallback()
        {
            try
            {
                if (VSEditor.IsWindows)
                {
                    return GetInstalledVisualStudios().Select(pair => new CodeEditor.Installation
                    {
                        Path = pair.Value[0],
                        Name = VisualStudioVersionToNiceName(pair.Key)
                    }).ToArray();
                }
                if (VSEditor.IsOSX)
                {
                    var installationList = new List<CodeEditor.Installation>();
                    AddIfDirectoryExists("Visual Studio", "/Applications/Visual Studio.app", installationList);
                    AddIfDirectoryExists("Visual Studio (Preview)", "/Applications/Visual Studio (Preview).app", installationList);
                    return installationList.ToArray();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Error detecting Visual Studio installations: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return new CodeEditor.Installation[0];
        }

        void AddIfDirectoryExists(string name, string path, List<CodeEditor.Installation> installations)
        {
            if (Directory.Exists(path))
            {
                installations.Add(new CodeEditor.Installation { Name = name, Path = path });
            }
        }

        static string GetRegistryValue(string path, string key)
        {
            try
            {
                return Registry.GetValue(path, key, null) as string;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Derives the Visual Studio installation path from the debugger path
        /// </summary>
        /// <returns>
        /// The Visual Studio installation path (to devenv.exe)
        /// </returns>
        /// <param name='debuggerPath'>
        /// The debugger path from the windows registry
        /// </param>
        static string DeriveVisualStudioPath(string debuggerPath)
        {
            string startSentinel = DeriveProgramFilesSentinel();
            string endSentinel = "Common7";
            bool started = false;
            string[] tokens = debuggerPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // Walk directories in debugger path, chop out "Program Files\INSTALLATION\PATH\HERE\Common7"
            foreach (var token in tokens)
            {
                if (!started && string.Equals(startSentinel, token, StringComparison.OrdinalIgnoreCase))
                {
                    started = true;
                    continue;
                }
                if (started)
                {
                    path = Path.Combine(path, token);
                    if (string.Equals(endSentinel, token, StringComparison.OrdinalIgnoreCase))
                        break;
                }
            }

            return Path.Combine(path, "IDE", "devenv.exe");
        }

        /// <summary>
        /// Derives the program files sentinel for grabbing the VS installation path.
        /// </summary>
        /// <remarks>
        /// From a path like 'c:\Archivos de programa (x86)', returns 'Archivos de programa'
        /// </remarks>
        static string DeriveProgramFilesSentinel()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();

            if (!string.IsNullOrEmpty(path))
            {
                // This needs to be the "real" Program Files regardless of 64bitness
                int index = path.LastIndexOf("(x86)");
                if (0 <= index)
                    path = path.Remove(index);
                return path.TrimEnd();
            }

            return "Program Files";
        }

        public static void ParseRawDevEnvPaths(string[] rawDevEnvPaths, Dictionary<VisualStudioVersion, string[]> versions)
        {
            if (rawDevEnvPaths == null)
            {
                return;
            }

            var v2017 = rawDevEnvPaths.Where(path => path.Contains("2017")).ToArray();
            var v2019 = rawDevEnvPaths.Where(path => path.Contains("2019")).ToArray();

            if (v2017.Length > 0)
            {
                versions[VisualStudioVersion.VisualStudio2017] = v2017;
            }

            if (v2019.Length > 0)
            {
                versions[VisualStudioVersion.VisualStudio2019] = v2019;
            }
        }

        /// <summary>
        /// Detects Visual Studio installations using the Windows registry
        /// </summary>
        /// <returns>
        /// The detected Visual Studio installations
        /// </returns>
        public static Dictionary<VisualStudioVersion, string[]> GetInstalledVisualStudios()
        {
            var versions = new Dictionary<VisualStudioVersion, string[]>();

            if (VSEditor.IsWindows)
            {
                foreach (VisualStudioVersion version in Enum.GetValues(typeof(VisualStudioVersion)))
                {
                    if (version > VisualStudioVersion.VisualStudio2015)
                        continue;

                    try
                    {
                        // Try COMNTOOLS environment variable first
                        FindLegacyVisualStudio(version, versions);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"VS: {e.Message}");
                    }
                }

                var raw = FindVisualStudioDevEnvPaths();

                ParseRawDevEnvPaths(raw.ToArray(), versions);
            }

            return versions;
        }

        static void FindLegacyVisualStudio(VisualStudioVersion version, Dictionary<VisualStudioVersion, string[]> versions)
        {
            string key = Environment.GetEnvironmentVariable($"VS{(int)version}0COMNTOOLS");
            if (!string.IsNullOrEmpty(key))
            {
                string path = Path.Combine(key, "..", "IDE", "devenv.exe");
                if (File.Exists(path))
                {
                    versions[version] = new[] { path };
                    return;
                }
            }

            // Try the proper registry key
            key = GetRegistryValue(
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\{(int)version}.0", "InstallDir");

            // Try to fallback to the 32bits hive
            if (string.IsNullOrEmpty(key))
                key = GetRegistryValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\{(int)version}.0", "InstallDir");

            if (!string.IsNullOrEmpty(key))
            {
                string path = Path.Combine(key, "devenv.exe");
                if (File.Exists(path))
                {
                    versions[version] = new[] { path };
                    return;
                }
            }

            // Fallback to debugger key
            key = GetRegistryValue(
                // VS uses this key for the local debugger path
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\{(int)version}.0\Debugger", "FEQARuntimeImplDll");
            if (!string.IsNullOrEmpty(key))
            {
                string path = DeriveVisualStudioPath(key);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    versions[version] = new[] { DeriveVisualStudioPath(key) };
            }
        }

        static IEnumerable<string> FindVisualStudioDevEnvPaths()
        {
            string asset = AssetDatabase.FindAssets("VSWhere a:packages").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(assetPath => assetPath.Contains("vswhere.exe"));
            if (string.IsNullOrWhiteSpace(asset)) // This may be called too early where the asset database has not replicated this information yet.
            {
                yield break;
            }
            UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(asset);
            var progpath = packageInfo.resolvedPath + asset.Substring("Packages/com.unity.ide.visualstudio".Length);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = progpath,
                    Arguments = "-prerelease -property productPath",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            process.Start();
            process.WaitForExit();

            while (!process.StandardOutput.EndOfStream)
            {
                yield return process.StandardOutput.ReadLine();
            }
        }
    }
}