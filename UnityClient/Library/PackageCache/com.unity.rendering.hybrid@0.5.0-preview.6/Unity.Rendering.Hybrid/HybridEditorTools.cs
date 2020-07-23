using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Unity.Rendering
{
    internal static class HybridEditorTools
    {
        internal struct HybridDebugSettings
        {
            // error CS0649: Field is never assigned to, and will always have its default value 0
#pragma warning disable CS0649
            public bool RecreateAllBatches;
            public bool ForceInstanceDataUpload;
#pragma warning restore CS0649
        }

#if UNITY_EDITOR
#if ENABLE_HYBRID_RENDERER_V2
        [MenuItem("DOTS/Hybrid Renderer/Reupload all instance data")]
        internal static void ReuploadAllInstanceData()
        {
            s_HybridDebugSettings.ForceInstanceDataUpload = true;
        }

        [MenuItem("DOTS/Hybrid Renderer/Recreate all batches")]
        internal static void RecreateAllBatches()
        {
            s_HybridDebugSettings.RecreateAllBatches = true;
        }

#endif
        internal static void EndFrame()
        {
            s_HybridDebugSettings = default;
        }

        private static HybridDebugSettings s_HybridDebugSettings;
        internal static HybridDebugSettings DebugSettings => s_HybridDebugSettings;

#else
        internal static void EndFrame()
        {
        }

        internal static HybridDebugSettings DebugSettings => default;
#endif
    }
}
