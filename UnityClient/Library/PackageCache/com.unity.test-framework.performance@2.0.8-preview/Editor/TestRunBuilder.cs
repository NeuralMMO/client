using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Unity.PerformanceTesting.Data;
using Unity.PerformanceTesting.Editor;
using Unity.PerformanceTesting.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

[assembly: PrebuildSetup(typeof(TestRunBuilder))]
[assembly: PostBuildCleanup(typeof(TestRunBuilder))]

namespace Unity.PerformanceTesting.Editor
{
    public class TestRunBuilder : IPrebuildSetup, IPostBuildCleanup
    {
        private const string cleanResources = "PT_ResourcesCleanup";

        public void Setup()
        {
            var run = new Run();
            run.Editor = GetEditorInfo();
            run.Dependencies = GetPackageDependencies();
            SetBuildSettings(run);

            run.Date = (int)Utils.ConvertToUnixTimestamp(DateTime.Now);

            CreateResourcesFolder();
            CreatePerformanceTestRunJson(run);
        }

        static List<string> GetPackageDependencies()
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true);
            while (!listRequest.IsCompleted)
                System.Threading.Thread.Sleep(10);
            if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
                Debug.LogError("Failed to list local packages");
            var packages = new List<UnityEditor.PackageManager.PackageInfo>(listRequest.Result);
            var reformated = packages.Select(p => $"{p.name}@{p.version}").ToList();
            return reformated;
        }

        public void Cleanup()
        {
            if (File.Exists(Utils.TestRunPath))
            {
                File.Delete(Utils.TestRunPath);
                File.Delete(Utils.TestRunPath + ".meta");
            }

            if (EditorPrefs.GetBool(cleanResources))
            {
                Directory.Delete("Assets/Resources/", true);
                File.Delete("Assets/Resources.meta");
            }

            AssetDatabase.Refresh();
        }

        private static Data.Editor GetEditorInfo()
        {
            var fullVersion = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion();
            const string pattern = @"(.+\.+.+\.\w+)|((?<=\().+(?=\)))";
            var matches = Regex.Matches(fullVersion, pattern);

            return new Data.Editor
            {
                Branch = GetEditorBranch(),
                Version = matches[0].Value,
                Changeset = matches[1].Value,
                Date = UnityEditorInternal.InternalEditorUtility.GetUnityVersionDate(),
            };
        }

        private static string GetEditorBranch()
        {
            foreach (var method in typeof(UnityEditorInternal.InternalEditorUtility).GetMethods())
            {
                if (method.Name.Contains("GetUnityBuildBranch"))
                {
                    return (string)method.Invoke(null, null);
                }
            }

            return "null";
        }

        private static void SetBuildSettings(Run run)
        {
            if (run.Player == null) run.Player = new Player();

            run.Player.GpuSkinning = PlayerSettings.gpuSkinning;
            run.Player.ScriptingBackend = PlayerSettings
                .GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup).ToString();
            run.Player.RenderThreadingMode = PlayerSettings.graphicsJobs ? "GraphicsJobs" :
                PlayerSettings.MTRendering ? "MultiThreaded" : "SingleThreaded";
            run.Player.AndroidTargetSdkVersion = PlayerSettings.Android.targetSdkVersion.ToString();
            run.Player.AndroidBuildSystem = EditorUserBuildSettings.androidBuildSystem.ToString();
            run.Player.BuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            run.Player.StereoRenderingPath = PlayerSettings.stereoRenderingPath.ToString();
        }

        private void CreateResourcesFolder()
        {
            if (Directory.Exists(Utils.ResourcesPath))
            {
                EditorPrefs.SetBool(cleanResources, false);
                return;
            }

            EditorPrefs.SetBool(cleanResources, true);
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        private void CreatePerformanceTestRunJson(Run run)
        {
            var json = JsonConvert.SerializeObject(run, Formatting.Indented);
            PlayerPrefs.SetString(Utils.PlayerPrefKeyRunJSON, json);
            File.WriteAllText(Utils.TestRunPath, json);
            AssetDatabase.Refresh();
        }
    }
}
