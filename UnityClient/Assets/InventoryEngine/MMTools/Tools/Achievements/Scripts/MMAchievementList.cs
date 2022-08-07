using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	[CreateAssetMenu(fileName="AchievementList",menuName="MoreMountains/Achievement List")]
	/// <summary>
	/// A scriptable object containing a list of achievements. You need to create one and store it in a Resources folder for this to work.
	/// </summary>
	public class MMAchievementList : ScriptableObject 
	{
		/// the unique ID of this achievement list. This is used to save/load data.
		public string AchievementsListID = "AchievementsList";

		/// the list of achievements 
		public List<MMAchievement> Achievements;

		/// <summary>
		/// Asks for a reset of all the achievements in this list (they'll all be locked again, their progress lost).
		/// </summary>
		public virtual void ResetAchievements()
		{
			Debug.LogFormat ("Reset Achievements");
			MMAchievementManager.ResetAchievements (AchievementsListID);
		}
	}
}