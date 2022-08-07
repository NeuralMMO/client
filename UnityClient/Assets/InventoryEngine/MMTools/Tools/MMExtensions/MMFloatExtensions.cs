using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Float extensions
    /// </summary>
    public static class MMFloatExtensions
    {
        /// <summary>
        /// Normalizes an angle in degrees
        /// </summary>
        /// <param name="angleInDegrees"></param>
        /// <returns></returns>
        public static float MMNormalizeAngle(this float angleInDegrees)
        {
            angleInDegrees = angleInDegrees % 360f;
            if (angleInDegrees < 0)
            {
                angleInDegrees += 360f;
            }
            return angleInDegrees;
        }

        /// <summary>
        /// Rounds a float down
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static float RoundDown(this float number, int decimalPlaces)
        {
            return Mathf.Floor(number * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        }
    }
}
