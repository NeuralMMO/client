using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;

namespace Unity.Properties.CodeGen.Tests
{
    static class Decompiler
    {
        const string kILSpyPath = "Packages/com.unity.properties/Tests/Unity.Properties.CodeGen.Tests/.tools/ilspycmd.exe";
        
        public static string DecompileIntoString(AssemblyDefinition assemblyDefinition)
        {
            var folder = Path.GetTempPath();
            var fileName = $@"{folder}TestAssembly.dll";
            var fileNamePdb = $@"{folder}TestAssembly.pdb";
            var stream = new FileStream(fileName, FileMode.Create);
            var symbols = new FileStream(fileNamePdb, FileMode.Create);
      
            assemblyDefinition.Write(stream, new WriterParameters
            {
                SymbolStream = symbols, 
                SymbolWriterProvider = new PortablePdbWriterProvider(), 
                WriteSymbols = true
            });
            
            stream.Close();
            symbols.Close();

            var builder = new StringBuilder();
            var processed = new HashSet<string>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic && !string.IsNullOrEmpty(a.Location)))
            {
                string path;
                
                try
                {
                    path = Path.GetDirectoryName(assembly.Location);
                }
                catch (ArgumentException)
                {
                    UnityEngine.Debug.Log("Unexpected path: " + assembly.Location);
                    continue;
                }

                if (!processed.Add(path))
                {
                    continue;
                }
                
                builder.Append($"--referencepath \"{path}\" ");
            }
            
            var isWin = Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT;
            var command = Path.GetFullPath(kILSpyPath);

            if (isWin)
            {
                command = command.Replace("/","\\");
            }
            
            var info = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = isWin ? command : $"{EditorApplication.applicationPath}/Contents/MonoBleedingEdge/bin/mono",
                Arguments = $"{(isWin ? "" : command)} \"{fileName}\" {builder}",
                RedirectStandardOutput = true
            };

            var process = new Process
            {
                StartInfo = info
            };
            
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
    }
}