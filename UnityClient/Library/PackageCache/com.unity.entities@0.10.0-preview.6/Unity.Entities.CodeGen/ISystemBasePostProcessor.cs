using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Unity.Entities.CodeGen
{
    internal class UnmanagedSystemPostprocessor : EntitiesILPostProcessor
    {
        protected override bool PostProcessImpl(TypeDefinition[] types)
        {
            return false;
        }

        struct TypeMemo
        {
            public TypeDefinition m_SystemType;
            public MethodDefinition[] m_Wrappers;
            public int m_BurstCompileBits;
        }

        protected override bool PostProcessUnmanagedImpl(TypeDefinition[] unmanagedComponentSystemTypes)
        {
            bool changes = false;

            var memos = new List<TypeMemo>();

            foreach (var td in unmanagedComponentSystemTypes)
            {
                if (td.HasGenericParameters)
                    continue;

                changes = true;

                memos.Add(AddStaticForwarders(td));
            }

            if (!changes)
                return false;

            AddRegistrationCode(memos);

            return changes;
        }

        private static readonly string GeneratedPrefix = "__codegen__";

        static readonly string[] MethodNames = new string[] { "OnCreate", "OnUpdate", "OnDestroy" };
        static readonly string[] GeneratedMethodNames = new string[] { GeneratedPrefix + "OnCreate", GeneratedPrefix + "OnUpdate", GeneratedPrefix + "OnDestroy" };

        private TypeMemo AddStaticForwarders(TypeDefinition systemType)
        {
            var mod = systemType.Module;
            var intPtrRef = mod.ImportReference(typeof(IntPtr));
            var intPtrToVoid = mod.ImportReference(intPtrRef.Resolve().Methods.FirstOrDefault(x => x.Name == nameof(IntPtr.ToPointer)));

            TypeMemo memo = default;
            memo.m_SystemType = systemType;
            memo.m_Wrappers = new MethodDefinition[3];

            for (int i = 0; i < MethodNames.Length; ++i)
            {
                var name = MethodNames[i];
                var methodDef = new MethodDefinition(GeneratedMethodNames[i], MethodAttributes.Static | MethodAttributes.Private, mod.ImportReference(typeof(void)));
                methodDef.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, intPtrRef));
                methodDef.Parameters.Add(new ParameterDefinition("state", ParameterAttributes.None, intPtrRef));

                var targetMethod = systemType.Methods.FirstOrDefault(x => x.Name == name && x.Parameters.Count == 1).Resolve();

                // Transfer any BurstCompile attribute from target function to the forwarding wrapper
                var burstAttribute = targetMethod.CustomAttributes.FirstOrDefault(x => x.Constructor.DeclaringType.Name == nameof(BurstCompileAttribute));
                if (burstAttribute != null)
                {
                    methodDef.CustomAttributes.Add(new CustomAttribute(burstAttribute.Constructor, burstAttribute.GetBlob()));
                    memo.m_BurstCompileBits |= 1 << i;
                }

                var processor = methodDef.Body.GetILProcessor();

                processor.Emit(OpCodes.Ldarga, 0);
                processor.Emit(OpCodes.Call, intPtrToVoid);
                processor.Emit(OpCodes.Ldarga, 1);
                processor.Emit(OpCodes.Call, intPtrToVoid);
                processor.Emit(OpCodes.Call, targetMethod);
                processor.Emit(OpCodes.Ret);

                systemType.Methods.Add(methodDef);
                memo.m_Wrappers[i] = methodDef;
            }

            return memo;
        }

        private void AddRegistrationCode(List<TypeMemo> memos)
        {
            var autoClassName = $"__UnmanagedPostProcessorOutput__{(uint)AssemblyDefinition.FullName.GetHashCode()}";
            var mod = AssemblyDefinition.MainModule;

            var classDef = new TypeDefinition("", autoClassName, TypeAttributes.Class, AssemblyDefinition.MainModule.ImportReference(typeof(object)));
            classDef.IsBeforeFieldInit = false;
            mod.Types.Add(classDef);

            var funcDef = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, AssemblyDefinition.MainModule.ImportReference(typeof(void)));
            funcDef.Body.InitLocals = false;

#if !UNITY_DOTSPLAYER
            var attributeCtor = AssemblyDefinition.MainModule.ImportReference(typeof(UnityEditor.InitializeOnLoadMethodAttribute).GetConstructor(Type.EmptyTypes));
            funcDef.CustomAttributes.Add(new CustomAttribute(attributeCtor));
#endif

            classDef.Methods.Add(funcDef);

            var processor = funcDef.Body.GetILProcessor();

            var registryType = mod.ImportReference(typeof(SystemBaseRegistry)).Resolve();
            var addMethod = mod.ImportReference(registryType.Methods.FirstOrDefault((x) => x.Name == nameof(SystemBaseRegistry.AddUnmanagedSystemType)));
            var delegateCtor = mod.ImportReference(registryType.NestedTypes.FirstOrDefault((x) => x.Name == nameof(SystemBaseRegistry.ForwardingFunc)).GetConstructors().FirstOrDefault((x) => x.Parameters.Count == 2));
            var genericHashFunc = mod.ImportReference(typeof(BurstRuntime)).Resolve().Methods.FirstOrDefault((x) => x.Name == nameof(BurstRuntime.GetHashCode64) && x.HasGenericParameters);

            foreach (var memo in memos)
            {
                processor.Emit(OpCodes.Call, mod.ImportReference(genericHashFunc.MakeGenericInstanceMethod(memo.m_SystemType)));

                for (int i = 0; i < memo.m_Wrappers.Length; ++i)
                {
                    processor.Emit(OpCodes.Ldnull);
                    processor.Emit(OpCodes.Ldftn, memo.m_Wrappers[i]);
                    processor.Emit(OpCodes.Newobj, delegateCtor);
                }

                processor.Emit(OpCodes.Ldstr, memo.m_SystemType.Name);
                processor.Emit(OpCodes.Ldc_I4, memo.m_BurstCompileBits);
                processor.Emit(OpCodes.Call, addMethod);
            }

            processor.Emit(OpCodes.Ret);
        }
    }
}
