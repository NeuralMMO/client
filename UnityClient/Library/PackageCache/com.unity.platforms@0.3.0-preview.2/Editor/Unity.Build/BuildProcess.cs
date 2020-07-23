using System;
using System.Diagnostics;
using System.Linq;

namespace Unity.Build
{
    /// <summary>
    /// Describe the state of an incremental build process.
    /// The <see cref="Update"/> method needs to be called until it returns <see langword="false"/>, indicating that the build has completed.
    /// The <see cref="BuildResult"/> can then be queried from the <see cref="Result"/> property.
    /// </summary>
    public class BuildProcess : IDisposable
    {
        readonly Func<BuildContext, BuildResult> m_OnBuild;
        readonly BuildContext m_Context;
        readonly Stopwatch m_Timer = new Stopwatch();

        /// <summary>
        /// Event fired when a build is completed.
        /// </summary>
        public static event Action<BuildResult> BuildCompleted;

        /// <summary>
        /// The result of the build. Only valid once <see cref="Update"/> returns <see langword="false"/>, indicating that the build has completed.
        /// </summary>
        public BuildResult Result { get; private set; }

        /// <summary>
        /// Determine if the build is completed. Once the build is completed, a <see cref="BuildResult"/> can be queried from the <see cref="Result"/> property.
        /// </summary>
        public bool IsCompleted => Result != null;

        /// <summary>
        /// Request the active build process to update.
        /// Returns <see langword="true"/> to indicate that it must be called again, otherwise <see langword="false"/> to indicate that the build has completed.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="Update"/> must be called again, otherwise <see langword="false"/>.</returns>
        public bool Update()
        {
            if (IsCompleted)
            {
                return false;
            }

            if (!m_Timer.IsRunning)
            {
                m_Timer.Restart();
            }

            try
            {
                Result = m_OnBuild(m_Context);
            }
            catch (Exception exception)
            {
                Result = m_Context.Failure(exception);
            }

            if (!IsCompleted)
            {
                return true;
            }

            m_Timer.Stop();
            Result.Duration = m_Timer.Elapsed;
            BuildArtifacts.Store(Result, m_Context.Values.OfType<IBuildArtifact>().ToArray());
            return false;
        }

        public void Dispose()
        {
            m_Context?.Dispose();
            BuildCompleted?.Invoke(Result);
        }

        internal BuildProcess() { }

        internal BuildProcess(BuildContext context, Func<BuildContext, BuildResult> onBuild)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
            m_OnBuild = onBuild ?? throw new ArgumentNullException(nameof(onBuild));
        }

        internal static BuildProcess Success(BuildPipelineBase pipeline, BuildConfiguration config) => new BuildProcess
        {
            Result = BuildResult.Success(pipeline, config)
        };

        internal static BuildProcess Failure(BuildPipelineBase pipeline, BuildConfiguration config, string reason) => new BuildProcess
        {
            Result = BuildResult.Failure(pipeline, config, reason)
        };

        internal static BuildProcess Failure(BuildPipelineBase pipeline, BuildConfiguration config, Exception exception) => new BuildProcess
        {
            Result = BuildResult.Failure(pipeline, config, exception)
        };
    }
}
