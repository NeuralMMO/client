using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Vector2 extensions
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Rotates a vector2 by angleInDegrees
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angleInDegrees"></param>
        /// <returns></returns>
        public static Vector2 MMRotate(this Vector2 vector, float angleInDegrees)
        {
            float sin = Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
            float tx = vector.x;
            float ty = vector.y;
            vector.x = (cos * tx) - (sin * ty);
            vector.y = (sin * tx) + (cos * ty);
            return vector;
        }

        /// <summary>
        /// Sets the X part of a Vector2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static Vector2 MMSetX(this Vector2 vector, float newValue)
        {
            vector.x = newValue;
            return vector;
        }

        /// <summary>
        /// Sets the Y part of a Vector2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static Vector2 MMSetY(this Vector2 vector, float newValue)
        {
            vector.y = newValue;
            return vector;
        }
    }
}
