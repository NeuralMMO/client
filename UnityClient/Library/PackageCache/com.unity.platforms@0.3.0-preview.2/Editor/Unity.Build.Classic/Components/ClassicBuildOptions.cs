using Unity.Properties;
using Unity.Serialization;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Classic
{
    public enum PlayerConnectionInitiateMode
    {
        /// <summary>
        /// The player connection is established by Editor connecting to the player. The player at the launch will broadcast its IP address, after Editor receives the IP address, it will start establishing the connection.
        /// </summary>
        Listen,
        /// <summary>
        /// The player connection is established by Player connecting to the Editor.
        /// </summary>
        Connect
    }

    /// <summary>
    /// Add <see cref="BuildOptions.ConnectToHost"/>, <see cref="BuildOptions.WaitForPlayerConnection"/>  to BuildPlayer options.
    /// </summary>
    public sealed class PlayerConnectionSettings : IBuildComponent
    {
        [CreateProperty]
        public PlayerConnectionInitiateMode Mode { get; set; } = PlayerConnectionInitiateMode.Connect;

        [CreateProperty]
        public bool WaitForConnection { get; set; } = false;
    }

    /// <summary>
    /// Add <see cref="BuildOptions.EnableHeadlessMode"/> to BuildPlayer options.
    /// </summary>
    public sealed class EnableHeadlessMode : IBuildComponent { }

    /// <summary>
    /// Add <see cref="BuildOptions.IncludeTestAssemblies"/> to BuildPlayer options.
    /// </summary>
    [FormerName("Unity.Build.Common.TestablePlayer, Unity.Build.Common")]
    public sealed class IncludeTestAssemblies : IBuildComponent { }

    /// <summary>
    /// Add <see cref="BuildOptions.InstallInBuildFolder"/> to BuildPlayer options.
    /// </summary>
    [FormerName("Unity.Build.Common.SourceBuildConfiguration, Unity.Build.Common")]
    public sealed class InstallInBuildFolder : IBuildComponent { }

    [HideInInspector]
    internal sealed class AutoRunPlayer : IBuildComponent { }
}
