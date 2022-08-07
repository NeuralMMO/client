using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Layermask Extensions
    /// </summary>
    public static class LayermaskExtensions
    {
        /// <summary>
        /// Returns bool if layer is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool MMContains(this LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) > 0);
        }

        /// <summary>
        /// Returns true if gameObject is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        public static bool MMContains(this LayerMask mask, GameObject gameobject)
        {
            return ((mask.value & (1 << gameobject.layer)) > 0);
        }
    }
}
