using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities.Conversion;
using UnityEngine;

namespace Unity.Entities.Hybrid.Internal
{
    /// <summary>
    /// These methods are only used by CodeGen. Treat them as implementation details.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GeneratedAuthoringComponentImplementation
    {
        public static void AddReferencedPrefab(List<GameObject> referencedPrefabs, GameObject gameObject)
        {
            if (gameObject != null && gameObject.IsPrefab())
                referencedPrefabs.Add(gameObject);
        }

        public static void AddReferencedPrefabs(List<GameObject> referencedPrefabs, IEnumerable<GameObject> gameObjects)
        {
            foreach (var obj in gameObjects)
                AddReferencedPrefab(referencedPrefabs, obj);
        }
    }
}
