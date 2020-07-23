using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestRunner;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Runtime;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.PerformanceTesting.Data;

[assembly: TestRunCallback(typeof(PlayerCallbacks))]

namespace Unity.PerformanceTesting
{
    public class PlayerCallbacks : ITestRunCallback
    {
        internal static bool saved;
        public void RunStarted(ITest testsToRun) { }

        public void RunFinished(ITestResult testResults)
        {
            saved = false;
        }

        public void TestStarted(ITest test) { }

        public void TestFinished(ITestResult result) { }

        internal static void LogMetadata()
        {
            if (saved) return;
            var run = ReadPerformanceTestRun();
            run.Hardware = GetHardware();
            SetPlayerSettings(run);
            run.TestSuite = Application.isPlaying ? "Playmode" : "Editmode";

            var json = JsonConvert.SerializeObject(run);
            TestContext.Out?.WriteLine("##performancetestruninfo2:" + json);
            saved = true;
        }

        private static Run ReadPerformanceTestRun()
        {
            try
            {
                var runResource = Resources.Load<TextAsset>(Utils.TestRunInfo.Replace(".json", ""));
                var json = Application.isEditor ? PlayerPrefs.GetString(Utils.PlayerPrefKeyRunJSON) : runResource.text;
                var run = JsonConvert.DeserializeObject<Run>(json);
                return run;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        private static Hardware GetHardware()
        {
            return new Hardware
            {
                OperatingSystem = SystemInfo.operatingSystem,
                DeviceModel = SystemInfo.deviceModel,
                DeviceName = SystemInfo.deviceName,
                ProcessorType = SystemInfo.processorType,
                ProcessorCount = SystemInfo.processorCount,
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                SystemMemorySizeMB = SystemInfo.systemMemorySize
            };
        }

        private static void SetPlayerSettings(Run run)
        {
            run.Player.Vsync = QualitySettings.vSyncCount;
            run.Player.AntiAliasing = QualitySettings.antiAliasing;
            run.Player.ColorSpace = QualitySettings.activeColorSpace.ToString();
            run.Player.AnisotropicFiltering = QualitySettings.anisotropicFiltering.ToString();
            run.Player.BlendWeights = QualitySettings.skinWeights.ToString();
            run.Player.ScreenRefreshRate = Screen.currentResolution.refreshRate;
            run.Player.ScreenWidth = Screen.currentResolution.width;
            run.Player.ScreenHeight = Screen.currentResolution.height;
            run.Player.Fullscreen = Screen.fullScreen;
            run.Player.Batchmode = Application.isBatchMode;
            run.Player.Development = Application.isEditor ? true : Debug.isDebugBuild;
            run.Player.Platform = Application.platform.ToString();
            run.Player.GraphicsApi = SystemInfo.graphicsDeviceType.ToString();
        }
    }
}
