using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Unity.Properties.CodeGen.Tests
{
    abstract class PostProcessTestBase
    {
        const string RootPath = "Packages/com.unity.properties/Tests/Unity.Properties.CodeGen.Tests/";

        class AssemblyResolver : IAssemblyResolver
        {
            public void Dispose() { }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == name.Name);
                var fileName = assembly.Location;
                parameters.AssemblyResolver = this;
                parameters.SymbolStream = OpenPdbStream(fileName);
                var bytes = File.ReadAllBytes(fileName);
                return AssemblyDefinition.ReadAssembly(new MemoryStream(bytes), parameters);
            }
        }
        
        protected static AssemblyDefinition GetAssemblyDefinition(Assembly assembly)
        {
            var location = assembly.Location;
            var definition = AssemblyDefinition.ReadAssembly(new MemoryStream(File.ReadAllBytes(location)),
                                                             new ReaderParameters(ReadingMode.Immediate)
                                                             {
                                                                 ReadSymbols = true,
                                                                 ThrowIfSymbolsAreNotMatching = true,
                                                                 SymbolReaderProvider = new PortablePdbReaderProvider(),
                                                                 AssemblyResolver = new AssemblyResolver(),
                                                                 SymbolStream = OpenPdbStream(location)
                                                             }
            );

            return definition;
        }

        static AssemblyDefinition CreateTestAssembly(AssemblyDefinition definition, string name = "TestAssembly")
        {
            return AssemblyDefinition.CreateAssembly
            (
                @assemblyName: new AssemblyNameDefinition(name, new Version(0, 0)),
                @moduleName: $"{name}.dll",
                @parameters: new ModuleParameters()
                {
                    AssemblyResolver = definition.MainModule.AssemblyResolver,
                    Kind = ModuleKind.Dll
                }
            );
        }
        
        static MemoryStream OpenPdbStream(string assemblyLocation)
        {
            var file = Path.ChangeExtension(assemblyLocation, ".pdb");
            return !File.Exists(file) ? null : new MemoryStream(File.ReadAllBytes(file));
        }
        
        static bool IsAssemblyBuiltAsDebug()
        {
            var debuggableAttributes = typeof(PostProcessTestBase).Assembly.GetCustomAttributes(typeof(DebuggableAttribute), false);
            return debuggableAttributes.Any(debuggableAttribute => ((DebuggableAttribute) debuggableAttribute).IsJITTrackingEnabled);
        }
        
        protected static void Test(string name, AssemblyDefinition source, Action<Context> action, bool overwriteExpectationWithReality)
        {
            // Ideally these tests to run in Release codegen or otherwise the generated IL won't be deterministic (due to differences between /optimize+ and /optimize-. 
            // We attempt to make the tests generate the same decompiled C# in any case (by making sure all variables are used).
            if (IsAssemblyBuiltAsDebug())
            {
                UnityEngine.Debug.LogWarning("PostProcessor tests should only be run with release code optimizations turned on for consistent codegen. Switch your settings in Preferences->External Tools->Editor Attaching (in 2019.3) or Preferences->General->Code Optimization On Startup (in 2020.1+) to be able to run these tests.");
            }

            var assembly = CreateTestAssembly(source);
            var context = new Context(assembly.MainModule, new[] {"UNITY_EDITOR"});

            action(context);

            var actualString = Decompiler.DecompileIntoString(assembly);
            var actualLines = actualString.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var actualAttributes = new List<string>();

            var expectationFile = Path.GetFullPath($"{RootPath}/{name}.cs");

            if (!File.Exists(expectationFile) || overwriteExpectationWithReality)
            {
                File.WriteAllText(expectationFile, actualString);
                Assert.Ignore("Test was not run! Run the test again with OverwriteExpectationWithReality set to false.");
                return;
            }

            var expectedString = File.ReadAllText(expectationFile);
            var expectedLines = expectedString.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var expectedAttributes = new List<string>();

            var attributeRegex = new Regex(@"^[\t, ]*\[[\w]+\][\s]*$");
            var success = expectedLines.Length == actualLines.Length;

            if (success)
            {
                for (var i = 0; i < actualLines.Length; ++i)
                {
                    var actualLine = actualLines[i];
                    var expectedLine = expectedLines[i];

                    if (attributeRegex.IsMatch(actualLine))
                    {
                        actualAttributes.Add(actualLine);
                        expectedAttributes.Add(expectedLine);
                        continue;
                    }

                    if (expectedLine != actualLine)
                    {
                        success = false;
                        break;
                    }
                }

                actualAttributes.Sort();
                expectedAttributes.Sort();
                success &= expectedAttributes.SequenceEqual(actualAttributes);
            }

            if (!success)
            {
                var path = $@"{Path.GetTempPath()}decompiled.cs";
                File.WriteAllText(path, actualString + Environment.NewLine + Environment.NewLine);

                Console.WriteLine("Decompiled C#: ");
                Console.WriteLine(actualString);
                UnityEngine.Debug.Log($"Wrote csharp to editor log and to {path}");
            }

            Assert.That(success, Is.True, "Decompiled string did not match the expected string.");
        }
    }
}