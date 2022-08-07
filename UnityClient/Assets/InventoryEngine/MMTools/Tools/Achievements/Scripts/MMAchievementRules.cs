using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	/// <summary>
	/// That class is meant to be extended to implement the achievement rules specific to your game.
	/// </summary>
	public class MMAchievementRules : MonoBehaviour, MMEventListener<MMGameEvent>
	{
		/// <summary>
		/// On Awake, loads the achievement list and the saved file
		/// </summary>
		protected virtual void Awake()
		{
			// we load the list of achievements, stored in a ScriptableObject in our Resources folder.
			MMAchievementManager.LoadAchievementList ();
			// we load our saved file, to update that list with the saved values.
			MMAchievementManager.LoadSavedAchievements ();
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
		}

		/// <summary>
		/// When we catch an MMGameEvent, we do stuff based on its name
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "Save":
					MMAchievementManager.SaveAchievements ();
					break;
				/*
				// These are just examples of how you could catch a GameStart MMGameEvent and trigger the potential unlock of a corresponding achievement 
				case "GameStart":
					MMAchievementManager.UnlockAchievement("theFirestarter");
					break;
				case "LifeLost":
					MMAchievementManager.UnlockAchievement("theEndOfEverything");
					break;
				case "Pause":
					MMAchievementManager.UnlockAchievement("timeStop");
					break;
				case "Jump":
					MMAchievementManager.UnlockAchievement ("aSmallStepForMan");
					MMAchievementManager.AddProgress ("toInfinityAndBeyond", 1);
					break;*/
			}
		} 
	}
}