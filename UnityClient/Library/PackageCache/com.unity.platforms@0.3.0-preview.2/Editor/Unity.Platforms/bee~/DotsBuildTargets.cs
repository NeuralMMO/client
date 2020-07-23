using Bee.NativeProgramSupport.Building;
using System.Collections.Generic;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;

namespace DotsBuildTargets
{
    abstract class DotsBuildSystemTarget
    {
        protected virtual bool CanRunMultiThreadedJobs => false; // Disabling by default; Eventually: ScriptingBackend == ScriptingBackend.Dotnet;
        /*
         * disabled by default because it takes work to enable each platform for burst
         */
        public virtual bool CanUseBurst => false;

        public abstract string Identifier { get; }
        public abstract ToolChain ToolChain { get; }

        public virtual ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.TinyIl2cpp;
        public virtual TargetFramework TargetFramework => TargetFramework.Tiny;

        protected virtual NativeProgramFormat GetExecutableFormatForConfig(DotsConfiguration config, bool enableManagedDebugger) => null;

        public virtual NativeProgramFormat CustomizeExecutableForSettings(FriendlyJObject settings) => null;

    }
}
