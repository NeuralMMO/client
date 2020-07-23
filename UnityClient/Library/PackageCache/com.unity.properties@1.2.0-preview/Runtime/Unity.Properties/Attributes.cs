using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Use this attribute to have a property generated for the member.
    /// </summary>
    /// <remarks>
    /// By default public fields will have properties generated.
    /// </remarks>
    /// <see cref="DontCreatePropertyAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CreatePropertyAttribute : Attribute
    {
        
    }
    
    /// <summary>
    /// Use this attribute to prevent have a property from being automatically generated on a public field.
    /// </summary>
    /// <see cref="CreatePropertyAttribute"/>
    [AttributeUsage(AttributeTargets.Field)]
    public class DontCreatePropertyAttribute : Attribute
    {
        
    }

    /// <summary>
    /// A set of options to customize the behaviour of the code generator.
    /// </summary>
    [Flags]
    public enum TypeOptions
    {
        /// <summary>
        /// If this option is selected, any inherited value types will have property bags generated.
        /// </summary>
        ValueType = 1 << 1,
        
        /// <summary>
        /// If this option is selected, any inherited reference types will have property bags generated.
        /// </summary>
        ReferenceType = 1 << 2,
        
        /// <summary>
        /// The default set of type options. This includes both <see cref="ValueType"/> and <see cref="ReferenceType"/>.
        /// </summary>
        Default = ValueType | ReferenceType
    }
    
    /// <summary>
    /// Use this attribute to have the properties ILPostProcessor generate property bags for types implementing the specified interface.
    /// </summary>
    /// <remarks>
    /// If you need to generate a property bag for a specific type use <see cref="GeneratePropertyBagsForTypeAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class GeneratePropertyBagsForTypesQualifiedWithAttribute : Attribute
    {
        /// <summary>
        /// The interface type to generate property bags for.
        /// </summary>
        public readonly Type Type;
        
        /// <summary>
        /// Options used for additional filtering.
        /// </summary>
        public readonly TypeOptions Options;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratePropertyBagsForTypesQualifiedWithAttribute"/> attribute.
        /// </summary>
        /// <param name="type">The interface type to generate property bags for.</param>
        /// <param name="options">Additional type filtering options.</param>
        /// <exception cref="ArgumentException">The type is null or the given type is not an interface.</exception>
        public GeneratePropertyBagsForTypesQualifiedWithAttribute(Type type, TypeOptions options = TypeOptions.Default)
        {
            if (type == null)
            {
                throw new ArgumentException($"{nameof(type)} is null.");
            }
            
            if (!type.IsInterface)
            {
                throw new ArgumentException($"{nameof(GeneratePropertyBagsForTypesQualifiedWithAttribute)} Type must be an interface type.");
            }
            
            Type = type;
            Options = options;
        }
    }
    
    /// <summary>
    /// Use this attribute to have the ILPostProcessor generate a property bag for a given type.
    /// This attribute works for the specified type ONLY, it does NOT include derived types.
    /// </summary>
    /// <remarks>
    /// If you need to generate property bags for types implementing a specific interface use <see cref="GeneratePropertyBagsForTypesQualifiedWithAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class GeneratePropertyBagsForTypeAttribute : Attribute
    {
        /// <summary>
        /// The type to generate a property bag for.
        /// </summary>
        public readonly Type Type;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratePropertyBagsForTypeAttribute"/> attribute.
        /// </summary>
        /// <param name="type">The type to generate a property bag for.</param>
        /// <exception cref="ArgumentException">The specified type is not a valid container type.</exception>
        public GeneratePropertyBagsForTypeAttribute(Type type)
        {
            if (!RuntimeTypeInfoCache.IsContainerType(type))
            {
                throw new ArgumentException($"{type.Name} is not a valid container type.");
            }
            
            Type = type;
        }
    }
    
    /// <summary>
    /// By default, property bags are only generated in builds. Use this attribute to force the ILPostProcessor to generate property bags in editor for the tagged assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class GeneratePropertyBagsInEditorAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Use this attribute to have the ILPostProcessor generate property bags for a given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class GeneratePropertyBagAttribute : Attribute
    {
    }
}