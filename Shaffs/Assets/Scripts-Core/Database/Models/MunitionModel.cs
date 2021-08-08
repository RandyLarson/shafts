using System;

namespace Assets.Scripts.Database.Models
{
	[Serializable]
	public class MunitionModel : BaseModel
	{
		public string MunitionObjectName { get; set; }
		public int Count { get; set; }
		public int Capacity { get; set; }
	}
}
