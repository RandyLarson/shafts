using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.Database.Models
{
	/// <summary>
	/// Keeping the tally of progress and activity for a particular level.
	/// This is ultimately stored in local game storage.
	/// </summary>
	[Serializable]
	public class LevelStatsModel : BaseModel
	{
		public string LevelName;
		public int LevelAttempt;
		public bool LevelComplete;
		public float TimeOfCompletion;
		public float TimeStarted;
		public int EnemiesDestroyed;
		public int ShotsFired;
		public int ShotsHit;
		public float DamageDone;
		public ResourceStat[] ResourceStatistics;

		public LevelStatsModel()
		{
			Clear();
		}

		public void Clear()
		{
			LevelName = string.Empty;
			LevelAttempt = 1;
			LevelComplete = false;
			TimeStarted = 0;
			EnemiesDestroyed = 0;
			ShotsFired = 0;
			ShotsHit = 0;
			DamageDone = 0;

			ResourceStatistics = new ResourceStat[0];
		}
		
	}
}
