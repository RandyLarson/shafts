using System;
using UnityEngine;

namespace Assets.Scripts.MissionPlanning
{
	public class Waypoint
	{
		public Vector3 Location;
		public Vector3 Direction;
	}

	class Pathfinding
	{
		public Waypoint[] WaypointsTo(Vector2 startingLocation, Vector2 destination, float maxWaypointDistance)
		{
			//for (int i = 0; i<)
			throw new NotImplementedException();

		}

		public bool IsObstructed(Vector2 origin, Vector2 destination)
		{
			var hits = Physics2D.RaycastAll(origin, destination, Vector2.Distance(origin, destination));

			foreach (var hit in hits)
			{
				if (hit.collider.gameObject.CompareTag(GameConstants.Terrain)) //  || hit.collider.gameObject.CompareTag("HomeTeam"))
					return true;
			}

			return false;
		}

	}
}
