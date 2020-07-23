using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

namespace Unity.Burst.Editor
{
    internal enum AvailX86Targets
    {
        SSE2 = (int)TargetCpu.X86_SSE2,
        SSE4 = (int)TargetCpu.X86_SSE4,
    }

    [Flags]
    internal enum BitsetX86Targets
    {
        SSE2 = 1 << AvailX86Targets.SSE2,
        SSE4 = 1 << AvailX86Targets.SSE4,
    }

    internal enum AvailX64Targets
    {
        SSE2 = (int)TargetCpu.X64_SSE2,
        SSE4 = (int)TargetCpu.X64_SSE4,
        AVX = (int)TargetCpu.AVX,
        AVX2 = (int)TargetCpu.AVX2,
    }

    [Flags]
    internal enum BitsetX64Targets
    {
        SSE2 = 1 << AvailX64Targets.SSE2,
        SSE4 = 1 << AvailX64Targets.SSE4,
        AVX = 1 << AvailX64Targets.AVX,
        AVX2 = 1 << AvailX64Targets.AVX2,
    }

    class BurstPlatformLegacySettings : ScriptableObject
    {
        [SerializeField]
        internal bool DisableOptimisations;
        [SerializeField]
        internal bool DisableSafetyChecks;
        [SerializeField]
        internal bool DisableBurstCompilation;

        BurstPlatformLegacySettings(BuildTarget target)
        {
            DisableSafetyChecks = true;
            DisableBurstCompilation = false;
            DisableOptimisations = false;
        }
    }

    // To add a setting,
    //  Add a
    //          [SerializeField] internal type settingname;
    //  Add a
    //          internal static readonly string settingname_DisplayName = "Name of option to be displayed in the editor (and searched for)";
    //  Add a
    //          internal static readonly string settingname_ToolTip = "tool tip information to display when hovering mouse
    // If the setting should be restricted to e.g. Standalone platform :
    //
    //  Add a
    //          internal static bool settingname_Display(BuildTarget selectedTarget) {}
    class BurstPlatformAotSettings : ScriptableObject
    {
        [SerializeField]
        internal int Version;
        [SerializeField]
        internal bool EnableBurstCompilation;
        [SerializeField]
        internal bool EnableOptimisations;
        [SerializeField]
        internal bool EnableSafetyChecks;
        [SerializeField]
        internal bool EnableDebugInAllBuilds;
        [SerializeField]
        internal bool UsePlatformSDKLinker;
        [SerializeField]
        internal AvailX86Targets CpuMinTargetX32;
        [SerializeField]
        internal AvailX86Targets CpuMaxTargetX32;
        [SerializeField]
        internal AvailX64Targets CpuMinTargetX64;
        [SerializeField]
        internal AvailX64Targets CpuMaxTargetX64;
        [SerializeField]
        internal BitsetX86Targets CpuTargetsX32;
        [SerializeField]
        internal BitsetX64Targets CpuTargetsX64;

        internal static readonly string EnableDebugInAllBuilds_DisplayName = "Force Debug Information";
        internal static readonly string EnableDebugInAllBuilds_ToolTip = "Generates debug information for the Burst-compiled code, irrespective of if Development Mode is ticked. This can be used to generate symbols for release builds for platforms that need it.";

        internal static readonly string EnableOptimisations_DisplayName = "Enable Optimisations";
        internal static readonly string EnableOptimisations_ToolTip = "Enables all optimisations for the currently selected platform.";

        internal static readonly string EnableSafetyChecks_DisplayName = "Enable Safety Checks";
        internal static readonly string EnableSafetyChecks_ToolTip = "Enables safety checks, results in faster runtime, but Out Of Bounds checks etc are not validated.";

        internal static readonly string EnableBurstCompilation_DisplayName = "Enable Burst Compilation";
        internal static readonly string EnableBurstCompilation_ToolTip = "Enables burst compilation for the selected platform.";

        internal static readonly string UsePlatformSDKLinker_DisplayName = "Use Platform SDK Linker";
        internal static readonly string UsePlatformSDKLinker_ToolTip = "Enabling this option will disable cross compilation support for desktops, and will require platform specific tools for Windows/Linux/Mac - use only if you encounter problems with the burst builtin solution.";
        internal static bool UsePlatformSDKLinker_Display(BuildTarget selectedTarget)
        {
            return IsStandalone(selectedTarget);
        }

