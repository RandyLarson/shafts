using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
	class ScanningHelpers
	{
		static public IEnumerable<GameObject> ScanForTargets(GameObject gameObject, float radarDistance, IEnumerable<string> targetTags)
		{
			if (targetTags?.Any() == false)
				yield break;

			var targets = Physics2D.OverlapCircleAll(gameObject.transform.position, radarDistance)
				.Where(other => targetTags.Contains(other.gameObject.tag))
				.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position));

			foreach (var nxt in targets)
				yield return nxt.gameObject;
		}
	}
}
