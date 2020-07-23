using System;
using Unity.Properties;
using UnityEditor;

namespace Unity.Entities.Editor
{
    partial class IMGUIAdapter : Properties.Adapters.IVisit
    {
        public VisitStatus Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            if (typeof(TValue).IsEnum)
            {
                var options = Enum.GetNames(typeof(TValue));
                var local = value;

                var index = Array.FindIndex(options, name => name == local.ToString());

                EditorGUILayout.Popup
                    (
                        typeof(TValue).Name,
                        index,
                        options
                    );

                return VisitStatus.Handled;
            }

            if (null == value)
            {
                EditorGUILayout.LabelField(GetDisplayName(property), "null");
                return VisitStatus.Stop;
            }

            if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(BlobAssetReference<>))
            {
                return VisitStatus.Stop;
            }

            return VisitStatus.Unhandled;
        }

        internal static string GetDisplayName(IProperty property)
        {
            switch (property)
            {
                case IListElementProperty listElementProperty:
                    return $"Element {listElementProperty.Index}";
                case IDictionaryElementProperty dictionaryElementProperty:
                    return $"Element {dictionaryElementProperty.ObjectKey}";
                default:
                    return property.Name;
            }
        }
    }
}
