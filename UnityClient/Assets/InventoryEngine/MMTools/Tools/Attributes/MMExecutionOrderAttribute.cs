using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this attribute to a class and its Execution Order will be changed to the value specified in parameters
    /// Usage : [ExecutionOrder(66)]
    /// </summary>
    public class MMExecutionOrderAttribute : Attribute
    {
#if UNITY_EDITOR
        /// the execution order you want for the class this attribute is applied to 
        public int ExecutionOrder = 0;

        protected static Dictionary<MonoScript, MMExecutionOrderAttribute> _monoScripts;
        protected static Type _executionOrderAttributeType;
        protected static Assembly _typeAssembly;
        protected static Type[] _assemblyTypes;

        /// <summary>
        /// Attribute method
        /// </summary>
        /// <param name="newExecutionOrder"></param>
        public MMExecutionOrderAttribute(int newExecutionOrder)
        {
            ExecutionOrder = newExecutionOrder;
        }


            /// <summary>
            /// When Unity loads, modifies the execution orders of monos with an ExecutionOrder attribute, if needed
            /// </summary>
            [InitializeOnLoadMethod]        
            protected static void ModifyExecutionOrder()
            {
                Initialization();

                FindExecutionOrderAttributes();
            
                if (ExecutionOrderHasChanged())
                {
                    UpdateExecutionOrders();
                }
            }

            /// <summary>
            /// Initialization method
            /// </summary>
            protected static void Initialization()
            {
                _monoScripts = new Dictionary<MonoScript, MMExecutionOrderAttribute>();
                _executionOrderAttributeType = typeof(MMExecutionOrderAttribute);
                _typeAssembly = _executionOrderAttributeType.Assembly;
                _assemblyTypes = _typeAssembly.GetTypes();
            }

            /// <summary>
            /// Goes through all assembly types and stores execution order attributes when found
            /// </summary>
            protected static void FindExecutionOrderAttributes()
            {
                foreach (Type assemblyType in _assemblyTypes)
                {
                    if (!HasExecutionOrderAttribute(assemblyType))
                    {
                        continue;
                    }

                    object[] attributes = assemblyType.GetCustomAttributes(_executionOrderAttributeType, false);
                    MMExecutionOrderAttribute attribute = attributes[0] as MMExecutionOrderAttribute;

                    string asset = "";
                    string[] guids = AssetDatabase.FindAssets(assemblyType.Name + " t:script");

                    if (guids.Length != 0)
                    {
                        foreach (string guid in guids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            string filename = Path.GetFileNameWithoutExtension(assetPath);
                            if (filename == assemblyType.Name)
                            {
                                asset = guid;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("MMTools' ExecutionOrderAttribute : Can't change "+ assemblyType.Name + "'s execution order");
                        return;
                    }

                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(asset));
                    _monoScripts.Add(monoScript, attribute);
                }
            }

            /// <summary>
            /// Returns true if the class in parameters has the ExecutionOrder attribute, false otherwise
            /// </summary>
            /// <param name="assemblyType"></param>
            /// <returns></returns>
            protected static bool HasExecutionOrderAttribute(Type assemblyType)
            {
                object[] attributes = assemblyType.GetCustomAttributes(_executionOrderAttributeType, false);
                return (attributes.Length == 1);
            }

            /// <summary>
            /// Returns true if the execution order has changed since last time 
            /// </summary>
            /// <returns></returns>
            protected static bool ExecutionOrderHasChanged()
            {
                bool executionOrderHasChanged = false;
                foreach (KeyValuePair<MonoScript, MMExecutionOrderAttribute> monoScript in _monoScripts)
                {
                    if (monoScript.Key != null)
                    {
                        if (MonoImporter.GetExecutionOrder(monoScript.Key) != monoScript.Value.ExecutionOrder)
                        {
                            executionOrderHasChanged = true;
                            break;
                        }
                    }                    
                }
                return executionOrderHasChanged;
            }

            /// <summary>
            /// Updates the execution orders for all pairs found by FindExecutionOrderAttributes()
            /// </summary>
            protected static void UpdateExecutionOrders()
            {
                foreach (KeyValuePair<MonoScript, MMExecutionOrderAttribute> monoScript in _monoScripts)
                {
                    if (monoScript.Key != null)
                    {
                        if (MonoImporter.GetExecutionOrder(monoScript.Key) != monoScript.Value.ExecutionOrder)
                        {
                            MonoImporter.SetExecutionOrder(monoScript.Key, monoScript.Value.ExecutionOrder);
                        }
                    }
                }
            }

#endif
    }
}