        internal static readonly string CpuTargetsX32_DisplayName = "Target 32Bit CPU Architectures";
        internal static readonly string CpuTargetsX32_ToolTip = "Use this to specify the set of target architectures to support for the currently selected platform.";
        internal static bool CpuTargetsX32_Display(BuildTarget selectedTarget)
        {
            return IsStandalone(selectedTarget) && Has32BitSupport(selectedTarget);
        }

        internal static readonly string CpuTargetsX64_DisplayName = "Target 64Bit CPU Architectures";
        internal static readonly string CpuTargetsX64_ToolTip = "Use this to specify the target architectures to support for the currently selected platform.";
        internal static bool CpuTargetsX64_Display(BuildTarget selectedTarget)
        {
            return IsStandalone(selectedTarget);
        }

        internal static bool IsStandalone(BuildTarget target)
        {
            switch (target)
            {
#if !UNITY_2019_2_OR_NEWER
                case BuildTarget.StandaloneLinux:
#endif
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                    return true;
                default:
                    return false;
            }
        }

        BurstPlatformAotSettings(BuildTarget target)
        {
            InitialiseDefaults();
        }

        internal void InitialiseDefaults()
        {
            Version = 3;
            EnableSafetyChecks = false;
            EnableBurstCompilation = true;
            EnableOptimisations = true;
            EnableDebugInAllBuilds = false;
            UsePlatformSDKLinker = false; // Only applicable for desktop targets (Windows/Mac/Linux)
            CpuMinTargetX32 = 0;
            CpuMaxTargetX32 = 0;
            CpuMinTargetX64 = 0;
            CpuMaxTargetX64 = 0;
            CpuTargetsX32 = BitsetX86Targets.SSE2 | BitsetX86Targets.SSE4;
            CpuTargetsX64 = BitsetX64Targets.SSE2 | BitsetX64Targets.AVX2;
        }

        internal static string GetPath(BuildTarget target)
        {
            return "ProjectSettings/BurstAotSettings_" + target.ToString() + ".json";
        }

        internal static BuildTarget ResolveTarget(BuildTarget target)
        {
            // Treat the 32/64 platforms the same from the point of view of burst settings
            // since there is no real way to distinguish from the platforms selector
            if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
                return BuildTarget.StandaloneWindows;

#if UNITY_2019_2_OR_NEWER
            // 32 bit linux support was deprecated
            if (target == BuildTarget.StandaloneLinux64)
                return BuildTarget.StandaloneLinux64;
#else
            if (target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinux)
                return BuildTarget.StandaloneLinux64;
#endif

            return target;
        }

        internal static BurstPlatformAotSettings GetOrCreateSettings(BuildTarget target)
        {
            target = ResolveTarget(target);
            var settings = CreateInstance<BurstPlatformAotSettings>();
            settings.InitialiseDefaults();
            string path = GetPath(target);

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                settings = SerialiseIn(target, json);
            }
            else
            {
                settings.Save(target);
            }

            return settings;
        }

        delegate bool SerialiseItem(BuildTarget selectedPlatform);

        internal static BurstPlatformAotSettings SerialiseIn(BuildTarget target, string json)
        {
            var versioned = (BurstPlatformAotSettings)ScriptableObject.CreateInstance<BurstPlatformAotSettings>();
            EditorJsonUtility.FromJsonOverwrite(json, versioned);

            if (versioned.Version == 0)
            {
                // Deal with pre versioned format
                var legacy = (BurstPlatformLegacySettings)ScriptableObject.CreateInstance<BurstPlatformLegacySettings>();
                EditorJsonUtility.FromJsonOverwrite(json, legacy);

                // Legacy file, upgrade it
                versioned.InitialiseDefaults();
                versioned.EnableOptimisations = !legacy.DisableOptimisations;
                versioned.EnableBurstCompilation = !legacy.DisableBurstCompilation;
                versioned.EnableSafetyChecks = !legacy.DisableSafetyChecks;
            }

            if (versioned.Version < 3)
            {
                // Upgrade the version first
                versioned.Version = 3;

                // Upgrade from min..max targets to bitset
                versioned.CpuTargetsX32 |= (BitsetX86Targets)(1 << (int)versioned.CpuMinTargetX32);
                versioned.CpuTargetsX32 |= (BitsetX86Targets)(1 << (int)versioned.CpuMaxTargetX32);

                versioned.CpuTargetsX64 |= (BitsetX64Targets)(1 << (int)versioned.CpuMinTargetX64);
                versioned.CpuTargetsX64 |= (BitsetX64Targets)(1 << (int)versioned.CpuMaxTargetX64);

                // Extra checks to add targets in the min..max range for 64-bit targets.
                switch (versioned.CpuMinTargetX64)
                {
                    default:
                        break;
                    case AvailX64Targets.SSE2:
                        switch (versioned.CpuMaxTargetX64)
                        {
                            default:
                                break;
                            case AvailX64Targets.AVX2:
                                versioned.CpuTargetsX64 |= (BitsetX64Targets)(1 << (int)AvailX64Targets.AVX);
                                goto case AvailX64Targets.AVX;
                            case AvailX64Targets.AVX:
                                versioned.CpuTargetsX64 |= (BitsetX64Targets)(1 << (int)AvailX64Targets.SSE4);
                                break;
                        }
                        break;
                    case AvailX64Targets.SSE4:
                        switch (versioned.CpuMaxTargetX64)
                        {
                            default:
                                break;
                            case AvailX64Targets.AVX2:
                                versioned.CpuTargetsX64 |= (BitsetX64Targets)(1 << (int)AvailX64Targets.AVX);
                                break;
                        }
                        break;

                }

                // Wipe the old min/max targets
                versioned.CpuMinTargetX32 = 0;
                versioned.CpuMaxTargetX32 = 0;
                versioned.CpuMinTargetX64 = 0;
                versioned.CpuMaxTargetX64 = 0;
            }

            // Otherwise should be a modern file with a valid version (we can use that to upgrade when the time comes)
            return versioned;
        }

