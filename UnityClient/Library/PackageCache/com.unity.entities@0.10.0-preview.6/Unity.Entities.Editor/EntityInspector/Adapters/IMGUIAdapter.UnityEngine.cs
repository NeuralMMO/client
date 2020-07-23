using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    partial class IMGUIAdapter : Properties.Adapters.Contravariant.IVisit<UnityEngine.Object>
    {
        public VisitStatus Visit<TContainer>(IProperty<TContainer> property, ref TContainer container, Object value)
        {
            var type = value ? value.GetType() : typeof(Object);
            value = EditorGUILayout.ObjectField(property.Name, value, type, true);
            return VisitStatus.Stop;
        }
    }
}
