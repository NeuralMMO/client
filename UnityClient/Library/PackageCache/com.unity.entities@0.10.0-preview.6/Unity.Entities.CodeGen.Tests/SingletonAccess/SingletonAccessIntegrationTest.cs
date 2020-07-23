using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;
using NUnit.Framework;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Unity.Entities.CodeGen.Tests.SingletonAccess.Infrastructure
{
    [TestFixture]
    public abstract class SingletonAccessIntegrationTest : IntegrationTest
    {
        protected override string ExpectedPath
        {
            get { return "Packages/com.unity.entities/Unity.Entities.CodeGen.Tests/SingletonAccess/IntegrationTests"; }
        }

        StringBuilder _methodIL;
        protected override string AdditionalIL
        {
            get { return _methodIL.ToString(); }
        }

        protected void RunTest(Type type)
        {
            var methodToAnalyze = MethodDefinitionForOnlyMethodOf(type);

            var entityQueries = new Dictionary<(TypeReference declaringType, string name, bool asReadOnly), (FieldDefinition field, TypeReference type)>();
            SingletonAccessPostProcessor.Rewrite(methodToAnalyze, entityQueries);
            SingletonAccessPostProcessor.InjectEntityQueriesIntoSystemBase(entityQueries);

            _methodIL = new StringBuilder();
            if (methodToAnalyze != null)
            {
                foreach (var instruction in methodToAnalyze.Body.Instructions)
                    _methodIL.AppendLine(instruction.ToString());
            }

            RunTest(methodToAnalyze.DeclaringType);
        }
    }
}
