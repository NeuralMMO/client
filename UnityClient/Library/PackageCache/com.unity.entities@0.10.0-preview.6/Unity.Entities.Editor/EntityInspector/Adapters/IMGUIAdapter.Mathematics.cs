using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Properties;
using Unity.Properties.Adapters;
using UnityEditor;

namespace Unity.Entities.Editor
{
    partial class IMGUIAdapter : IVisit<Hash128>
        , IVisit<quaternion>
        , IVisit<float2>
        , IVisit<float3>
        , IVisit<float4>
        , IVisit<float2x2>
        , IVisit<float3x3>
        , IVisit<float4x4>
    {
        public VisitStatus Visit<TContainer>(Property<TContainer, Hash128> property, ref TContainer container, ref Hash128 value)
        {
            EditorGUILayout.TextField(GetDisplayName(property), value.ToString());
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, quaternion> property, ref TContainer container, ref quaternion value)
        {
            value = new quaternion(EditorGUILayout.Vector4Field(GetDisplayName(property), value.value));
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float2> property, ref TContainer container, ref float2 value)
        {
            value = EditorGUILayout.Vector2Field(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float3> property, ref TContainer container, ref float3 value)
        {
            value = EditorGUILayout.Vector3Field(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float4> property, ref TContainer container, ref float4 value)
        {
            value = EditorGUILayout.Vector4Field(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float2x2> property, ref TContainer container, ref float2x2 value)
        {
            value.c0 = EditorGUILayout.Vector2Field(GetDisplayName(property), value.c0);
            value.c1 = EditorGUILayout.Vector2Field(" ", value.c1);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float3x3> property, ref TContainer container, ref float3x3 value)
        {
            value.c0 = EditorGUILayout.Vector3Field(GetDisplayName(property), value.c0);
            value.c1 = EditorGUILayout.Vector3Field(" ", value.c1);
            value.c2 = EditorGUILayout.Vector3Field(" ", value.c2);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float4x4> property, ref TContainer container, ref float4x4 value)
        {
            value.c0 = EditorGUILayout.Vector4Field(GetDisplayName(property), value.c0);
            value.c1 = EditorGUILayout.Vector4Field(" ", value.c1);
            value.c2 = EditorGUILayout.Vector4Field(" ", value.c2);
            value.c3 = EditorGUILayout.Vector4Field(" ", value.c3);
            return VisitStatus.Stop;
        }
    }
}
