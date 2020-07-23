using System;
using System.Linq;
using Mono.Cecil;

namespace Unity.Properties.CodeGen
{
    static class MethodReferenceExtensions
    {
        /// <summary>
        /// Generates a closed/specialized MethodReference for the given method and types[]
        /// e.g. 
        /// struct Foo { T Bar<T>(T val) { return default(T); }
        ///
        /// In this case, if one would like a reference to "Foo::int Bar(int val)" this method will construct such a method
        /// reference when provided the open "T Bar(T val)" method reference and the TypeReferences to the types you'd like
        /// specified as generic arguments (in this case a TypeReference to "int" would be passed in).
        /// </summary>
        /// <param name="method"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static MethodReference MakeGenericInstanceMethod(this MethodReference method, params TypeReference[] types)
        {
            var result = new GenericInstanceMethod(method);
            
            foreach (var type in types)
                result.GenericArguments.Add(type);
            
            return result;
        }
        
        /// <summary>
        /// Allows one to generate reference to a method contained in a generic type which has been closed/specialized.
        /// e.g. 
        /// struct Foo<T> { T Bar(T val) { return default(T); }
        /// 
        /// In this case, if one would like a reference to "Foo<int>::int Bar(int val)" this method will construct such a method
        /// reference when provided the open "T Bar(T val)" method reference and the closed declaring TypeReference, "Foo<int>". 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="closedDeclaringType">See summary above for example. Typically construct this type using `MakeGenericInstanceMethod`</param>
        /// <returns></returns>
        public static MethodReference MakeGenericHostMethod(this MethodReference self, TypeReference closedDeclaringType)
        {
            var reference = new MethodReference(self.Name, self.ReturnType, closedDeclaringType)
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }
    }

    static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Determines whether the given attribute type exists on the given type reference.
        /// </summary>
        /// <param name="provider">The attribute provider to check.</param>
        /// <param name="type">The attribute constructor.</param>
        /// <returns><see langword="true"/> if the given attribute constructor is attached to the provider, otherwise <see langword="false"/>.</returns>
        public static bool HasAttribute(this ICustomAttributeProvider provider, MemberReference type)
        {
            return provider.HasCustomAttributes && provider.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == type.FullName);
        }
    }

    static class TypeReferenceExtensions
    {
        public static TypeReference ResolveGenericParameter(this TypeReference type, TypeReference parent)
        {
            if (type.IsGenericParameter)
            {
                var genericParameter = (GenericParameter) type;
                var genericInstanceType = parent as GenericInstanceType;

                if (null == genericInstanceType || genericInstanceType.Name != type.DeclaringType.Name)
                {
                    var baseType = parent.Resolve()?.BaseType;

                    if (null == baseType)
                    {
                        throw new Exception($"Failed to resolve generic parameter for type {parent.FullName}");
                    }

                    return type.ResolveGenericParameter(baseType.ResolveGenericParameter(parent));
                }

                return genericInstanceType.GenericArguments[genericParameter.Position];
            }

            if (type.IsArray)
            {
                var array = type as ArrayType;
                array.ElementType.ResolveGenericParameter(parent);
                return array;
            }

            if (!type.IsGenericInstance)
            {
                return type;
            }
            
            if (!(parent is GenericInstanceType parentGenericInstanceType))
            {
                return type;
            }
            
            var instance = type as GenericInstanceType;
            
            for (var i = 0; i < instance.GenericArguments.Count; i++)
            {
                if (!instance.GenericArguments[i].IsGenericParameter)
                {
                    continue;
                }

                var parameter = instance.GenericArguments[i] as GenericParameter;
                instance.GenericArguments[i] = parentGenericInstanceType.GenericArguments[parameter.Position];
            }

            return instance;
        }
    }

    static class IMetadataTokenProviderExtensions
    {
        public static bool IsPrivate(this IMetadataTokenProvider member)
        {
            if (member is FieldDefinition field)
            {
                return field.IsPrivate || field.IsAssembly;
            }

            if (member is PropertyDefinition property)
            {
                if (property.GetMethod.IsPrivate || property.GetMethod.IsAssembly)
                {
                    return true;
                }
                if (null != property.SetMethod)
                {
                    if (property.SetMethod.IsPrivate || property.SetMethod.IsAssembly)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}