using System.Linq;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A goal that is successful if a GameObject enters it -- the player reaching a destination.
/// </summary>
public class GlobalEventGoal : GoalBase
{
	public string EventName;
	public int SuccessAfterEventCount = 1;

	public bool IsSuccessful = false;
	public int TimesTriggered = 0;

	public GlobalEventGoal()
	{
	}

	private void Awake()
	{
		Name = $"GlobalEvent - {EventName}";
		GameController.TheController.OnGameEvent += TheController_GameEvent;
	}

	private void TheController_GameEvent(string eventName, float _)
	{
		if (EventName.EqualsIgnoreCase(eventName))
		{
			TimesTriggered++;

			if (TimesTriggered >= SuccessAfterEventCount)
			{
				IsSuccessful = true;
				OnSuccess();
			}
		}
	}

	override public GoalStatus GetGoalStatus(out string statusMessage)
	{
		statusMessage = string.Empty;
		if (IsSuccessful)
		{
			statusMessage = SuccessMessage;
			return GoalStatus.Successful;
		}
		else
		{
			statusMessage = UnresolvedMessage;
			return GoalStatus.Unresolved;
		}
	}

}
