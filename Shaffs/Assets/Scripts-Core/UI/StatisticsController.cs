using System;
using System.Text;
using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;

[Serializable]
public class StatisticsUIConfig
{
	public bool ShowStatsPanel = true;
	public bool ShowOverallStats = true;
}

public class StatisticsController : MonoBehaviour
{
	public GameObject OverallStatPane;
	public TextMeshProUGUI PeopleSaved;
	public TextMeshProUGUI PeopleLost;
	public TextMeshProUGUI EnemiesDestroyed;
	public UIStatGoalController GoalDisplayController;

	public StatisticsUIConfig StatisticsUIConfig
	{
		get => innerStatisticsUIConfig;
		set
		{
			innerStatisticsUIConfig = value;
			UpdateDisplay();
		}
	}



	public void SetLevelStatistics(GameLevelStats levelStats)
	{
		if (levelStats != null)
		{
			levelStats.OnStatisticsChanged += UpdateDisplay;
		}

		if (GoalDisplayController != null)
		{
			GoalDisplayController.SetLevelStatistics(levelStats);
		}
		UpdateDisplay();
	}

	public void TriggerUpdates()
	{
		UpdateDisplay();
		GoalDisplayController?.UpdateDisplay();
	}

	StringBuilder DisplayBuilder = new StringBuilder();
	private StatisticsUIConfig innerStatisticsUIConfig;

	private void UpdateDisplay()
	{
		if (GameController.LevelStatistics == null)
		{
			return;
		}

		OverallStatPane.gameObject.SetActive(StatisticsUIConfig?.ShowOverallStats == true);
		if (StatisticsUIConfig?.ShowOverallStats == true)
		{
			ResourceStat peopleStats = GameController.LevelStatistics.GetResourceStatistics(Resource.People);

			PeopleSaved.SafeSetText($"{peopleStats.Delivered}");
			PeopleLost.SafeSetText($"{peopleStats.Lost}");
			EnemiesDestroyed.SafeSetText($"{GameController.LevelStatistics.EnemiesDestroyed}");
		}
	}
}
