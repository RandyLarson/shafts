using System;
using System.Collections.Generic;

namespace Assets.Scripts.Database.Models
{
	[Serializable]
	public class PlayerModel :BaseModel
	{
		/// <summary>
		/// Player identifier
		/// </summary>
		public string PlayerName { get; set; }

		/// <summary>
		/// The name of the current level scene.
		/// </summary>
		public string CurrentLevelName;

		/// <summary>
		/// The currently active ship.
		/// </summary>
		public Guid? CurrentShipId;

		/// <summary>
		/// Stats for levels played
		/// </summary>
		public List<LevelStatsModel> LevelHistory { get; private set; } = new List<LevelStatsModel>();

		public List<ShipModel> AvailableShips { get; private set; } = new List<ShipModel>();

		// Input preference
		public PlayerInputStyle? UserInputStyle { get; set; } = null;

		// Input mapping overrides
		// here => | |


		public PlayerModel()
		{
			Id = Guid.NewGuid();
		}

	}
}
