using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Unity.Properties.CodeGen.Blocks
{
    static class PropertyBagRegistry
    {
        const string kTypeName = "PropertyBagRegistry";
        const string kInitializeMethodName = "Initialize";
        
        public static TypeDefinition Generate(Context context, IEnumerable<Tuple<TypeDefinition, TypeReference>> types)
        {
            var type = new TypeDefinition
            (
                @namespace: Context.kNamespace,
                @name: kTypeName,
                @attributes: TypeAttributes.Class | TypeAttributes.NotPublic,
                @baseType: context.ImportReference(typeof(object))
            )
            {
                IsBeforeFieldInit = true
            };
            
            type.Methods.Add(CreateInitializeMethodDefinition(context, types));

            return type;
        }

        static MethodDefinition CreateInitializeMethodDefinition(Context context, IEnumerable<Tuple<TypeDefinition, TypeReference>> propertyBagTypes)
        {
            var method = new MethodDefinition
            (
                @name: kInitializeMethodName,
                @attributes: MethodAttributes.Static | MethodAttributes.Public,
                @returnType: context.ImportReference(typeof(void))
            );
                
#if !UNITY_DOTSPLAYER
            // We need our registration to be triggered as soon as the assemblies are loaded so we do so with the following
            // custom attributes in hybrid. DOTS Player will solve this elsewhere (in TypeRegGen)
            context.AddInitializeOnLoadMethodAttribute(method);
            context.AddPreserveAttribute(method);
#else
            throw new Exception("InitializeOnLoadMethodAttribute not supported in UNITY_DOTSPLAYER.")
#endif
            
            method.Body.InitLocals = true;

            var il = method.Body.GetILProcessor();
            
            foreach (var (propertyBagTypeDefinition, containerTypeReference) in propertyBagTypes)
            {
                var propertyBagTypeConstructor = context.Module.ImportReference(propertyBagTypeDefinition.GetConstructors().First());
                var propertyBagRegisterMethodReference = context.Module.ImportReference(context.PropertyBagRegisterGenericMethodReference.Value.MakeGenericInstanceMethod(containerTypeReference)); 
                
                il.Emit(OpCodes.Newobj, propertyBagTypeConstructor);
                il.Emit(OpCodes.Call, propertyBagRegisterMethodReference);
            }
            
            il.Emit(OpCodes.Ret);
            
            return method;
        }
    }
}