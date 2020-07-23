using Unity.Properties;
using Unity.Properties.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    delegate string ResolveEntityNameCallback(Entity entity);

    delegate void SelectEntityButtonCallback(Entity entity);

    partial class IMGUIAdapter :
        IVisit<Entity>
    {
        readonly SelectEntityButtonCallback m_selectButtonCallback;
        readonly ResolveEntityNameCallback m_resolveNameCallback;

        public IMGUIAdapter(SelectEntityButtonCallback selectButtonCallback, ResolveEntityNameCallback resolveEntityNameCallback)
        {
            m_selectButtonCallback = selectButtonCallback;
            m_resolveNameCallback = resolveEntityNameCallback;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, Entity> property, ref TContainer container, ref Entity value)
        {
            var enabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GetDisplayName(property), GUILayout.Width(EditorGUIUtility.labelWidth - 1));

            var name = value == Entity.Null ? "Entity.Null" : $"[{value.Index}:{value.Version}] {m_resolveNameCallback(value)}";

            GUI.enabled = true;

            if (GUILayout.Button(name, "ObjectField"))
            {
                if (value != Entity.Null)
                    m_selectButtonCallback?.Invoke(value);
            }

            GUI.enabled = enabled;

            EditorGUILayout.EndHorizontal();
            return VisitStatus.Stop;
        }
    }
}
