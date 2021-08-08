using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using UnityEngine;

public enum ShipKind
{
	Bomber,
	Interceptor,
	Patrol,
	Civilian
}
public enum MissionType
{
	WidePatrol,
	LocalPatrol,
	Assault,
	Intercept,
	CivilianTraffic,
	CivilianFleeing,
	Custom,
	PatrolAndAssault
}


[RequireComponent(typeof(PathFollower))]
public class MissionPlanner : MonoBehaviour
{
	public ShipKind CurrentShipKind = ShipKind.Bomber;
	public ShipMode CurrentShipMode = ShipMode.Idle;
	public MissionType Mission = MissionType.WidePatrol;

	public float RadarDistance = 30f;
	public float MaxDistanceToTarget = 30f;
	public string[] TargetTags;
	public TargetKind MissionTargetKind = TargetKind.GroundAny;
	public bool OnlyLaunchIfTargetAvailable = true;

	/// <summary>
	/// Set if the patrolling targets are all on a particular layer (e.g., PatrolBeacon items).
	/// </summary>
	private int? ScanLayerMask = null;

	/// <summary>
	/// This can be static because we are single threaded in the Unity engine. Each patrol craft will
	/// reuse this as pre-allocated storage.
	/// </summary>
	static Collider2D[] TargetScanResults = new Collider2D[25];

	public Target CurrentTarget { get; private set; }

	private PathFollower WaypointFollower = null;
	private IMissionControl AssaultImpl = null;

	private bool HasRegistered { get; set; } = false;
	public Rect WorkingRegion { get; internal set; }

	public void ChangeShipMode(ShipMode newMode)
	{
		if (HasRegistered && CurrentShipMode != newMode)
		{
			MissionTracker.UpdateShipRegistry(CurrentShipKind, CurrentShipMode, newMode);
			CurrentShipMode = newMode;
		}
	}

	private void Start()
	{
		// We will limit the scan to certain layers depending on the target tags/kind
		// Patrol Beacons
		//	* No Target Tags or
		//	* Only `PatrolTarget` as Tag
		if (TargetTags.Length == 0)
			ScanLayerMask = GameConstants.LayerMaskPatrolBeacon;

		WaypointFollower = gameObject.GetComponent<PathFollower>();
		AssaultImpl = WaypointFollower.gameObject.GetComponent<IMissionControl>();

		if (!HasRegistered)
		{
			MissionTracker.UpdateShipRegistryShipCreated(CurrentShipKind);
			HasRegistered = true;
		}

		SetupMission(Mission);
	}


	private void SetupMission(MissionType missionKind)
	{
		switch (missionKind)
		{
			case MissionType.WidePatrol:
				ChangeShipMode(ShipMode.Patrolling);
				SetupWidePatrolWayPoints(WaypointFollower);
				break;
			case MissionType.LocalPatrol:
				ChangeShipMode(ShipMode.Patrolling);
				SetupHyperLocalPatrolWayPoints(WaypointFollower);
				break;
			case MissionType.CivilianTraffic:
				ChangeShipMode(ShipMode.Other);
				SetupCivilianTrafficWayPoints(WaypointFollower);
				break;
			case MissionType.CivilianFleeing:
				ChangeShipMode(ShipMode.Other);
				SetupCivilianFleeingWayPoints(WaypointFollower);
				break;
		}
	}


	private void OnDestroy()
	{
		if (HasRegistered)
			MissionTracker.UpdateShipRegistryShipDestroyed(CurrentShipKind, CurrentShipMode);
	}


	private void Update()
	{
		if (null == WaypointFollower)
			return;

		// Allow overriding the built in missions we have in the planner.
		// The Raider mission is coded in its own module.
		if (AssaultImpl?.MissionUpdate(this, WaypointFollower) == true)
			return;

		if (CurrentShipMode == ShipMode.Engaged && AssaultImpl?.PayloadCount == 0)
			ChangeShipMode(ShipMode.Leaving);

		if (CurrentShipMode == ShipMode.Leaving)
			return;

		if (CurrentTarget != null && CurrentTarget.IsValidGameObject == true)
			Debug.DrawLine(transform.position, CurrentTarget.gameObject.transform.position, Color.red);

		switch (Mission)
		{
			case MissionType.PatrolAndAssault:
				SetupAssaultWayPoints();
				DoPatrolTasks();
				break;

			case MissionType.Assault:
				//if (!WaypointFollower.HasWaypointPlan || CurrentTarget?.IsValidTarget == false)
				SetupAssaultWayPoints();
				break;
			case MissionType.LocalPatrol:
			case MissionType.WidePatrol:
				DoPatrolTasks();
				break;

			case MissionType.Intercept:
				DoInterceptorTasks();
				DoPatrolTasks();
				break;

			case MissionType.Custom:
				break;
		}
	}


