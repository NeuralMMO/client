using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace Packages.Rider.Editor
{
  [InitializeOnLoad]
  public class RiderScriptEditor : IExternalCodeEditor
  {
    IDiscovery m_Discoverability;
    IGenerator m_ProjectGeneration;
    RiderInitializer m_Initiliazer = new RiderInitializer();

    static RiderScriptEditor()
    {
      var projectGeneration = new ProjectGeneration();
      var editor = new RiderScriptEditor(new Discovery(), projectGeneration);
      CodeEditor.Register(editor);
      if (IsRiderInstallation(CodeEditor.CurrentEditorInstallation))
      {
        editor.CreateIfDoesntExist();
        editor.m_Initiliazer.Initialize(CodeEditor.CurrentEditorInstallation);
      }
    }

    const string unity_generate_all = "unity_generate_all_csproj";
    static bool IsOSX => Application.platform == RuntimePlatform.OSXEditor;

    public RiderScriptEditor(IDiscovery discovery, IGenerator projectGeneration)
    {
      m_Discoverability = discovery;
      m_ProjectGeneration = projectGeneration;
    }

    public void OnGUI()
    {
      var prevGenerate = EditorPrefs.GetBool(unity_generate_all, false);
      var generateAll = EditorGUILayout.Toggle("Generate all .csproj files.", prevGenerate);
      if (generateAll != prevGenerate)
      {
        EditorPrefs.SetBool(unity_generate_all, generateAll);
      }

      m_ProjectGeneration.GenerateAll(generateAll);
    }

    public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles,
      string[] importedFiles)
    {
      m_ProjectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles),
        importedFiles);
    }

    public void SyncAll()
    {
      AssetDatabase.Refresh();
      m_ProjectGeneration.Sync();
    }

    public void Initialize(string editorInstallationPath)
    {
    }

    public bool OpenProject(string path, int line, int column)
    {
      var fastOpenResult = EditorPluginInterop.OpenFileDllImplementation(path, line, column);

      if (fastOpenResult)
        return true;
      
      if (IsOSX)
      {
        return OpenOSXApp(path, line, column);
      }

      var solution = GetSolutionFile(path); // TODO: If solution file doesn't exist resync.
      solution = solution == "" ? "" : $"\"{solution}\"";
      var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = CodeEditor.CurrentEditorInstallation,
          Arguments = $"{solution} -l {line} \"{path}\"",
          UseShellExecute = true,
        }
      };

      process.Start();

      return true;
    }

    private bool OpenOSXApp(string path, int line, int column)
    {
      var solution = GetSolutionFile(path); // TODO: If solution file doesn't exist resync.
      solution = solution == "" ? "" : $"\"{solution}\"";
      var pathArguments = path == "" ? "" : $"-l {line} \"{path}\"";
      var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = "open",
          Arguments = $"\"{CodeEditor.CurrentEditorInstallation}\" --args {solution} {pathArguments}",
          CreateNoWindow = true,
          UseShellExecute = true,
        }
      };

      process.Start();

      return true;
    }

    private string GetSolutionFile(string path)
    {
      if (UnityEditor.Unsupported.IsDeveloperBuild())
      {
        var baseFolder = GetBaseUnityDeveloperFolder();
        var lowerPath = path.ToLowerInvariant();

        bool isUnitySourceCode = lowerPath.Contains((baseFolder + "/Runtime").ToLowerInvariant());

        if (lowerPath.Contains((baseFolder + "/Editor").ToLowerInvariant()))
        {
          isUnitySourceCode = true;
        }

        if (isUnitySourceCode)
        {
          return Path.Combine(baseFolder, "Projects/CSharp/Unity.CSharpProjects.gen.sln");
        }
      }

      var solutionFile = m_ProjectGeneration.SolutionFile();
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

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
      if (IsRiderInstallation(editorPath))
      {
        try
        {
          installation = Installations.First(inst => inst.Path == editorPath);
        }
        catch (InvalidOperationException)
        {
          installation = new CodeEditor.Installation {Name = editorPath, Path = editorPath};
        }

        return true;
      }

      installation = default;
      return false;
    }

    public static bool IsRiderInstallation(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return false;
      }
      
      var fileInfo = new FileInfo(path);
      var filename = fileInfo.Name.ToLower();
      return filename.StartsWith("rider");
    }

    public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

    public void CreateIfDoesntExist()
    {
      if (!m_ProjectGeneration.HasSolutionBeenGenerated())
      {
        m_ProjectGeneration.Sync();
      }
    }
  }
}