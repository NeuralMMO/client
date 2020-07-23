using System;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Allows to configure how the UI hierarchy is generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InspectorOptionsAttribute : InspectorAttribute
    {
        /// <summary>
        /// Suppresses the "Reset to default" context menu item.
        /// </summary>
        public bool HideResetToDefault { get; set; }
    }
}