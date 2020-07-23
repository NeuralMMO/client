using System;
using System.Collections.Generic;
using System.Linq;
using Bee.Core;
using Bee.DotNet;
using JetBrains.Annotations;
using NiceIO;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildTools;

/*

// Activate this part once we have found a workaround for compiling against a custom Unity.Burst.Unsafe
// compatible with Tiny. See issue #1490
[UsedImplicitly]
class CustomizerForUnityBurst : AsmDefCSharpProgramCustomizer
{
    public override string CustomizerFor => "Unity.Burst";

    // not exactly right, but good enough for now
    public override void CustomizeSelf(AsmDefCSharpProgram program)
    {
        var path = program.AsmDefDescription.Path.Parent.Parent.Combine("Unity.Burst.Unsafe.dll");
        program.References.Add(new DotNetAssembly(path, Framework.NetStandard20));
    }
}
*/

/*
 * This file exists as an interface to programs that want to invoke bcl.exe from a bee buildprogram.
 * The idea is that when bcl.exe command line options change, this file should change, and then programs using
 * burst will pick up the changes for free, without depending on specific command line options of bcl.exe.
 *
 * How well that works, is another question.
 */
public abstract class BurstCompiler
{
    public static NPath BurstExecutable { get; set; }
    public abstract string TargetPlatform { get; set; }

    // TODO: This should become a list of target architectures to add.
    public abstract string TargetArchitecture { get; set; }

    public abstract string ObjectFormat { get; set; }
    public abstract string ObjectFileExtension { get; set; }
    public abstract bool UseOwnToolchain { get; set; }
    public virtual bool OnlyStaticMethods { get; set; } = false;

    public virtual string BurstBackend { get; set; } = "burst-llvm-9";

    // Options
    public virtual bool SafetyChecks { get; set; } = false;
    public virtual bool DisableVectors { get; set; } = false;
    public virtual bool Link { get; set; } = true;
    public virtual bool Verbose { get; set; } = false;
    public abstract string FloatPrecision { get; set; }
    public virtual int Threads { get; set; } = 9;
    public virtual bool DisableOpt { get; set; } = false;
    public virtual bool EnableGuard { get; set; } = false;
    public virtual string ExecuteMethodName { get; set; } = "ProducerExecuteFn_Gen";
    public virtual bool EnableStaticLinkage { get; set; } = false;
    public virtual bool EnableJobMarshalling { get; set; } = false;
    public virtual bool EnableDirectExternalLinking { get; set; } = false;

    static string[] GetBurstCommandLineArgs(
        BurstCompiler compiler,
        NPath outputPrefixForObjectFile,
        NPath outputDirForPatchedAssemblies,
        string pinvokeName,
        DotNetAssembly[] inputAssemblies)
    {
        var commandLineArguments = new[]
        {
            $"--platform={compiler.TargetPlatform}",
            $"--target={compiler.TargetArchitecture}",
            $"--format={compiler.ObjectFormat}",
            compiler.SafetyChecks ? "--safety-checks" : "",
            $"--dump=\"None\"",
            compiler.DisableVectors ? "--disable-vectors" : "",
            compiler.Link ? "" : "--nolink",
            $"--float-precision={compiler.FloatPrecision}",
            $"--keep-intermediate-files",
            compiler.Verbose ? "--verbose" : "",
            $"--patch-assemblies-into={outputDirForPatchedAssemblies}",
            $"--output={outputPrefixForObjectFile}",
            compiler.OnlyStaticMethods ? "--only-static-methods" : "",
            "--method-prefix=burstedmethod_",
            $"--pinvoke-name={pinvokeName}",
            $"--backend={compiler.BurstBackend}",
            $"--execute-method-name={compiler.ExecuteMethodName}",
            "--debug=Full",
            compiler.EnableDirectExternalLinking ? "--enable-direct-external-linking" : "",
            compiler.DisableOpt ? "--disable-opt" : "",
            $"--threads={compiler.Threads}",
            compiler.EnableGuard ? "--enable-guard" : ""
        }.Concat(inputAssemblies.Select(asm => $"--root-assembly={asm.Path}"));
        if (!compiler.UseOwnToolchain)
            commandLineArguments = commandLineArguments.Concat(new[] {"--no-native-toolchain"});


        if (!HostPlatform.IsWindows)
            commandLineArguments = new[] {BurstExecutable.ToString(SlashMode.Native)}.Concat(commandLineArguments);
        if (compiler.EnableStaticLinkage)
            commandLineArguments = commandLineArguments.Concat(new[] {"--generate-static-linkage-methods"});
        if (compiler.EnableJobMarshalling)
            commandLineArguments = commandLineArguments.Concat(new[] { "--generate-job-marshalling-methods" });

        return commandLineArguments.ToArray();
    }

