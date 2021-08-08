using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
/// <summary>
/// Keeping the tally of progress and activity for a particular level.
/// This is ultimately stored in local game storage.
/// </summary>
public class GameLevelStats
{
	public string LevelName { get; set; }
	public int LevelAttempt { get; set; } = 0;
	public bool LevelComplete { get; set; } = false;
	public float TimeOfCompletion { get; set; }
	public float TimeStarted { get; set; }
	public int EnemiesDestroyed { get; set; }
	public int ShotsFired { get; set; }
	public int ShotsHit { get; set; }
	public float DamageDone { get; set; }


	public List<ResourceStat> ResourceStatistics { get; private set; } = new List<ResourceStat>();

	[XmlIgnore]
	public TMPro.TextMeshProUGUI Visualizer;

	public event Action<Resource, ResourceAction, float> OnResourceChanged;
	public event Action OnStatisticsChanged;


	public GameLevelStats()
	{
		Clear();
	}

	public void Clear()
	{
		LevelName = string.Empty;
		LevelAttempt = 1;
		LevelComplete = false;
		TimeStarted = 0;
		EnemiesDestroyed = 0;
		ShotsFired = 0;
		ShotsHit = 0;
		DamageDone = 0;

		ResourceStatistics.Clear();
		ResourceStatistics.Add(new ResourceStat() { Kind = Resource.Food });
		ResourceStatistics.Add(new ResourceStat() { Kind = Resource.Material });
		ResourceStatistics.Add(new ResourceStat() { Kind = Resource.People });
		ResourceStatistics.Add(new ResourceStat() { Kind = Resource.Shield });
		ResourceStatistics.Add(new ResourceStat() { Kind = Resource.Time });
	}

	public void ShotFired() { ShotsFired++; SignalGeneralStatChange(); }
	public void EnemyDestroyed() { EnemiesDestroyed++; SignalGeneralStatChange(); }
	public void DamageInflicted(float amt) { DamageDone += amt; ShotsHit++; SignalGeneralStatChange(); }

	/// <summary>
	/// Delegate target for freight status changes so we can monitor creation/destroyed/delivered
	/// states of freight.
	/// </summary>
	/// <param name="whichFreight"></param>
	/// <param name="freightStatus"></param>
	public void OnFreightStatusChange(Freight whichFreight, ResourceAction freightStatus)
	{
		AdjustResourceStatistic(whichFreight.Kind, freightStatus, whichFreight.Amount);
	}

	public ResourceStat GetResourceStatistics(Resource kind)
	{
		foreach (var statistics in ResourceStatistics)
		{
			if (statistics.Kind == kind)
				return statistics;
		}

		Debug.LogError($"Statistics // Could not find requested resource `{kind}`");
		return null;
	}

	public void GeneralInventoryUpdate()
	{
		SignalGeneralStatChange();
	}

	public void AdjustResourceStatistic(Resource kind, ResourceAction action, float amount)
	{
		var statistics = GetResourceStatistics(kind);
		if (statistics != null )
		{
			switch (action)
			{
				case ResourceAction.Potential:
					statistics.Potential += amount;
					break;
				case ResourceAction.Produced:
					statistics.Produced += amount;
					break;
				case ResourceAction.Lost:
					statistics.Lost += amount;
					break;
				case ResourceAction.Delivered:
					statistics.Delivered += amount;
					break;
				default:
					throw new ArgumentOutOfRangeException($"Un-handled resource action kind: {kind}");
			}
		}

		OnResourceChanged?.Invoke(kind, action, amount);
		SignalGeneralStatChange();
	}

	private void SignalGeneralStatChange()
	{
		OnStatisticsChanged?.Invoke();
	}
}
