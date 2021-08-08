using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Database.Models;

namespace Assets.Scripts.Database
{
	public static class DBToGameConverter
	{
		public static LevelStatsModel AsLevelStatsModel(this GameLevelStats src)
		{
			LevelStatsModel asModel = new LevelStatsModel();

			if (src != null)
			{
				asModel.DamageDone = src.DamageDone;
				asModel.EnemiesDestroyed = src.EnemiesDestroyed;
				asModel.LevelAttempt = src.LevelAttempt;
				asModel.LevelComplete = src.LevelComplete;
				asModel.LevelName = src.LevelName;
				asModel.ShotsFired = src.ShotsFired;
				asModel.ShotsHit = src.ShotsHit;
				asModel.TimeOfCompletion = src.TimeOfCompletion;
				asModel.TimeStarted = src.TimeStarted;
				asModel.ResourceStatistics = src.ResourceStatistics?.Select(srcStat => srcStat.Clone()).ToArray();
			}
			return asModel;
		}


		public static GameLevelStats AsLevelStatsModel(this LevelStatsModel src)
		{
			GameLevelStats asGameStats = new GameLevelStats();

			if (src != null)
			{
				asGameStats.DamageDone = src.DamageDone;
				asGameStats.EnemiesDestroyed = src.EnemiesDestroyed;
				asGameStats.LevelAttempt = src.LevelAttempt;
				asGameStats.LevelComplete = src.LevelComplete;
				asGameStats.LevelName = src.LevelName;
				asGameStats.ShotsFired = src.ShotsFired;
				asGameStats.ShotsHit = src.ShotsHit;
				asGameStats.TimeOfCompletion = src.TimeOfCompletion;
				asGameStats.TimeStarted = src.TimeStarted;
				asGameStats.ResourceStatistics.AddRange(src.ResourceStatistics?.ToList());
			}
			return asGameStats;
		}
	}
}
