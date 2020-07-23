using System.Linq;
using Unity.Properties;
using Unity.Properties.Adapters;
using UnityEditor;

namespace Unity.Entities.Editor
{
    partial class IMGUIAdapter : IVisitPrimitives, IVisit<string>
    {
        public VisitStatus Visit<TContainer>(Property<TContainer, sbyte> property, ref TContainer container, ref sbyte value)
        {
            value = (sbyte)EditorGUILayout.IntField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, short> property, ref TContainer container, ref short value)
        {
            value = (short)EditorGUILayout.IntField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, int> property, ref TContainer container, ref int value)
        {
            value = EditorGUILayout.IntField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, long> property, ref TContainer container, ref long value)
        {
            value = EditorGUILayout.LongField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, byte> property, ref TContainer container, ref byte value)
        {
            value = (byte)EditorGUILayout.IntField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, ushort> property, ref TContainer container, ref ushort value)
        {
            value = (ushort)EditorGUILayout.IntField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, uint> property, ref TContainer container, ref uint value)
        {
            value = (uint)EditorGUILayout.LongField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, ulong> property, ref TContainer container, ref ulong value)
        {
            EditorGUILayout.TextField(GetDisplayName(property), text: value.ToString());
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, float> property, ref TContainer container, ref float value)
        {
            value = EditorGUILayout.FloatField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, double> property, ref TContainer container, ref double value)
        {
            value = EditorGUILayout.DoubleField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, bool> property, ref TContainer container, ref bool value)
        {
            value = EditorGUILayout.Toggle(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, char> property, ref TContainer container, ref char value)
        {
            value = EditorGUILayout.TextField(GetDisplayName(property), value.ToString()).FirstOrDefault();
            return VisitStatus.Stop;
        }

        public VisitStatus Visit<TContainer>(Property<TContainer, string> property, ref TContainer container, ref string value)
        {
            value = EditorGUILayout.TextField(GetDisplayName(property), value);
            return VisitStatus.Stop;
        }
    }
}
