using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

[assembly: InternalsVisibleTo("Unity.Entities.Hybrid.CodeGen")]
namespace Unity.Entities.CodeGen
{
    internal class EntitiesILPostProcessors : ILPostProcessor
    {
        public static string[] Defines { get; internal set; }

        static EntitiesILPostProcessor[] FindAllEntitiesILPostProcessors()
        {
            var processorTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains(".CodeGen"))
                    processorTypes.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(EntitiesILPostProcessor)) && !t.IsAbstract));
            }

            return processorTypes.Select(t => (EntitiesILPostProcessor)Activator.CreateInstance(t)).ToArray();
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            bool madeAnyChange = false;
            Defines = compiledAssembly.Defines;
            var assemblyDefinition = AssemblyDefinitionFor(compiledAssembly);
            var postProcessors = FindAllEntitiesILPostProcessors();

            var componentSystemTypes = assemblyDefinition.MainModule.GetAllTypes().Where(TypeDefinitionExtensions.IsComponentSystem).ToArray();
            foreach (var systemType in componentSystemTypes)
            {
                InjectOnCreateForCompiler(systemType);
                madeAnyChange = true;
            }

            var diagnostics = new List<DiagnosticMessage>();
            foreach (var postProcessor in postProcessors)
            {
                diagnostics.AddRange(postProcessor.PostProcess(assemblyDefinition, componentSystemTypes, out var madeChange));
                madeAnyChange |= madeChange;
            }

            var unmanagedComponentSystemTypes = assemblyDefinition.MainModule.GetAllTypes().Where((x) => x.TypeImplements(typeof(ISystemBase))).ToArray();
            foreach (var postProcessor in postProcessors)
            {
                diagnostics.AddRange(postProcessor.PostProcessUnmanaged(assemblyDefinition, unmanagedComponentSystemTypes, out var madeChange));
                madeAnyChange |= madeChange;
            }

            if (!madeAnyChange || diagnostics.Any(d => d.DiagnosticType == DiagnosticType.Error))
                return new ILPostProcessResult(null, diagnostics);

            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(), SymbolStream = pdb, WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), diagnostics);
        }

        static void InjectOnCreateForCompiler(TypeDefinition typeDefinition)
        {
            // Turns out it's not trivial to inject some code that has to be run OnCreate of the system.
            // We cannot just create an OnCreate() method, because there might be a deriving class that also implements it.
            // That child method is probably not calling base.OnCreate(), but even when it is (!!) the c# compiler bakes base.OnCreate()
            // into a direct reference to whatever is the first baseclass to have OnCreate() at the time of compilation.  So if we go
            // and inject an OnCreate() in this class later on,  the child's base.OnCreate() call will actually bypass it.
            //
            // Instead what we do is add OnCreateForCompiler,  hide it from intellisense, give you an error if wanna be that guy that goes
            // and implement it anyway,  and then we inject a OnCreateForCompiler method into each and every ComponentSystem.  The reason we have to emit it in
            // each and every system, and not just the ones where we have something to inject,  is that when we emit these method, we need
            // to also emit base.OnCreateForCompiler().  However, when we are processing an user system type,  we do not know yet if its baseclass
            // also needs an OnCreateForCompiler().   So we play it safe, and assume it does.  So every OnCreateForCompiler() that we emit,
            // will assume its basetype also has an implementation and invoke that.

            if (typeDefinition.Name == nameof(ComponentSystemBase) && typeDefinition.Namespace == "Unity.Entities") return;

            var onCreateForCompilerName = EntitiesILHelpers.GetOnCreateForCompilerName();
            var preExistingMethod = typeDefinition.Methods.FirstOrDefault(m => m.Name == onCreateForCompilerName);
            if (preExistingMethod != null)
                UserError.DC0026($"It's not allowed to implement {onCreateForCompilerName}'", preExistingMethod).Throw();

            EntitiesILHelpers.GetOrMakeOnCreateForCompilerMethodFor(typeDefinition);
        }

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            if (compiledAssembly.Name == "Unity.Entities")
                return true;
            return compiledAssembly.References.Any(f => Path.GetFileName(f) == "Unity.Entities.dll") &&
                !compiledAssembly.Name.Contains("CodeGen.Tests");
        }

        class PostProcessorAssemblyResolver : IAssemblyResolver
        {
            private readonly string[] _references;
            Dictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();
            private ICompiledAssembly _compiledAssembly;
            private AssemblyDefinition _selfAssembly;

            public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
            {
                _compiledAssembly = compiledAssembly;
                _references = compiledAssembly.References;
            }

            public void Dispose()
            {
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                lock (_cache)
                {
                    if (name.Name == _compiledAssembly.Name)
                        return _selfAssembly;

                    var fileName = FindFile(name);
                    if (fileName == null)
                        return null;

                    var lastWriteTime = File.GetLastWriteTime(fileName);

                    var cacheKey = fileName + lastWriteTime.ToString();

                    if (_cache.TryGetValue(cacheKey, out var result))
                        return result;

                    parameters.AssemblyResolver = this;

                    var ms = MemoryStreamFor(fileName);

                    var pdb = fileName + ".pdb";
                    if (File.Exists(pdb))
                        parameters.SymbolStream = MemoryStreamFor(pdb);

                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                    _cache.Add(cacheKey, assemblyDefinition);
                    return assemblyDefinition;
                }
            }

            private string FindFile(AssemblyNameReference name)
            {
                var fileName = _references.FirstOrDefault(r => Path.GetFileName(r) == name.Name + ".dll");
                if (fileName != null)
                    return fileName;

                // perhaps the type comes from an exe instead
                fileName = _references.FirstOrDefault(r => Path.GetFileName(r) == name.Name + ".exe");
                if (fileName != null)
                    return fileName;

                //Unfortunately the current ICompiledAssembly API only provides direct references.
                //It is very much possible that a postprocessor ends up investigating a type in a directly
                //referenced assembly, that contains a field that is not in a directly referenced assembly.
                //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
                //in the ILPostProcessing api. As a workaround, we rely on the fact here that the indirect references
                //are always located next to direct references, so we search in all directories of direct references we
                //got passed, and if we find the file in there, we resolve to it.
                foreach (var parentDir in _references.Select(Path.GetDirectoryName).Distinct())
                {
                    var candidate = Path.Combine(parentDir, name.Name + ".dll");
                    if (File.Exists(candidate))
                        return candidate;
                }

                return null;
            }

            static MemoryStream MemoryStreamFor(string fileName)
            {
                return Retry(10, TimeSpan.FromSeconds(1), () => {
                    byte[] byteArray;
                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byteArray = new byte[fs.Length];
                        var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                        if (readLength != fs.Length)
                            throw new InvalidOperationException("File read length is not full length of file.");
                    }

                    return new MemoryStream(byteArray);
                });
            }

            private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
            {
                try
                {
                    return func();
                }
                catch (IOException)
                {
                    if (retryCount == 0)
                        throw;
                    Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                    Thread.Sleep(waitTime);
                    return Retry(retryCount - 1, waitTime, func);
                }
            }

            public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
            {
                _selfAssembly = assemblyDefinition;
            }
        }

        private static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
        {
            var resolver = new PostProcessorAssemblyResolver(compiledAssembly);
            var readerParameters = new ReaderParameters
            {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray()),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = resolver,
                ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
                ReadingMode = ReadingMode.Immediate
            };

            var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray());
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

            //apparently, it will happen that when we ask to resolve a type that lives inside Unity.Entities, and we
            //are also postprocessing Unity.Entities, type resolving will fail, because we do not actually try to resolve
            //inside the assembly we are processing. Let's make sure we do that, so that we can use postprocessor features inside
            //unity.entities itself as well.
            resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }
    }

    internal class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
        {
            return new PostProcessorReflectionImporter(module);
        }
    }

    internal class PostProcessorReflectionImporter : DefaultReflectionImporter
    {
        private const string SystemPrivateCoreLib = "System.Private.CoreLib";
        private AssemblyNameReference _correctCorlib;

        public PostProcessorReflectionImporter(ModuleDefinition module) : base(module)
        {
            _correctCorlib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib" || a.Name == "netstandard" || a.Name == SystemPrivateCoreLib);
        }

        public override AssemblyNameReference ImportReference(AssemblyName reference)
        {
            if (_correctCorlib != null && reference.Name == SystemPrivateCoreLib)
                return _correctCorlib;

            return base.ImportReference(reference);
        }
    }

    abstract class EntitiesILPostProcessor
    {
        protected AssemblyDefinition AssemblyDefinition;

        protected List<DiagnosticMessage> _diagnosticMessages = new List<DiagnosticMessage>();

        public IEnumerable<DiagnosticMessage> PostProcess(AssemblyDefinition assemblyDefinition, TypeDefinition[] componentSystemTypes, out bool madeAChange)
        {
            AssemblyDefinition = assemblyDefinition;
            try
            {
                madeAChange = PostProcessImpl(componentSystemTypes);
            }
            catch (FoundErrorInUserCodeException e)
            {
                madeAChange = false;
                return e.DiagnosticMessages;
            }

            return _diagnosticMessages;
        }

        protected abstract bool PostProcessImpl(TypeDefinition[] componentSystemTypes);
        protected abstract bool PostProcessUnmanagedImpl(TypeDefinition[] unmanagedComponentSystemTypes);

        protected void AddDiagnostic(DiagnosticMessage diagnosticMessage)
        {
            _diagnosticMessages.Add(diagnosticMessage);
        }

        public IEnumerable<DiagnosticMessage> PostProcessUnmanaged(AssemblyDefinition assemblyDefinition, TypeDefinition[] unmanagedComponentSystemTypes, out bool madeAChange)
        {
            AssemblyDefinition = assemblyDefinition;
            try
            {
                madeAChange = PostProcessUnmanagedImpl(unmanagedComponentSystemTypes);
            }
            catch (FoundErrorInUserCodeException e)
            {
                madeAChange = false;
                return e.DiagnosticMessages;
            }

            return _diagnosticMessages;
        }
    }
}
