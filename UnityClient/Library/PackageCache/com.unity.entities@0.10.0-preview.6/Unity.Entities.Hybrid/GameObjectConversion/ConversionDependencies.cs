using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Entities.Conversion
{
    class ConversionDependencies : IDisposable
    {
        internal UnsafeHashMap<int, int> GameObjectIndexByInstanceId = new UnsafeHashMap<int, int>(0, Allocator.Persistent);
        internal List<GameObject> DependentGameObjects = new List<GameObject>();

        internal UnsafeMultiHashMap<int, int> GameObjectDependentsByInstanceId;
        internal UnsafeMultiHashMap<int, int> AssetDependentsByInstanceId = new UnsafeMultiHashMap<int, int>(0, Allocator.Persistent);
        readonly bool m_IsLiveLink;

        public ConversionDependencies(bool isLiveLink)
        {
            m_IsLiveLink = isLiveLink;
            if (m_IsLiveLink)
            {
                GameObjectDependentsByInstanceId = new UnsafeMultiHashMap<int, int>(0, Allocator.Persistent);
            }
        }

        int RegisterDependentGameObject(GameObject dependent)
        {
            int index = DependentGameObjects.Count;
            var instanceId = dependent.GetInstanceID();
            if (GameObjectIndexByInstanceId.TryAdd(instanceId, index))
                DependentGameObjects.Add(dependent);
            else
                index = GameObjectIndexByInstanceId[instanceId];
            return index;
        }

        public void DependOnGameObject(GameObject dependent, GameObject dependsOn)
        {
            if (!m_IsLiveLink)
            {
                // this dependency only needs to be tracked when using LiveLink, since otherwise subscenes are converted
                // as a whole.
                return;
            }

            if (dependsOn == null)
                throw new ArgumentNullException(nameof(dependsOn));
            if (dependent == null)
                throw new ArgumentNullException(nameof(dependent));
            int index = RegisterDependentGameObject(dependent);
            GameObjectDependentsByInstanceId.Add(dependsOn.GetInstanceID(), index);
        }

        public void DependOnAsset(GameObject dependent, Object dependsOn)
        {
            if (dependent == null)
                throw new ArgumentNullException(nameof(dependent));
            if (dependsOn == null)
                throw new ArgumentNullException(nameof(dependsOn));
            if (!dependsOn.IsAsset() && !dependsOn.IsPrefab())
                throw new ArgumentException($"The target object {dependsOn.name} is not an asset.", nameof(dependsOn));
            int index = RegisterDependentGameObject(dependent);
            AssetDependentsByInstanceId.Add(dependsOn.GetInstanceID(), index);
        }

        void CalculateDependents(IEnumerable<GameObject> gameObjects,  HashSet<GameObject> dependents)
        {
            var toBeProcessed = new Stack<GameObject>(gameObjects);
            while (toBeProcessed.Count != 0)
            {
                var go = toBeProcessed.Pop();

                if (dependents.Add(go))
                {
                    var indices = GameObjectDependentsByInstanceId.GetValuesForKey(go.GetInstanceID());
                    foreach (var index in indices)
                    {
                        var dependentGO = DependentGameObjects[index];
                        if (!dependents.Contains(dependentGO))
                            toBeProcessed.Push(dependentGO);
                    }
                }
            }
        }

        public HashSet<GameObject> CalculateDependents(IEnumerable<GameObject> gameObjects)
        {
            var dependents = new HashSet<GameObject>();
            CalculateDependents(gameObjects, dependents);
            return dependents;
        }

        public void Dispose()
        {
            if (GameObjectDependentsByInstanceId.IsCreated)
                GameObjectDependentsByInstanceId.Dispose();
            GameObjectIndexByInstanceId.Dispose();
            AssetDependentsByInstanceId.Dispose();
        }
    }
}
