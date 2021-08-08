using System.Collections.Generic;
using UnityEngine;

public interface IMissionControl
{
	/// <summary>
	/// Return a position from which an attack on the target can take place.
	/// </summary>
	/// <param name="givenTarget">The target of interest.</param>
	/// <returns>A suitable vector.</returns>
	IEnumerable<Vector3> PlanAttackPath(MissionPlanner missionPlanner, Vector3 startingPosition, Vector3 givenTarget);

	/// <summary>
	/// Payload count for the current weapon/item.
	/// </summary>
	float PayloadCount { get; }

	/// <summary>
	/// Called during Update by the MissionPlanner, giving the implementor a chance to override the
	/// default behavior of the mission planner.
	/// </summary>
	/// <param name="missionPlanner">The mission planner on the object.</param>
	/// <param name="waypointFollower">The waypoint follower on the object.</param>
	/// <returns>True if override behavior defined (the default MissionPlanner actions will not be taken).</returns>
	bool MissionUpdate(MissionPlanner missionPlanner, PathFollower waypointFollower);
}
