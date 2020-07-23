using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Unity.Properties.CodeGen.Blocks
{
    static class PropertyBag
    {
        public static TypeDefinition Generate(Context context, TypeReference containerType)
        {
            var basePropertyBagType = context.ImportReference(typeof(ContainerPropertyBag<>)).MakeGenericInstanceType(containerType);
            
            var propertyBagType = new TypeDefinition
            (
                @namespace: Context.kNamespace,
                @name: Utility.GetSanitizedName(containerType.FullName, "_PropertyBag"),
                @attributes: TypeAttributes.Class | TypeAttributes.NotPublic,
                @baseType: basePropertyBagType
            )
            {
                Scope = containerType.Scope
            };
                
            // NOTE: We create our own method reference since this assembly may not reference Unity.Properties on it's own. Thus any attempt
            // to Resolve() a TypeReference from Properties will return null. So instead we create MethodReferences for methods we
            // know will exist ourselves and let the new assembly, which will now include a reference to Properties, resolve at runtime
            var baseCtorMethod = new MethodReference(".ctor", context.ImportReference(typeof(void)), basePropertyBagType)
            {
                HasThis = true,
                ExplicitThis = false,
                CallingConvention = MethodCallingConvention.Default
            };
            
            var ctorMethod = new MethodDefinition
            (
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                context.ImportReference(typeof(void))
            );
            
            propertyBagType.Methods.Add(ctorMethod);
            
            var il = ctorMethod.Body.GetILProcessor();
            
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Call, context.Module.ImportReference(baseCtorMethod));
            
            var addPropertyMethod = context.Module.ImportReference(context.ContainerPropertyBagAddPropertyGenericMethodReference.Value.MakeGenericHostMethod(basePropertyBagType));

            foreach (var member in Utility.GetPropertyMembers(context, containerType.Resolve()))
            {
                var memberType = context.ImportReference(Utility.GetMemberType(member).ResolveGenericParameter(containerType));

                if (memberType.IsGenericInstance || memberType.IsArray)
                {
                    RegisterCollectionTypes(context, containerType, memberType, il);
                }

                TypeDefinition propertyType;
                
                if (!member.IsPrivate())
                {
                    propertyType = Property.Generate(context, containerType, member);
                }
                else
                {
#if !NET_DOTS
                    propertyType = ReflectedProperty.Generate(context, containerType, member);
#else
                    throw new Exception("Private properties require reflection which is not supported in NET_DOTS.");
#endif
                }
                
                propertyBagType.NestedTypes.Add(propertyType);

                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Newobj, propertyType.GetConstructors().First());
                il.Emit(OpCodes.Call, context.Module.ImportReference(addPropertyMethod.MakeGenericInstanceMethod(memberType)));
            }
            
            il.Emit(OpCodes.Ret);
            return propertyBagType;
        }

        static void RegisterCollectionTypes(Context context, TypeReference containerType, TypeReference memberType, ILProcessor il)
        {
            var resolvedMember = memberType.Resolve();

            if (memberType.IsArray)
            {
                var elementType = (memberType as ArrayType).ElementType;
                var method =  context.Module.ImportReference(context.PropertyBagRegisterListGenericMethodReference.Value.MakeGenericInstanceMethod(containerType, memberType, elementType));
                il.Emit(OpCodes.Call, method);
                
                RegisterCollectionTypes(context, memberType, elementType, il);
            }
            else if (resolvedMember.Interfaces.Any(i => i.InterfaceType.FullName.Contains(typeof(System.Collections.Generic.IList<>).FullName)))
            {
                var elementType = (memberType as GenericInstanceType).GenericArguments[0];
                var method =  context.Module.ImportReference(context.PropertyBagRegisterListGenericMethodReference.Value.MakeGenericInstanceMethod(containerType, memberType, elementType));
                il.Emit(OpCodes.Call, method);
                
                RegisterCollectionTypes(context, memberType, elementType, il);
            }
            else if (resolvedMember.Interfaces.Any(i => i.InterfaceType.FullName.Contains(typeof(System.Collections.Generic.ISet<>).FullName)))
            {
                var elementType = (memberType as GenericInstanceType).GenericArguments[0];
                var method = context.Module.ImportReference(context.PropertyBagRegisterSetGenericMethodReference.Value.MakeGenericInstanceMethod(containerType, memberType, elementType));
                il.Emit(OpCodes.Call, method);
                
                RegisterCollectionTypes(context, memberType, elementType, il);
            }
            else if (resolvedMember.Interfaces.Any(i => i.InterfaceType.FullName.Contains(typeof(System.Collections.Generic.IDictionary<,>).FullName)))
            {
                var keyType = (memberType as GenericInstanceType).GenericArguments[0];
                var valueType = (memberType as GenericInstanceType).GenericArguments[1];
                var method = context.Module.ImportReference(context.PropertyBagRegisterDictionaryGenericMethodReference.Value.MakeGenericInstanceMethod(containerType, memberType, keyType, valueType));
                il.Emit(OpCodes.Call, method);
                
                RegisterCollectionTypes(context, memberType, keyType, il);
                RegisterCollectionTypes(context, memberType, valueType, il);
            }
        }
    }
}