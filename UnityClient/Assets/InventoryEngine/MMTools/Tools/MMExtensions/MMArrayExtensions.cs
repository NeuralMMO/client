using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Array extensions
    /// </summary>
    public static class MMArrayExtensions
    {
        /// <summary>
        /// Returns a random value inside the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T MMRandomValue<T>(this T[] array)
        {
            int newIndex = Random.Range(0, array.Length);
            return array[newIndex];
        }        
    }
}
