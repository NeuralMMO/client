using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{	
	[CustomEditor(typeof(Inventory),true)]
	/// <summary>
	/// Custom editor for the Inventory component
	/// </summary>
	public class InventoryEditor : Editor 
	{
		/// <summary>
		/// Gets the target inventory component.
		/// </summary>
		/// <value>The inventory target.</value>
		public Inventory InventoryTarget 
		{ 
			get 
			{ 
				return (Inventory)target;
			}
		} 
	   
	   /// <summary>
	   /// Custom editor for the inventory panel.
	   /// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck ();

			// if there's a change in the inspector, we resize our inventory and grid, and redraw the whole thing.
			if (InventoryTarget.InventoryType==Inventory.InventoryTypes.Main)
			{
				Editor.DrawPropertiesExcluding(serializedObject, new string[] { "TargetChoiceInventory" });
			}
			if (InventoryTarget.InventoryType==Inventory.InventoryTypes.Equipment)
			{
				Editor.DrawPropertiesExcluding(serializedObject, new string[] {  });
			}

			// if for some reason we don't have a target inventory, we do nothing and exit
			if (InventoryTarget==null )
			{
				Debug.LogWarning("inventory target is null");
				return;
			}

			// if we have a content and are in debug mode, we draw the content of the Content (I know) variable in the inspector
			if (InventoryTarget.Content!=null && InventoryTarget.DrawContentInInspector)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Debug Content",EditorStyles.boldLabel);
				if (InventoryTarget.NumberOfFilledSlots>0)
				{
					for (int i = 0; i < InventoryTarget.Content.Length; i++)
					{
						GUILayout.BeginHorizontal ();
						if (!InventoryItem.IsNull(InventoryTarget.Content[i]))
						{
							EditorGUILayout.LabelField("Content["+i+"]",InventoryTarget.Content[i].Quantity.ToString()+" "+InventoryTarget.Content[i].ItemName);

							if (GUILayout.Button("Empty Slot"))
							{
								InventoryTarget.DestroyItem(i)	;
							}
						}
						else
						{
							EditorGUILayout.LabelField("Content["+i+"]","empty");
						}
						GUILayout.EndHorizontal ();
					}
				}

				// we draw the number of slots (total, free and filled) to the inspector.
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Free slots",InventoryTarget.NumberOfFreeSlots.ToString());
				EditorGUILayout.LabelField("Filled slots",InventoryTarget.NumberOfFilledSlots.ToString());
				EditorGUILayout.Space();
			}

			// we add a button to manually empty the inventory
			EditorGUILayout.Space();
			if (GUILayout.Button("Empty inventory"))
			{
				InventoryTarget.EmptyInventory()	;
			}
			if (GUILayout.Button("Reset saved inventory"))
			{
				InventoryTarget.ResetSavedInventory()	;
			}
			if (EditorGUI.EndChangeCheck ()) 
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}
			// we apply our changes
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// On each update
		/// </summary>
		public void Update()
		 {
		     // We repaint the editor
		     Repaint();
		 }	
	}
}