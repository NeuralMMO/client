using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMAchievementList),true)]
	/// <summary>
	/// Custom inspector for the MMAchievementList scriptable object. 
	/// </summary>
	public class MMAchievementListInspector : Editor 
	{
		/// <summary>
		/// When drawing the GUI, adds a "Reset Achievements" button, that does exactly what you think it does.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			MMAchievementList achievementList = (MMAchievementList)target;
			if(GUILayout.Button("Reset Achievements"))
			{
				achievementList.ResetAchievements();
			}	
			EditorUtility.SetDirty (achievementList);
		}
	}
}