// As BurstCompiler.Compile is not supported on Tiny/ZeroPlayer, we can ifdef the entire file
#if !UNITY_DOTSPLAYER && !NET_DOTS
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if !BURST_COMPILER_SHARED
using Unity.Jobs.LowLevel.Unsafe;
#endif

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// NOTE: This file is shared via a csproj cs link in Burst.Compiler.IL
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

namespace Unity.Burst
{
    /// <summary>
    /// Options available at Editor time and partially at runtime to control the behavior of the compilation and to enable/disable burst jobs.
    /// </summary>
#if BURST_COMPILER_SHARED
    internal sealed partial class BurstCompilerOptionsInternal
#else
    public sealed partial class BurstCompilerOptions
#endif
    {
        private const string DisableCompilationArg = "--burst-disable-compilation";

        private const string ForceSynchronousCompilationArg = "--burst-force-sync-compilation";

        internal const string DefaultLibraryName = "lib_burst_generated";

        internal const string BurstInitializeName = "burst.initialize";

        internal static readonly string DefaultCacheFolder = Path.Combine(Environment.CurrentDirectory, "Library", "BurstCache", "JIT");
        internal const string DeleteCacheMarkerFileName = "DeleteCache.txt";

        // -------------------------------------------------------
        // Common options used by the compiler
        // -------------------------------------------------------
        internal const string OptionGroup = "group";
        internal const string OptionPlatform = "platform=";
        internal const string OptionBackend = "backend=";
        internal const string OptionSafetyChecks = "safety-checks";
        internal const string OptionDisableSafetyChecks = "disable-safety-checks";
        internal const string OptionDisableOpt = "disable-opt";
        internal const string OptionFastMath = "fastmath";
        internal const string OptionTarget = "target=";
        internal const string OptionIROpt = "ir-opt";
        internal const string OptionCpuOpt = "cpu-opt=";
        internal const string OptionFloatPrecision = "float-precision=";
        internal const string OptionFloatMode = "float-mode=";
        internal const string OptionDump = "dump=";
        internal const string OptionFormat = "format=";
        internal const string OptionDebugTrap = "debugtrap";
        internal const string OptionDisableVectors = "disable-vectors";
        internal const string OptionDebug = "debug=";
        internal const string OptionDebugMode = "debugMode";
        internal const string OptionStaticLinkage = "generate-static-linkage-methods";
        internal const string OptionJobMarshalling = "generate-job-marshalling-methods";
        internal const string OptionTempDirectory = "temp-folder=";
        internal const string OptionEnableDirectExternalLinking = "enable-direct-external-linking";

        // -------------------------------------------------------
        // Options used by the Jit and Bcl compilers
        // -------------------------------------------------------
        internal const string OptionCacheDirectory = "cache-directory=";

        // -------------------------------------------------------
        // Options used by the Jit compiler
        // -------------------------------------------------------

        internal const string OptionJitDisableFunctionCaching = "disable-function-caching";
        internal const string OptionJitDisableAssemblyCaching = "disable-assembly-caching";
        internal const string OptionJitEnableAssemblyCachingLogs = "enable-assembly-caching-logs";
        internal const string OptionJitEnableSynchronousCompilation = "enable-synchronous-compilation";

        // TODO: Remove this option and use proper dump flags or revisit how we log timings
        internal const string OptionJitLogTimings = "log-timings";

        internal const string OptionJitIsForFunctionPointer = "is-for-function-pointer";

        internal const string OptionJitManagedFunctionPointer = "managed-function-pointer=";

        internal const string OptionJitProvider = "jit-provider=";
        internal const string OptionJitSkipCheckDiskCache = "skip-check-disk-cache";

        // -------------------------------------------------------
        // Options used by the Aot compiler
        // -------------------------------------------------------
        internal const string OptionAotAssemblyFolder = "assembly-folder=";
        internal const string OptionRootAssembly = "root-assembly=";
        internal const string OptionIncludeRootAssemblyReferences = "include-root-assembly-references=";
        internal const string OptionAotMethod = "method=";
        internal const string OptionAotType = "type=";
        internal const string OptionAotAssembly = "assembly=";
        internal const string OptionAotOutputPath = "output=";
        internal const string OptionAotKeepIntermediateFiles = "keep-intermediate-files";
        internal const string OptionAotNoLink = "nolink";
        internal const string OptionAotPatchedAssembliesOutputFolder = "patch-assemblies-into=";
        internal const string OptionAotPinvokeNameToPatch = "pinvoke-name=";
        internal const string OptionAotExecuteMethodNameToFind = "execute-method-name=";

        internal const string OptionAotUsePlatformSDKLinkers = "use-platform-sdk-linkers";
        internal const string OptionAotOnlyStaticMethods = "only-static-methods";
        internal const string OptionMethodPrefix = "method-prefix=";
        internal const string OptionAotNoNativeToolchain = "no-native-toolchain";        
        internal const string OptionAotKeyFolder = "key-folder=";
        internal const string OptionAotDecodeFolder = "decode-folder=";
        internal const string OptionVerbose = "verbose";
        internal const string OptionValidateExternalToolChain = "validate-external-tool-chain";
        internal const string OptionCompilerThreads = "threads=";
        internal const string OptionChunkSize = "chunk-size=";
        internal const string OptionPrintLogOnMissingPInvokeCallbackAttribute = "print-monopinvokecallbackmissing-message";
        internal const string OptionOutputMode = "output-mode=";
        internal const string OptionAlwaysCreateOutput = "always-create-output=";
        internal const string OptionAotPdbSearchPaths = "pdb-search-paths=";

        internal const string CompilerCommandShutdown = "$shutdown";
        internal const string CompilerCommandCancel = "$cancel";
        internal const string CompilerCommandEnableCompiler = "$enable_compiler";
        internal const string CompilerCommandDisableCompiler = "$disable_compiler";
        internal const string CompilerCommandTriggerRecompilation = "$trigger_recompilation";
        internal const string CompilerCommandReset = "$reset";
        internal const string CompilerCommandDomainReload = "$domain_reload";
        internal const string CompilerCommandUpdateAssemblyFolders = "$update_assembly_folders";
        internal const string CompilerCommandVersionNotification = "$version";

        // All the following content is exposed to the public interface

#if !BURST_COMPILER_SHARED
        // These fields are only setup at startup
        internal static readonly bool ForceDisableBurstCompilation;
        private static readonly bool ForceBurstCompilationSynchronously;

#if UNITY_EDITOR
        private readonly bool _isConstructing;
#endif

        private bool _enableBurstCompilation;
        private bool _enableBurstCompileSynchronously;
        private bool _enableBurstSafetyChecks;
        private bool _enableBurstTimings;
        private bool _enableBurstDebug;

        private BurstCompilerOptions() : this(false)
        {
        }

        internal BurstCompilerOptions(bool isGlobal)
        {
#if UNITY_EDITOR
            _isConstructing = true;
#endif

            try
            {
                IsGlobal = isGlobal;
                // By default, burst is enabled as well as safety checks
                EnableBurstCompilation = true;
                EnableBurstSafetyChecks = true;
            }
            finally
            {
#if UNITY_EDITOR
                _isConstructing = false;
#endif
            }
        }

        /// <summary>
        /// <c>true</c> if this option is the global options that affects menus
        /// </summary>
        private bool IsGlobal { get; }

        /// <summary>
        /// Gets a boolean indicating whether burst is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => EnableBurstCompilation && !ForceDisableBurstCompilation;
        }

