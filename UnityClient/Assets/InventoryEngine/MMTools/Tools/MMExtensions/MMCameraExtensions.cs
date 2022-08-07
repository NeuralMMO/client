using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Camera extensions
    /// </summary>
    public static class MMCameraExtensions
    {
        /// <summary>
        /// Returns the width of the camera in world space units, at the specified depths for perspective cameras, everywhere for orthographic ones
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static float MMCameraWorldSpaceWidth(this Camera camera, float depth = 0f)
        {
            if (camera.orthographic)
            {
                return camera.aspect * camera.orthographicSize * 2f;
            }
            else
            {
                float fieldOfView = camera.fieldOfView * Mathf.Deg2Rad;
                return camera.aspect * depth * Mathf.Tan(fieldOfView);
            }
        }

        /// <summary>
        /// Returns the height of the camera in world space units, at the specified depths for perspective cameras, everywhere for orthographic ones
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static float MMCameraWorldSpaceHeight(this Camera camera, float depth = 0f)
        {
            if (camera.orthographic)
            {
                return camera.orthographicSize * 2f;
            }
            else
            {
                float fieldOfView = camera.fieldOfView * Mathf.Deg2Rad;
                return depth * Mathf.Tan(fieldOfView);
            }
        }
    }
}
