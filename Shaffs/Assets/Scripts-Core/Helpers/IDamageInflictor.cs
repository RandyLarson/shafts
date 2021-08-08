using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Helpers
{
	public interface IDamageInflictor
	{
		void ApplyAreaDamage();
		void DestroySelf();
	}
}
