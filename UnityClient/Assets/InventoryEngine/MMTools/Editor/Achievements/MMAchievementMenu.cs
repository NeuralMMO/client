using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{	
	public static class MMAchievementMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all achievements", false,21)]
		/// <summary>
		/// Adds a menu item to enable help
		/// </summary>
		private static void EnableHelpInInspectors()
		{
			MMAchievementManager.ResetAllAchievements ();
		}
	}
}