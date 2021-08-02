using System;
using UnityEngine;
using Assets.Scripts.Extensions;
using Assets.Scripts.MissionPlanning;

[Serializable]
public class LaunchQueueItem
{
	public MissionType Mission;
	public string[] TargetTags;
	public TargetKind MissionTargetKind = TargetKind.None;
	public GameObject ToLaunch;
	public int Count = 0;
}

public class ShipSpawner : MonoBehaviour
{
	public LaunchQueueItem[] LaunchQueue;
	public GameObject LaunchVector;
	public float ActivateAfterTime = 0f;
	public float SpawnInterval = 3f;
	public float WaveInterval = 10f;
	public bool LaunchWhenTargetsAvailable = false;
	public float LaunchPower = 200f;
	public int NumberPerWave = 1;
	public int LoopsOfLaunchQueue = 1;
	public Rect WorkingRegion;
	public GameObject WayPoints;
	private float NextSpawnTime = 3f;
	private int QueuePostion = 0;
	private int WaveCount = 0;
	private float StartTime = 0f;

	private IConfigurator[] Configurators { get; set; }

	private void Start()
	{
		StartTime = Time.time;

		Configurators = GetComponentsInChildren<IConfigurator>();
	}


	void Update()
	{
		if (LaunchQueue.Length == 0 || StartTime + ActivateAfterTime > Time.time)
			return;

		// Remove ourselves if we've exhausted all the waves to launch.
		if (LoopsOfLaunchQueue > 0 && (QueuePostion / LaunchQueue.Length) >= LoopsOfLaunchQueue)
		{
			Destroy(gameObject);
			return;
		}

		if (Time.time >= NextSpawnTime)
		{
			LaunchQueueItem currentQueueEntry = LaunchQueue[QueuePostion % LaunchQueue.Length];

			if (currentQueueEntry == null || currentQueueEntry.ToLaunch == null)
			{
				Debug.LogError("Spawner lacking valid item" + name + " at: " + transform.position);
				//Debug.Break();
			}

			// There may be limits on the kinds of ship or mission we are to run, or there may 
			// be no targets available to launch against. If we can't launch, 
			// make the next launch window half of the standard time.
			if (!MissionTracker.CanLaunch(currentQueueEntry.Mission, currentQueueEntry.MissionTargetKind, LaunchWhenTargetsAvailable, WorkingRegion))
			{
				NextSpawnTime = Time.time + SpawnInterval;
				return;
			}

			if (currentQueueEntry.ToLaunch != null)
			{
				GameObject spawnedObject = Instantiate(currentQueueEntry.ToLaunch, transform.position, transform.rotation);
				if (spawnedObject.GetComponent(out MissionPlanner itsPlanner))
				{
					// Overrides from the basic profile of the launched item.
					itsPlanner.Mission = currentQueueEntry.Mission;

					if (currentQueueEntry.TargetTags?.Length > 0)
						itsPlanner.TargetTags = currentQueueEntry.TargetTags;

					itsPlanner.MissionTargetKind = currentQueueEntry.MissionTargetKind;

					// Set down the WorkingRegion to the ship's planner so it an constrain itself to patrol or take targets from.
					itsPlanner.WorkingRegion = WorkingRegion;
				}

				if (WayPoints != null && spawnedObject.GetComponent(out PathFollower itsPathFollower))
				{
					itsPathFollower.SetWaypointsFromParent(WayPoints);
				}

				if (LaunchPower > 0 && spawnedObject.GetComponent(out Rigidbody2D itsRB))
				{
					Vector3 lVector = (LaunchVector != null ? LaunchVector.transform.position - transform.position : transform.up);
					itsRB.AddForce(lVector * LaunchPower, ForceMode2D.Impulse);
				}

				if (Configurators != null)
				{
					for (int i=0; i<Configurators.Length; i++)
					{
						var wishedComponent = spawnedObject.GetComponent(Configurators[i].TargetType);
						Configurators[i].ConfigureTarget(wishedComponent);
					}
				}
			}

			WaveCount++;

			if (WaveCount >= (currentQueueEntry.Count != 0 ? currentQueueEntry.Count : NumberPerWave))
			{
				WaveCount = 0;
				QueuePostion++;
				NextSpawnTime = Time.time + WaveInterval;
			}
			else
			{
				NextSpawnTime = Time.time + SpawnInterval;
			}
		}
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawIcon(transform.position, "VisitorSpawn-B.png", false);
		Gizmos.DrawWireSphere(transform.position, 5);
		if (LaunchVector != null)
			Gizmos.DrawLine(transform.position, LaunchVector.transform.position);

		if (WorkingRegion != Rect.zero)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(WorkingRegion.center, WorkingRegion.size);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (WorkingRegion != Rect.zero)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(WorkingRegion.center, WorkingRegion.size);
			Vector2 box2 = new Vector2(WorkingRegion.size.x - 2, WorkingRegion.size.y - 2);
			Gizmos.DrawWireCube(WorkingRegion.center, box2);
		}
	}

}
