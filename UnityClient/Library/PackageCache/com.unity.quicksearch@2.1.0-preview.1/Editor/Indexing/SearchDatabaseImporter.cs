using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Unity.QuickSearch
{
    [ExcludeFromPreset, ScriptedImporter(version: SearchDatabase.version, ext: "index", importQueueOffset: 1999)] // kImportOrderPrefabs = 1500
    class SearchDatabaseImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            try
            {
                var db = ScriptableObject.CreateInstance<SearchDatabase>();
                db.Import(ctx.assetPath);
                ctx.AddObjectToAsset("index", db);
                ctx.SetMainObject(db);

                #if UNITY_2020_1_OR_NEWER
                ctx.DependsOnCustomDependency(nameof(CustomObjectIndexerAttribute));
                #endif

                hideFlags |= HideFlags.HideInInspector;
            }
            catch (SearchDatabaseException ex)
            {
                ctx.LogImportError(ex.Message, AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ex.guid)));
            }
        }

        public static string CreateTemplateIndex(string templateFilename, string path, string name = null)
        {
            var templatePath = $"{Utils.packageFolderName}/Templates/{templateFilename}.index.template";

            if (!File.Exists(templatePath))
                return null;

            var dirPath = path;
            var templateContent = File.ReadAllText(templatePath);

            if (File.Exists(path))
            {
                dirPath = Path.GetDirectoryName(path);
                if (Selection.assetGUIDs.Length > 1)
                    path = dirPath;
            }

            var indexFileName = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name;
            var indexPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dirPath, $"{indexFileName}.index")).Replace("\\", "/");

            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                $"Creating {templateFilename} index at <a file=\"{indexPath}\">{indexPath}</a>");

            File.WriteAllText(indexPath, templateContent);
            AssetDatabase.ImportAsset(indexPath);

            return indexPath;
        }

        private static bool ValidateTemplateIndexCreation<T>() where T : UnityEngine.Object
        {
            var asset = Selection.activeObject as T;
            if (asset)
                return true;
            return CreateIndexProjectValidation();
        }

        [MenuItem("Assets/Create/Quick Search/Project Index")]
        internal static void CreateIndexProject()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Assets", folderPath);
        }

        [MenuItem("Assets/Create/Quick Search/Project Index", validate = true)]
        internal static bool CreateIndexProjectValidation()
        {
            var folder = Selection.activeObject as DefaultAsset;
            if (!folder)
                return false;
            return Directory.Exists(AssetDatabase.GetAssetPath(folder));
        }

        [MenuItem("Assets/Create/Quick Search/Prefab Index")]
        internal static void CreateIndexPrefab()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Prefabs", assetPath);
        }

        [MenuItem("Assets/Create/Quick Search/Prefab Index", validate = true)]
        internal static bool CreateIndexPrefabValidation()
        {
            return ValidateTemplateIndexCreation<GameObject>();
        }

        [MenuItem("Assets/Create/Quick Search/Scene Index")]
        internal static void CreateIndexScene()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Scenes", assetPath);
        }

        [MenuItem("Assets/Create/Quick Search/Scene Index", validate = true)]
        internal static bool CreateIndexSceneValidation()
        {
            return ValidateTemplateIndexCreation<SceneAsset>();
        }
    }
}
