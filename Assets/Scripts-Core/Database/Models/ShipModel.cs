using System;
using System.Collections.Generic;

namespace Assets.Scripts.Database.Models
{
	[Serializable]
	public class ShipModel :BaseModel
	{
		/// <summary>
		/// Name of the game asset
		/// </summary>
		public string ShipName { get; set; }

		/// <summary>
		/// Currently installed munition inventory.
		/// </summary>
		public List<MunitionModel> MunitionRack { get; set; }

		/// <summary>
		/// Current shield
		/// </summary>
		public ShieldModel ShieldInstalled { get; set; }
	}
}
