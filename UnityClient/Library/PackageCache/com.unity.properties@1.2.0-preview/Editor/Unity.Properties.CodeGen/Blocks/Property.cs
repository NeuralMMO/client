using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.Collections;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Unity.Properties.CodeGen.Blocks
{
    static class Property
    {
        public static TypeDefinition Generate(Context context, TypeReference containerType, IMemberDefinition member)
        {
            if (null == member)
            {
                throw new ArgumentException(nameof(member));
            }

            var memberType = context.Module.ImportReference(Utility.GetMemberType(member).ResolveGenericParameter(containerType));
            
            var propertyBaseType = context.ImportReference(typeof(Property<,>)).MakeGenericInstanceType(containerType, memberType);
            
            var type = new TypeDefinition
            (
                @namespace: string.Empty,
                @name: Utility.GetSanitizedName(member.Name, "_Property"),
                @attributes: TypeAttributes.Class | TypeAttributes.NestedPrivate,
                @baseType: propertyBaseType
            )
            {
                Scope = containerType.Scope
            };

            var isReadOnly = (member is PropertyDefinition p && p.SetMethod == null) || (member is FieldDefinition f && f.IsInitOnly);

            var nameProperty = CreateNameProperty(context, member.Name);
            var readOnlyProperty = CreateIsReadOnlyProperty(context, isReadOnly);
            var ctorMethod = CreatePropertyCtorMethod(context, containerType, propertyBaseType, member);
            var getValueMethod = CreateGetValueMethod(context, containerType, memberType, member);
            var setValueMethod = CreateSetValueMethod(context, containerType, memberType, member, isReadOnly);

            type.Properties.Add(nameProperty);
            type.Properties.Add(readOnlyProperty);
        
            type.Methods.Add(nameProperty.GetMethod);
            type.Methods.Add(readOnlyProperty.GetMethod);
            type.Methods.Add(ctorMethod);
            type.Methods.Add(getValueMethod);
            type.Methods.Add(setValueMethod);
            
            return type;
        }

        static MethodDefinition CreatePropertyCtorMethod(Context context, TypeReference containerType, TypeReference baseType, IMemberDefinition member)
        {
            // NOTE: We create our own method reference since this assembly may not reference Unity.Properties on it's own. Thus any attempt
            // to Resolve() a TypeReference from Properties will return null. So instead we create MethodReferences for methods we
            // know will exist ourselves and let the new assembly, which will now include a reference to Properties, resolve at runtime
            var basePropertyConstructor = new MethodReference(".ctor", context.ImportReference(typeof(void)), baseType)
            {
                HasThis = true,
                ExplicitThis = false,
                CallingConvention = MethodCallingConvention.Default
            };
            
            var method = new MethodDefinition
            (
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                context.ImportReference(typeof(void))
            );
            
            var il = method.Body.GetILProcessor();
            
            // this.base()
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, context.Module.ImportReference(basePropertyConstructor));

            if (ShouldGenerateAddAttributes(context, member))
            {
                il.Emit(OpCodes.Ldarg_0);

                il.Emit(OpCodes.Ldtoken, containerType);
                il.Emit(OpCodes.Call, context.TypeGetTypeFromTypeHandleMethodReference.Value);

                il.Emit(OpCodes.Ldstr, member.Name);
                
                var flags = BindingFlags.Instance;

                if (member.IsPrivate())
                {
                    flags |= BindingFlags.NonPublic;
                }
                else
                {
                    flags |= BindingFlags.Public;
                }
                
                il.Emit(OpCodes.Ldc_I4_S, (sbyte) flags);

                if (member is FieldDefinition)
                {
                    // GetField
                    il.Emit(OpCodes.Callvirt, context.TypeGetFieldMethodReference.Value);

                    // GetCustomAttributes
                    il.Emit(OpCodes.Call, context.CustomAttributeExtensionsGetCustomAttributesMethodReference.Value);
                }
                else if (member is PropertyDefinition)
                {
                    // GetProperty
                    il.Emit(OpCodes.Callvirt, context.TypeGetPropertyMethodReference.Value);

                    // GetCustomAttributes
                    il.Emit(OpCodes.Call, context.CustomAttributeExtensionsGetCustomAttributesMethodReference.Value);
                }

                var baseAddAttributesMethod = new MethodReference("AddAttributes", context.ImportReference(typeof(void)), baseType)
                {
                    HasThis = true,
                    ExplicitThis = false,
                    CallingConvention = MethodCallingConvention.Default
                };

                baseAddAttributesMethod.Parameters.Add(new ParameterDefinition("attributes", ParameterAttributes.None, context.ImportReference(typeof(IEnumerable<Attribute>))));

                il.Emit(OpCodes.Call, baseAddAttributesMethod);
            }

            il.Emit(OpCodes.Ret);

            return method;
        }

        static bool ShouldGenerateAddAttributes(Context context, ICustomAttributeProvider member)
        {
            if (!member.HasCustomAttributes)
            {
                return false;
            }
            
            if (member.CustomAttributes.Count == 1 && member.CustomAttributes[0].AttributeType.FullName == context.ImportReference(typeof(CreatePropertyAttribute)).FullName)
            {
                return false;
            }

            return true;
        }

        static PropertyDefinition CreateNameProperty(Context context, string name)
        {
            var property = new PropertyDefinition("Name", PropertyAttributes.None, context.ImportReference(typeof(string)));
            
            var method = new MethodDefinition
            (
                @name: "get_Name",
                @attributes: MethodAttributes.Public | 
                             MethodAttributes.HideBySig | 
                             MethodAttributes.SpecialName | 
                             MethodAttributes.Virtual |
                             MethodAttributes.ReuseSlot,
                @returnType: context.ImportReference(typeof(string))
            )
            {
                Body = {InitLocals = true}, 
                SemanticsAttributes = MethodSemanticsAttributes.Getter
            };

            var il = method.Body.GetILProcessor();
            
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Ret);

            property.GetMethod = method;

            return property;
        }

        static PropertyDefinition CreateIsReadOnlyProperty(Context context, bool isReadOnly)
        {
            var property = new PropertyDefinition("IsReadOnly", PropertyAttributes.None, context.ImportReference(typeof(bool)));
            
            var method = new MethodDefinition
            (
                @name: "get_IsReadOnly",
                @attributes: MethodAttributes.Public | 
                             MethodAttributes.HideBySig | 
                             MethodAttributes.SpecialName | 
                             MethodAttributes.Virtual |
                             MethodAttributes.ReuseSlot,
                @returnType: context.ImportReference(typeof(bool))
            )
            {
                Body = {InitLocals = true}, 
                SemanticsAttributes = MethodSemanticsAttributes.Getter
            };

            var il = method.Body.GetILProcessor();
            
            il.Emit(isReadOnly ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            property.GetMethod = method;

            return property;
        }

        static MethodDefinition CreateGetValueMethod(Context context, TypeReference containerType, TypeReference memberType, IMetadataTokenProvider member)
        {
            var method = new MethodDefinition
            (
                @name: "GetValue",
                @attributes: MethodAttributes.Public | 
                             MethodAttributes.HideBySig | 
                             MethodAttributes.Virtual |
                             MethodAttributes.ReuseSlot,
                @returnType: memberType
            )
            {
                Body = {InitLocals = true}
            };
            
            var containerParameter = new ParameterDefinition("container", ParameterAttributes.None, new ByReferenceType(containerType));
            
            method.Parameters.Add(containerParameter);

            var il = method.Body.GetILProcessor();
            
            il.Emit(OpCodes.Ldarg_1); // container
                
            if (!containerType.IsValueType)
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
            
            if (member is FieldDefinition field)
            {
                il.Emit(OpCodes.Ldfld, new FieldReference(field.Name, memberType, containerType));
            }
            else if (member is PropertyDefinition property)
            {
                il.Emit(OpCodes.Call, context.Module.ImportReference(property.GetMethod));
            }
            
            il.Emit(OpCodes.Ret);
            
            return method;
        }
        
        static MethodDefinition CreateSetValueMethod(Context context, TypeReference containerType, TypeReference memberType, IMetadataTokenProvider member, bool isReadOnly)
        {
            var method = new MethodDefinition
            (
                @name: "SetValue",
                @attributes: MethodAttributes.Public | 
                             MethodAttributes.HideBySig | 
                             MethodAttributes.Virtual |
                             MethodAttributes.ReuseSlot,
                @returnType: context.ImportReference(typeof(void))
            )
            {
                Body = {InitLocals = true}
            };

            method.Parameters.Add(new ParameterDefinition("container", ParameterAttributes.None, new ByReferenceType(containerType)));
            method.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, memberType));

            var il = method.Body.GetILProcessor();

            if (isReadOnly)
            {
                il.Emit(OpCodes.Ldstr, "Property is ReadOnly");
                il.Emit(OpCodes.Newobj, context.ExceptionConstructor.Value);
                il.Emit(OpCodes.Throw);
            }
            else
            {
                if (member is FieldDefinition field)
                {
                    il.Emit(OpCodes.Ldarg_1); // container
                
                    if (!containerType.IsValueType) il.Emit(OpCodes.Ldind_Ref);

                    il.Emit(OpCodes.Ldarg_2); // value
                    il.Emit(OpCodes.Stfld, new FieldReference(field.Name, memberType, containerType));
                    il.Emit(OpCodes.Ret);
                }
                else if (member is PropertyDefinition property)
                {
                    il.Emit(OpCodes.Ldarg_1); // container
            
                    if (!containerType.IsValueType) il.Emit(OpCodes.Ldind_Ref);

                    il.Emit(OpCodes.Ldarg_2); // value
                    il.Emit(OpCodes.Call, context.Module.ImportReference(property.SetMethod));
                    il.Emit(OpCodes.Ret);
                }
            }

            return method;
        }
    }
}