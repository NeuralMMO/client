using System;
using UnityEngine;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Tag a <see cref="Vector2"/> or a <see cref="Vector2Int"/> field to draw it as a min-max slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MinMaxAttribute : InspectorAttribute
    {
        /// <summary>
        /// Low-limit value.
        /// </summary>
        public readonly float Min;
        
        /// <summary>
        /// High-limit value.
        /// </summary>
        public readonly float Max;

        /// <summary>
        /// Constructs a new <see cref="MinMaxAttribute"/> with specified min and max values. 
        /// </summary>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}