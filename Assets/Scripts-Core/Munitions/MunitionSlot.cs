using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Munitions
{
	public class MunitionSlot
	{
		private Munition InnerMunition;

		public MunitionSlot(Munition payload)
		{
			Munition = payload;
		}

		public int Count { get; set; }

		public Munition Munition
		{
			get => InnerMunition;
			set
			{
				if (InnerMunition == null || InnerMunition.Identification != value.Identification)
					Count = 0;

				InnerMunition = value;
				int ammoDelta = value.Count > 0 ? value.Count : value.Capacity;

				Count = Math.Min(Count + ammoDelta, InnerMunition.Capacity);
			}
		}

	}
}
