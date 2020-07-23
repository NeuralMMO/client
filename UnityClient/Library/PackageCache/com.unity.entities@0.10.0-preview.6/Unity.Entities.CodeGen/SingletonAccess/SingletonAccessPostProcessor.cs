using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.Collections;
using Unity.CompilationPipeline.Common.Diagnostics;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Unity.Entities.CodeGen
{
    class SingletonAccessPostProcessor : EntitiesILPostProcessor
    {
        static readonly bool _enable = true;

        static readonly Dictionary<string, (string methodName, int parameterCount, bool needsReadOnlyQuery, bool patchedIsGeneric)> SingletonAccessMethodDescriptions
            = new Dictionary<string, (string methodName, int parameterCount, bool needsReadOnlyQuery, bool patchedIsGeneric)>()
            {
            { nameof(SystemBase.GetSingleton), (nameof(EntityQuery.GetSingleton), 0, true, true) },
            { nameof(SystemBase.SetSingleton), (nameof(EntityQuery.SetSingleton), 1, false, true) },
            { nameof(SystemBase.GetSingletonEntity), (nameof(EntityQuery.GetSingletonEntity), 0, true, false) }
            };

        protected override bool PostProcessImpl(TypeDefinition[] componentSystemTypes)
        {
            if (!_enable)
                return false;

            bool madeChange = false;

            Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)> entityQueryFields =
                new Dictionary<(TypeReference, string, bool), (FieldDefinition, TypeReference)>();

            var systemBaseTypes = componentSystemTypes.Where(TypeDefinitionExtensions.IsSystemBase).ToArray();
            foreach (var systemBaseType in systemBaseTypes)
            {
                foreach (var containingMethod in systemBaseType.Methods.ToList())
                {
                    if (containingMethod.Body == null)
                        continue;

                    try
                    {
                        madeChange |= Rewrite(containingMethod, entityQueryFields);
                    }
                    catch (PostProcessException ppe)
                    {
                        AddDiagnostic(ppe.ToDiagnosticMessage(containingMethod));
                    }
                    catch (FoundErrorInUserCodeException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var seq = containingMethod.DebugInformation.SequencePoints.FirstOrDefault();
                        AddDiagnostic(new DiagnosticMessage
                        {
                            MessageData = $"Unexpected error while post-processing Singleton methods in {containingMethod.DeclaringType.FullName}:{containingMethod.Name}. Please report this error.{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}",
                            DiagnosticType = DiagnosticType.Error,
                            Line = seq?.StartLine ?? 0,
                            Column = seq?.StartColumn ?? 0,
                        });
                    }
                }
            }

            if (madeChange)
                InjectEntityQueriesIntoSystemBase(entityQueryFields);

            return madeChange;
        }

        internal static void InjectEntityQueriesIntoSystemBase(Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)> entityQueryFields)
        {
            foreach (var entityQueryKVP in entityQueryFields)
            {
                // TODO: Move InjectAndInitializeEntityQueryField out of LambdaJobs as it's functionality needs to be shared across two post-processors
                InjectAndInitializeEntityQueryField.InjectAndInitialize(entityQueryKVP.Key.declaringType.Resolve(), entityQueryKVP.Value.field, entityQueryKVP.Value.type,
                    entityQueryKVP.Key.asReadOnly);
            }
        }

        internal static bool Rewrite(MethodDefinition containingMethod,
            Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)> entityQueryFields)
        {
            var madeChange = false;
            var instructions = containingMethod.Body.Instructions.ToArray();
            foreach (var instruction in instructions)
            {
                if (instruction.IsInvocation(out var methodReference))
                {
                    // Ensure a good match with the singleton method we want to patch
                    if (methodReference.DeclaringType.TypeReferenceEquals(typeof(ComponentSystemBase)) &&
                        SingletonAccessMethodDescriptions.TryGetValue(methodReference.Name, out var methodDescription) &&
                        methodReference is GenericInstanceMethod genericInvocation && genericInvocation.GenericArguments.Count() == 1 &&
                        methodReference.Parameters.Count() == methodDescription.parameterCount)
                    {
                        if (!madeChange)
                            containingMethod.Body.SimplifyMacros();
                        PatchSingletonMethod(containingMethod, methodReference, instruction, entityQueryFields, methodDescription);
                        madeChange = true;
                    }
                }
            }

            return madeChange;
        }

        // Perform IL patching
        // https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AXEBLANmgExAGoAfAAQCYBGAWACgHyBmAAmwDsMYoAzAQzAxWASQDCEALYAHCBxhcAIvwz9WAbwC+TNgGcMUAK5gMrAMqcA5rhgY5y1axCiJMuQowO1WnayqsAUS5sDABPAEVDHlCGdQZWBL82ABVWAHFbCw5rWzkAHmSAPgAKAEp4xMqAdwALHmFU530jEzQXKVl5JRV+CoS4+krK8gB2VgIYAUNcDABuPtZtQcSWPxRzTKsbOw4CktSAN35cKPLlodZa+tZG1mbjDDbxDvdu1QWByqWl1f8xVliCyCGBCESiUFCrAA+gBHcGhebnAFI1apDIYLI5HZ7MoLap1WA3Jx3AwPJ6uToeLwfPErMYTKYzRFfBa/dZmTbZbb5IrFQ7HU60hJXQm3e6tdpuLqeHo0pFLSqstjkdYAWTKyMqnwu0Lh0VYAF5WPIqoFgmFItEysydULWEcoHctrkOIb0pysflMdyOF4SqUbRcHU6uS7KG7YfCAHTo70uvJxnZ+612u0cjHO7GJ+w9Eq6TNyAN2yPRKPp7O7CvJ/OhnaUIvyhiaIA=
        static void PatchSingletonMethod(MethodDefinition containingMethod, MethodReference methodReference, Instruction instruction,
            Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)> entityQueryFields,
            (string methodName, int parameterCount, bool needsReadOnlyQuery, bool patchedIsGeneric) methodDescription)
        {
            var invokedGenericMethod = (GenericInstanceMethod)methodReference;
            var singletonType = invokedGenericMethod.GenericArguments.First();
            var singletonQueryField = GetOrCreateEntityQueryField(containingMethod.DeclaringType, singletonType, entityQueryFields, methodDescription);
            var ilProcessor = containingMethod.Body.GetILProcessor();

            var loadEntityQueryField = ilProcessor.Create(OpCodes.Ldflda, singletonQueryField);
            var instructionThatPushedThisArg = CecilHelpers.FindInstructionThatPushedArg(containingMethod, 0, instruction);
            ilProcessor.InsertAfter(instructionThatPushedThisArg, loadEntityQueryField);

            var singletonMethod = containingMethod.Module.ImportReference(typeof(EntityQuery).GetMethod(methodDescription.methodName));
            if (methodDescription.patchedIsGeneric)
                singletonMethod = singletonMethod.MakeGenericInstanceMethod(singletonType);
            ilProcessor.Replace(instruction, ilProcessor.Create(OpCodes.Call, singletonMethod));
        }

        // Collect EntityQueries (unique per declaring type, singleton type and readonly access
        static FieldDefinition GetOrCreateEntityQueryField(TypeDefinition declaringType, TypeReference singletonType,
            Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)> entityQueryFields,
            (string methodName, int parameterCount, bool needsReadOnlyQuery, bool patchedIsGeneric) methodDescription)
        {
            if (entityQueryFields.TryGetValue((declaringType, singletonType.FullName, methodDescription.needsReadOnlyQuery), out var result))
                return result.Item1;

            var fieldType = singletonType.Module.ImportReference(typeof(EntityQuery));
            var uniqueCounter = entityQueryFields.Count;
            var newField = new FieldDefinition($"_SingletonEntityQuery_{singletonType.Name}_{uniqueCounter}", FieldAttributes.Private, fieldType);
            if (methodDescription.needsReadOnlyQuery)
                newField.AddReadOnlyAttribute(singletonType.Module);
            result = (newField, singletonType);

            entityQueryFields[(declaringType, singletonType.FullName, methodDescription.needsReadOnlyQuery)] = result;
            return result.Item1;
        }

        protected override bool PostProcessUnmanagedImpl(TypeDefinition[] unmanagedComponentSystemTypes)
        {
            return false;
        }
    }
}
