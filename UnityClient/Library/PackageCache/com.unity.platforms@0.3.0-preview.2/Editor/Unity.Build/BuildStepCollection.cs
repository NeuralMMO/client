using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;

namespace Unity.Build
{
    /// <summary>
    /// Represent a collection of build steps that can be run.
    /// </summary>
    public sealed class BuildStepCollection : IEnumerable<BuildStepBase>
    {
        readonly BuildStepBase[] m_BuildSteps;

        /// <summary>
        /// Default constructor for empty build step collection.
        /// </summary>
        public BuildStepCollection()
        {
            m_BuildSteps = new BuildStepBase[0];
        }

        /// <summary>
        /// Construct a new build step collection using the specified types.
        /// The types must derive from <see cref="BuildStepBase"/>.
        /// </summary>
        /// <param name="types">The build step types.</param>
        public BuildStepCollection(params Type[] types)
        {
            m_BuildSteps = types.Select(type => TypeConstruction.Construct<BuildStepBase>(type)).ToArray();
        }

        /// <summary>
        /// Implicit conversion from types array to <see cref="BuildStepCollection"/>.
        /// The types must derive from <see cref="BuildStepBase"/>.
        /// </summary>
        /// <param name="types">The build step types.</param>
        public static implicit operator BuildStepCollection(Type[] types) => new BuildStepCollection(types);

        /// <summary>
        /// Run all enabled build steps and cleanup.
        /// </summary>
        /// <param name="context">The current build context.</param>
        /// <returns>A build result indicating if successful or not.</returns>
        public BuildResult Run(BuildContext context)
        {
            var results = new List<BuildResult>();
            var title = context.BuildProgress?.Title ?? string.Empty;

            // Setup build steps list
            var cleanupSteps = new Stack<BuildStepBase>();
            var enabledSteps = m_BuildSteps.Where(step => step.IsEnabled(context)).ToArray();

            // Run build steps and stop on first failure of any kind
            for (var i = 0; i < enabledSteps.Length; ++i)
            {
                var step = enabledSteps[i];

                // Update build progress
                var cancelled = context.BuildProgress?.Update($"{title} (Step {i + 1} of {enabledSteps.Length})", step.Description + "...", (float)i / enabledSteps.Length) ?? false;
                if (cancelled)
                {
                    results.Add(context.Failure($"{title} was cancelled."));
                    break;
                }

                // Add step to cleanup stack only if it overrides implementation
                if (step.GetType().GetMethod(nameof(BuildStepBase.Cleanup)).DeclaringType != typeof(BuildStepBase))
                {
                    cleanupSteps.Push(step);
                }

                // Run step
                try
                {
                    results.Add(step.Run(context));
                    if (results.Last().Failed)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    results.Add(context.Failure(exception));
                    break;
                }
            }

            // Execute cleanup (even if there are failures):
            // 1) in opposite order of steps that ran.
            // 2) can't be cancelled; cleanup must run.
            foreach (var step in cleanupSteps)
            {
                context.BuildProgress?.Update($"{title} (Cleanup)", step.Description + "...", 1.0F);
                try
                {
                    results.Add(step.Cleanup(context));
                }
                catch (Exception exception)
                {
                    results.Add(context.Failure(exception));
                }
            }

            // Return the first failed result if any
            var failure = results.FirstOrDefault(result => result.Failed);
            return failure != null ? failure : context.Success();
        }

        public IEnumerator<BuildStepBase> GetEnumerator()
        {
            foreach (var step in m_BuildSteps)
            {
                yield return step;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
