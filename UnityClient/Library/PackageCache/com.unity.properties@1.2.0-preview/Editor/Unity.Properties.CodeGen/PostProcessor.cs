using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Properties.CodeGen
{
    class PostProcessor : ILPostProcessor
    {
        static readonly string[] s_ExcludeIfAssemblyNameContains = {
            // Core Unity assemblies do not reference properties. We can simply skip processing them.
            "UnityEngine",
            "UnityEditor",
            
            // Any codegen related tests should be skipped.
            "CodeGen.Tests",
            "CodeGen.PerformanceTests"
        };

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            var name = compiledAssembly.Name;

            // Exclude based on name.
            if (s_ExcludeIfAssemblyNameContains.Any(x => name.Contains(x))) return false;
            
            var isEditor = compiledAssembly.Defines.Contains("UNITY_EDITOR");
            return !isEditor || Utility.ShouldGeneratePropertyBagsInEditor(compiledAssembly.Name);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            ILPostProcessResult result = null;

            using (var assemblyDefinition = CreateAssemblyDefinition(compiledAssembly))
            {
                if (Generate(assemblyDefinition, compiledAssembly.Defines))
                {
                    result = CreatePostProcessResult(assemblyDefinition);
                }
            }
            
            return result;
        }
        
        static bool Generate(AssemblyDefinition assembly, IEnumerable<string> defines)
        {
            var context = new Context(assembly.MainModule, defines);
            var containerTypeReferences = Utility.GetContainerTypes(context).ToArray();

            if (containerTypeReferences.Length == 0)
                return false;

            var propertyBagTypeDefinitions = new Tuple<TypeDefinition, TypeReference>[containerTypeReferences.Length];

            for (var i = 0; i < containerTypeReferences.Length; i++)
            {
                // We need to re-import since the type might not come from this assembly.
                var containerTypeReference = context.ImportReference(containerTypeReferences[i]);
                var propertyBagTypeDefinition = Blocks.PropertyBag.Generate(context, containerTypeReferences[i]);
                
                propertyBagTypeDefinitions[i] = new Tuple<TypeDefinition, TypeReference>(propertyBagTypeDefinition, containerTypeReference);
                context.Module.Types.Add(propertyBagTypeDefinition);
            }
            
            var propertyBagRegistryTypeDefinition = Blocks.PropertyBagRegistry.Generate(context, propertyBagTypeDefinitions);
            context.Module.Types.Add(propertyBagRegistryTypeDefinition);
            return true;
        }

        static ILPostProcessResult CreatePostProcessResult(AssemblyDefinition assembly)
        {
            using (var pe = new MemoryStream())
            using (var pdb = new MemoryStream())
            {
                var writerParameters = new WriterParameters
                { 
                    WriteSymbols = true,
                    SymbolStream = pdb,
                    SymbolWriterProvider = new PortablePdbWriterProvider()
                };

                assembly.Write(pe, writerParameters);
                return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()));
            }
        }

        static AssemblyDefinition CreateAssemblyDefinition(ICompiledAssembly compiledAssembly)
        {
            var resolver = new AssemblyResolver(compiledAssembly);

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadingMode = ReadingMode.Deferred
            };

            if (null != compiledAssembly.InMemoryAssembly.PdbData)
            {
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray());
                readerParameters.SymbolReaderProvider = new PortablePdbReaderProvider();
            }
            
            var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray());
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

            resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }
    }
    
    static class PostProcessorUtility
    {
        class CompiledAssembly : ICompiledAssembly
        {
            public InMemoryAssembly InMemoryAssembly { get; set; }
            public string Name { get; set; }
            public string[] References { get; set; }
            public string[] Defines { get; set; }
        }
        
        internal static void Process(string name, byte[] peData, byte[] pdbData, string[] defines, string[] references)
        {
            var compiledAssembly = new CompiledAssembly
            {
                Name = name,
                InMemoryAssembly = new InMemoryAssembly(peData, pdbData),
                Defines = defines,
                References = references
            };

            new PostProcessor().Process(compiledAssembly);
        }
    }
}