	private void DoPatrolTasks()
	{
		var targetsFound = ScanForTargets();
		if (targetsFound != null && targetsFound.Any())
			foreach (var target in targetsFound)
				MissionTracker.AddTarget(target);
	}


	private void DoInterceptorTasks()
	{
		var toIntercept = MissionTracker.GetClosestTarget(CurrentTarget, MissionTargetKind, transform.position);

		if (toIntercept != null && toIntercept.IsValidGameObject)
        {
			var distanceToTarget = Vector2.Distance(gameObject.transform.position, toIntercept.gameObject.GetPosition());
			if (distanceToTarget > MaxDistanceToTarget)
				toIntercept = null;
		}

		if (toIntercept == null || !toIntercept.IsValidGameObject)
		{
			if (CurrentTarget != null)
			{
				CurrentTarget = null;
				SetupMission(MissionType.LocalPatrol);
			}
		}
		else
		{
		
			CurrentTarget = toIntercept;
			ChangeShipMode(ShipMode.Engaged);
			var wayPoints = AssaultImpl.PlanAttackPath(this, gameObject.transform.position, toIntercept.gameObject.transform.position);
			WaypointFollower.SetWaypoints(wayPoints);
		}
	}

	private IOrderedEnumerable<GameObject> ScanForTargets()
	{
		if (ScanLayerMask.HasValue)
			return ScanForObjectsOnLayer(ScanLayerMask.Value);
		else
			return ScanForObjectsByTag();
	}


