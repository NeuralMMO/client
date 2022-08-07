using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Renderer extensions
    /// </summary>
    public static class RendererExtensions
    {
        /// <summary>
        /// Returns true if a renderer is visible from a camera
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool MMIsVisibleFrom(this Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
    }
}
