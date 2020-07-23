using System;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Tag a field or a property to try to create a new instance if it is null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CreateInstanceOnInspectionAttribute : InspectorAttribute
    {
        /// <summary>
        /// Returns the type of the instance that should be instantiated.
        /// </summary>
        public Type Type { get; }
        
        /// <summary>
        /// Constructs a new instance of <see cref="CreateInstanceOnInspectionAttribute"/>.
        /// </summary>
        public CreateInstanceOnInspectionAttribute()
        {
            Type = default;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="CreateInstanceOnInspectionAttribute"/> with the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The targeted <see cref="Type"/> when creating a new instance.</param>
        public CreateInstanceOnInspectionAttribute(Type type)
        {
            Type = type;
        }
    }
    
    /// <summary>
    /// Tag a collection field or a property to try to create an instance when adding a new element to the collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CreateElementOnAddAttribute : Attribute
    {
        /// <summary>
        /// Returns the type of the instance that should be instantiated.
        /// </summary>
        public Type Type { get; }
        
        /// <summary>
        /// Constructs a new instance of <see cref="CreateElementOnAddAttribute"/>.
        /// </summary>
        public CreateElementOnAddAttribute()
        {
            Type = default;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="CreateElementOnAddAttribute"/> with the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The targeted <see cref="Type"/> when creating a new instance.</param>
        public CreateElementOnAddAttribute(Type type)
        {
            Type = type;
        }
    }
}