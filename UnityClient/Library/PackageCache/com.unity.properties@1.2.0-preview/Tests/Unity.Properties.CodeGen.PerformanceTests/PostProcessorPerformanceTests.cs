using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.Compilation;
using UnityEngine;

namespace Unity.Properties.CodeGen.PerformanceTests
{
    [TestFixture]
    sealed class PostProcessorPerformanceTests
    {
        string CreateAssembly(int typeCount, int propertyCount)
        {
            var directory = Application.temporaryCachePath + "/" + "TestAssembly";

            Directory.CreateDirectory(directory);

            var output = Path.Combine(directory, "TestAssembly.dll");
            var scripts = new List<string>();
            
            File.WriteAllText(Path.Combine(directory, $"Base.cs"), "public class Base {}");
            scripts.Add(Path.Combine(directory, $"Base.cs"));
            
            for (var i = 0; i < typeCount; i++)
            {
                var builder = new StringBuilder();

                builder.AppendLine("using Unity.Properties;");
                builder.AppendLine();
                builder.AppendLine($"[{nameof(GeneratePropertyBagAttribute)}]");
                builder.AppendLine($"public class Test{i} {{");

                for (var p = 0; p < propertyCount; p++)
                {
                    builder.AppendLine($"    public int field{p};");
                }
                
                builder.AppendLine($"}}");

                var script = Path.Combine(directory, $"Script{i}.cs");
                File.WriteAllText(script, builder.ToString());
                scripts.Add(script);
            }

            var assemblyBuilder = new AssemblyBuilder(output, scripts.ToArray());

            // Start build of assembly
            if (!assemblyBuilder.Build())
            {
                throw new Exception($"Failed to start build of assembly {assemblyBuilder.assemblyPath}!");
            }

            while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
            {
                Thread.Sleep(10);
            }

            return output;
        }

        [Test, Performance]
        [TestCase(10, 10)]
        [TestCase(0, 0)]
        public void ProcessAssemblyWithPropertyContainers(int typeCount, int fieldCount)
        {
            var output = CreateAssembly(typeCount, fieldCount);

            var peData = File.ReadAllBytes(output);
            var pdbData = File.ReadAllBytes(Path.ChangeExtension(output, ".pdb"));

            Measure.Method(() =>
                   {
                       PostProcessorUtility.Process("TestAssembly.dll", peData, pdbData, new [] {"UNITY_EDITOR"}, new[]
                       {
                           // Not sure where we should find the assemblies... Lets just use the ScriptAssemblies for now.
                           string.Format("Library{0}ScriptAssemblies{0}Unity.Properties.dll", Path.DirectorySeparatorChar)
                       });
                   })
                   .WarmupCount(2)
                   .MeasurementCount(100)
                   .Run();
        }
    }
}