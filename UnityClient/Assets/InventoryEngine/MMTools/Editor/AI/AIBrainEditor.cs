using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AIBrain))]
    public class AIBrainEditor : Editor
    {
        protected ReorderableList _list;
        protected SerializedProperty _brainActive;
        protected SerializedProperty _timeInThisState;
        protected SerializedProperty _target;
        protected SerializedProperty _actionsFrequency;
        protected SerializedProperty _decisionFrequency;

        protected virtual void OnEnable()
        {
            _list = new ReorderableList(serializedObject.FindProperty("States"));
            _list.elementNameProperty = "States";
            _list.elementDisplayType = ReorderableList.ElementDisplayType.Expandable;

            _brainActive = serializedObject.FindProperty("BrainActive");
            _timeInThisState = serializedObject.FindProperty("TimeInThisState");
            _target = serializedObject.FindProperty("Target");
            _actionsFrequency = serializedObject.FindProperty("ActionsFrequency");
            _decisionFrequency = serializedObject.FindProperty("DecisionFrequency");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _list.DoLayoutList();
            EditorGUILayout.PropertyField(_brainActive);
            EditorGUILayout.PropertyField(_timeInThisState);
            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_actionsFrequency);
            EditorGUILayout.PropertyField(_decisionFrequency);
            serializedObject.ApplyModifiedProperties();

            AIBrain brain = (AIBrain)target;
            if (brain.CurrentState != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current State", brain.CurrentState.StateName);
            }
        }
    }
}
