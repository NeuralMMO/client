using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Unity.CodeEditor;
using System.Runtime.InteropServices;


namespace VisualStudioEditor
{
    public enum VisualStudioVersion
    {
        Invalid = 0,
        VisualStudio2008 = 9,
        VisualStudio2010 = 10,
        VisualStudio2012 = 11,
        VisualStudio2013 = 12,
        VisualStudio2015 = 14,
        VisualStudio2017 = 15,
        VisualStudio2019 = 16,
    }

    [InitializeOnLoad]
    public class VSEditor : IExternalCodeEditor
    {
        static readonly string k_ExpressNotSupportedMessage = L10n.Tr(
            "Unfortunately Visual Studio Express does not allow itself to be controlled by external applications. " +
            "You can still use it by manually opening the Visual Studio project file, but Unity cannot automatically open files for you when you doubleclick them. " +
            "\n(This does work with Visual Studio Pro)"
        );

        static VSEditor()
        {
            try
            {
                InstalledVisualStudios = Discovery.GetInstalledVisualStudios();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($@"Error detecting Visual Studio installations: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                InstalledVisualStudios = new Dictionary<VisualStudioVersion, string[]>();
            }
            var editor = new VSEditor(new Discovery(), new ProjectGeneration());
            CodeEditor.Register(editor);
            var current = CodeEditor.CurrentEditorInstallation;
            if (editor.TryGetInstallationForPath(current, out var installation))
            {
                if (installation.Name != "MonoDevelop")
                {
                    editor.Initialize(current);
                }
                return;
            }
        }

        const string unity_generate_all = "unity_generate_all_csproj";
        IDiscovery m_Discoverability;
        IGenerator m_Generation;
        CodeEditor.Installation m_Installation;
        VSInitializer m_Initializer = new VSInitializer();

        public VSEditor(IDiscovery discovery, IGenerator projectGeneration)
        {
            m_Discoverability = discovery;
            m_Generation = projectGeneration;
        }

        internal static Dictionary<VisualStudioVersion, string[]> InstalledVisualStudios { get; private set; }

        internal static bool IsOSX => Application.platform == RuntimePlatform.OSXEditor;
        internal static bool IsWindows => !IsOSX && Path.DirectorySeparatorChar == '\\' && Environment.NewLine == "\r\n";

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public void CreateIfDoesntExist()
        {
            if (!m_Generation.HasSolutionBeenGenerated())
            {
                m_Generation.Sync();
            }
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            var lowerCasePath = editorPath.ToLower();
            if (lowerCasePath.EndsWith("vcsexpress.exe", StringComparison.OrdinalIgnoreCase))
            {
                installation = new CodeEditor.Installation
                {
                    Name = "VSExpress",
                    Path = editorPath
                };
                m_Installation = installation;
                return true;
            }

            if (lowerCasePath.EndsWith("devenv.exe", StringComparison.OrdinalIgnoreCase)
                || lowerCasePath.Replace(" ", "").EndsWith("visualstudio.app", StringComparison.OrdinalIgnoreCase)
                || lowerCasePath.Replace(" ", "").EndsWith("visualstudio(preview).app", StringComparison.OrdinalIgnoreCase))
            {
                installation = new CodeEditor.Installation
                {
                    Name = "VisualStudio",
                    Path = editorPath
                };
                m_Installation = installation;
                return true;
            }

            if (lowerCasePath.Contains("monodevelop")
                || lowerCasePath.Replace(" ", "").Contains("xamarinstudio"))
            {
                installation = new CodeEditor.Installation
                {
                    Name = "MonoDevelop",
                    Path = editorPath
                };
                m_Installation = installation;
                return true;
            }

            installation = default;
            m_Installation = installation;
            return false;
        }

        public void OnGUI()
        {
            if (m_Installation.Name.Equals("VSExpress"))
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("", "CN EntryWarn");
                GUILayout.Label(k_ExpressNotSupportedMessage, "WordWrappedLabel");
                GUILayout.EndHorizontal();
            }

            var prevGenerate = EditorPrefs.GetBool(unity_generate_all, false);
            var generateAll = EditorGUILayout.Toggle("Generate all .csproj files.", prevGenerate);
            if (generateAll != prevGenerate)
            {
                EditorPrefs.SetBool(unity_generate_all, generateAll);
            }
            m_Generation.GenerateAll(generateAll);
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            m_Generation.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles), importedFiles);
        }

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            m_Generation.Sync();
        }

        public void Initialize(string editorInstallationPath)
        {
            m_Initializer.Initialize(editorInstallationPath, InstalledVisualStudios);
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (m_Installation.Name == "MonoDevelop") {
                return OpenAppMonoDev(path, line, column);
            }

            if (IsOSX)
            {
                return OpenOSXApp(path, line, column);
            }

            if (IsWindows)
            {
                return OpenWindowsApp(path, line);
            }

            return false;
        }

        private bool OpenWindowsApp(string path, int line)
        {
            var comAssetPath = AssetDatabase.FindAssets("COMIntegration a:packages").Select(AssetDatabase.GUIDToAssetPath).First(assetPath => assetPath.Contains("COMIntegration.dom"));
            if (string.IsNullOrWhiteSpace(comAssetPath)) // This may be called too early where the asset database has not replicated this information yet.
            {
                return false;
            }
            UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(comAssetPath);
            var progpath = packageInfo.resolvedPath + comAssetPath.Substring("Packages/com.unity.ide.visualstudio".Length);
            string absolutePath = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                absolutePath = Path.GetFullPath(path);
            }

            
            var solution = GetOrGenerateSolutionFile(path);
            solution = solution == "" ? "" : $"\"{solution}\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = progpath,
                    Arguments = $"\"{CodeEditor.CurrentEditorInstallation}\" \"{absolutePath}\" {solution} {line}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };
            var result = process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                var outputLine = process.StandardOutput.ReadLine();
                if (outputLine == "displayProgressBar")
                {
                    EditorUtility.DisplayProgressBar("Opening Visual Studio", "Starting up Visual Studio, this might take some time.", .5f);
                }

                if (outputLine == "clearprogressbar")
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            var errorOutput = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorOutput))
            {
                Console.WriteLine("Error: \n" + errorOutput);
            }

            process.WaitForExit();
            return result;
        }

        bool OpenAppMonoDev(string path, int line, int column)
        {
            string absolutePath = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                absolutePath = Path.GetFullPath(path);
            }

            var solution = GetOrGenerateSolutionFile(path);
            solution = solution == "" ? "" : $"\"{solution}\"";
            var pathArguments = path == "" ? "" : $"\"{path}\";{line}";
            var fileName = IsOSX ? "open" : CodeEditor.CurrentEditorInstallation;
            var arguments = IsOSX
                ? $"\"{CodeEditor.CurrentEditorInstallation}\" --args --nologo {solution} {pathArguments}"
                : $"--nologo {solution} {pathArguments}";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };

            process.Start();

            return true;
        }

        [DllImport ("AppleEventIntegrationPlugin")]
        static extern void OpenVisualStudio(string appPath, string solutionPath, string filePath, int line, StringBuilder sb, int sbLength);

        bool OpenOSXApp(string path, int line, int column)
        {
            string absolutePath = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                absolutePath = Path.GetFullPath(path);
            }

            string solution = GetOrGenerateSolutionFile(path);

            StringBuilder sb = new StringBuilder(4096);

            OpenVisualStudio(CodeEditor.CurrentEditorInstallation, solution, absolutePath, line, sb, sb.Capacity);

            Console.WriteLine(sb.ToString());

            return true;
        }

        private string GetOrGenerateSolutionFile(string path)
        {
            var solution = GetSolutionFile(path);
            if (solution == "")
            {
                m_Generation.Sync();
                solution = GetSolutionFile(path);
            }

            return solution;
        }

        string GetSolutionFile(string path)
        {
            if (UnityEditor.Unsupported.IsDeveloperBuild())
            {
                var baseFolder = GetBaseUnityDeveloperFolder();
                var lowerPath = path.ToLowerInvariant();
                var isUnitySourceCode =
                    lowerPath.Contains((baseFolder + "/Runtime").ToLowerInvariant())
                    || lowerPath.Contains((baseFolder + "/Editor").ToLowerInvariant());

                if (isUnitySourceCode)
                {
                    return Path.Combine(baseFolder, "Projects/CSharp/Unity.CSharpProjects.gen.sln");
                }
            }
            var solutionFile = m_Generation.SolutionFile();
            if (File.Exists(solutionFile))
            {
                return solutionFile;
            }
            return "";
        }

        static string GetBaseUnityDeveloperFolder()
        {
            return Directory.GetParent(EditorApplication.applicationPath).Parent.Parent.FullName;
        }
    }
}
