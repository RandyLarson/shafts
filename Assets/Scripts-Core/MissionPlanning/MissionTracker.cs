using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Extensions;
using Assets.Scripts.MissionPlanning;
using Assets.Scripts.UI;
using UnityEngine;


public enum ShipMode
{
	Idle,
	Patrolling,
	Engaged,
	Leaving,
	Destroyed,
	Other
}

[Serializable]
public class ShipTrackInfo
{
	/// <summary>
	/// Its type.
	/// </summary>
	public ShipKind Kind;

	/// <summary>
	/// The total number on the board.
	/// </summary>
	public int NumActive;

	/// <summary>
	/// Bombers - the number that have dropped their payload and are exiting the area.
	/// </summary>
	public int NumLeaving;

	/// <summary>
	/// Actively seeking target or objective.
	/// </summary>
	public int NumEngaged;
}



public static class MissionTracker
{
	private static int? targetIndex = null;
	private static List<Target> TrackedTargets = new List<Target>();
	private static PlayerTarget PlayerTarget { get; set; } = new PlayerTarget();

	private static Target MostRecent = null;
	private static List<GameObject> TargetMarkers = new List<GameObject>();
	private static Dictionary<ShipKind, ShipTrackInfo> ShipRegistry { get; set; } = CreateShipRegistry();
	private static Dictionary<ShipKind, ShipTrackInfo> PlanningConfiguration { get; set; } = new Dictionary<ShipKind, ShipTrackInfo>();

	public static Dictionary<ShipKind, ShipTrackInfo> CreateShipRegistry()
	{
		var theRegistry = new Dictionary<ShipKind, ShipTrackInfo>()
		{
			{ShipKind.Bomber, new ShipTrackInfo(){Kind=ShipKind.Bomber} },
			{ShipKind.Interceptor, new ShipTrackInfo(){Kind=ShipKind.Interceptor} },
			{ShipKind.Patrol, new ShipTrackInfo(){Kind=ShipKind.Patrol} }
		};

		return theRegistry;
	}

	public static void GatherDiagnostics()
	{
		return;

		DiagnosticController.Add($"Player Valid: {PlayerTarget.IsValidGameObject}");
		DiagnosticController.Add($"Player At: {PlayerTarget.Location}");
		DiagnosticController.Add($"Target Count: {TrackedTargets.Count()}");

		DiagnosticController.Add("");
		DiagnosticController.Add("Limits:");
		foreach (var config in PlanningConfiguration)
		{
			DiagnosticController.Add($"{config.Key}");
			DiagnosticController.Add($"   Active: {config.Value.NumActive}");
			DiagnosticController.Add($"   Engaged: {config.Value.NumEngaged}");
			DiagnosticController.Add($"   Leaving: {config.Value.NumLeaving}");
		}

		DiagnosticController.Add("");
		DiagnosticController.Add("Registry:");
		foreach (var regEntry in ShipRegistry)
		{
			DiagnosticController.Add($"{regEntry.Key}");
			DiagnosticController.Add($"   Active: {regEntry.Value.NumActive}");
			DiagnosticController.Add($"   Engaged: {regEntry.Value.NumEngaged}");
			DiagnosticController.Add($"   Leaving: {regEntry.Value.NumLeaving}");
		}
	}

	static MissionTracker()
	{
	}

	private static MissionPlannerConfig InnerMissionPlanningConfig { get; set; }
	public static MissionPlannerConfig PlannerConfig
	{
		get => InnerMissionPlanningConfig;
		internal set
		{
			InnerMissionPlanningConfig = value;
			PlanningConfiguration = InnerMissionPlanningConfig?.Configuration.ToDictionary(info => info.Kind);
		}

	}


