using System;
using TMPro;
using UnityEngine;

public class TimeGoal : GoalBase
{
	public float Duration;
	public int TimesToTrigger = 1;
	public int SuccessUponTriggerNumber = 1;
	public float SubsequentInterval = 5;
	public bool IsCountdown = false;
	[Tooltip("Is reaching the goal time considered success or failure?")]
	public bool TriggeringIsSuccess = true;
	public string TimerLabel;

	public TimerVisualizer Visualizer;
	public bool Visualize = false;
	public bool UseGlobalVisualizer = false;
	public bool HideAfterLastTrigger = true;

	private float GoalTime;
	private float StartTime;
	private int NumberOfTriggers = 0;

	public TimeGoal()
	{
		Name = "TimeGoal";
	}

	void Start()
	{
		SetupVisualization(Visualize);
		OnActivationChanged();
	}

	void Update()
	{
		if (!IsGoalActive)
			return;

		float elapsed = Mathf.Min(Duration, Time.time - StartTime);
		UpdateVisualization(elapsed);

		if (GoalTime <= Time.time)
		{
			NumberOfTriggers++;
			if (NumberOfTriggers <= TimesToTrigger)
			{
				if (TriggeringIsSuccess)
					OnSuccess();
				else
					OnFailure();

				if (TimesToTrigger > NumberOfTriggers)
					ResetTimerFor(SubsequentInterval);

				HideOrShowVisualization();
			}
		}
	}


	public void ResetTimerFor(float duration)
	{
		Duration = duration;

		StartTime = Time.time;
		GoalTime = StartTime + Duration;
	}


	override public GoalStatus GetGoalStatus(out string statusMessage)
	{
		statusMessage = string.Empty;
		if (NumberOfTriggers < SuccessUponTriggerNumber)
			return GoalStatus.Unresolved;

		if (TriggeringIsSuccess)
		{
			statusMessage = SuccessMessage;
			return GoalStatus.Successful;
		}
		else
		{
			statusMessage = FailureMessage;
			return GoalStatus.Failed;
		}
	}


	public void UpdateVisualization(float elapsedTime)
	{
		if (Visualizer?.Visualize == true)
		{
			string toDisplay;
			if (IsCountdown)
				toDisplay = $"{Duration - elapsedTime:N0}";
			else
				toDisplay = $"{elapsedTime:N0}/{Duration:N0}";

			Visualizer.SetTimer(toDisplay);
		}
	}

	internal void SetFrom(TimeGoal srcTimeGoal)
	{
		TimesToTrigger = srcTimeGoal.TimesToTrigger;
		SuccessUponTriggerNumber = srcTimeGoal.SuccessUponTriggerNumber;
		SubsequentInterval = srcTimeGoal.SubsequentInterval;
		IsCountdown = srcTimeGoal.IsCountdown;
		Visualize = srcTimeGoal.Visualize;
		ResetTimerFor(srcTimeGoal.Duration);
		IsMandatory = srcTimeGoal.IsMandatory;

		HideOrShowVisualization();
	}

	public void SetLabel(string to)
	{
		Visualizer?.SetLabel(to);
	}

	public void SetupVisualization(bool enableVisualization)
	{
		if (!enableVisualization && Visualizer != null)
			Visualizer.Visualize = false;

		Visualize = enableVisualization;

		if (Visualize)
		{
			if (UseGlobalVisualizer)
				Visualizer = GameUIController.Controller.GlobalTimer;
		}

		if (Visualizer != null)
		{
			Visualizer.Visualize = Visualize;
			Visualizer.SetLabel(TimerLabel);
		}
	}

	public void HideOrShowVisualization()
	{
		if (Visualizer != null)
		{
			Visualizer.Visualize =
				IsGoalActive &&
				Visualize &&
				(!HideAfterLastTrigger || NumberOfTriggers < TimesToTrigger);
		}
	}

	protected override void OnActivationChanged()
	{
		base.OnActivationChanged();

		HideOrShowVisualization();
		if (IsGoalActive)
		{
			ResetTimerFor(Duration);
		}
	}
}
