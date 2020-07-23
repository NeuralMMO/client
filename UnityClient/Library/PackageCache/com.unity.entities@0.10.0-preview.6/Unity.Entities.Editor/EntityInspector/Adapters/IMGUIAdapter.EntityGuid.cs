using Unity.Properties;
using Unity.Properties.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    partial class IMGUIAdapter : IVisit<EntityGuid>
    {
        public VisitStatus Visit<TContainer>(Property<TContainer, EntityGuid> property, ref TContainer container, ref EntityGuid value)
        {
            var enabled = GUI.enabled;
            GUI.enabled = true;
            EditorGUILayout.LabelField(GetDisplayName(property), new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold });
            GUI.enabled = enabled;
            EditorGUI.indentLevel++;
            EditorGUILayout.TextField("Originating Id", value.OriginatingId.ToString());
            EditorGUILayout.TextField("Namespace Id", value.NamespaceId.ToString());
            EditorGUILayout.TextField("Serial", value.Serial.ToString());
            EditorGUI.indentLevel--;
            return VisitStatus.Stop;
        }
    }
}
