using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
#if !UNITY_DOTSPLAYER
using UnityEngine.Scripting;
#endif
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Unity.Entities.CodeGen
{
    static class EntitiesILHelpers
    {
        public static string GetOnCreateForCompilerName() => nameof(ComponentSystemBase.OnCreateForCompiler);

        public static MethodDefinition GetOrMakeOnCreateForCompilerMethodFor(TypeDefinition type)
        {
            var onCreateForCompilerName = GetOnCreateForCompilerName();
            var onCreateForCompilerMethod = type.Methods.SingleOrDefault(m => m.Name == onCreateForCompilerName);

            if (onCreateForCompilerMethod == null)
            {
                var typeSystemVoid = type.Module.TypeSystem.Void;
                onCreateForCompilerMethod = new MethodDefinition(onCreateForCompilerName,
                    MethodAttributes.FamORAssem | MethodAttributes.Virtual | MethodAttributes.HideBySig, typeSystemVoid);

                var ilProcessor = onCreateForCompilerMethod.Body.GetILProcessor();
                ilProcessor.Emit(OpCodes.Ldarg_0);
                ilProcessor.Emit(OpCodes.Call, new MethodReference(onCreateForCompilerName, type.Module.TypeSystem.Void, type.BaseType) { HasThis = true});
                ilProcessor.Emit(OpCodes.Ret);
                type.Methods.Add(onCreateForCompilerMethod);
            }

            return onCreateForCompilerMethod;
        }
    }
}