        internal string SerialiseOut(BuildTarget target)
        {
            // Version 2 and onwards serialise a custom object in order to avoid serialising all the settings.
            StringBuilder s = new StringBuilder();
            s.Append("{\n");
            s.Append("  \"MonoBehaviour\": {\n");
            var platformFields = typeof(BurstPlatformAotSettings).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            int total = 0;
            for (int i = 0; i < platformFields.Length; i++)
            {
                var method = typeof(BurstPlatformAotSettings).GetMethod(platformFields[i].Name + "_Display", BindingFlags.Static | BindingFlags.NonPublic);
                if (method != null)
                {
                    var shouldSerialise = (SerialiseItem)Delegate.CreateDelegate(typeof(SerialiseItem), method);
                    if (!shouldSerialise(target))
                        continue;
                }

                total++;
            }
            for (int i = 0; i < platformFields.Length; i++)
            {
                var method = typeof(BurstPlatformAotSettings).GetMethod(platformFields[i].Name + "_Display", BindingFlags.Static | BindingFlags.NonPublic);
                if (method != null)
                {
                    var shouldSerialise = (SerialiseItem)Delegate.CreateDelegate(typeof(SerialiseItem), method);
                    if (!shouldSerialise(target))
                        continue;
                }

                s.Append($"    \"{platformFields[i].Name}\": ");
                if (platformFields[i].FieldType.IsEnum)
                    s.Append((int)platformFields[i].GetValue(this));
                else if (platformFields[i].FieldType == typeof(string))
                    s.Append($"\"{platformFields[i].GetValue(this)}\"");
                else if (platformFields[i].FieldType == typeof(bool))
                    s.Append(((bool)platformFields[i].GetValue(this)) ? "true" : "false");
                else
                    s.Append((int)platformFields[i].GetValue(this));

                total--;
                if (total != 0)
                    s.Append(",");
                s.Append("\n");
            }
            s.Append("  }\n");
            s.Append("}\n");

            return s.ToString();
        }

        internal void Save(BuildTarget target)
        {
            target = ResolveTarget(target);
            var path = GetPath(target);
#if UNITY_2019_3_OR_NEWER
            if (!AssetDatabase.IsOpenForEdit(path))
            {
                if (!AssetDatabase.MakeEditable(path))
                {
                    Debug.LogWarning($"Burst could not save AOT settings file {path}");
                    return;
                }
            }
#endif

            File.WriteAllText(path, SerialiseOut(target));
        }

        internal static SerializedObject GetSerializedSettings(BuildTarget target)
        {
            return new SerializedObject(GetOrCreateSettings(target));
        }

        internal static bool Has32BitSupport(BuildTarget target)
        {
            switch (target)
            {
#if !UNITY_2019_2_OR_NEWER
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
#endif
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return true;
                default:
                    return false;
            }
        }

        private static TargetCpu GetCpu(int v)
        {
            // https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
            var r = ((v > 0xFFFF) ? 1 : 0) << 4; v >>= r;
            var shift = ((v > 0xFF) ? 1 : 0) << 3; v >>= shift; r |= shift;
            shift = ((v > 0xF) ? 1 : 0) << 2; v >>= shift; r |= shift;
            shift = ((v > 0x3) ? 1 : 0) << 1; v >>= shift; r |= shift;
            r |= (v >> 1);
            return (TargetCpu)r;
        }

