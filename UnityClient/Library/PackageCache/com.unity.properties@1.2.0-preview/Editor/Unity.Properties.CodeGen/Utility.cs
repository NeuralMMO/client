using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Unity.Properties.CodeGen
{
    static class Utility
    {
        static readonly GeneratePropertyBagsForTypeAttribute[] s_AssemblyDefinedTypeAttributes;
        static readonly GeneratePropertyBagsForTypesQualifiedWithAttribute[] s_AssemblyDefinedTypesQualifiedWithAttributes;
        static readonly HashSet<string> s_AssembliesWithEditorCodeGenEnabled = new HashSet<string>();

        static Utility()
        {
            s_AssemblyDefinedTypeAttributes = AppDomain.CurrentDomain.GetAssemblies()
                                                       .Where(a => !a.FullName.StartsWith("UnityEngine"))
                                                       .Where(a => !a.FullName.StartsWith("UnityEditor"))
                                                       .SelectMany(a => a.GetCustomAttributes<GeneratePropertyBagsForTypeAttribute>())
                                                       .ToArray();
           
            s_AssemblyDefinedTypesQualifiedWithAttributes = AppDomain.CurrentDomain.GetAssemblies()
                                                   .Where(a => !a.FullName.StartsWith("UnityEngine"))
                                                   .Where(a => !a.FullName.StartsWith("UnityEditor"))
                                                   .SelectMany(a => a.GetCustomAttributes<GeneratePropertyBagsForTypesQualifiedWithAttribute>())
                                                   .ToArray();
            
            var assembliesWithEditorCodeGenEnabled = AppDomain.CurrentDomain.GetAssemblies()
                                                           .Where(a => !a.FullName.StartsWith("UnityEngine"))
                                                           .Where(a => !a.FullName.StartsWith("UnityEditor"))
                                                           .Where(a => a.GetCustomAttributes<GeneratePropertyBagsInEditorAttribute>().Any())
                                                           .Select(a => a.GetName().Name).ToArray();

            foreach (var name in assembliesWithEditorCodeGenEnabled)
                s_AssembliesWithEditorCodeGenEnabled.Add(name);
        }

        /// <summary>
        /// Returns true if the given assembly has edit time property bag generation enabled.
        /// </summary>
        /// <param name="assemblyName">The full name of the assembly.</param>
        /// <returns></returns>
        internal static bool ShouldGeneratePropertyBagsInEditor(string assemblyName)
        {
            return s_AssembliesWithEditorCodeGenEnabled.Contains(assemblyName);
        }

        /// <summary>
        /// We will be gathering types from all over, so we want to collapse TypeReferences that refer to the same types
        /// which a TypeDefinition does, however TypeReferences keep the generic specifications which we care about. As
        /// such we provide name based comparer to allow us to have a unique set of TypeReferences
        /// </summary>
        class TypeReferenceComparer : IEqualityComparer<TypeReference>
        {
            public bool Equals(TypeReference lhs, TypeReference rhs)
                => lhs.FullName.Equals(rhs.FullName);
            
            public int GetHashCode(TypeReference obj)
                => obj.FullName.GetHashCode();
        }

        /// <summary>
        /// Returns a set of all <see cref="TypeReference"/> which should have property bags generated.
        /// </summary>
        /// <param name="context">The code generation context.</param>
        /// <returns>An <see cref="IEnumerable{TypeReference}"/> that contains all property container types.</returns>
        public static IEnumerable<TypeReference> GetContainerTypes(Context context)
        {
            var visited = new HashSet<TypeReference>(new TypeReferenceComparer());

            foreach (var type in GetRootContainerTypes(context))
            {
                foreach (var inner in GetContainerTypesRecursive(context, type, visited))
                {
                    yield return inner;
                }
            }
        }

        static IEnumerable<TypeReference> GetRootContainerTypes(Context context)
        {
            var propertyContainerAttribute = context.ImportReference(typeof(GeneratePropertyBagAttribute));
            var stringTypeReference = context.ImportReference(typeof(string));

            foreach (var type in context.Module.GetAllTypes())
            {
                if (type.IsPrimitive || type.IsPointer || type.FullName == stringTypeReference.FullName)
                {
                    continue;
                }

                if (type.IsEnum || type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                if (type.HasCustomAttributes && type.CustomAttributes.Any(a => a.AttributeType.FullName == propertyContainerAttribute.FullName))
                {
                    yield return type;
                }

                if (type.Interfaces.Any(i => s_AssemblyDefinedTypesQualifiedWithAttributes.Any(a => i.InterfaceType.FullName == context.ImportReference(a.Type).FullName && MatchesPropertyContainerOptions(type, a.Options))))
                {
                    yield return type;
                }

                if (s_AssemblyDefinedTypeAttributes.Any(a => a.Type.FullName == type.FullName))
                {
                    yield return type;
                }
            }
        }

        static bool MatchesPropertyContainerOptions(TypeReference type, TypeOptions options)
        {
            if (type.IsValueType) return (options & TypeOptions.ValueType) != 0;
            return (options & TypeOptions.ReferenceType) != 0;
        }

        static bool IsUnityEngineType(Context context, TypeReference type)
        {
#if !UNITY_DOTSPLAYER
            while (true)
            {
                var unityEngineObjectTypeReference = context.ImportReference(typeof(UnityEngine.Object));
                var systemObjectTypeReference = context.ImportReference(typeof(object));

                if (null == type) return false;
                if (type.FullName == systemObjectTypeReference.FullName) return false;
                if (type.FullName == unityEngineObjectTypeReference.FullName) return true;

                type = type.Resolve()?.BaseType;
            }
#else 
            return false;
#endif
        }
        
        static IEnumerable<TypeReference> GetContainerTypesRecursive(Context context, TypeReference type, ISet<TypeReference> visited)
        {
            type = context.ImportReference(type);
            
            if (type.IsPrimitive || type.IsPointer || type.FullName == context.ImportReference(typeof(string)).FullName || type.FullName == context.ImportReference(typeof(object)).FullName)
            {
                yield break;
            }
            
            if (!visited.Add(type))
            {
                yield break;
            }

            if (IsUnityEngineType(context, type))
            {
                yield break;
            }
            
            var resolved = type.Resolve();
            
            if (resolved == null || resolved.IsEnum || resolved.IsAbstract || resolved.IsInterface)
            {
                yield break;
            }

            if (type.IsArray)
            {
                foreach (var inner in GetContainerTypesRecursive(context, (type as ArrayType).ElementType, visited))
                {
                    yield return inner;
                }

                yield break;
            }

            if (type.IsGenericInstance || type.IsGenericParameter || type.HasGenericParameters)
            {
                if (resolved.Interfaces.Any(i => i.InterfaceType.FullName.Contains("System.Collections.Generic.IList`1")))
                {
                    foreach (var inner in GetContainerTypesRecursive(context, (type as GenericInstanceType).GenericArguments[0], visited))
                    {
                        yield return inner;
                    }

                    yield break;
                }
                
                if (resolved.Interfaces.Any(i => i.InterfaceType.FullName.Contains("System.Collections.Generic.ISet`1")))
                {
                    foreach (var inner in GetContainerTypesRecursive(context, (type as GenericInstanceType).GenericArguments[0], visited))
                    {
                        yield return inner;
                    }
                    
                    yield break;
                }
                
                if (resolved.Interfaces.Any(i => i.InterfaceType.FullName.Contains("System.Collections.Generic.IDictionary`2")))
                {
                    foreach (var inner in GetContainerTypesRecursive(context, (type as GenericInstanceType).GenericArguments[0], visited))
                    {
                        yield return inner;
                    }

                    foreach (var inner in GetContainerTypesRecursive(context, (type as GenericInstanceType).GenericArguments[1], visited))
                    {
                        yield return inner;
                    }
                    
                    yield break;
                }
            }
            
            yield return type;
            
            foreach (var member in GetPropertyMembers(context, resolved))
            {
                foreach (var inner in GetContainerTypesRecursive(context, GetMemberType(member).ResolveGenericParameter(type), visited))
                {
                    yield return inner;
                }
            }
        }

        internal static TypeReference GetMemberType(IMemberDefinition member)
        {
            if (member is FieldDefinition field) return field.FieldType;
            if (member is PropertyDefinition property) return property.PropertyType;
            return default;
        }  
        
        internal static IEnumerable<IMemberDefinition> GetPropertyMembers(Context context, TypeDefinition type)
        {
            var objectTypeReference = context.ImportReference(typeof(object));
            
            var dontCreatePropertyAttributeTypeReference = context.ImportReference(typeof(DontCreatePropertyAttribute));
            var createPropertyAttributeTypeReference = context.ImportReference(typeof(CreatePropertyAttribute));
            var nonSerializedAttributeTypeReference = context.ImportReference(typeof(NonSerializedAttribute));
#if !UNITY_DOTSPLAYER
            var serializeFieldAttributeTypeReference = context.ImportReference(typeof(UnityEngine.SerializeField));
#endif
            for (;;)
            {
                foreach (var field in type.Fields)
                {
                    if (field.FieldType.IsPointer || field.IsStatic)
                    {
                        continue;
                    }
                    
                    if (field.DeclaringType != type)
                    {
                        continue;
                    }

                    if (field.HasCustomAttributes)
                    {
                        if (field.HasAttribute(dontCreatePropertyAttributeTypeReference))
                        {
                            continue;
                        }
                        
                        if (field.HasAttribute(createPropertyAttributeTypeReference))
                        {
                            yield return field;
                            continue;
                        }
                        
                        if (field.HasAttribute(nonSerializedAttributeTypeReference))
                        {
                            continue;
                        }
                        
#if !UNITY_DOTSPLAYER
                        if (field.HasAttribute(serializeFieldAttributeTypeReference))
                        {
                            yield return field;
                            continue;
                        }
#endif
                    }
                    
                    if (field.IsPublic)
                    {
                        yield return field;
                    }
                }
                
                foreach (var property in type.Properties)
                {
                    if (property.PropertyType.IsPointer)
                    {
                        continue;
                    }
                    
                    if (property.DeclaringType != type)
                    {
                        continue;
                    }

                    if (property.GetMethod == null || property.GetMethod.IsStatic)
                    {
                        continue;
                    }

                    if (property.HasCustomAttributes)
                    {
                        if (property.HasAttribute(dontCreatePropertyAttributeTypeReference))
                        {
                            continue;
                        }

                        if (property.HasAttribute(createPropertyAttributeTypeReference))
                        {
                            yield return property;
                            continue;
                        }

                        if (property.HasAttribute(nonSerializedAttributeTypeReference))
                        {
                            continue;
                        }

#if !UNITY_DOTSPLAYER
                        if (property.HasAttribute(serializeFieldAttributeTypeReference))
                        {
                            yield return property;
                        }
#endif
                    }
                }

                if (null == type.BaseType || type.BaseType.FullName == objectTypeReference.FullName)
                {
                    break;
                }

                type = type.BaseType.Resolve();
            } 
        }
        
        internal static unsafe string GetSanitizedName(string name, string suffix)
        {
            var length = name.Length + suffix.Length;
            
            char* chars = stackalloc char[length];

            for (var i = 0; i < name.Length; i++)
            {
                switch (name[i])
                {
                    case '.':
                    case '+':
                    case '/':
                    case ',':
                    case '`':
                    case '<':
                    case '>':
                    case '[':
                    case ']':
                        chars[i] = '_';
                        break;
                    default:
                        chars[i] = name[i];
                        break;
                }
            }

            for (var i = 0; i < suffix.Length; i++)
            {
                chars[name.Length + i] = suffix[i];
            }

            return new string(chars, 0, length);
        }
    }
}