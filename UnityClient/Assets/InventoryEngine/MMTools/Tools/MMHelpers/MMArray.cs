using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Array helpers
    /// </summary>
    public class MMArray : MonoBehaviour
    {
        /// <summary>
        /// Rounds an int to the closest int in an array (array has to be sorted)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int RoundIntToArray(int value, int[] array)
        {
            int min = 0;
            if (array[min] >= value) return array[min];

            int max = array.Length - 1;
            if (array[max] <= value) return array[max];

            while (max - min > 1)
            {
                int mid = (max + min) / 2;

                if (array[mid] == value)
                {
                    return array[mid];
                }
                else if (array[mid] < value)
                {
                    min = mid;
                }
                else
                {
                    max = mid;
                }
            }

            if (array[max] - value <= value - array[min])
            {
                return array[max];
            }
            else
            {
                return array[min];
            }
        }

        /// <summary>
        /// Rounds a float to the closest float in an array (array has to be sorted)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static float RoundFloatToArray(float value, float[] array)
        {
            int min = 0;
            if (array[min] >= value) return array[min];

            int max = array.Length - 1;
            if (array[max] <= value) return array[max];

            while (max - min > 1)
            {
                int mid = (max + min) / 2;

                if (array[mid] == value)
                {
                    return array[mid];
                }
                else if (array[mid] < value)
                {
                    min = mid;
                }
                else
                {
                    max = mid;
                }
            }

            if (array[max] - value <= value - array[min])
            {
                return array[max];
            }
            else
            {
                return array[min];
            }

        }
    }
}