        /// <summary>
        /// Gets or sets a boolean to enable or disable compilation of burst jobs.
        /// </summary>
        public bool EnableBurstCompilation
        {
            get => _enableBurstCompilation;
            set
            {
                // If we are in the global settings, and we are forcing to no burst compilation
                if (IsGlobal && ForceDisableBurstCompilation) value = false;

                bool changed = _enableBurstCompilation != value;

#if UNITY_EDITOR && !UNITY_2019_3_OR_NEWER // Enabling Burst while in PlayMode is only supported in 2019.3+
                // Prevent Burst compilation being enabled while in PlayMode, because
                // we can't currently support this for jobs.
                if (!_isConstructing && IsGlobal && changed && value && UnityEngine.Application.isPlaying)
                {
                    throw new InvalidOperationException("Burst compilation can't be switched on while in PlayMode");
                }
#endif

                _enableBurstCompilation = value;

                // Modify only JobsUtility.JobCompilerEnabled when modifying global settings
                if (IsGlobal)
                {
                    // We need also to disable jobs as functions are being cached by the job system
                    // and when we ask for disabling burst, we are also asking the job system
                    // to no longer use the cached functions
                    JobsUtility.JobCompilerEnabled = value;

                    if (changed)
                    {
                        // Send the command to the compiler service
                        if (value)
                        {
                            BurstCompiler.Enable();
                            MaybeTriggerRecompilation();
                        }
                        else
                        {
                            BurstCompiler.Disable();
                        }
                    }
                }

                if (changed)
                {
                    OnOptionsChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean to force the compilation of all burst jobs synchronously.
        /// </summary>
        /// <remarks>
        /// This is only available at Editor time. Does not have an impact on player mode.
        /// </remarks>
        public bool EnableBurstCompileSynchronously
        {
            get => _enableBurstCompileSynchronously;
            set
            {
                bool changed = _enableBurstCompileSynchronously != value;
                _enableBurstCompileSynchronously = value;
                if (changed) OnOptionsChanged();
            }
        }

        /// <summary>
        /// Gets or sets a boolean to enable or disable safety checks.
        /// </summary>
        /// <remarks>
        /// This is only available at Editor time. Does not have an impact on player mode.
        /// </remarks>
        public bool EnableBurstSafetyChecks
        {
            get => _enableBurstSafetyChecks;
            set
            {
                bool changed = _enableBurstSafetyChecks != value;
                _enableBurstSafetyChecks = value;
                if (changed)
                {
                    OnOptionsChanged();
                    MaybeTriggerRecompilation();
                }
            }
        }

        public bool EnableBurstDebug
        {
            get => _enableBurstDebug;
            set
            {
                bool changed = _enableBurstDebug != value;
                _enableBurstDebug = value;
                if (changed)
                {
                    OnOptionsChanged();
                    MaybeTriggerRecompilation();
                }
            }
        }

        /// <summary>
        /// This property is no longer used and will be removed in a future major release.
        /// </summary>
        [Obsolete("This property is no longer used and will be removed in a future major release")]
        public bool DisableOptimizations
        {
            get => false;
            set
            {
            }
        }

        /// <summary>
        /// This property is no longer used and will be removed in a future major release. Use the [BurstCompile(FloatMode = FloatMode.Fast)] on the method directly to enable this feature
        /// </summary>
        [Obsolete("This property is no longer used and will be removed in a future major release. Use the [BurstCompile(FloatMode = FloatMode.Fast)] on the method directly to enable this feature")]
        public bool EnableFastMath
        {
            get => true;

            set
            {
                // ignored
            }
        }

        internal bool EnableBurstTimings
        {
            get => _enableBurstTimings;
            set
            {
                bool changed = _enableBurstTimings != value;
                _enableBurstTimings = value;
                if (changed) OnOptionsChanged();
            }
        }

        internal Action OptionsChanged { get; set; }

        internal BurstCompilerOptions Clone()
        {
            // WARNING: for some reason MemberwiseClone() is NOT WORKING on Mono/Unity
            // so we are creating a manual clone
            var clone = new BurstCompilerOptions
            {
                EnableBurstCompilation = EnableBurstCompilation,
                EnableBurstCompileSynchronously = EnableBurstCompileSynchronously,
                EnableBurstSafetyChecks = EnableBurstSafetyChecks,
                EnableBurstTimings = EnableBurstTimings,
                EnableBurstDebug = EnableBurstDebug
            };
            return clone;
        }

        private static bool TryGetAttribute(MemberInfo member, out BurstCompileAttribute attribute)
        {
            attribute = null;
            // We don't fail if member == null as this method is being called by native code and doesn't expect to crash
            if (member == null)
            {
                return false;
            }

            // Fetch options from attribute
            attribute = GetBurstCompileAttribute(member);
            return attribute != null;
        }

        private static BurstCompileAttribute GetBurstCompileAttribute(MemberInfo memberInfo)
        {
            var result = memberInfo.GetCustomAttribute<BurstCompileAttribute>();
            if (result != null)
            {
                return result;
            }

            foreach (var a in memberInfo.GetCustomAttributes())
            {
                if (a.GetType().FullName == "Burst.Compiler.IL.Tests.TestCompilerAttribute")
                {
                    return new BurstCompileAttribute(FloatPrecision.Standard, FloatMode.Default) { CompileSynchronously = true };
                }
            }

            return null;
        }

        internal static bool HasBurstCompileAttribute(MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            BurstCompileAttribute attr;
            return TryGetAttribute(member, out attr);
        }

        /// <summary>
        /// Gets the options for the specified member. Returns <c>false</c> if the `[BurstCompile]` attribute was not found
        /// </summary>
        /// <returns><c>false</c> if the `[BurstCompile]` attribute was not found; otherwise <c>true</c></returns>
        internal bool TryGetOptions(MemberInfo member, bool isJit, out string flagsOut)
        {
            flagsOut = null;
            BurstCompileAttribute attr;
            if (!TryGetAttribute(member, out attr))
            {
                return false;
            }

            flagsOut = GetOptions(isJit, attr);
            return true;
        }

        internal string GetOptions(bool isJit, BurstCompileAttribute attr = null)
        {
            // Add debug to Jit options instead of passing it here
            // attr.Debug

            var flagsBuilderOut = new StringBuilder();

            if (isJit && ((attr?.CompileSynchronously ?? false) || ForceBurstCompilationSynchronously || EnableBurstCompileSynchronously))
            {
                AddOption(flagsBuilderOut, GetOption(OptionJitEnableSynchronousCompilation));
            }

            if (attr != null)
            {
                if (attr.FloatMode != FloatMode.Default)
                {
                    AddOption(flagsBuilderOut, GetOption(OptionFloatMode, attr.FloatMode));
                }

                if (attr.FloatPrecision != FloatPrecision.Standard)
                {
                    AddOption(flagsBuilderOut, GetOption(OptionFloatPrecision, attr.FloatPrecision));
                }

                if (attr.Options != null)
                {
                    foreach (var option in attr.Options)
                    {
                        if (!String.IsNullOrEmpty(option))
                        {
                            AddOption(flagsBuilderOut, option);
                        }
                    }
                }
            }

            // Fetch options from attribute
            if (EnableBurstSafetyChecks)
            {
                AddOption(flagsBuilderOut, GetOption(OptionSafetyChecks));
            }
            else
            {
                AddOption(flagsBuilderOut, GetOption(OptionDisableSafetyChecks));
            }

            if (isJit && EnableBurstTimings)
            {
                AddOption(flagsBuilderOut, GetOption(OptionJitLogTimings));
            }

            if (EnableBurstDebug || (attr?.Debug ?? false))
            {
                AddOption(flagsBuilderOut, GetOption(OptionDebugMode));
            }

            return flagsBuilderOut.ToString();
        }

        private static void AddOption(StringBuilder builder, string option)
        {
            if (builder.Length != 0)
                builder.Append('\n'); // Use \n to separate options

            builder.Append(option);
        }
        internal static string GetOption(string optionName, object value = null)
        {
            if (optionName == null) throw new ArgumentNullException(nameof(optionName));
            return "--" + optionName + (value ?? String.Empty);
        }

        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke();
        }

        private void MaybeTriggerRecompilation()
        {
#if UNITY_EDITOR
            if (IsGlobal && IsEnabled && !_isConstructing)
            {
                BurstCompiler.TriggerRecompilation();
            }
#endif
        }

#if !UNITY_DOTSPLAYER && !NET_DOTS
        /// <summary>
        /// Static initializer based on command line arguments
        /// </summary>
        static BurstCompilerOptions()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                switch (arg)
                {
                    case DisableCompilationArg:
                        ForceDisableBurstCompilation = true;
                        break;
                    case ForceSynchronousCompilationArg:
                        ForceBurstCompilationSynchronously = false;
                        break;
                }
            }
        }
#endif
#endif // !BURST_COMPILER_SHARED
    }

