using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    [Serializable]
    class SearchVariable : IEquatable<SearchVariable>, IComparable<SearchVariable>
    {
        public enum Type
        {
            String,
            Number,
            Path,
            Set
        }

        public SearchVariable(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }

        public SearchVariable(string name, string text)
            : this(name, Type.String)
        {
            this.text = text;
        }

        public SearchVariable(string name, Type type, object value)
            : this(name, type)
        {
            switch (type)
            {
                case Type.String: text = Convert.ToString(value); break;
                case Type.Number: double.TryParse(value.ToString(), out number); break;
                case Type.Path: text = Convert.ToString(value); break;
                case Type.Set: set = value as SearchExpressionAsset; break;
            }
        }

        public SearchVariable(string name, double number)
           : this(name, Type.Number)
        {
            this.number = number;
        }

        public SearchVariable(string name, SearchExpressionAsset set)
            : this(name, Type.Set)
        {
            this.set = set;
        }

        public string name;
        public Type type;
        public string text;
        public double number;
        [SerializeReference] public SearchExpressionAsset set;

        public override bool Equals(object obj)
        {
            return obj is SearchVariable v && Equals(v);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public bool Equals(SearchVariable other)
        {
            return string.Equals(name, other.name);
        }

        public int CompareTo(SearchVariable other)
        {
            return string.Compare(name, other.name);
        }

        public static SearchVariable[] UpdateVariables(SearchExpression se, in SearchVariable[] variables)
        {
            var mv = new List<SearchVariable>();
            foreach (var node in se.nodes.Where(n => !string.IsNullOrEmpty(n.name) && (n.type == ExpressionType.Value || n.type == ExpressionType.Expression)))
            {
                var existingVariable = Array.Find(variables, v => v.name == node.name);

                if (node.type == ExpressionType.Value)
                {
                    if (node.value != null && double.TryParse(node.value.ToString(), out var d))
                    {
                        if (existingVariable?.type == Type.Number)
                            mv.Add(existingVariable);
                        else
                            mv.Add(new SearchVariable(node.name, d));
                    }
                    else if (node.value is string ns && File.Exists(ns))
                    {
                        if (existingVariable?.type == Type.Path)
                            mv.Add(existingVariable);
                        else
                            mv.Add(new SearchVariable(node.name, Type.Path, ns));
                    }
                    else
                    {
                        if (existingVariable?.type == Type.String)
                            mv.Add(existingVariable);
                        else
                            mv.Add(new SearchVariable(node.name, node.value?.ToString()));
                    }
                }
                else if (node.type == ExpressionType.Expression)
                {
                    if (existingVariable?.type == Type.Set)
                        mv.Add(existingVariable);
                    else
                        mv.Add(new SearchVariable(node.name, node.value as SearchExpressionAsset));
                }
                else
                    throw new NotImplementedException();
            }

            mv.Sort();
            if (!Array.Equals(mv, variables))
                return mv.ToArray();

            return variables;
        }

        public static bool DrawVariables(IEnumerable<SearchVariable> variables, float width)
        {
            EditorGUIUtility.labelWidth = width * .25f;
            EditorGUI.BeginChangeCheck();
            foreach (var v in variables)
            {
                if (v.type == Type.String)
                    v.text = EditorGUILayout.TextField(v.name, v.text);
                else if (v.type == Type.Number)
                    v.number = EditorGUILayout.DoubleField(v.name, v.number);
                else if (v.type == Type.Path)
                    v.text = AssetDatabase.GetAssetPath(EditorGUILayout.ObjectField(v.name, AssetDatabase.LoadMainAssetAtPath(v.text), typeof(UnityEngine.Object), false));
                else if (v.type == Type.Set)
                    v.set = EditorGUILayout.ObjectField(v.name, v.set, typeof(SearchExpressionAsset), false) as SearchExpressionAsset;
            }
            return EditorGUI.EndChangeCheck();
        }

        public static void Evaluate(IEnumerable<SearchVariable> variables, ISearchExpression se, Action finished)
        {
            foreach (var v in variables)
            {
                if (v.type == Type.String || v.type == Type.Path)
                    se.SetValue(v.name, v.text);
                else if (v.type == Type.Number)
                    se.SetValue(v.name, v.number);
                else if (v.type == Type.Set)
                    se.SetValue(v.name, v.set);
            }
            se.Evaluate(finished);
        }
    }

    [CustomPropertyDrawer(typeof(SearchVariable), true)]
    class SearchVariableEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = position.width * .25f;

            var pname = property.FindPropertyRelative(nameof(SearchVariable.name));
            var ptype = property.FindPropertyRelative(nameof(SearchVariable.type));
            var type = (SearchVariable.Type)ptype.enumValueIndex;

            if (type == SearchVariable.Type.String)
            {
                var pvalue = property.FindPropertyRelative(nameof(SearchVariable.text));
                EditorGUI.PropertyField(position, pvalue, new GUIContent(pname.stringValue));
            }
            else if (type == SearchVariable.Type.Number)
            {
                var pnumber = property.FindPropertyRelative(nameof(SearchVariable.number));
                EditorGUI.PropertyField(position, pnumber, new GUIContent(pname.stringValue));
            }
            else if (type == SearchVariable.Type.Path)
            {
                var ppath = property.FindPropertyRelative(nameof(SearchVariable.text));
                ppath.stringValue = AssetDatabase.GetAssetPath(
                    EditorGUI.ObjectField(position, pname.stringValue, AssetDatabase.LoadMainAssetAtPath(ppath.stringValue), typeof(UnityEngine.Object), false));
            }
            else if (type == SearchVariable.Type.Set)
            {
                var pset = property.FindPropertyRelative("set");
                EditorGUI.PropertyField(position, pset, new GUIContent(pname.stringValue));
            }
        }
    }
}
