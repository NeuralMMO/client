using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This achievement system supports 2 types of achievements : simple (do something > get achievement), and progress based (jump X times, kill X enemies, etc).
	/// </summary>
	public enum AchievementTypes { Simple, Progress }

	[Serializable]
	public class MMAchievement  
	{
		[Header("Identification")]
		/// the unique identifier for this achievement
		public string AchievementID;
		/// is this achievement progress based or 
		public AchievementTypes AchievementType;
		/// if this is true, the achievement won't be displayed in a list
		public bool HiddenAchievement;
		/// if this is true, the achievement has been unlocked. Otherwise, it's still up for grabs
		public bool UnlockedStatus;

		[Header("Description")]
		/// the achievement's name/title
		public string Title;
		/// the achievement's description
		public string Description;
		/// the amount of points unlocking this achievement gets you
		public int Points;

		[Header("Image and Sounds")]
		/// the image to display while this achievement is locked
		public Sprite LockedImage;
		/// the image to display when the achievement is unlocked
		public Sprite UnlockedImage;
		/// a sound to play when the achievement is unlocked
		public AudioClip UnlockedSound;

		[Header("Progress")]
		/// the amount of progress needed to unlock this achievement.
		public int ProgressTarget;
		/// the current amount of progress made on this achievement
		public int ProgressCurrent;

		protected MMAchievementDisplayItem _achievementDisplayItem;

		/// <summary>
		/// Unlocks the achievement, asks for a save of the current achievements, and triggers an MMAchievementUnlockedEvent for this achievement.
		/// This will usually then be caught by the MMAchievementDisplay class.
		/// </summary>
		public virtual void UnlockAchievement()
		{
			// if the achievement has already been unlocked, we do nothing and exit
			if (UnlockedStatus)
			{
				return;
			}

			UnlockedStatus = true;

			MMGameEvent.Trigger("Save");
			MMAchievementUnlockedEvent.Trigger(this);
		}

		/// <summary>
		/// Locks the achievement.
		/// </summary>
		public virtual void LockAchievement()
		{
			UnlockedStatus = false;
		}

		/// <summary>
		/// Adds the specified value to the current progress.
		/// </summary>
		/// <param name="newProgress">New progress.</param>
		public virtual void AddProgress(int newProgress)
		{
			ProgressCurrent += newProgress;
			EvaluateProgress();
		}

		/// <summary>
		/// Sets the progress to the value passed in parameter.
		/// </summary>
		/// <param name="newProgress">New progress.</param>
		public virtual void SetProgress(int newProgress)
		{
			ProgressCurrent = newProgress;
			EvaluateProgress();
		}

		/// <summary>
		/// Evaluates the current progress of the achievement, and unlocks it if needed.
		/// </summary>
		protected virtual void EvaluateProgress()
		{
			if (ProgressCurrent >= ProgressTarget)
			{
				ProgressCurrent = ProgressTarget;
				UnlockAchievement();
			}
		}

		/// <summary>
		/// Copies this achievement (useful when loading from a scriptable object list)
		/// </summary>
		public virtual MMAchievement Copy()
		{
			MMAchievement clone = new MMAchievement ();
			// we use Json utility to store a copy of our achievement, not a reference
			clone = JsonUtility.FromJson<MMAchievement>(JsonUtility.ToJson(this));
			return clone;
		}
	}
}