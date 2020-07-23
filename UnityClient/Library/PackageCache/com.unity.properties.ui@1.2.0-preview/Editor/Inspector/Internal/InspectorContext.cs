using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Context of the inspector that give access to the data.
    /// </summary>
    /// <typeparam name="T">The type of the value being inspected.</typeparam>
    readonly struct InspectorContext<T>
    {
        public readonly PropertyElement Root;
        public readonly PropertyPath BasePath;
        public readonly PropertyPath PropertyPath;
        public readonly PropertyPath.Part Part;

        public readonly string Name;
        public readonly string DisplayName;
        public readonly string Tooltip;

        public readonly bool IsDelayed;
 
        public List<Attribute> Attributes { get; }

        public InspectorContext(
            PropertyElement root,
            PropertyPath propertyPath,
            IProperty property,
            IEnumerable<Attribute> attributes = null
        ){
            Root = root;
            PropertyPath = propertyPath;
            BasePath = new PropertyPath();
            BasePath.PushPath(PropertyPath);
            if (BasePath.PartsCount > 0)
                BasePath.Pop();
            
            Name = property.Name;
            Part = PropertyPath.PartsCount> 0 ? PropertyPath[PropertyPath.PartsCount - 1] : default;
            var attributeList = new List<Attribute>(attributes ?? property.GetAttributes());
            Attributes = attributeList;
            Tooltip =  property.GetAttribute<TooltipAttribute>()?.tooltip;
            DisplayName = GuiFactory.GetDisplayName(property);
            IsDelayed = property.HasAttribute<DelayedAttribute>();
        }

        /// <summary>
        /// Accessor for the data.
        /// </summary>
        public T Data
        {
            get => GetData();
            set => SetData(value);
        }

        T GetData()
        {
            if (PropertyPath.PartsCount == 0)
            {
                return Root.GetTarget<T>();
            }

            if (Root.TryGetValue<T>(PropertyPath, out var value))
            {
                return value;
            }
            throw new InvalidOperationException();
        }

        void SetData(T value)
        {
            if (PropertyPath.PartsCount == 0)
            {
                Root.SetTarget(value);
            }
            else
            {
                Root.SetValue(PropertyPath, value);
            }
            Root.NotifyChanged(PropertyPath);
        }
    }
}