#if UNITY_EDITOR
using System;
using Unity.Entities;
using UnityEditor;
using UnityEditor.Experimental;

namespace Unity.Scenes
{
    enum ImportMode
    {
        Synchronous,
        Asynchronous,
        NoImport
    }

    static class AssetDatabaseCompatibility
    {
        internal static Hash128 GetArtifactHash(string guid, Type importerType, ImportMode mode)
        {
            switch (mode)
            {
#if UNITY_2020_2_OR_NEWER
                case ImportMode.Asynchronous:
                    return AssetDatabaseExperimental.ProduceArtifactAsync(new ArtifactKey(new GUID(guid), importerType)).value;
                case ImportMode.Synchronous:
                    return AssetDatabaseExperimental.ProduceArtifact(new ArtifactKey(new GUID(guid), importerType)).value;
                case ImportMode.NoImport:
                    return AssetDatabaseExperimental.LookupArtifact(new ArtifactKey(new GUID(guid), importerType)).value;
#else
                case ImportMode.Asynchronous:
                    return AssetDatabaseExperimental.GetArtifactHash(guid, importerType, AssetDatabaseExperimental.ImportSyncMode.Queue);
                case ImportMode.Synchronous:
                    return AssetDatabaseExperimental.GetArtifactHash(guid, importerType);
                case ImportMode.NoImport:
                    return AssetDatabaseExperimental.GetArtifactHash(guid, importerType, AssetDatabaseExperimental.ImportSyncMode.Poll);
#endif
            }

            return default;
        }

        internal static bool GetArtifactPaths(Hash128 artifactHash, out string[] paths)
        {
#if UNITY_2020_2_OR_NEWER
            return AssetDatabaseExperimental.GetArtifactPaths(new ArtifactID
            {
                value = artifactHash
            }, out paths);
#else
            return AssetDatabaseExperimental.GetArtifactPaths(artifactHash, out paths);
#endif
        }
    }
}
#endif
