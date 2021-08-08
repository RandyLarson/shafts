using Assets.Scripts.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(ShipCharacteristics))]
public class PathFollower : MonoBehaviour
{
	public Transform[] Waypoints;
	public Vector3[] WorkingWaypoints;
	public bool FollowBackToStart = true;
	public float MaxNumberOfCycles = 0;
	private int CyclesCompleted = 0;
	public bool Paused = false;
	public bool DestroyWhenDone = false;
	public bool SlowAtWayPoints = true;
	public bool DebugPathVisuals = false;

	public string[] AdditionalTagsToAvoid = null;

	private Rigidbody2D OurRB;
	private ShipCharacteristics ShipCharacteristics;

	IEnumerator Follower()
	{
		int i = 0;
		int increment = 1; // forward
		float maxSpeedSquared = ShipCharacteristics.MaxSpeed * ShipCharacteristics.MaxSpeed;

		if (ShipCharacteristics == null)
		{
			Debug.LogError("ShipCharacteristics required, but missing.");
			yield break;
		}

		while (MaxNumberOfCycles == 0 || CyclesCompleted < MaxNumberOfCycles)
		{
			yield return new WaitForFixedUpdate();

			// we're at the index we want
			if (!Paused && !ShipCharacteristics.AreSystemsDisabled && WorkingWaypoints != null && WorkingWaypoints.Length > 0)
			{
				if (i >= WorkingWaypoints.Length)
					i = 0;

				MoveToward.DebugVisualize = GameController.TheController.DiagnosticPathFindingVisualize && DebugPathVisuals;

				MoveToward.MoveToIntercept(
					ShipCharacteristics,
					OurRB,
					0,
					WorkingWaypoints[i],
					Vector3.zero,
					SlowAtWayPoints,
					maxSpeedSquared,
					AdditionalTagsToAvoid);

				float dx = Vector2.Distance(transform.position, WorkingWaypoints[i]);
				if (dx < 10)
				{
					// On to next point
					i += increment;
					if (i < 0 || i >= WorkingWaypoints.Length)
					{
						increment = -increment;
						CyclesCompleted++;
					}
					i = Mathf.Clamp(i, 0, WorkingWaypoints.Length - 1);
				}

			}
			yield return null;
		}

		if (DestroyWhenDone)
			Destroy(OurRB.gameObject);

		yield break;
	}

	private void OnDrawGizmosSelected()
	{
		if (WorkingWaypoints == null)
			return;

		Vector3 op = transform.position;
		foreach (var pt in WorkingWaypoints)
		{
			Debug.DrawLine(op, pt, Color.white);
			op = pt;
		}
	}

	public bool HasWaypointPlan { get { return WorkingWaypoints.Length > 0; } }

	public void SetWaypoints(IEnumerable<Vector3> newWaypoints)
	{
		WorkingWaypoints = newWaypoints.ToArray();
	}

	internal void ClearWaypoints()
	{
		WorkingWaypoints = new Vector3[0];
	}

	public void Awake()
	{
	}

	private void Start()
	{
		OurRB = GetComponent<Rigidbody2D>();
		ShipCharacteristics = GetComponent<ShipCharacteristics>();

		// Clone the array of points so we have a snapshot of the global coordinates.
		// Otherwise the points will be relative to us (the parent).
		if (Waypoints != null && Waypoints.Length > 0)
		{
			WorkingWaypoints = Waypoints
				.Where(t => t != null)
				.Select(t => t.position)
				.ToArray();
		}

		StartCoroutine(Follower());
	}

	internal void SetWaypointsFromParent(GameObject wayPoints)
	{
		WorkingWaypoints = new Vector3[wayPoints.transform.childCount];
		for (int i = 0; i < wayPoints.transform.childCount; i++)
		{
			WorkingWaypoints[i] = wayPoints.transform.GetChild(i).transform.position;
		}
	}
}