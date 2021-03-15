using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
	/// <summary>
	/// This static class is in charge of storing the current state of the achievements, unlocking/locking them, and saving them to data files
	/// </summary>
	public static class MMAchievementManager
	{
		public static List<MMAchievement> AchievementsList { get { return _achievements; }}

		private static List<MMAchievement> _achievements;
		private static MMAchievement _achievement = null;

		private const string _defaultFileName = "Achievements";
		private const string _saveFolderName = "MMAchievements/";
		private const string _saveFileExtension = ".achievements";

		private static string _saveFileName;
		private static string _listID;

		/// <summary>
		/// You'll need to call this method to initialize the manager
		/// </summary>
		public static void LoadAchievementList()
		{
			_achievements = new List<MMAchievement> ();

			// the Achievement List scriptable object must be in a Resources folder inside your project, like so : Resources/Achievements/PUT_SCRIPTABLE_OBJECT_HERE
			MMAchievementList achievementList = (MMAchievementList) Resources.Load("Achievements/AchievementList");

			if (achievementList == null)
			{
				return;
			}

			// we store the ID for save purposes
			_listID = achievementList.AchievementsListID;

			foreach (MMAchievement achievement in achievementList.Achievements)
			{
				_achievements.Add (achievement.Copy());
			}
		}

		/// <summary>
		/// Unlocks the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement I.</param>
		public static void UnlockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.UnlockAchievement();
			}
		}

		/// <summary>
		/// Locks the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		public static void LockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.LockAchievement();
			}
		}

		/// <summary>
		/// Adds progress to the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		/// <param name="newProgress">New progress.</param>
		public static void AddProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.AddProgress(newProgress);
			}
		}

		/// <summary>
		/// Sets the progress of the specified achievement (if found) to the specified progress.
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		/// <param name="newProgress">New progress.</param>
		public static void SetProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.SetProgress(newProgress);
			}
		}		

		/// <summary>
		/// Determines if the achievement manager contains an achievement of the specified ID. Returns it if found, otherwise returns null
		/// </summary>
		/// <returns>The achievement corresponding to the searched ID if found, otherwise null.</returns>
		/// <param name="searchedID">Searched I.</param>
		private static MMAchievement AchievementManagerContains(string searchedID)
		{
			if (_achievements.Count == 0)
			{
				return null;
			}
			foreach(MMAchievement achievement in _achievements)
			{
				if (achievement.AchievementID == searchedID)
				{
					return achievement;					
				}
			}
			return null;
		}

		// SAVE ------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes saved data and resets all achievements from a list
		/// </summary>
		/// <param name="listID">The ID of the achievement list to reset.</param>
		public static void ResetAchievements(string listID)
		{
			if (_achievements != null)
			{
				foreach(MMAchievement achievement in _achievements)
				{
					achievement.ProgressCurrent = 0;
					achievement.UnlockedStatus = false;
				}	
			}

			DeterminePath (listID);
			MMSaveLoadManager.DeleteSave(_saveFileName + _saveFileExtension, _saveFolderName);
			Debug.LogFormat ("Achievements Reset");
		}

		public static void ResetAllAchievements()
		{
			LoadAchievementList ();
			ResetAchievements (_listID);
		}

		/// <summary>
		/// Loads the saved achievements file and updates the array with its content.
		/// </summary>
		public static void LoadSavedAchievements()
		{
			DeterminePath ();
			SerializedMMAchievementManager serializedMMAchievementManager = (SerializedMMAchievementManager)MMSaveLoadManager.Load(typeof(SerializedMMAchievementManager), _saveFileName+ _saveFileExtension, _saveFolderName);
			ExtractSerializedMMAchievementManager(serializedMMAchievementManager);
		}

		/// <summary>
		/// Saves the achievements current status to a file on disk
		/// </summary>
		public static void SaveAchievements()
		{
			DeterminePath ();
			SerializedMMAchievementManager serializedMMAchievementManager = new SerializedMMAchievementManager();
			FillSerializedMMAchievementManager(serializedMMAchievementManager);
			MMSaveLoadManager.Save(serializedMMAchievementManager, _saveFileName+_saveFileExtension, _saveFolderName);
		}

		/// <summary>
		/// Determines the path the achievements save file should be saved to.
		/// </summary>
		private static void DeterminePath(string specifiedFileName = "")
		{
			string tempFileName = (_listID != "") ? _listID : _defaultFileName;
			if (specifiedFileName != "")
			{
				tempFileName = specifiedFileName;
			}

			_saveFileName = tempFileName;
		}

		/// <summary>
		/// Serializes the contents of the achievements array to a serialized, ready to save object
		/// </summary>
		/// <param name="serializedInventory">Serialized inventory.</param>
		public static void FillSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			serializedAchievements.Achievements = new SerializedMMAchievement[_achievements.Count];

			for (int i = 0; i < _achievements.Count(); i++)
			{
				SerializedMMAchievement newAchievement = new SerializedMMAchievement (_achievements[i].AchievementID, _achievements[i].UnlockedStatus, _achievements[i].ProgressCurrent);
				serializedAchievements.Achievements [i] = newAchievement;
			}
		}

		/// <summary>
		/// Extracts the serialized achievements into our achievements array if the achievements ID match.
		/// </summary>
		/// <param name="serializedAchievements">Serialized achievements.</param>
		public static void ExtractSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			if (serializedAchievements == null)
			{
				return;
			}

			for (int i = 0; i < _achievements.Count(); i++)
			{
				for (int j=0; j<serializedAchievements.Achievements.Length; j++)
				{
					if (_achievements[i].AchievementID == serializedAchievements.Achievements[j].AchievementID)
					{
						_achievements [i].UnlockedStatus = serializedAchievements.Achievements [j].UnlockedStatus;
						_achievements [i].ProgressCurrent = serializedAchievements.Achievements [j].ProgressCurrent;
					}
				}
			}
		}
	}
}