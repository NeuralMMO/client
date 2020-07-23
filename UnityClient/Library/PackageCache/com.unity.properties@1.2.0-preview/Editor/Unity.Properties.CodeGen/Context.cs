using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace Unity.Properties.CodeGen
{
    /// <summary>
    /// <see cref="Context"/> is used to pass around the main module as well as any resolved references.
    /// </summary>
    class Context
    {
        static readonly ConstructorInfo s_ExceptionConstructorInfo;
        static readonly MethodInfo s_PropertyBagRegisterGenericMethodInfo;
        static readonly MethodInfo s_PropertyBagRegisterListGenericMethodInfo;
        static readonly MethodInfo s_PropertyBagRegisterSetGenericMethodInfo;
        static readonly MethodInfo s_PropertyBagRegisterDictionaryGenericMethodInfo;
        static readonly MethodInfo s_ContainerPropertyBagAddPropertyGenericMethodInfo;
        static readonly MethodInfo s_TypeGetTypeFromHandleMethodInfo;
        static readonly MethodInfo s_TypeGetFieldMethodInfo;
        static readonly MethodInfo s_TypeGetPropertyMethodInfo;
        static readonly MethodInfo s_CustomAttributeExtensionsGetCustomAttributesMethodInfo;

        static Context()
        {
            s_ExceptionConstructorInfo = typeof(Exception).GetConstructor(new[]{typeof(string)});
            s_PropertyBagRegisterGenericMethodInfo = typeof(PropertyBag).GetMethods().First(x => x.GetParameters().Length == 1 && x.Name == nameof(PropertyBag.Register));
            s_PropertyBagRegisterListGenericMethodInfo = typeof(PropertyBag).GetMethods().First(x => x.GetParameters().Length == 0 && x.Name == nameof(PropertyBag.RegisterList));
            s_PropertyBagRegisterSetGenericMethodInfo = typeof(PropertyBag).GetMethods().First(x => x.GetParameters().Length == 0 && x.Name == nameof(PropertyBag.RegisterSet));
            s_PropertyBagRegisterDictionaryGenericMethodInfo = typeof(PropertyBag).GetMethods().First(x => x.GetParameters().Length == 0 && x.Name == nameof(PropertyBag.RegisterDictionary));
            s_ContainerPropertyBagAddPropertyGenericMethodInfo = typeof(ContainerPropertyBag<>).GetMethod("AddProperty", BindingFlags.NonPublic | BindingFlags.Instance);
            s_TypeGetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle");
            s_TypeGetFieldMethodInfo = typeof(Type).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(f => f.Name == "GetField" && f.GetParameters().Length == 2);
            s_TypeGetPropertyMethodInfo = typeof(Type).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(f => f.Name == "GetProperty" && f.GetParameters().Length == 2);
            s_CustomAttributeExtensionsGetCustomAttributesMethodInfo = typeof(CustomAttributeExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).First(f => f.Name == "GetCustomAttributes" && f.GetParameters().FirstOrDefault()?.ParameterType == typeof(MemberInfo));
        }
        
        public const string kNamespace = "Unity.Properties.Generated";

        public readonly ModuleDefinition Module;
        
        // Cached Defines
        readonly bool IsEditor;

        // Cached Type References
        readonly Dictionary<Type, TypeReference> m_TypeToTypeReference = new Dictionary<Type, TypeReference>();
        readonly Dictionary<TypeReference, TypeReference> m_TypeReferenceToTypeReference = new Dictionary<TypeReference, TypeReference>();

        public readonly Lazy<MethodReference> ExceptionConstructor;
        public readonly Lazy<MethodReference> PropertyBagRegisterGenericMethodReference;
        public readonly Lazy<MethodReference> PropertyBagRegisterListGenericMethodReference;
        public readonly Lazy<MethodReference> PropertyBagRegisterSetGenericMethodReference;
        public readonly Lazy<MethodReference> PropertyBagRegisterDictionaryGenericMethodReference;
        public readonly Lazy<MethodReference> ContainerPropertyBagAddPropertyGenericMethodReference;
        public readonly Lazy<MethodReference> TypeGetTypeFromTypeHandleMethodReference;
        public readonly Lazy<MethodReference> TypeGetFieldMethodReference;
        public readonly Lazy<MethodReference> TypeGetPropertyMethodReference;
        public readonly Lazy<MethodReference> CustomAttributeExtensionsGetCustomAttributesMethodReference;

        public Context(ModuleDefinition module, IEnumerable<string> defines)
        {
            Module = module;
            
            IsEditor = defines.Contains("UNITY_EDITOR");
            
            ExceptionConstructor = new Lazy<MethodReference>(() => Module.ImportReference(s_ExceptionConstructorInfo));
            PropertyBagRegisterGenericMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_PropertyBagRegisterGenericMethodInfo));
            PropertyBagRegisterListGenericMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_PropertyBagRegisterListGenericMethodInfo));
            PropertyBagRegisterSetGenericMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_PropertyBagRegisterSetGenericMethodInfo));
            PropertyBagRegisterDictionaryGenericMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_PropertyBagRegisterDictionaryGenericMethodInfo));
            ContainerPropertyBagAddPropertyGenericMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_ContainerPropertyBagAddPropertyGenericMethodInfo));
            TypeGetTypeFromTypeHandleMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_TypeGetTypeFromHandleMethodInfo));
            TypeGetFieldMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_TypeGetFieldMethodInfo));
            TypeGetPropertyMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_TypeGetPropertyMethodInfo));
            CustomAttributeExtensionsGetCustomAttributesMethodReference = new Lazy<MethodReference>(() => Module.ImportReference(s_CustomAttributeExtensionsGetCustomAttributesMethodInfo));
        }

        public TypeReference ImportReference(Type type)
        {
            if (m_TypeToTypeReference.TryGetValue(type, out var reference))
            {
                return reference;
            }

            reference = Module.ImportReference(type);

            m_TypeToTypeReference[type] = reference;
            m_TypeReferenceToTypeReference[reference] = reference;

            return reference;
        }

        public TypeReference ImportReference(TypeReference typeReference)
        {
            if (m_TypeReferenceToTypeReference.TryGetValue(typeReference, out var reference))
            {
                return reference;
            }

            reference = Module.ImportReference(typeReference);

            m_TypeReferenceToTypeReference[typeReference] = reference;

            return reference;
        }
        
