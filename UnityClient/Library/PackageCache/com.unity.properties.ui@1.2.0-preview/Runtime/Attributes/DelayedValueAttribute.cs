using System;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Attribute used to make a float, int, or string value be delayed.
    ///
    /// When this attribute is used, the float, int, or text field will not return a new value until the user has pressed enter or focus is moved away from the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DelayedValueAttribute : InspectorAttribute
    {
    }
}