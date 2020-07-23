using System;
using System.ComponentModel;

namespace Unity.Build
{
    /// <summary>
    /// Attribute to configure various properties of a <see cref="BuildStepBase"/> derived type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BuildStepAttribute : Attribute
    {
        /// <summary>
        /// Display description of the build step.
        /// </summary>
        public string Description { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Remove usage, build steps no longer have names. (RemovedAfter 2020-07-01)", true)]
        public string Name
        {
            get => throw null;
            set => throw null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Remove usage, build steps no longer have categories. (RemovedAfter 2020-07-01)", true)]
        public string Category
        {
            get => throw null;
            set => throw null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("To hide a BuildStep in inspector or searcher menu, please use attribute [HideInInspector] instead. (RemovedAfter 2020-05-01)")]
        public enum Flags
        {
            None = 0,
            Hidden = 1
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("To hide a BuildStep in inspector or searcher menu, please use attribute [HideInInspector] instead. (RemovedAfter 2020-05-01)")]
        public Flags flags = Flags.None;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("description has been renamed to Description. (RemovedAfter 2020-05-01) (UnityUpgradable) -> Description")]
        public string description
        {
            get => throw null;
            set => throw null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("category has been renamed to Category. (RemovedAfter 2020-05-01) (UnityUpgradable) -> Category")]
        public string category
        {
            get => throw null;
            set => throw null;
        }
    }
}
