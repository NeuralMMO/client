using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace MoreMountains.Tools
{
	[CanEditMultipleObjects()]
	[CustomEditor(typeof(MMTrailRendererSortingLayer))]
	public class RendererLayerEditor : Editor
	{
		int popupMenuIndex;
		string[] sortingLayerNames;
		protected MMTrailRendererSortingLayer _mmTrailRendererSortingLayer;
		protected TrailRenderer _trailRenderer;

		void OnEnable()
		{
			sortingLayerNames = GetSortingLayerNames(); 
			_mmTrailRendererSortingLayer = (MMTrailRendererSortingLayer)target;
			_trailRenderer = _mmTrailRendererSortingLayer.GetComponent<TrailRenderer> ();

			for (int i = 0; i<sortingLayerNames.Length;i++) //here we initialize our popupMenuIndex with the current Sort Layer Name
			{
				if (sortingLayerNames[i] == _trailRenderer.sortingLayerName)
					popupMenuIndex = i;
			}
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (_trailRenderer == null)
			{
				return; 
			}

			popupMenuIndex = EditorGUILayout.Popup("Sorting Layer", popupMenuIndex, sortingLayerNames);
			int newSortingLayerOrder = EditorGUILayout.IntField("Order in Layer", _trailRenderer.sortingOrder);
		
			if (sortingLayerNames[popupMenuIndex] != _trailRenderer.sortingLayerName 
				|| newSortingLayerOrder != _trailRenderer.sortingOrder) 
			{
				Undo.RecordObject(_trailRenderer, "Change Particle System Renderer Order"); 

				_trailRenderer.sortingLayerName = sortingLayerNames[popupMenuIndex];
				_trailRenderer.sortingOrder = newSortingLayerOrder;

				EditorUtility.SetDirty(_trailRenderer); 
			}
		}

		public string[] GetSortingLayerNames()
		{
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}
	}
}