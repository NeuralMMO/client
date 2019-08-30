using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Packages.Rider.Editor
{
  public static class EditorPluginInterop
  {
    private static string ourEntryPointTypeName = "JetBrains.Rider.Unity.Editor.PluginEntryPoint";

    public static string LogPath
    {
      get
      {
        try
        {
          var assembly = GetEditorPluginAssembly();
          if (assembly == null) return null;
          var type = assembly.GetType(ourEntryPointTypeName);
          if (type == null) return null;
          var field = type.GetField("LogPath", BindingFlags.NonPublic | BindingFlags.Static);
          if (field == null) return null;
          return field.GetValue(null) as string;
        }
        catch (Exception)
        {
          Debug.Log("Unable to do OpenFile to Rider from dll, fallback to com.unity.ide.rider implementation.");
        }

        return null;
      }
    }

    public static bool OpenFileDllImplementation(string path, int line, int column)
    {
      var openResult = false;
      // reflection for fast OpenFileLineCol, when Rider is started and protocol connection is established
      try
      {
        var assembly = GetEditorPluginAssembly();
        if (assembly == null) return false;
        var type = assembly.GetType(ourEntryPointTypeName);
        if (type == null) return false;
        var field = type.GetField("OpenAssetHandler", BindingFlags.NonPublic | BindingFlags.Static);
        if (field == null) return false;
        var handlerInstance = field.GetValue(null);
        var method = handlerInstance.GetType()
          .GetMethod("OnOpenedAsset", new[] {typeof(string), typeof(int), typeof(int)});
        if (method == null) return false;
        var assetFilePath = Path.GetFullPath(path);
        
        openResult = (bool) method.Invoke(handlerInstance, new object[] {assetFilePath, line, column});
      }
      catch (Exception)
      {
        Debug.Log("Unable to do OpenFile to Rider from dll, fallback to com.unity.ide.rider implementation.");
      }

      return openResult;
    }

    public static Assembly GetEditorPluginAssembly()
    {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      var assembly = assemblies.FirstOrDefault(a => a.GetName().Name.Equals("JetBrains.Rider.Unity.Editor.Plugin.Full.Repacked"));
      return assembly;
    }

    public static bool EditorPluginIsLoadedFromAssets()
    {
      var currentDir = Directory.GetCurrentDirectory();
      var assembly = GetEditorPluginAssembly();
      if (assembly == null)
        return false;
      var location = assembly.Location;
      return location.StartsWith(currentDir, StringComparison.InvariantCultureIgnoreCase);
    }


    internal static void InitEntryPoint()
    {
      try
      {
        var type = GetEditorPluginAssembly().GetType("JetBrains.Rider.Unity.Editor.AfterUnity56.EntryPoint");
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
      }
      catch (TypeInitializationException ex)
      {
        Debug.LogException(ex.InnerException);
      }
    }
  }
}