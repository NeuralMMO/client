using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class adds names for each LevelMapPathElement next to it on the scene view, for easier setup
	/// </summary>
	[CustomEditor(typeof(MMSceneViewIcon))]
	[InitializeOnLoad]
	public class SceneViewIconEditor : Editor 
	{		
		//protected SceneViewIcon _sceneViewIcon;

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawGameObjectName(MMSceneViewIcon sceneViewIcon, GizmoType gizmoType)
		{   
			GUIStyle style = new GUIStyle();
	        style.normal.textColor = Color.blue;	 
			Handles.Label(sceneViewIcon.transform.position, sceneViewIcon.gameObject.name,style);
		}


	}
}