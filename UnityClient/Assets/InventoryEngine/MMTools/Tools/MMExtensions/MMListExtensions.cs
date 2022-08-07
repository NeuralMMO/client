using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// List extensions
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Swaps two items in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void MMSwap<T>(this IList<T> list, int i, int j)
        {
            var temporary = list[i];
            list[i] = list[j];
            list[j] = temporary;
        }

        /// <summary>
        /// Shuffles a list randomly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void MMShuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.MMSwap(i, Random.Range(i, list.Count));
            }                
        }
    }
}

