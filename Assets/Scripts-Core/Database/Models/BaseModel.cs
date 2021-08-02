using System;

namespace Assets.Scripts.Database.Models
{
	[Serializable]
	public class BaseModel
	{
		public Guid Id { get; set; } = Guid.NewGuid();
	}
}
