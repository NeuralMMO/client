using System;
using System.IO;
using System.Linq;
using Unity.Properties;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// Provides the necessary implementation to use properties and serialization with a <see cref="ScriptableObject"/> of type <typeparamref name="TContainer"/>.
    /// </summary>
    /// <typeparam name="TContainer">The type of the container.</typeparam>
    [Serializable]
    public abstract class ScriptableObjectPropertyContainer<TContainer> : ScriptableObject, ISerializationCallbackReceiver
        where TContainer : ScriptableObjectPropertyContainer<TContainer>
    {
        [SerializeField, DontCreateProperty] string m_AssetContent;

        //@TODO: replace with deserialization context object when its available
        internal static string CurrentDeserializationAssetPath { get; private set; }
        internal static TContainer CurrentDeserializationAsset { get; private set; }

        /// <summary>
        /// Reset this asset in preparation for deserialization.
        /// </summary>
        protected virtual void Reset()
        {
            m_AssetContent = null;
        }

        /// <summary>
        /// Sanitize this asset after deserialization.
        /// </summary>
        protected virtual void Sanitize() { }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="mutator">Optional mutator that can be used to modify the asset.</param>
        /// <returns>The new asset instance.</returns>
        public static TContainer CreateInstance(Action<TContainer> mutator = null)
        {
            var instance = CreateInstance<TContainer>();
            mutator?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Create a new asset instance saved to disk.
        /// </summary>
        /// <param name="assetPath">The location where to create the asset.</param>
        /// <param name="mutator">Optional mutator that can be used to modify the asset.</param>
        /// <returns>The new asset instance.</returns>
        public static TContainer CreateAsset(string assetPath, Action<TContainer> mutator = null)
        {
            var asset = CreateInstance(mutator);
            if (asset != null && asset)
            {
                asset.SerializeToPath(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                return AssetDatabase.LoadAssetAtPath<TContainer>(assetPath);
            }
            return null;
        }

        /// <summary>
        /// Create a new asset instance saved to disk, in the active directory.
        /// </summary>
        /// <param name="assetName">The asset file name with extension.</param>
        /// <param name="mutator">Optional mutator that can be used to modify the asset.</param>
        /// <returns>The new asset instance.</returns>
        public static TContainer CreateAssetInActiveDirectory(string assetName, Action<TContainer> mutator = null)
        {
            string path = null;
            if (Selection.activeObject != null)
            {
                var activeObjectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(activeObjectPath))
                {
                    if (Directory.Exists(activeObjectPath))
                    {
                        path = Path.Combine(activeObjectPath, assetName);
                    }
                    else
                    {
                        path = Path.Combine(Path.GetDirectoryName(activeObjectPath), assetName);
                    }
                }
            }
            return CreateAsset(AssetDatabase.GenerateUniqueAssetPath(path), mutator);
        }

        /// <summary>
        /// Load an asset from the specified asset path.
        /// </summary>
        /// <param name="assetPath">The asset path to load from.</param>
        /// <returns>The loaded asset if successful, <see langword="null"/> otherwise.</returns>
        public static TContainer LoadAsset(string assetPath) => AssetDatabase.LoadAssetAtPath<TContainer>(assetPath);

        /// <summary>
        /// Load an asset from the specified asset <see cref="GUID"/>.
        /// </summary>
        /// <param name="assetGuid">The asset <see cref="GUID"/> to load from.</param>
        /// <returns>The loaded asset if successful, <see langword="null"/> otherwise.</returns>
        public static TContainer LoadAsset(GUID assetGuid) => LoadAsset(AssetDatabase.GUIDToAssetPath(assetGuid.ToString()));

        /// <summary>
        /// Save this asset to disk.
        /// If no asset path is provided, asset is saved at its original location.
        /// </summary>
        /// <param name="assetPath">Optional file path where to save the asset.</param>
        /// <returns><see langword="true"/> if the operation is successful, <see langword="false"/> otherwise.</returns>
        public bool SaveAsset(string assetPath = null)
        {
            assetPath = assetPath ?? AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            try
            {
                SerializeToPath(assetPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize {this.ToHyperLink()} to '{assetPath}'.\n{e.Message}");
                return false;
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return true;
        }

        /// <summary>
        /// Restore this asset from disk.
        /// </summary>
        /// <returns><see langword="true"/> if the operation is successful, <see langword="false"/> otherwise.</returns>
        public bool RestoreAsset()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }
            return DeserializeFromPath(this as TContainer, assetPath);
        }

        /// <summary>
        /// Determine if there is unsaved modifications.
        /// </summary>
        /// <returns><see langword="true"/> if there is unsaved modifications, <see langword="false"/> otherwise.</returns>
        public bool IsModified()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }
            return SerializeToJson() != File.ReadAllText(assetPath);
        }

        /// <summary>
        /// Serialize this container to a JSON <see cref="string"/>.
        /// </summary>
        /// <returns>The container as a JSON <see cref="string"/> if the serialization is successful, <see langword="null"/> otherwise.</returns>
        public string SerializeToJson()
        {
            return JsonSerialization.ToJson(this, new JsonSerializationParameters
            {
                DisableRootAdapters = true,
                SerializedType = typeof(TContainer)
            });
        }

        /// <summary>
        /// Deserialize from a JSON <see cref="string"/> into the container.
        /// </summary>
        /// <param name="container">The container to deserialize into.</param>
        /// <param name="json">The JSON string to deserialize from.</param>
        /// <returns><see langword="true"/> if the operation is successful, <see langword="false"/> otherwise.</returns>
        public static bool DeserializeFromJson(TContainer container, string json)
        {
            return TryDeserialize(container, json, null);
        }

        /// <summary>
        /// Serialize this container to a file.
        /// </summary>
        /// <param name="path">The file path to write into.</param>
        public void SerializeToPath(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, SerializeToJson());
        }

        /// <summary>
        /// Deserialize from a file into the container.
        /// </summary>
        /// <param name="container">The container to deserialize into.</param>
        /// <param name="path">The file path to deserialize from.</param>
        /// <returns><see langword="true"/> if the operation is successful, <see langword="false"/> otherwise.</returns>
        public static bool DeserializeFromPath(TContainer container, string path)
        {
            return TryDeserialize(container, File.ReadAllText(path), path);
        }

        public void OnBeforeSerialize()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
            {
                m_AssetContent = File.ReadAllText(assetPath);
            }
            else
            {
                m_AssetContent = SerializeToJson();
            }
        }

        public void OnAfterDeserialize()
        {
            // Can't deserialize here, throws: "CreateJobReflectionData is not allowed to be called during serialization, call it from OnEnable instead."
        }

        void OnEnable()
        {
            var container = this as TContainer;
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
            {
                DeserializeFromPath(container, assetPath);
            }
            else if (!string.IsNullOrEmpty(m_AssetContent))
            {
                DeserializeFromJson(container, m_AssetContent);
            }
        }

        static bool TryDeserialize(TContainer container, string json, string assetPath)
        {
            CurrentDeserializationAsset = container;
            CurrentDeserializationAssetPath = assetPath;
            try
            {
                container.Reset();

                container.m_AssetContent = json;
                JsonSerialization.TryFromJsonOverride(json, ref container, out var result, new JsonSerializationParameters
                {
                    DisableRootAdapters = true,
                    SerializedType = typeof(TContainer)
                });

                if (!result.DidSucceed())
                {
                    var errors = result.Errors.Select(e => e.ToString());
#if UNITY_2020_1_OR_NEWER
                    // nothing to do here
#else
                    errors = errors.Where(e => !e.Contains("Asset is not yet loaded and will result in a null reference"));
#endif
                    if (errors.Count() > 0)
                    {
                        LogDeserializeError(string.Join("\n", errors), container);
                    }
                }

                container.Sanitize();
                return true;
            }
            catch (Exception e)
            {
                LogDeserializeError(e.Message, container);
                container.Sanitize();
                return false;
            }
            finally
            {
                CurrentDeserializationAsset = null;
                CurrentDeserializationAssetPath = null;
            }
        }

        static void LogDeserializeError(string message, TContainer container)
        {
            var what = !string.IsNullOrEmpty(CurrentDeserializationAssetPath) ?
                CurrentDeserializationAssetPath.ToHyperLink() :
                $"memory container of type '{container.GetType().FullName}'";
            Debug.LogError($"Failed to deserialize {what}:\n{message}");
        }
    }
}
