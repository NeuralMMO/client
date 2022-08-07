using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Custom editor for the FloatController, conditional hiding and dropdown fill
    /// </summary>
    [CustomEditor(typeof(FloatController), true)]
    [CanEditMultipleObjects]    
    public class FloatControllerEditor : Editor
    {
        protected SerializedProperty _TargetObject;

        protected SerializedProperty _Curve;
        protected SerializedProperty _MinValue;
        protected SerializedProperty _MaxValue;
        protected SerializedProperty _Duration;

        protected SerializedProperty _Amplitude;
        protected SerializedProperty _Frequency;
        protected SerializedProperty _Shift;

        protected SerializedProperty _OneTimeDuration;
        protected SerializedProperty _OneTimeAmplitude;
        protected SerializedProperty _OneTimeCurve;
        protected SerializedProperty _OneTimeButton;

        protected SerializedProperty _InitialValue;
        protected SerializedProperty _CurrentValue;

        protected SerializedProperty _ChoiceIndex;
        protected SerializedProperty _PropertyName;

        protected SerializedProperty _AudioAnalyzer;
        protected SerializedProperty _BeatID;
        protected SerializedProperty _AudioAnalyzerMultiplier;

        /// <summary>
        /// On enable, grabs our serialized properties
        /// </summary>
        protected virtual void OnEnable()
        {
            FloatController myTarget = (FloatController)target;

            _TargetObject = serializedObject.FindProperty("TargetObject");

            _Curve = serializedObject.FindProperty("Curve");
            _MinValue = serializedObject.FindProperty("MinValue");
            _MaxValue = serializedObject.FindProperty("MaxValue");
            _Duration = serializedObject.FindProperty("Duration");

            _Amplitude = serializedObject.FindProperty("Amplitude");
            _Frequency = serializedObject.FindProperty("Frequency");
            _Shift = serializedObject.FindProperty("Shift");

            _OneTimeDuration = serializedObject.FindProperty("OneTimeDuration");
            _OneTimeAmplitude = serializedObject.FindProperty("OneTimeAmplitude");
            _OneTimeCurve = serializedObject.FindProperty("OneTimeCurve");
            _OneTimeButton = serializedObject.FindProperty("OneTimeButton");

            _InitialValue = serializedObject.FindProperty("InitialValue");
            _CurrentValue = serializedObject.FindProperty("CurrentValue");
            
            _ChoiceIndex = serializedObject.FindProperty("ChoiceIndex");
            _PropertyName = serializedObject.FindProperty("PropertyName");

            _AudioAnalyzer = serializedObject.FindProperty("AudioAnalyzer");
            _BeatID = serializedObject.FindProperty("BeatID");
            _AudioAnalyzerMultiplier = serializedObject.FindProperty("AudioAnalyzerMultiplier");

            VerifyChosenIndex();
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        }

        protected virtual void OnDisable()
        {
            //BindPropertyName();
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        protected virtual void BindPropertyName()
        {
            FloatController myTarget = (FloatController)target;

            if (myTarget.ChoiceIndex > myTarget.AttributeNames.Length - 1)
            {
                _ChoiceIndex.intValue = 0;
                _PropertyName.stringValue = FloatController._undefinedString;
            }
            else
            {
                _PropertyName.stringValue = myTarget.AttributeNames[myTarget.ChoiceIndex];
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void VerifyChosenIndex()
        {
            FloatController myTarget = (FloatController)target;

            // determine choice index
            int index = 0;
            bool found = false;
            foreach (string attName in myTarget.AttributeNames)
            {
                if (attName == myTarget.PropertyName)
                {
                    _ChoiceIndex.intValue = index;
                    found = true;
                }
                index++;
            }
            if (!found)
            {
                _ChoiceIndex.intValue = 0;
                _PropertyName.stringValue = FloatController._undefinedString;
            }
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnAfterAssemblyReload()
        {
            FloatController myTarget = (FloatController)target;
            myTarget.FillDropDownList();
            VerifyChosenIndex();
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Draws a custom conditional inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, "Modified FloatController");

            FloatController myTarget = (FloatController)target;

            EditorGUILayout.PropertyField(_TargetObject);
            if (myTarget.AttributeNames != null)
            {
                if (myTarget.AttributeNames.Length > 0)
                {
                    // draws a dropdown with all our properties
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Property");

                    _ChoiceIndex.intValue = EditorGUILayout.Popup(myTarget.ChoiceIndex, myTarget.AttributeNames);
                    BindPropertyName();
                    EditorGUILayout.EndHorizontal();

                    Editor.DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "TargetObject", "Curve", "MinValue", "MaxValue", "Duration", "Amplitude",
                                                                            "Frequency", "Shift", "InitialValue", "CurrentValue",
                                                                            "OneTimeDuration", "OneTimeAmplitude", "OneTimeCurve", "OneTimeButton",
                                                                            "AudioAnalyzer", "BeatID", "AudioAnalyzerMultiplier" });

                    if (myTarget.ControlMode == FloatController.ControlModes.PingPong)
                    {
                        EditorGUILayout.PropertyField(_Curve);
                        EditorGUILayout.PropertyField(_MinValue);
                        EditorGUILayout.PropertyField(_MaxValue);
                        EditorGUILayout.PropertyField(_Duration);
                    }
                    else if (myTarget.ControlMode == FloatController.ControlModes.Random)
                    {
                        EditorGUILayout.PropertyField(_Amplitude);
                        EditorGUILayout.PropertyField(_Frequency);
                        EditorGUILayout.PropertyField(_Shift);
                    }
                    else if (myTarget.ControlMode == FloatController.ControlModes.OneTime)
                    {
                        EditorGUILayout.PropertyField(_OneTimeDuration);
                        EditorGUILayout.PropertyField(_OneTimeAmplitude);
                        EditorGUILayout.PropertyField(_OneTimeCurve);
                        EditorGUILayout.PropertyField(_OneTimeButton);
                    }
                    else if (myTarget.ControlMode == FloatController.ControlModes.AudioAnalyzer)
                    {
                        EditorGUILayout.PropertyField(_AudioAnalyzer);
                        EditorGUILayout.PropertyField(_BeatID);
                        EditorGUILayout.PropertyField(_AudioAnalyzerMultiplier);
                    }

                    EditorGUILayout.PropertyField(_InitialValue);
                    EditorGUILayout.PropertyField(_CurrentValue);
                }
            }
            

            serializedObject.ApplyModifiedProperties();
        }
    }
}
