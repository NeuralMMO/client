using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Burst.LowLevel;
using UnityEditor;
using UnityEditor.Compilation;

namespace Unity.Burst.Editor
{
    /// <summary>
    /// Main entry point for initializing the burst compiler service for both JIT and AOT
    /// </summary>
    [InitializeOnLoad]
    internal class BurstLoader
    {
        // Cache the delegate to make sure it doesn't get collected.
        private static readonly BurstCompilerService.ExtractCompilerFlags TryGetOptionsFromMemberDelegate = TryGetOptionsFromMember;

        /// <summary>
        /// Gets the location to the runtime path of burst.
        /// </summary>
        public static string RuntimePath { get; private set; }

        public static bool IsDebugging { get; private set; }

        public static int DebuggingLevel { get; private set; }

        private static void VersionUpdateCheck()
        {
            var seek = "com.unity.burst@";
            var first = RuntimePath.LastIndexOf(seek);
            var last = RuntimePath.LastIndexOf(".Runtime");
            string version;
            if (first == -1 || last == -1 || last <= first)
            {
                version = "Unknown";
            }
            else
            {
                first += seek.Length;
                last -= 1;
                version = RuntimePath.Substring(first, last - first);
            }

            var result = BurstCompiler.VersionNotify(version);
            // result will be empty if we are shutting down, and thus we shouldn't popup a dialog
            if (!String.IsNullOrEmpty(result) && result != version)
            {
                if (IsDebugging)
                {
                    UnityEngine.Debug.LogWarning($"[com.unity.burst] - '{result}' != '{version}'");
                }
                OnVersionChangeDetected();
            }
        }

        private static void OnVersionChangeDetected()
        {
            // Write marker file to tell Burst to delete the cache at next startup.
            try
            {
                File.Create(Path.Combine(BurstCompilerOptions.DefaultCacheFolder, BurstCompilerOptions.DeleteCacheMarkerFileName)).Dispose();
            }
            catch (IOException)
            {
                // In the unlikely scenario that two processes are creating this marker file at the same time,
                // and one of them fails, do nothing because the other one has hopefully succeeded.
            }

            EditorUtility.DisplayDialog("Burst Package Update Detected", "The version of Burst used by your project has changed. Please restart the Editor to continue.", "OK");
            BurstCompiler.Shutdown();
        }

        static BurstLoader()
        {
            if (BurstCompilerOptions.ForceDisableBurstCompilation)
            {
                UnityEngine.Debug.LogWarning("[com.unity.burst] Burst is disabled entirely from the command line");
                return;
            }

            // This can be setup to get more diagnostics
            var debuggingStr = Environment.GetEnvironmentVariable("UNITY_BURST_DEBUG");
            IsDebugging = debuggingStr != null;
            if(IsDebugging)
            {
                UnityEngine.Debug.LogWarning("[com.unity.burst] Extra debugging is turned on.");
                int debuggingLevel;
                int.TryParse(debuggingStr, out debuggingLevel);
                if (debuggingLevel <= 0) debuggingLevel = 1;
                DebuggingLevel = debuggingLevel;
            }

            // Try to load the runtime through an environment variable
            RuntimePath = Environment.GetEnvironmentVariable("UNITY_BURST_RUNTIME_PATH");

            // Otherwise try to load it from the package itself
            if (!Directory.Exists(RuntimePath))
            {
                RuntimePath = Path.GetFullPath("Packages/com.unity.burst/.Runtime");
            }

            if(IsDebugging)
            {
                UnityEngine.Debug.LogWarning($"[com.unity.burst] Runtime directory set to {RuntimePath}");
            }

            BurstEditorOptions.EnsureSynchronized();

            BurstCompilerService.Initialize(RuntimePath, TryGetOptionsFromMemberDelegate);

            EditorApplication.quitting += BurstCompiler.Shutdown;

            CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;

            VersionUpdateCheck();

            // Workaround to update the list of assembly folders as soon as possible
            // in order for the JitCompilerService to not fail with AssemblyResolveExceptions.
            try
            {
                var assemblyList = BurstReflection.GetAssemblyList(AssembliesType.Editor);
                var assemblyFolders = new HashSet<string>();
                foreach (var assembly in assemblyList)
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(assembly.Location);
                        var assemblyFolder = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(assemblyFolder))
                        {
                            assemblyFolders.Add(assemblyFolder);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }

                // Notify the compiler
                var assemblyFolderList = assemblyFolders.ToList();
                if (IsDebugging)
                {
                    UnityEngine.Debug.Log($"Burst - Change of list of assembly folders:\n{string.Join("\n", assemblyFolderList)}");
                }
                BurstCompiler.UpdateAssemblerFolders(assemblyFolderList);
            }
            catch
            {
                // ignore
            }

            // Notify the compiler about a domain reload
            if (IsDebugging)
            {
                UnityEngine.Debug.Log("Burst - Domain Reload");
            }

            // Notify the JitCompilerService about a domain reload
            BurstCompiler.DomainReload();

            // Make sure that the X86 CSR function pointers are compiled
            Intrinsics.X86.CompileManagedCsrAccessors();

            // Make sure BurstRuntime is initialized
            BurstRuntime.Initialize();
        }

        private static void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (DebuggingLevel > 2)
            {
                UnityEngine.Debug.Log($"Burst - Change of Editor State: {state}");
            }
        }

        private static void OnAssemblyCompilationFinished(string arg1, CompilerMessage[] arg2)
        {
            // On assembly compilation finished, we cancel all pending compilation
            if (DebuggingLevel > 2)
            {
                UnityEngine.Debug.Log("Burst - Assembly compilation finished - cancelling any pending jobs");
            }

            BurstCompiler.Cancel();
        }

        private static void OnAssemblyCompilationStarted(string obj)
        {
            if (DebuggingLevel > 2)
            {
                UnityEngine.Debug.Log("Burst - Assembly compilation started - cancelling any pending jobs");
            }
        }

        private static bool TryGetOptionsFromMember(MemberInfo member, out string flagsOut)
        {
            return BurstCompiler.Options.TryGetOptions(member, true, out flagsOut);
        }
    }
}
