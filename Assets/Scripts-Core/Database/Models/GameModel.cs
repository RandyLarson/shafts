using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Database.Models
{
	/// <summary>
	/// Saved game state, per-play level progression and achievements.
	/// </summary>
	[Serializable]
	public class GameModel : BaseModel
	{
		public List<PlayerModel> KnownPlayers { get; private set; } = new List<PlayerModel>();
		public Guid? LastPlayer { get; set; }
	}
}
