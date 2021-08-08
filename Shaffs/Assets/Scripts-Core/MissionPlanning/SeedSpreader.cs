using Assets.Scripts.Helpers;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpreader : MonoBehaviour, IMissionControl
{
	public SpawnPayload WhatToSpread;
	public float SpreadAltitude = 80f;
	public float SpreadDistance = 30f;
	private Vector3? Target;

	public IEnumerable<Vector3> PlanAttackPath(MissionPlanner missionPlanner, Vector3 startingPosition, Vector3 givenTarget)
	{
		// Need to dip below the spread altitude a bit to ensure the trigger.
		// Stagger the x a bit to one side or the other.
		List<Vector3> plan = new List<Vector3>();

		Target = givenTarget;

		int mult = (startingPosition.x > givenTarget.x) ? 1 : -1;

		var xPos = givenTarget.x + Random.Range(60, 100) * mult;
		var yPos = SpreadAltitude - 5;
		plan.Add(new Vector3(xPos, yPos));

		xPos = givenTarget.x + Random.Range(-10, 10);
		//yPos = SpreadAltitude - 5;
		plan.Add(new Vector3(xPos, yPos));

		plan.Add(new Vector3(givenTarget.x + Random.Range(-50, 50), 2000));
		return plan;
	}

	public float PayloadCount { get => WhatToSpread?.InventoryCount ?? 0; }

	void Start()
	{
		if (WhatToSpread != null)
			WhatToSpread.CanSpawn = false;
	}

	void Update()
	{
		if (WhatToSpread == null)
			return;

		if (!WhatToSpread.CanSpawn &&
			transform.position.y <= SpreadAltitude && Target.HasValue && (Mathf.Abs(Target.Value.x - transform.position.x) < SpreadDistance))
		{
			WhatToSpread.CanSpawn = true;
		}

		WhatToSpread.TrySpawn();
	}

	public bool MissionUpdate(MissionPlanner missionPlanner, PathFollower waypointFollower)
	{
		return false;
	}
}
