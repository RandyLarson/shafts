using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using Assets.Scripts.Database;
using Assets.Scripts.Database.Models;
using UnityEngine;

namespace Assets.Scripts.SavedGame
{
	public class DatabaseController
	{
		GameModel InnerDb { get; set; }

		public void SaveCurrentPlayer(PlayerModel playerState)
		{

		}

		string LocalPlayerPath { get => Path.Combine(Application.persistentDataPath, "Milkman.save"); }

		void BackupCurrentDatabase()
		{
			try
			{
				string finalPath = LocalPlayerPath;
				if (File.Exists(finalPath))
				{
					string dstFile = string.Join(".", finalPath, DateTime.Now.Ticks, "backup");
					File.Copy(finalPath, dstFile);
				}
			}
			catch (Exception caught)
			{
				Debug.LogError(caught);
				throw;
			}
		}

		private XmlSerializer CreateDbSerializer()
		{
			XmlSerializer xs = null;
			try
			{
				xs = new XmlSerializer(typeof(GameModel));
			}
			catch (FileNotFoundException)
			{
			}
			return xs;
		}

		public bool Save()
		{
			try
			{
				BackupCurrentDatabase();

				string finalPath = LocalPlayerPath;
				XmlSerializer xs = new XmlSerializer(typeof(GameModel));

				using (Stream fs = File.Create(finalPath))
				{
					xs.Serialize(fs, InnerDb);
					//var bf = new BinaryFormatter();
					//bf.Serialize(fs, InnerDb);
				}
				return true;
			}
			catch (Exception caught)
			{
				Debug.LogError(caught);
				return false;
			}
		}

		public void Load()
		{
			try
			{
				string finalPath = LocalPlayerPath;
				if (File.Exists(finalPath))
				{
					XmlSerializer xs = new XmlSerializer(typeof(GameModel));
					using (FileStream fs = File.OpenRead(finalPath))
					{
						InnerDb = (GameModel) xs.Deserialize(fs);
					}
				}
				else
				{
					InitializeNewGameDatabase();
					Save();
				}
			}
			catch (Exception caught)
			{
				Debug.LogError(caught);
			}
		}

		public PlayerModel CreateNewPlayer()
		{
			PlayerModel newPlayer = new PlayerModel();
			return newPlayer;
		}

		public bool IsValid { get => InnerDb != null; }

		private void ValidateDb()
		{
			if (InnerDb == null)
				throw new InvalidOperationException("Game database not initialized");
		}

		public void AddPlayer(PlayerModel player)
		{
			ValidateDb();
			if (!InnerDb.KnownPlayers.Exists(existing => existing.Id == player.Id))
				InnerDb.KnownPlayers.Add(player);

			if (InnerDb.KnownPlayers.Count == 1)
				InnerDb.LastPlayer = player.Id;
		}

		public void MakePlayerDefault(PlayerModel player)
		{
			ValidateDb();

			if (player == null)
				throw new ArgumentNullException("player", "Player is null");

			InnerDb.LastPlayer = player.Id;
		}

		private void InitializeNewGameDatabase()
		{
			InnerDb = new GameModel();
			PlayerModel player = CreateNewPlayer();
			AddPlayer(player);
		}

		public PlayerModel GetDefaultPlayer()
		{
			ValidateDb();
			PlayerModel playerProfile = null;
			if (InnerDb.LastPlayer.HasValue)
			{
				playerProfile = InnerDb.KnownPlayers.Where(existing => existing.Id == InnerDb.LastPlayer).FirstOrDefault();
				if (playerProfile == null)
				{
					Debug.LogError($"Player `{InnerDb.LastPlayer}` not found in known players.");
				}

			}

			if (playerProfile == null)
			{
				// Create the default profile
				PlayerModel player = new PlayerModel();
				AddPlayer(player);
				InnerDb.LastPlayer = player.Id;
			}

			return playerProfile;
		}

		public void SaveLevelStatistics(PlayerModel playerModel, GameLevelStats stats)
		{
			// Store it as the level name + the attempt
			LevelStatsModel asModel = stats.AsLevelStatsModel();

			LevelStatsModel existingStats = playerModel?.LevelHistory
				.Find(existing => existing.LevelName == stats.LevelName && existing.LevelAttempt == stats.LevelAttempt);

			if (existingStats != null)
				playerModel.LevelHistory.Remove(existingStats);

			playerModel.LevelHistory.Add(asModel);
		}

		public LevelStatsModel GetMostStatisticsForLevel(PlayerModel playerModel, string levelName)
		{
			// Store it as the level name + the attempt
			LevelStatsModel mostRecentStats = playerModel?.LevelHistory?
				.Where(existing => existing.LevelName == levelName)
				.OrderByDescending(existing => existing.LevelAttempt)
				.FirstOrDefault();

			return mostRecentStats;
		}

		public void SaveCurrentPlayerShip(ShipModel currentShip, bool makeDefault)
		{
			PlayerModel owningPlayer = GetDefaultPlayer();

			if (!owningPlayer.AvailableShips.Any(existing => existing.Id == currentShip.Id))
			{
				owningPlayer.AvailableShips.Add(currentShip);
			}

			if (makeDefault)
			{
				owningPlayer.CurrentShipId = currentShip.Id;
			}
		}
	}
}
