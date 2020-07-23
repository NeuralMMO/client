using System;
using UnityEditor;

namespace Unity.Build.Classic
{
    /// <summary>
    /// The classic build pipeline allows itself to be customized through this base class. In order to use this functionality, create a class in your own code,
    /// and make it implement this class. You then have an opportunity to overide one of the several customization methods.
    /// </summary>
    public abstract class ClassicBuildPipelineCustomizer
    {
        internal abstract class Info
        {
            public abstract BuildContext Context { get; }
            public abstract string StreamingAssetsDirectory { get; }
            public abstract string OutputBuildDirectory { get; }
            public abstract string WorkingDirectory { get; }
            public abstract BuildTarget BuildTarget { get; }
            public abstract string[] EmbeddedScenes { get; }
        }

        internal Info m_Info;

        protected BuildContext Context => m_Info.Context;

        /// <summary>
        /// If you use this property to construct a target path in your RegisterAdditionalFilesToDeploy implementation,
        /// your file will be placed in the builds' streamingassets folder (which is in slightly different locations on different platforms)
        /// </summary>
        protected string StreamingAssetsDirectory => m_Info.StreamingAssetsDirectory;


        /// <summary>
        /// The output directory this build is being built into
        /// </summary>
        protected string OutputBuildDirectory => m_Info.OutputBuildDirectory;

        /// <summary>
        /// This is a working directory you can use to store files for this build. Common usecase is to generate the files you want to add to a
        /// build in this directory, and then use RegisterAdditionalFilesToDeploy to copy them to a directory in the build outputs. (Usually StreamingAssetsDirectory).
        /// We provide this working directory, because in many cases generating these files can be done much faster if you have previously generated files
        /// around. (Like when you use Unity.ScriptableBuildPipeline package to create assetbundles.
        /// </summary>
        protected string WorkingDirectory => m_Info.WorkingDirectory;

        /// <summary>
        /// The BuildTarget enum that is used for this build
        /// </summary>
        protected BuildTarget BuildTarget => m_Info.BuildTarget;

        /// <summary>
        /// The list of scenes that is going to be embedded in this build.
        /// </summary>
        protected string[] EmbeddedScenes => m_Info.EmbeddedScenes;

        /// <summary>
        /// Override this method to have an opportunity to run some code before the build is built.
        /// </summary>
        public virtual void OnBeforeBuild() { }

        /// <summary>
        /// Override this method if you want to change which scenes will be embedded in this build.
        /// </summary>
        /// <param name="scenes"></param>
        /// <returns></returns>
        public virtual string[] ModifyEmbeddedScenes(string[] scenes) => scenes;

        /// <summary>
        /// Override this method to register additional files you want to copy into the build. You will not do the copy
        /// yourself, instead call the provided registerAdditionalFileToDeploy callback, with a sourcefile and a targetfile.
        /// You can use the WorkingDirectory property to find a good place to generate your source file. You can use the
        /// StreamingAssetsDirectory property to find a good place to copy your file into.
        /// </summary>
        /// <param name="registerAdditionalFileToDeploy"></param>
        public virtual void RegisterAdditionalFilesToDeploy(Action<string, string> registerAdditionalFileToDeploy) { }

        /// <summary>
        /// If in any of the methods if this class that you implemented, you query the BuildContext for certain compmonents
        /// You have to register those component types ahead of time. This allows us to show the user visually which components
        /// will be actually used by a build, and which ones will not.
        /// </summary>
        public virtual Type[] UsedComponents { get; } = Array.Empty<Type>();

        /// <summary>
        /// Override this method if you want to change the BuildOptions enum that the build gets used for the build. You can only add flags, not remove them.
        /// </summary>
        /// <returns></returns>
        public virtual BuildOptions ProvideBuildOptions() => BuildOptions.None;
    }
}