#if !UNITY_DOTSPLAYER
        public void AddInitializeOnLoadMethodAttribute(ICustomAttributeProvider provider)
        {
            if (IsEditor)
            {
                var initializeOnLoadMethodAttributeConstructor = typeof(UnityEditor.InitializeOnLoadMethodAttribute).GetConstructor(Type.EmptyTypes);
                var initializeOnLoadMethodAttribute = new CustomAttribute(Module.ImportReference(initializeOnLoadMethodAttributeConstructor));
                provider.CustomAttributes.Add(initializeOnLoadMethodAttribute);
            }
            
            var runtimeInitializeOnLoadMethodAttributeConstructor = typeof(UnityEngine.RuntimeInitializeOnLoadMethodAttribute).GetConstructor(new[]{typeof(UnityEngine.RuntimeInitializeLoadType)});
            var runtimeInitializeOnLoadMethodAttribute = new CustomAttribute(Module.ImportReference(runtimeInitializeOnLoadMethodAttributeConstructor))
            {
                ConstructorArguments =
                {
                    new CustomAttributeArgument(Module.ImportReference(typeof(UnityEngine.RuntimeInitializeLoadType)), UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)
                }
            };
            provider.CustomAttributes.Add(runtimeInitializeOnLoadMethodAttribute);  
        }
        
        public void AddPreserveAttribute(ICustomAttributeProvider provider)
        {
            var preserveAttributeConstructor = typeof(UnityEngine.Scripting.PreserveAttribute).GetConstructor(Type.EmptyTypes);
            provider.CustomAttributes.Add(new CustomAttribute(Module.ImportReference(preserveAttributeConstructor)));
        }
#endif
    }
}