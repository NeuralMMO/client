using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    [CustomEditor(typeof(MMTriggerAndCollision), true)]
    [CanEditMultipleObjects]
    public class MMTriggerAndCollisionEditor : Editor
    {
        protected SerializedProperty _CollisionLayerMask;
        protected SerializedProperty _OnCollisionEnterEvent;
        protected SerializedProperty _OnCollisionExitEvent;
        protected SerializedProperty _OnCollisionStayEvent;

        protected SerializedProperty _TriggerLayerMask;
        protected SerializedProperty _OnTriggerEnterEvent;
        protected SerializedProperty _OnTriggerExitEvent;
        protected SerializedProperty _OnTriggerStayEvent;

        protected SerializedProperty _Collision2DLayerMask;
        protected SerializedProperty _OnCollision2DEnterEvent;
        protected SerializedProperty _OnCollision2DExitEvent;
        protected SerializedProperty _OnCollision2DStayEvent;

        protected SerializedProperty _Trigger2DLayerMask;
        protected SerializedProperty _OnTrigger2DEnterEvent;
        protected SerializedProperty _OnTrigger2DExitEvent;
        protected SerializedProperty _OnTrigger2DStayEvent;

        protected bool OnCollision;
        protected bool OnTrigger;
        protected bool OnCollision2D;
        protected bool OnTrigger2D;

        protected virtual void OnEnable()
        {

            _CollisionLayerMask = serializedObject.FindProperty("CollisionLayerMask");
            _OnCollisionEnterEvent = serializedObject.FindProperty("OnCollisionEnterEvent");
            _OnCollisionExitEvent = serializedObject.FindProperty("OnCollisionExitEvent");
            _OnCollisionStayEvent = serializedObject.FindProperty("OnCollisionStayEvent");

            _TriggerLayerMask = serializedObject.FindProperty("TriggerLayerMask");
            _OnTriggerEnterEvent = serializedObject.FindProperty("OnTriggerEnterEvent");
            _OnTriggerExitEvent = serializedObject.FindProperty("OnTriggerExitEvent");
            _OnTriggerStayEvent = serializedObject.FindProperty("OnTriggerStayEvent");

            _Collision2DLayerMask = serializedObject.FindProperty("Collision2DLayerMask");
            _OnCollision2DEnterEvent = serializedObject.FindProperty("OnCollision2DEnterEvent");
            _OnCollision2DExitEvent = serializedObject.FindProperty("OnCollision2DExitEvent");
            _OnCollision2DStayEvent = serializedObject.FindProperty("OnCollision2DStayEvent");

            _Trigger2DLayerMask = serializedObject.FindProperty("Trigger2DLayerMask");
            _OnTrigger2DEnterEvent = serializedObject.FindProperty("OnTrigger2DEnterEvent");
            _OnTrigger2DExitEvent = serializedObject.FindProperty("OnTrigger2DExitEvent");
            _OnTrigger2DStayEvent = serializedObject.FindProperty("OnTrigger2DStayEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, "Modified MMTriggerAndCollision");

            OnCollision = EditorGUILayout.Foldout(OnCollision, "OnCollision");

            if (OnCollision)
            {
                EditorGUILayout.PropertyField(_CollisionLayerMask);
                EditorGUILayout.PropertyField(_OnCollisionEnterEvent);
                EditorGUILayout.PropertyField(_OnCollisionExitEvent);
                EditorGUILayout.PropertyField(_OnCollisionStayEvent);
            }

            OnTrigger = EditorGUILayout.Foldout(OnTrigger, "OnTrigger");

            if (OnTrigger)
            {
                EditorGUILayout.PropertyField(_TriggerLayerMask);
                EditorGUILayout.PropertyField(_OnTriggerEnterEvent);
                EditorGUILayout.PropertyField(_OnTriggerExitEvent);
                EditorGUILayout.PropertyField(_OnTriggerStayEvent);
            }

            OnCollision2D = EditorGUILayout.Foldout(OnCollision2D, "OnCollision2D");

            if (OnCollision2D)
            {
                EditorGUILayout.PropertyField(_Collision2DLayerMask);
                EditorGUILayout.PropertyField(_OnCollision2DEnterEvent);
                EditorGUILayout.PropertyField(_OnCollision2DExitEvent);
                EditorGUILayout.PropertyField(_OnCollision2DStayEvent);
            }

            OnTrigger2D = EditorGUILayout.Foldout(OnTrigger2D, "OnTrigger2D");

            if (OnTrigger2D)
            {
                EditorGUILayout.PropertyField(_Trigger2DLayerMask);
                EditorGUILayout.PropertyField(_OnTrigger2DEnterEvent);
                EditorGUILayout.PropertyField(_OnTrigger2DExitEvent);
                EditorGUILayout.PropertyField(_OnTrigger2DStayEvent);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