    static IEnumerable<NPath> AddDebugSymbolPaths(DotNetAssembly[] assemblies)
    {
        return assemblies.SelectMany(
            asm =>
            {
                var ret = new List<NPath> {asm.Path};
                if (asm.DebugSymbolPath != null)
                    ret.Add(asm.DebugSymbolPath);
                return ret;
            });
    }

    public static BagOfObjectFilesLibrary SetupBurstCompilationForAssemblies(
        BurstCompiler compiler,
        DotNetAssembly unpatchedInputAssembly,
        NPath outputDirForObjectFile,
        NPath outputDirForPatchedAssemblies,
        string pinvokeName,
        out DotNetAssembly patchedAssembly)
    {
        /*
         * Note that you can have Link be true and still use this, because on iOS for example if you
         * DON'T pass --no-link, it will NOT link, but it WILL correctly override the llvm backend.
         *
         * if you DO pass --no-link, it will also not link, but then incorrectly use llvm 9.
         */

        patchedAssembly = unpatchedInputAssembly.ApplyDotNetAssembliesPostProcessor(
            outputDirForPatchedAssemblies,
            (inputAssemblies, targetDir) =>
            {
                var executableStringFor = HostPlatform.IsWindows ? BurstExecutable.ToString(SlashMode.Native) : "mono";
                var commandLineArgs = GetBurstCommandLineArgs(
                    compiler,
                    outputDirForObjectFile.Combine(pinvokeName),
                    outputDirForPatchedAssemblies,
                    pinvokeName,
                    inputAssemblies);

                var inputPaths = AddDebugSymbolPaths(inputAssemblies);
                var targetFiles = inputPaths.Select(p => targetDir.Combine(p.FileName));

                Backend.Current.AddAction(
                    "Burst",
                    //todo: make burst process pdbs
                    targetFiles.ToArray(),
                    inputPaths.Concat(new[] {BurstExecutable}).ToArray(),
                    executableStringFor,
                    commandLineArgs,
                    targetDirectories: new[] {outputDirForObjectFile}
                );
            });
        var needFake = true;
        NPath[] objectFileList = null;
        if (outputDirForObjectFile.Exists())
        {
            objectFileList = outputDirForObjectFile.Files($"*{compiler.ObjectFileExtension}");
            if (objectFileList.Length > 0)
                needFake = false;
        }
        if (needFake)
            objectFileList = new[] {outputDirForObjectFile.Combine($"fake{compiler.ObjectFileExtension}")};
        return new BagOfObjectFilesLibrary(objectFileList);
    }

    public static DynamicLibrary SetupBurstCompilationAndLinkForAssemblies(
        BurstCompiler compiler,
        DotNetAssembly unpatchedInputAssembly,
        NPath targetNativeLibrary,
        NPath outputDirForPatchedAssemblies,
        out DotNetAssembly patchedAssembly)
    {
        if (!compiler.Link)
        {
            throw new ArgumentException("BurstCompiler.Link must be true for SetupBurstCompilationAndLinkForAssemblies");
        }

        patchedAssembly = unpatchedInputAssembly.ApplyDotNetAssembliesPostProcessor(
            outputDirForPatchedAssemblies,
            (inputAssemblies, targetDir) =>
            {
                var executableStringFor = HostPlatform.IsWindows ? BurstExecutable.ToString(SlashMode.Native) : "mono";

                var pinvokeName = HostPlatform.IsWindows
                    ? targetNativeLibrary.FileNameWithoutExtension
                    : targetNativeLibrary.FileName;
                var commandLineArgs = GetBurstCommandLineArgs(
                    compiler,
                    targetNativeLibrary.ChangeExtension(""),
                    outputDirForPatchedAssemblies,
                    pinvokeName,
                    inputAssemblies);

                var inputPaths = AddDebugSymbolPaths(inputAssemblies);
                var targetFiles = inputPaths.Select(p => targetDir.Combine(p.FileName))
                    .Concat(new[] {targetNativeLibrary});

                Backend.Current.AddAction(
                    "Burst",
                    //todo: make burst process pdbs
                    targetFiles.ToArray(),
                    inputPaths.Concat(new[] {BurstExecutable}).ToArray(),
                    executableStringFor,
                    commandLineArgs);
            });

        return new DynamicLibrary(targetNativeLibrary, symbolFiles: null);
    }
}

