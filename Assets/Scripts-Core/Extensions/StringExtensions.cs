using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
	public static  class StringExtensions
	{
		public static bool EqualsIgnoreCase(this string lhs, string rhs)
		{
			if (lhs == null)
				return false;

			return string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
		}
	}
}
