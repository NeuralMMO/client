using System;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    class ClassicBuildOptionsCustomizer : ClassicBuildPipelineCustomizer
    {
        public override Type[] UsedComponents { get; } =
        {
            typeof(ClassicBuildProfile),
            typeof(AutoRunPlayer),
            typeof(EnableHeadlessMode),
            typeof(IncludeTestAssemblies),
            typeof(InstallInBuildFolder),
            typeof(PlayerConnectionSettings),
        };

        public override BuildOptions ProvideBuildOptions()
        {
            var options = BuildOptions.None;

            // Build options from build type
            if (Context.TryGetComponent<ClassicBuildProfile>(out var profile))
            {
                switch (profile.Configuration)
                {
                    case BuildType.Debug:
                        options |= BuildOptions.AllowDebugging | BuildOptions.Development;
                        break;
                    case BuildType.Develop:
                        options |= BuildOptions.Development;
                        break;
                }
            }

            // Build options from components
            if (Context.HasComponent<AutoRunPlayer>())
                options |= BuildOptions.AutoRunPlayer;
            if (Context.HasComponent<EnableHeadlessMode>())
                options |= BuildOptions.EnableHeadlessMode;
            if (Context.HasComponent<IncludeTestAssemblies>())
                options |= BuildOptions.IncludeTestAssemblies;
            if (Context.HasComponent<InstallInBuildFolder>())
                options |= BuildOptions.InstallInBuildFolder;
            if (Context.TryGetComponent<PlayerConnectionSettings>(out PlayerConnectionSettings value))
            {
                if (value.Mode == PlayerConnectionInitiateMode.Connect)
                    options |= BuildOptions.ConnectToHost;
                if (value.WaitForConnection)
                    options |= BuildOptions.WaitForPlayerConnection;
            }
            return options;
        }
    }
}
