using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	/// <summary>
	/// An event type used to broadcast the fact that an achievement has been unlocked
	/// </summary>
	public struct MMAchievementUnlockedEvent
	{
		/// the achievement that has been unlocked
		public MMAchievement Achievement;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="newAchievement">New achievement.</param>
		public MMAchievementUnlockedEvent(MMAchievement newAchievement)
		{
			Achievement = newAchievement;
        }

        static MMAchievementUnlockedEvent e;
        public static void Trigger(MMAchievement newAchievement)
        {
            e.Achievement = newAchievement;
            MMEventManager.TriggerEvent(e);
        }
    }
}