	private IOrderedEnumerable<GameObject> ScanForObjectsByTag()
	{
		IOrderedEnumerable<GameObject> targets = null;
		if (TargetTags != null && TargetTags.Length > 0)
		{
			int numHits = Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, RadarDistance, TargetScanResults);
			if (numHits > 0)
				targets = TargetScanResults
					.Where(other => other != null && other.gameObject.tag != null && TargetTags.Contains(other.gameObject.tag))
					.Where(other => other.gameObject != gameObject)
					.Select(other => other.gameObject)
					.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position));
		}

		var result = targets.ToArray();

		return targets;
	}



	private IOrderedEnumerable<GameObject> ScanForObjectsOnLayer(int layerMask)
	{
		// -- Draw the radar 'ring'
		//Vector2 ep = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
		//ep.x -= RadarDistance;
		//Debug.DrawLine(gameObject.transform.position, ep, Color.yellow);
		//ep.x += RadarDistance * 2;
		//Debug.DrawLine(gameObject.transform.position, ep, Color.yellow);
		//ep.x -= RadarDistance;
		//ep.y -= RadarDistance;
		//Debug.DrawLine(gameObject.transform.position, ep, Color.yellow);
		//ep.y += RadarDistance * 2;
		//Debug.DrawLine(gameObject.transform.position, ep, Color.yellow);

		IOrderedEnumerable<GameObject> targets = null;
		int numHits = Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, RadarDistance, TargetScanResults, layerMask);
		if (numHits > 0)
			targets = TargetScanResults
				.Where((other, idx) => idx < numHits)
				.Select(other => other.gameObject)
				.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position));

		return targets;
	}


	private Vector3 PickRandomMapSpot()
	{
		return PointWithin(
			GameController.TheController.LevelBoundary.xMin, GameController.TheController.LevelBoundary.xMax,
			GameController.TheController.MinPatrolAltitude, GameController.TheController.LevelBoundary.yMax);
	}

	public static RaycastHit2D[] SharedHitPool = new RaycastHit2D[2];

	public bool IsPointInsideTerrain(Vector2 whichPoint)
	{
		int numHits = Physics2D.CircleCastNonAlloc(whichPoint, 5f, whichPoint, SharedHitPool, 0f, GameConstants.LayerMaskTerrain);
		return numHits > 0;
	}


	private Vector3 PointWithin(float xmin, float xmax, float ymin, float ymax)
	{
		int attempt = 0;
		Vector3 thePoint;
		do
		{
			thePoint = new Vector3(Random.Range(xmin, xmax), Random.Range(ymin, ymax));
		} while (IsPointInsideTerrain(thePoint) && ++attempt < 4);

		return thePoint;
	}


	public void ChangeTargetTo(Target toAssault)
	{
		if (WaypointFollower == null)
			return;

		if (CurrentTarget == toAssault)
			return;

		MissionTracker.TargetAssigned(CurrentTarget, toAssault, gameObject);
		CurrentTarget = toAssault;
		ChangeShipMode(CurrentTarget == null ? ShipMode.Idle : ShipMode.Engaged);

		// Do we want to exit the field at this point?
		if (toAssault == null)
			return;

		// Get the position of the target and then allow it to be adjusted by the object that is
		// doing the assault itself.
		Vector3 targetPosition = toAssault.gameObject.transform.position;

		IEnumerable<Vector3> startingAssaultPath = null;
		if (AssaultImpl != null)
		{
			startingAssaultPath = AssaultImpl.PlanAttackPath(this, gameObject.transform.position, targetPosition);
		}
		else
		{
			var exitPt = new Vector3(targetPosition.x + Random.Range(-40, 40), GameController.TheController.LevelBoundary.yMax + 500);
			startingAssaultPath = new Vector3[] { targetPosition, exitPt };
		}

		WaypointFollower.SetWaypoints(startingAssaultPath);
	}

	public void PlotCourse(IEnumerable<Vector3> theCourse)
	{
		WaypointFollower.SetWaypoints(theCourse);
	}

	public void PlotCourseTo(Vector3 position)
	{
		Vector3[] path = new Vector3[] { position };
		WaypointFollower.SetWaypoints(path);
	}

	public void PlotCourseTo(GameObject go)
	{
		if (go == null)
		{
			Debug.LogError("MissionPlanner // PlotCourse given null GameObject.");
			return;
		}
		PlotCourseTo(go.transform.position);
	}

	private void SetupAssaultWayPoints()
	{
		Target toAssault = MissionTracker.GetClosestTarget(CurrentTarget, TargetKind.GroundAny, gameObject.transform.position);
		ChangeTargetTo(toAssault);
	}

	private void SetupCivilianTrafficWayPointsLocal(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		float minAltitude = GameController.TheController.MinCivilianAltitude;
		float maxAltitude = GameController.TheController.LevelBoundary.yMax + (GameController.TheController.LevelBoundary.yMax * .5f);
		float yRegionCenter = (minAltitude + maxAltitude) / 2;

		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;
		float xRegionCenter = (xMin + xMax) / 2;

		// Pick a bottom-left point, then points offset from there.
		float x = Random.Range(xMin, xMax);
		float y = Random.Range(minAltitude, maxAltitude);

		float diameter = 200;

		List<Vector3> waypoints = new List<Vector3>();
		for (int i = 0; i < 3; i++)
		{
			int attempt = 0;
			Vector2 wp;
			do
			{
				wp = Random.insideUnitCircle * diameter;
				wp.x = wp.x + x;
				wp.y = Mathf.Min(maxAltitude, Mathf.Max(minAltitude, wp.y + y));
			} while (IsPointInsideTerrain(wp) && ++attempt < 3);
			waypoints.Add(wp);
		}

		waypointFollower.SetWaypoints(waypoints);
	}

	private void SetupCivilianTrafficWayPoints(PathFollower waypointFollower)
	{
		if (Random.value < .5)
			SetupCivilianTrafficWayPointsLocal(waypointFollower);
		else
			SetupCivilianTrafficWayPointsWide(waypointFollower);
	}


	private void SetupCivilianTrafficWayPointsWide(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		float yMin = GameController.TheController.MinCivilianAltitude;
		float yMax = GameController.TheController.LevelBoundary.yMax + 50;
		float yRegionCenter = (yMin + yMax) / 2;

		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;
		float xRegionCenter = (xMin + xMax) / 2;

		List<Vector3> waypoints = new List<Vector3>();
		for (int i = 0; i < 3; i++)
		{
			waypoints.Add(PointWithin(xMin, xMax, yMin, yMax));
		}

		waypointFollower.SetWaypoints(waypoints);
	}


	private void SetupCivilianFleeingWayPoints(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		float minAltitude = GameController.TheController.MinPatrolAltitude;

		// Pick a point away from the current spot, then one off the map
		float altitude = Mathf.Max(minAltitude, Random.Range(GameController.TheController.LevelBoundary.yMin, GameController.TheController.LevelBoundary.yMax + 50));

		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;

		float x1 = Random.Range(xMin, xMax);
		float x2 = x1 + (x1 - transform.position.x) * 10;
		var pt1 = PointWithin(xMin, xMax, minAltitude, GameController.TheController.LevelBoundary.yMax + 50);
		var pt2 = PointWithin(x2, x2 + 20, altitude, altitude + 100);
		waypointFollower.SetWaypoints(new Vector3[]
		{
			pt1,
			pt2
		});

	}


	private void SetupHyperLocalPatrolWayPoints(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		float minAltitude = GameController.TheController.MinPatrolAltitude;

		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;
		float yMin = WorkingRegion != Rect.zero ? WorkingRegion.yMin : minAltitude;
		float yMax = WorkingRegion != Rect.zero ? WorkingRegion.yMax : GameController.TheController.LevelBoundary.xMax;

		List<Vector3> waypoints = new List<Vector3>();
		for (int i = 0; i < 3; i++)
		{
			int attempt = 0;
			Vector2 wp;
			do
			{
				wp.x = Random.Range(xMin, xMax);
				wp.y = Random.Range(yMin, yMax);
			} while (IsPointInsideTerrain(wp) && ++attempt < 3);
			waypoints.Add(wp);
		}

		waypointFollower.SetWaypoints(waypoints);
	}
	
	private void SetupLocalPatrolWayPoints(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		float minAltitude = GameController.TheController.MinPatrolAltitude;

		// Pick a center point, then points within a circle centered there.
		float radius = 100;
		float altitude = Mathf.Max(minAltitude, Random.Range(GameController.TheController.LevelBoundary.yMin, GameController.TheController.LevelBoundary.yMax + 50));

		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;

		float offset = Random.Range(xMin + radius, xMax - radius);

		List<Vector3> waypoints = new List<Vector3>();
		for (int i = 0; i < 3; i++)
		{
			int attempt = 0;
			Vector2 wp;
			do
			{
				wp = Random.insideUnitCircle * radius;
				wp.x += offset;
				wp.y = Mathf.Max(minAltitude, wp.y + altitude);
			} while (IsPointInsideTerrain(wp) && ++attempt < 3);
			waypoints.Add(wp);
		}

		waypointFollower.SetWaypoints(waypoints);
	}

	private void SetupWidePatrolWayPoints(PathFollower waypointFollower)
	{
		if (waypointFollower == null)
			return;

		// Constrain by the WorkingRegion if non-zero
		float xMin = WorkingRegion != Rect.zero ? WorkingRegion.xMin : GameController.TheController.LevelBoundary.xMin;
		float xMax = WorkingRegion != Rect.zero ? WorkingRegion.xMax : GameController.TheController.LevelBoundary.xMax;

		float yMin = Mathf.Max(GameController.TheController.MinPatrolAltitude, WorkingRegion != Rect.zero ? WorkingRegion.yMin : GameController.TheController.MinPatrolAltitude);
		float yMax = WorkingRegion != Rect.zero ? WorkingRegion.yMax : GameController.TheController.LevelBoundary.yMax;


		// Pick an altitude to patrol, then horizontal points.

		float altitude = Random.Range(yMin, yMax);
		float left = Random.Range(xMin, xMax / 2);
		float right = Random.Range(xMax / 2, xMax);

		float quarterStep = (left + right) / 4;
		waypointFollower.SetWaypoints(new Vector3[]
		{
			PointWithin(quarterStep*3, quarterStep*3, altitude-30, altitude+30),
			PointWithin(quarterStep*2, quarterStep*3, altitude-30, altitude+30),
			PointWithin(left, left+10, altitude-30, altitude+30),
			PointWithin(quarterStep*2, quarterStep*3, altitude-30, altitude+30),
			PointWithin(quarterStep*3, quarterStep*3, altitude-30, altitude+30),
			PointWithin(right, right-10, altitude-30, altitude+30)
		});
	}
}