	/// <summary>
	/// Test the given kind against any limits imposed by the current level to see if it is allowed;
	/// </summary>
	/// <param name="kind">The kind of the ship.</param>
	/// <returns>True if it is within the limits.</returns>
	public static bool CanLaunch(ShipKind kind)
	{
		if (PlanningConfiguration.TryGetValue(kind, out ShipTrackInfo kindPlan) &&
			 ShipRegistry.TryGetValue(kind, out ShipTrackInfo kindTrackInfo))
		{
			return kindTrackInfo.NumActive < kindPlan.NumActive &&
					(kindTrackInfo.NumEngaged == 0 || kindTrackInfo.NumEngaged < kindPlan.NumEngaged);
		}

		return true;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="mission"></param>
	/// <param name="onlyIfTargetsAvailable"></param>
	/// <param name="targetRegion">A non-zero rect will constrain the search for targets to the given region. Valid only if {onlyIfTargetsAvailable} is true.</param>
	/// <returns></returns>
	internal static bool CanLaunch(MissionType mission, TargetKind targetKind, bool onlyIfTargetsAvailable, Rect targetRegion)
	{
		ShipKind asKind = ShipKind.Bomber;

		switch (mission)
		{
			case MissionType.WidePatrol:
			case MissionType.LocalPatrol:
				asKind = ShipKind.Patrol;
				break;

			case MissionType.Assault:
				asKind = ShipKind.Bomber;
				break;

			case MissionType.Intercept:
				asKind = ShipKind.Interceptor;
				break;

			case MissionType.CivilianTraffic:
				asKind = ShipKind.Civilian;
				break;
		}

		// Check threshold values for counts per ship type to 
		// see if we are at the limit for the kind of ship we are trying to launch.
		bool canLaunch = CanLaunch(asKind);

		if (canLaunch && onlyIfTargetsAvailable)
		{
			switch (asKind)
			{
				case ShipKind.Bomber:
				case ShipKind.Patrol:
					if (!AnyTargets(targetRegion, targetKind))
						canLaunch = false;
					break;
				case ShipKind.Interceptor:
					var thePlayer = GetPlayerTarget();

					if (thePlayer == null)
					{
						canLaunch = false;
					}
					else
					{
						var position = thePlayer.gameObject.GetPosition();
						if (position != Vector3.zero && targetRegion != Rect.zero && !targetRegion.Contains(position))
							canLaunch = false;
					}
					break;
			}
		}

		return canLaunch;
	}


	/// <summary>
	/// Notice that a new ship has been spawned.
	/// </summary>
	/// <param name="kind"></param>
	public static void UpdateShipRegistryShipCreated(ShipKind kind)
	{
		if (ShipRegistry.TryGetValue(kind, out ShipTrackInfo kindTrackInfo))
		{
			kindTrackInfo.NumActive++;
			UpdateShipRegistry(kindTrackInfo, ShipMode.Idle, ShipMode.Idle);
		}
	}

	public static void UpdateShipRegistryShipDestroyed(ShipKind kind, ShipMode itsMode)
	{
		if (ShipRegistry.TryGetValue(kind, out ShipTrackInfo kindTrackInfo))
		{
			kindTrackInfo.NumActive--;
			UpdateShipRegistry(kindTrackInfo, itsMode, ShipMode.Destroyed);
		}
	}


	public static void UpdateShipRegistry(ShipTrackInfo kindTrackInfo, ShipMode oldMode, ShipMode newMode)
	{
		if (oldMode != newMode)
		{
			switch (oldMode)
			{
				case ShipMode.Engaged:
					kindTrackInfo.NumEngaged--;
					break;
				case ShipMode.Leaving:
					kindTrackInfo.NumLeaving--;
					break;
			}

			switch (newMode)
			{
				case ShipMode.Engaged:
					kindTrackInfo.NumEngaged++;
					break;

				case ShipMode.Leaving:
					kindTrackInfo.NumLeaving++;
					break;
			}
		}
	}

	public static void UpdateShipRegistry(ShipKind shipKind, ShipMode oldMode, ShipMode newMode)
	{
		if (ShipRegistry.TryGetValue(shipKind, out ShipTrackInfo kindTrackInfo))
		{
			UpdateShipRegistry(kindTrackInfo, oldMode, newMode);
		}
	}


	public static void ResetTargetList()
	{
		TrackedTargets.Clear();
		TargetMarkers.Clear();
		ShipRegistry = CreateShipRegistry();
		MostRecent = null;
		targetIndex = null;
	}

	public static void AddTarget(GameObject target)
	{
		if (target == null)
			return;

		// Classify -
		//
		// The Player's ship is tracked, but not added to the ground target queue
		// Buildings go to the ground target queue
		if (target.CompareTag(GameConstants.Player))
		{
			PlayerTarget.Location = target.GetPosition();
			PlayerTarget.WhenFound = Time.time;
			PlayerTarget.gameObject = target.GetTopmostParent();
			return;
		}

		// Civilian Air Targets
		if (target.CompareTag(GameConstants.CiviAircraft))
		{
			TrackTarget(TargetKind.CivilianAir, target);
			return;
		}
		else
		{
			// Ground Targets
			TrackTarget(TargetKind.GroundAny, target);
		}

	}

	private static void TrackTarget(TargetKind kind, GameObject target)
	{
		if (target == null)
			return;

		var existingTarget = TrackedTargets.FirstOrDefault(kt => kt.gameObject == target);

		if (existingTarget != null)
		{
			existingTarget.WhenFound = Time.time;
		}
		else
		{
			var nextTarget = new Target()
			{
				Kind = kind,
				gameObject = target,
				WhenFound = Time.time
			};

			TrackedTargets.Add(nextTarget);

			Debug.Log($"Found target: {nextTarget.Kind} {nextTarget.gameObject.name}, {nextTarget.gameObject.GetPosition()}");
		}
	}

	public static Target GetMostRecentTarget()
	{
		if (MostRecent?.IsValidGameObject == true)
			MostRecent = null;

		return MostRecent;
	}

	public static bool AnyTargets(Rect withinRegion, TargetKind targetKind)
	{
		if (withinRegion == Rect.zero)
			return TrackedTargets.Any(kt => (kt.Kind & targetKind) != 0);
		else
			return TrackedTargets.Any(potential =>
				potential.IsValidGameObject &&
				(potential.Kind & targetKind) != 0 &&
				withinRegion.Contains(potential.gameObject.GetPosition()));
	}


	public static Target GetNextTarget()
	{
		if (targetIndex == null)
			targetIndex = 0;
		else
			targetIndex = (targetIndex + 1) % TrackedTargets.Count;

		Target nextTarget = null;
		while (nextTarget == null && TrackedTargets.Any())
		{
			nextTarget = TrackedTargets[targetIndex.Value];
			if (nextTarget.gameObject == null)
			{
				TrackedTargets.Remove(nextTarget);
				if (targetIndex >= TrackedTargets.Count)
					targetIndex = 0;
			}
			else
			{
				MostRecent = nextTarget;
				return nextTarget;
			}
		}

		return null;
	}

	private static void PruneNullTargets()
	{
		TrackedTargets.Where(t => t.gameObject == null)
			.ToList()
			.ForEach(t =>
			{
				Debug.Log($"Removing item {t.Kind} from target queue");
				TrackedTargets.Remove(t);
			});

		TrackedTargets.ForEach(t => t.PruneAttackers());
	}


	internal static void TargetAssigned(Target oldTarget, Target chosenTarget, GameObject assignedAttacker)
	{
		if (oldTarget?.gameObject != null)
			oldTarget.RemoveAttacker(assignedAttacker);

		chosenTarget?.AddAttacker(assignedAttacker);
	}


	public static void DrawTargetMarkers()
	{
		PruneNullTargets();
		foreach (var target in TrackedTargets)
		{
			if (target.IsValidGameObject && target.gameObject.GetPosition() != Vector3.zero)
			{
				Debug.DrawLine(target.gameObject.GetPosition(), new Vector3(target.gameObject.transform.position.x, target.gameObject.transform.position.y + 40, 0), Color.red);
			}
		}

		if (GetPlayerTarget()?.IsValidGameObject == true)
			Debug.DrawLine(PlayerTarget.gameObject.GetPosition(), new Vector3(PlayerTarget.gameObject.transform.position.x, PlayerTarget.gameObject.transform.position.y + 40, 0), Color.red);
	}

	public static Target GetClosestTarget(Target currentTarget, TargetKind targetKind, Vector3 toLocation, int maxCurrentAttackers = 3)
	{
		if (targetKind == TargetKind.Player)
			return MissionTracker.GetPlayerTarget();

		PruneNullTargets();

		var targetsByDistance = TrackedTargets
			.Where(other => other != null && other.IsValidGameObject)
			.Where(other => 0 != (other.Kind & targetKind))
			.OrderBy(other => Vector2.Distance(toLocation, other.gameObject.GetPosition()));

		// First take the best below the threshold of attacker count.
		// If there are none, fall back to the target with the least number of attackers.
		var closest = targetsByDistance
			.FirstOrDefault(other => other == currentTarget || other.AssignedAttackers.Count() < maxCurrentAttackers);

		if (closest == null)
		{
			closest = targetsByDistance
				.OrderBy(target => target.AssignedAttackers.Count)
				.FirstOrDefault();
		}

		if (closest != null)
			MostRecent = closest;

		return closest;
	}


	public static Target GetOldestTarget()
	{
		while (TrackedTargets.Any())
		{
			Target t = TrackedTargets.ElementAt(0);
			if (!t.IsValidGameObject)
			{
				TrackedTargets.Remove(t);
			}
			else
			{
				MostRecent = t;
				return t;
			}
		}
		return null;
	}


	public static PlayerTarget GetPlayerTarget()
	{
		if (PlayerTarget != null &&
			PlayerTarget.IsValidGameObject &&
			PlayerTarget.TimeSinceFound <= (InnerMissionPlanningConfig?.TimePlayerTracked ?? 10f))
		{
			return PlayerTarget;
		}
		else
		{
			return null;
		}
	}

}