#if UNITY_EDITOR
    // NOTE: This must be synchronized with Backend.TargetPlatform
    internal enum TargetPlatform
    {
        Windows = 0,
        macOS = 1,
        Linux = 2,
        Android = 3,
        iOS = 4,
        PS4 = 5,
        XboxOne = 6,
        WASM = 7,
        UWP = 8,
        Lumin = 9,
        Switch = 10,
        Stadia = 11,
    }

    // NOTE: This must be synchronized with Backend.TargetCpu
    internal enum TargetCpu
    {
        Auto = 0,
        X86_SSE2 = 1,
        X86_SSE4 = 2,
        X64_SSE2 = 3,
        X64_SSE4 = 4,
        AVX = 5,
        AVX2 = 6,
        WASM32 = 7,
        ARMV7A_NEON32 = 8,
        ARMV8A_AARCH64 = 9,
        THUMB2_NEON32 = 10,
        ARMV8A_AARCH64_HALFFP = 11,
    }
#endif

    /// <summary>
    /// Flags used by <see cref="NativeCompiler.CompileMethod"/> to dump intermediate compiler results.
    /// </summary>
    [Flags]
#if BURST_COMPILER_SHARED
    public enum NativeDumpFlags
#else
    internal enum NativeDumpFlags
#endif
    {
        /// <summary>
        /// Nothing is selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dumps the IL of the method being compiled
        /// </summary>
        IL = 1 << 0,

        /// <summary>
        /// Dumps the reformated backend API Calls
        /// </summary>
        Backend = 1 << 1,

        /// <summary>
        /// Dumps the generated module without optimizations
        /// </summary>
        IR = 1 << 2,

        /// <summary>
        /// Dumps the generated backend code after optimizations (if enabled)
        /// </summary>
        IROptimized = 1 << 3,

        /// <summary>
        /// Dumps the generated ASM code
        /// </summary>
        Asm = 1 << 4,

        /// <summary>
        /// Generate the native code
        /// </summary>
        Function = 1 << 5,

        /// <summary>
        /// Dumps the result of analysis
        /// </summary>
        Analysis = 1 << 6,

        /// <summary>
        /// Dumps the diagnostics from optimisation
        /// </summary>
        IRPassAnalysis = 1 << 7,

        /// <summary>
        /// Dumps the IL before all transformation of the method being compiled
        /// </summary>
        ILPre = 1 << 8,

        /// <summary>
        /// Dumps all normal output.
        /// </summary>
        All = IL | ILPre | IR | IROptimized | Asm | Function | Analysis | IRPassAnalysis
    }
}
#endif