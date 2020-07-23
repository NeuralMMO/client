using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Properties.CodeGen
{
    class AssemblyResolver : IAssemblyResolver
    {
        readonly struct CacheKey : IEquatable<CacheKey>
        {
            readonly string Name;
            readonly DateTime Time;

            public CacheKey(string name, DateTime time)
            {
                Name = name;
                Time = time;
            }

            public bool Equals(CacheKey other) => Name == other.Name && Time.Equals(other.Time);
            public override bool Equals(object obj) => obj is CacheKey other && Equals(other);
            public static bool operator ==(CacheKey left, CacheKey right) =>  left.Equals(right);
            public static bool operator !=(CacheKey left, CacheKey right) => !left.Equals(right);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Time.GetHashCode();
                }
            }
        }
        
        readonly string[] m_References;
        readonly Dictionary<CacheKey, AssemblyDefinition> m_Cache = new Dictionary<CacheKey, AssemblyDefinition>();
        readonly ICompiledAssembly m_CompiledAssembly;
        AssemblyDefinition m_SelfAssembly;

        public AssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            m_CompiledAssembly = compiledAssembly;
            m_References = compiledAssembly.References;
        }
            
        public void Dispose()
        {
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            m_SelfAssembly = assemblyDefinition;
        }
        
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
        }

        static string GetReferencePath(string[] references, AssemblyNameReference name)
        {
            // Check for dll
            var dll = name.Name + ".dll";
            foreach (var reference in references)
            {
                if (reference.EndsWith(dll)) return reference;
            }
            
            // Check for exe
            var exe = name.Name + ".exe";
            foreach (var reference in references)
            {
                if (reference.EndsWith(exe)) return reference;
            }
            
            // Unfortunately the current ICompiledAssembly API only provides direct references.
            // It is very much possible that a postprocessor ends up investigating a type in a directly
            // referenced assembly, that contains a field that is not in a directly referenced assembly.
            // if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            // in the ILPostProcessing api. As a workaround, we rely on the fact here that the indirect references
            // are always located next to direct references, so we search in all directories of direct references we
            // got passed, and if we find the file in there, we resolve to it.
            foreach (var directory in references.Select(Path.GetDirectoryName).Distinct())
            { 
                var reference = Path.Combine(directory, dll);
                if (File.Exists(reference)) return reference;
            }

            return null;
        }
            
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            lock (m_Cache)
            {
                if (name.Name == m_CompiledAssembly.Name)
                {
                    return m_SelfAssembly;
                }

                var path = GetReferencePath(m_References, name);
                
                if (path == null)
                {
                    return null;
                }

                var cacheKey = new CacheKey(path, File.GetLastWriteTime(path));

                if (m_Cache.TryGetValue(cacheKey, out var result))
                {
                    return result;
                }

                parameters.ReadSymbols = false;
                parameters.ReadWrite = false;
                parameters.AssemblyResolver = this;
                parameters.ReadingMode = ReadingMode.Deferred;

                using (var stream = OpenMemoryStream(path))
                {
                    var pdb = Path.ChangeExtension(path, ".pdb");
                
                    if (File.Exists(pdb))
                    {
                        parameters.SymbolStream = OpenMemoryStream(pdb);
                    }
                    
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, parameters);
                    m_Cache.Add(cacheKey, assemblyDefinition);
                    return assemblyDefinition;
                }
            }
        }

        static MemoryStream OpenMemoryStream(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () => new MemoryStream(File.ReadAllBytes(fileName)));
        }

        static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                {
                    throw;
                }
                
                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);
                return Retry(retryCount - 1, waitTime, func);
            }
        }
    }
}