public class BurstCompilerForEmscripten : BurstCompiler
{
    public override string TargetPlatform { get; set; } = "Wasm";
    public override string TargetArchitecture { get; set; } = "WASM32";
    public override string ObjectFormat { get; set; } = "Wasm";
    public override string FloatPrecision { get; set; } = "High";
    public override bool SafetyChecks { get; set; } = true;
    public override bool DisableVectors { get; set; } = true;
    public override bool Link { get; set; } = false;
    public override string ObjectFileExtension { get; set; } = ".bc";
    public override bool UseOwnToolchain { get; set; } = false;
    public override bool EnableStaticLinkage { get; set; } = true;
    public override bool EnableJobMarshalling { get; set; } = false;
    public override bool EnableDirectExternalLinking { get; set; } = true;
}

public class BurstCompilerForWindows : BurstCompiler
{
    public override string TargetPlatform { get; set; } = "Windows";

    //--target=VALUE         Target CPU <Auto|X86_SSE2|X86_SSE4|X64_SSE2|X64_
    //    SSE4|AVX|AVX2|AVX512|WASM32|ARMV7A_NEON32|ARMV8A_
    //    AARCH64|THUMB2_NEON32> Default: Auto
    public override string TargetArchitecture { get; set; } = "X64_SSE2";
    public override string ObjectFormat { get; set; } = "Coff";
    public override string FloatPrecision { get; set; } = "High";
    public override bool SafetyChecks { get; set; } = true;
    public override bool DisableVectors { get; set; } = false;
    public override bool Link { get; set; } = false; //true;
    public override string ObjectFileExtension { get; set; } = ".obj";
    public override bool UseOwnToolchain { get; set; } = true;
    public override bool EnableDirectExternalLinking { get; set; } = false;
    //public override string BurstBackend { get; set; } = "burst-llvm-custom";
    public override bool EnableJobMarshalling { get; set; } = true;
}

public class BurstCompilerForWindows64 : BurstCompilerForWindows
{
    public override string TargetArchitecture { get; set; } = "X64_SSE4";
}

public class BurstCompilerForMac : BurstCompiler
{
    public override string TargetPlatform { get; set; } = "macOS";

    //--target=VALUE         Target CPU <Auto|X86_SSE2|X86_SSE4|X64_SSE2|X64_
    //    SSE4|AVX|AVX2|AVX512|WASM32|ARMV7A_NEON32|ARMV8A_
    //    AARCH64|THUMB2_NEON32> Default: Auto
    public override string TargetArchitecture { get; set; } = "X64_SSE2";
    public override string ObjectFormat { get; set; } = "MachO";
    public override string FloatPrecision { get; set; } = "High";
    public override bool SafetyChecks { get; set; } = true;
    public override bool DisableVectors { get; set; } = false;
    public override bool Link { get; set; } = false;
    public override string ObjectFileExtension { get; set; } = ".o";
    public override bool UseOwnToolchain { get; set; } = true;
}

public class BurstCompilerForAndroid : BurstCompiler
{
    public override string TargetPlatform { get; set; } = "Android";

    //--target=VALUE         Target CPU <Auto|X86_SSE2|X86_SSE4|X64_SSE2|X64_
    //    SSE4|AVX|AVX2|AVX512|WASM32|ARMV7A_NEON32|ARMV8A_
    //    AARCH64|THUMB2_NEON32> Default: Auto
    public override string TargetArchitecture { get; set; } = "ARMV7A_NEON32";
    public override string ObjectFormat { get; set; } = "Elf";
    public override string FloatPrecision { get; set; } = "High";
    public override bool SafetyChecks { get; set; } = true;
    public override bool DisableVectors { get; set; } = false;
    public override bool Link { get; set; } = false;
    public override string ObjectFileExtension { get; set; } = ".o";
    public override bool UseOwnToolchain { get; set; } = true;

    public override bool EnableDirectExternalLinking { get; set; } = true;
}

public class BurstCompilerForiOS : BurstCompiler
{
    public override string TargetPlatform { get; set; } = "iOS";

    //--target=VALUE         Target CPU <Auto|X86_SSE2|X86_SSE4|X64_SSE2|X64_
    //    SSE4|AVX|AVX2|AVX512|WASM32|ARMV7A_NEON32|ARMV8A_
    //    AARCH64|THUMB2_NEON32> Default: Auto
    public override string TargetArchitecture { get; set; } = "ARMV8A_AARCH64";
    public override string ObjectFormat { get; set; } = "Elf";
    public override bool SafetyChecks { get; set; } = true;
    public override bool DisableVectors { get; set; } = false;
    public override bool Link { get; set; } = true;
    public override string FloatPrecision { get; set; } = "High";
    public override string ObjectFileExtension { get; set; } = ".o";
    public override bool UseOwnToolchain { get; set; } = true;
    public override bool EnableDirectExternalLinking { get; set; } = true;
}