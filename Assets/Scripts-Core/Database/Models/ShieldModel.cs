using System;

namespace Assets.Scripts.Database.Models
{
	[Serializable]
	public class ShieldModel : BaseModel
	{
		public string ShieldObjectName { get; set; }
		public float HP { get; set; }
	}
}
