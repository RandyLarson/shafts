using UnityEngine;
using Assets.Scripts.Extensions;

/// <summary>
/// A goal that is successful if the attached gameObject has taken damage and/or been destroyed.
/// Initially used a progression tool to transition between levels -- progression is triggered when a visitor 
/// damages or destroys a building. 
/// 
/// Can also be used if a player must destroy a target.
/// </summary>
public class DamageTrigger : GoalBase
{
	public GameObject TargetItem;
	public bool TriggerOnDestruction = false;
	public bool TriggerEachDamage = false;
	public int TriggerAfterDamageCount = 1;

	private int NumTriggers = 0;
	private int NumHealthCallbacks { get; set; } = 0;
	private bool IsDestroyed { get; set; } = false;


	private void Start()
	{
		if (TargetItem != null && TargetItem.GetInterface<IHealthCallback>(out var healthComponent))
		{
			healthComponent.OnHealthChanged += HealthComponent_OnHealthChanged;
		}
	}

	private void Update()
	{
		if ((NumTriggers == 0 || TriggerEachDamage) && GoalSuccessEvent != null && GetGoalStatus(out _) == GoalStatus.Successful)
		{
			NumTriggers++;
			OnSuccess();
		}
	}

	private void HealthComponent_OnHealthChanged(GameObject gameObject, float orgValue, float currentValue)
	{
		if (currentValue < orgValue)
		{
			NumHealthCallbacks++;

			if (currentValue == 0)
				IsDestroyed = true;
		}
	}

	override public GoalStatus GetGoalStatus(out string statusMessage)
	{
		GoalStatus overallStatus = GoalStatus.Unresolved;
		statusMessage = string.Empty;

		if ((TriggerAfterDamageCount > 0 && NumHealthCallbacks > TriggerAfterDamageCount) || (TriggerOnDestruction && IsDestroyed))
		{
			overallStatus = GoalStatus.Successful;
			statusMessage = SuccessMessage;
		}

		return overallStatus;
	}

}
