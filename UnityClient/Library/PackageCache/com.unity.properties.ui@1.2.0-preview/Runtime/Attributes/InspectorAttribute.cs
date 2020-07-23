using System;
using UnityEngine;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Base class to derive property attributes that can work on fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class InspectorAttribute : PropertyAttribute
    {
    }
}