        private static IEnumerable<Enum> GetFlags(Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                {
                    yield return value;
                }
            }
        }

        internal TargetCpus GetDesktopCpu32Bit()
        {
            var cpus = new TargetCpus();

            foreach (var target in GetFlags(CpuTargetsX32))
            {
                cpus.Cpus.Add(GetCpu((int)(BitsetX86Targets)target));
            }

            // If no targets were specified just default to the oldest CPU supported.
            if (cpus.Cpus.Count == 0)
            {
                cpus.Cpus.Add(TargetCpu.X86_SSE2);
            }

            return cpus;
        }

        internal TargetCpus GetDesktopCpu64Bit()
        {
            var cpus = new TargetCpus();

            foreach (var target in GetFlags(CpuTargetsX64))
            {
                cpus.Cpus.Add(GetCpu((int)(BitsetX64Targets)target));
            }

            // If no targets were specified just default to the oldest CPU supported.
            if (cpus.Cpus.Count == 0)
            {
                cpus.Cpus.Add(TargetCpu.X64_SSE2);
            }

            return cpus;
        }
    }

    static class BurstAotSettingsIMGUIRegister
    {
        class BurstAotSettingsProvider : SettingsProvider
        {
            SerializedObject[] m_PlatformSettings;
            SerializedProperty[][] m_PlatformProperties;
            DisplayItem[][] m_PlatformVisibility;
            GUIContent[][] m_PlatformToolTips;

            BuildPlatform[] validPlatforms;

            delegate bool DisplayItem(BuildTarget selectedTarget);

            static bool DefaultShow(BuildTarget selectedTarget)
            {
                return true;
            }
            static bool DefaultHide(BuildTarget selectedTarget)
            {
                return false;
            }

            public BurstAotSettingsProvider()
                : base("Project/Burst AOT Settings", SettingsScope.Project, null)
            {
                int a;

                validPlatforms = BuildPlatforms.instance.GetValidPlatforms(true).ToArray();

                var platformFields = typeof(BurstPlatformAotSettings).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                int numPlatformFields = platformFields.Length;
                int numKeywords = numPlatformFields;
                var tempKeywords = new string[numKeywords];

                for (a = 0; a < numPlatformFields; a++)
                {
                    tempKeywords[a] = typeof(BurstPlatformAotSettings).GetField(platformFields[a].Name + "_ToolTip", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as string;
                }

                keywords = new HashSet<string>(tempKeywords);

                m_PlatformSettings = new SerializedObject[validPlatforms.Length];
                m_PlatformProperties = new SerializedProperty[validPlatforms.Length][];
                m_PlatformVisibility = new DisplayItem[validPlatforms.Length][];
                m_PlatformToolTips=new GUIContent[validPlatforms.Length][];
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                var platformFields = typeof(BurstPlatformAotSettings).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                for (int p = 0; p < validPlatforms.Length; p++)
                {
                    InitialiseSettingsForPlatform(p,platformFields);
                }
            }

            private void InitialiseSettingsForPlatform(int platform, FieldInfo[] platformFields)
            {
                if (validPlatforms[platform].targetGroup == BuildTargetGroup.Standalone)
                    m_PlatformSettings[platform] = BurstPlatformAotSettings.GetSerializedSettings(EditorUserBuildSettings.selectedStandaloneTarget);
                else
                    m_PlatformSettings[platform] = BurstPlatformAotSettings.GetSerializedSettings(validPlatforms[platform].defaultTarget);

                m_PlatformProperties[platform] = new SerializedProperty[platformFields.Length];
                m_PlatformToolTips[platform] = new GUIContent[platformFields.Length];
                m_PlatformVisibility[platform] = new DisplayItem[platformFields.Length];
                for (int i = 0; i < platformFields.Length; i++)
                {
                    m_PlatformProperties[platform][i] = m_PlatformSettings[platform].FindProperty(platformFields[i].Name);
                    var displayName = typeof(BurstPlatformAotSettings).GetField(platformFields[i].Name + "_DisplayName", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as string;
                    var toolTip = typeof(BurstPlatformAotSettings).GetField(platformFields[i].Name + "_ToolTip", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as string;
                    m_PlatformToolTips[platform][i] = EditorGUIUtility.TrTextContent(displayName, toolTip);

                    var method = typeof(BurstPlatformAotSettings).GetMethod(platformFields[i].Name + "_Display", BindingFlags.Static | BindingFlags.NonPublic);
                    if (method == null)
                    {
                        if (displayName == null)
                        {
                            m_PlatformVisibility[platform][i] = DefaultHide;
                        }
                        else
                        {
                            m_PlatformVisibility[platform][i] = DefaultShow;
                        }
                    }
                    else
                    {
                        m_PlatformVisibility[platform][i] = (DisplayItem)Delegate.CreateDelegate(typeof(DisplayItem), method);
                    }
                }
            }

            private string FetchStandaloneTargetName()
            {
                switch (EditorUserBuildSettings.selectedStandaloneTarget)
                {
                    case BuildTarget.StandaloneOSX:
                        return "Mac OS X";    // Matches the Build Settings Dialog names
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return "Windows";
                    default:
                        return "Linux";
                }
            }

            public override void OnGUI(string searchContext)
            {
                var rect = EditorGUILayout.BeginVertical();

                EditorGUIUtility.labelWidth = rect.width / 2;

                int selectedPlatform = EditorGUILayout.BeginPlatformGrouping(validPlatforms, null);

                // During a build and other cases, the settings object can become invalid, if it does, we re-build it for the current platform
                // this fixes the settings failing to save if modified after a build has finished, and the settings were still open
                if (!m_PlatformSettings[selectedPlatform].isValid)
                {
                    var platformFields = typeof(BurstPlatformAotSettings).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    InitialiseSettingsForPlatform(selectedPlatform, platformFields);
                }

                var selectedTarget = validPlatforms[selectedPlatform].defaultTarget;
                if (validPlatforms[selectedPlatform].targetGroup == BuildTargetGroup.Standalone)
                    selectedTarget = EditorUserBuildSettings.selectedStandaloneTarget;

                if (validPlatforms[selectedPlatform].targetGroup == BuildTargetGroup.Standalone)
                {
                    // Note burst treats Windows and Windows32 as the same target from a settings point of view (same for linux)
                    // So we only display the standalone platform
                    EditorGUILayout.LabelField(EditorGUIUtility.TrTextContent("Target Platform", "Shows the currently selected standalone build target, can be switched in the Build Settings dialog"), EditorGUIUtility.TrTextContent(FetchStandaloneTargetName()));
                }

                for (int i = 0; i < m_PlatformProperties[selectedPlatform].Length; i++)
                {
                    if (m_PlatformVisibility[selectedPlatform][i](selectedTarget))
                    {
                        EditorGUILayout.PropertyField(m_PlatformProperties[selectedPlatform][i], m_PlatformToolTips[selectedPlatform][i]);
                    }
                }

                EditorGUILayout.EndPlatformGrouping();

                EditorGUILayout.EndVertical();

                if (m_PlatformSettings[selectedPlatform].hasModifiedProperties)
                {
                    m_PlatformSettings[selectedPlatform].ApplyModifiedPropertiesWithoutUndo();
                    ((BurstPlatformAotSettings)m_PlatformSettings[selectedPlatform].targetObject).Save(selectedTarget);
                }
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateBurstAotSettingsProvider()
        {
            return new BurstAotSettingsProvider();
        }
    }
}
#else
// Mirror old behaviour
namespace Unity.Burst.Editor
{
    class BurstPlatformAotSettings
    {
        internal bool EnableOptimisations;
        internal bool EnableSafetyChecks;
        internal bool EnableBurstCompilation;
        internal bool EnableDebugInAllBuilds;
        internal bool UsePlatformSDKLinker;

        internal static BurstPlatformAotSettings GetOrCreateSettings(BuildTarget target)
        {
            BurstPlatformAotSettings settings = new BurstPlatformAotSettings();

            settings.EnableOptimisations = true;
            settings.EnableSafetyChecks = BurstEditorOptions.EnableBurstSafetyChecks;
            settings.EnableBurstCompilation = BurstEditorOptions.EnableBurstCompilation;
            settings.UsePlatformSDKLinker = false;
            settings.EnableDebugInAllBuilds = false;

            return settings;
        }

        internal TargetCpus GetDesktopCpu32Bit()
        {
            var cpus = new TargetCpus();

            cpus.Cpus.Add(TargetCpu.X86_SSE2);
            cpus.Cpus.Add(TargetCpu.X86_SSE4);

            return cpus;
        }

        internal TargetCpus GetDesktopCpu64Bit()
        {
            var cpus = new TargetCpus();

            cpus.Cpus.Add(TargetCpu.X64_SSE2);
            cpus.Cpus.Add(TargetCpu.X64_SSE4);

            return cpus;
        }
    }
}

#endif
