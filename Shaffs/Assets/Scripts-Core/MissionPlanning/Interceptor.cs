using System.Collections.Generic;
using UnityEngine;

class Interceptor : MonoBehaviour, IMissionControl
{
	public bool ApproachXClosestSide = true;
	public bool ApproachYClosestSide = true;
	public Vector2 ApproachOffset = Vector2.zero;

	protected virtual void Start()
	{
	}

	public virtual float PayloadCount => 1;

	public virtual bool MissionUpdate(MissionPlanner missionPlanner, PathFollower waypointFollower)
	{
		// No override
		return false;
	}

	/// <summary>
	/// Plots a course directly to the given location, taking into account constraints (mountains, angle of approach)
	/// </summary>
	public virtual IEnumerable<Vector3> PlanAttackPath(MissionPlanner missionPlanner, Vector3 startingPosition, Vector3 givenTarget)
	{

		// Working region -- constrain to this area if designated to do so.
		// If the target is outside our region
		if (missionPlanner != null && !missionPlanner.WorkingRegion.Contains(givenTarget))
		{

		}

		// Account for constraints, such as approaching from above.
		float xOffset = ApproachOffset.x;
		float yOffset = ApproachOffset.y;

		if (ApproachXClosestSide)
			xOffset = (givenTarget.x < startingPosition.x) ? Mathf.Abs(xOffset) : -(Mathf.Abs(xOffset));

		if (ApproachYClosestSide)
			yOffset = (givenTarget.y < startingPosition.y) ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);

		Vector3 targetVector = new Vector3(givenTarget.x + xOffset, givenTarget.y + yOffset, givenTarget.z);
		

		return new Vector3[] { targetVector };
	}


}
