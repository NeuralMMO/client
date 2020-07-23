using System;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Use this attribute on fields and properties to change the display name shown when inspected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : InspectorAttribute
    {
        /// <summary>
        /// Name to display.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Constructs a new instance of <see cref="DisplayNameAttribute"/> with the provided name.
        /// </summary>
        /// <param name="name">The name to use for the field or property in the inspector.</param>
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}