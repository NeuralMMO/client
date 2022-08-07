#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class adds names for each LevelMapPathElement next to it on the scene view, for easier setup
	/// </summary>
	[CustomEditor(typeof(MMPathMovement),true)]
	[InitializeOnLoad]
	public class MMPathMovementEditor : Editor 
	{		
		public MMPathMovement pathMovementTarget
		{
			get
			{
				return (MMPathMovement)target;
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update ();

			if (pathMovementTarget.AccelerationType == MMPathMovement.PossibleAccelerationType.AnimationCurve)
			{
				DrawDefaultInspector ();
			}
			else
			{
				Editor.DrawPropertiesExcluding (serializedObject, new string [] { "Acceleration" });
			}

			serializedObject.ApplyModifiedProperties ();
		}

		/// <summary>
		/// OnSceneGUI, draws repositionable handles at every point in the path, for easier setup
		/// </summary>
		protected virtual void OnSceneGUI()
	    {
			Handles.color=Color.green;
			MMPathMovement t = (target as MMPathMovement);

			if (t.GetOriginalTransformPositionStatus() == false)
			{
				return;
			}

			for (int i=0;i<t.PathElements.Count;i++)
			{
	       		EditorGUI.BeginChangeCheck();

				Vector3 oldPoint = t.GetOriginalTransformPosition()+t.PathElements[i].PathElementPosition;
				GUIStyle style = new GUIStyle();

				// draws the path item number
		        style.normal.textColor = Color.yellow;	 
				Handles.Label(t.GetOriginalTransformPosition()+t.PathElements[i].PathElementPosition+(Vector3.down*0.4f)+(Vector3.right*0.4f), ""+i,style);

				// draws a movable handle
				Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, Quaternion.identity,.5f,new Vector3(.25f,.25f,.25f),Handles.CircleHandleCap);

				// records changes
				if (EditorGUI.EndChangeCheck())
		        {
		            Undo.RecordObject(target, "Free Move Handle");
					t.PathElements[i].PathElementPosition = newPoint - t.GetOriginalTransformPosition();
		        }
			}	        
	    }
	}
}

#endif