using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIStatGoalController : MonoBehaviour
{
	public UIStatGoal[] Prototypes;

	public List<UIStatGoal> ActiveGoals;
	public List<UIStatGoal> InactiveGoals;

	private bool UpdateRequested { get; set; } = false;
	private float NextUpdateTime = 0;
	private float UpdateInterval = 1;

	public void SetLevelStatistics(GameLevelStats levelStats)
	{
		if (levelStats != null)
		{
			levelStats.OnStatisticsChanged += UpdateDisplay;
			levelStats.OnResourceChanged += LevelStats_OnResourceChanged;
		}
		UpdateRequested = true;
	}

	private void LevelStats_OnResourceChanged(Resource kind, ResourceAction action, float amt)
	{
		UpdateRequested = true;
	}

	void PruneOrphanGoals()
	{
		var toPrune = ActiveGoals
			.Where(goal => goal.UpdateTime != Time.time)
			.ToArray();

		foreach (var pruneMe in toPrune)
		{
			DecommissionGoalUIFor(pruneMe);
		}
	}

	UIStatGoal GetExistingGoalUIFor(IGameGoal soughtGoal)
	{
		return ActiveGoals.FirstOrDefault(existingGoal => existingGoal.DisplayedGoal == soughtGoal);
	}

	void DecommissionGoalUIFor(UIStatGoal toHide)
	{
		toHide.gameObject.SetActive(false);
		toHide.gameObject.transform.SetParent(null);
		ActiveGoals.Remove(toHide);
		InactiveGoals.Add(toHide);
	}

	UIStatGoal CreateGoalDisplayFor(Resource kind)
	{
		var toUse = InactiveGoals.FirstOrDefault(goal => goal.Kind == kind);
		if (toUse != null)
		{
			InactiveGoals.Remove(toUse);
			toUse.Clear();
		}
		else
		{
			var thePrototype = Prototypes.FirstOrDefault(protoType => protoType.Kind == kind);
			if ( null != thePrototype )
			{
				toUse = GameObject.Instantiate(thePrototype);
			}
		}
		ActiveGoals.Add(toUse);
		return toUse;
	}


	private void FixedUpdate()
	{
		if (UpdateRequested)
		{
			if (NextUpdateTime <= Time.time)
			{
				UpdateDisplay();
				NextUpdateTime = Time.time + UpdateInterval;
				UpdateRequested = false;
			}
		}
		else
		{
			NextUpdateTime = Time.time + UpdateInterval;
		}
	}

	public void UpdateDisplay()
	{
		int max = 4;

		foreach (IGameGoal nextGoal in GoalManager.GetMandatoryGoals())
		{
			var displayController = GetExistingGoalUIFor(nextGoal);

			if (nextGoal.GetGoalStatus(out _) == GoalStatus.Successful)
			{
				if ( displayController != null )
					DecommissionGoalUIFor(displayController);
				continue;
			}

			if (displayController != null)
			{
				displayController.UpdateTime = Time.time;
				displayController.UpdateDisplay();
				continue;
			}

			var primaryObjective = nextGoal.GetGoalProgress().FirstOrDefault();
			if (null == primaryObjective)
				continue;

			UIStatGoal goalVisual = CreateGoalDisplayFor(primaryObjective.Kind);
			if (goalVisual != null)
			{
				goalVisual.transform.SetParent(transform);
				goalVisual.gameObject.SetActive(true);
				goalVisual.DisplayedGoal = nextGoal;
				goalVisual.UpdateTime = Time.time;
				goalVisual.UpdateDisplay();
			}
		}

		PruneOrphanGoals();
	}
}
