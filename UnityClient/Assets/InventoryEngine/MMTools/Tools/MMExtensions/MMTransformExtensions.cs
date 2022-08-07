using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Transform extensions
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Destroys a transform's children
        /// </summary>
        /// <param name="transform"></param>
        public static void MMDestroyAllChildren(this Transform transform)
        {
            for (int t = transform.childCount - 1; t >= 0; t--)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(transform.GetChild(t).gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(transform.GetChild(t).gameObject);
                }
            }
        }

        /// <summary>
        /// Finds children by name, breadth first
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform MMFindDeepChildBreadthFirst(this Transform parent, string transformName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (child.name == transformName)
                {
                    return child;
                }
                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }
            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform MMFindDeepChildDepthFirst(this Transform parent, string transformName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == transformName)
                {
                    return child;
                }

                var result = child.MMFindDeepChildDepthFirst(transformName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
