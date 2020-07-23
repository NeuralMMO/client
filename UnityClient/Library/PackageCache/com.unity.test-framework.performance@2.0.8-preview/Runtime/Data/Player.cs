using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.PerformanceTesting.Data
{
    public class Player
    {
        public bool Development;
        public int ScreenWidth;
        public int ScreenHeight;
        public int ScreenRefreshRate;
        public bool Fullscreen;
        public int Vsync;
        public int AntiAliasing;
        public bool Batchmode;
        public string RenderThreadingMode;
        public bool GpuSkinning;
        
        // enum to string converter is stripped out in il2cpp builds
        // and numbers are too unreadable so parsing to strings
        public string Platform;
        public string ColorSpace;
        public string AnisotropicFiltering;
        public string BlendWeights;
        public string GraphicsApi;
        
        // strings because their enums are editor only
        public string ScriptingBackend;
        public string AndroidTargetSdkVersion;
        public string AndroidBuildSystem;
        public string BuildTarget;
        public string StereoRenderingPath;

    }
}
