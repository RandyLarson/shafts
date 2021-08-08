using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class GoalBase : MonoBehaviour, IGameGoal
{
	public string Name = string.Empty;
	public bool IsMandatory = true;
	public bool IsActive = true;
	public string SuccessMessage = string.Empty;
	public string FailureMessage = string.Empty;
	public string UnresolvedMessage = string.Empty;
	public bool DisableWhenNotMandatory = false;

	public string SuccessAudioMsgName = null;
	public string FailureAudioMsgName = null;
	public UnityEvent GoalSuccessEvent;
	public UnityEvent GoalFailureEvent;

	public bool IsGoalActive
	{
		get => IsActive;
		set => ChangeActivation(value);
	}


	public virtual string GoalName { get => Name; }
	public virtual bool IsMandatoryGoal
	{
		get => IsMandatory;
		set => IsMandatory = value;
	}

	public abstract GoalStatus GetGoalStatus(out string message);

	private void ChangeActivation(bool value)
	{
		if (IsActive != value)
		{
			IsActive = value;
			UpdateRegistration();
			OnActivationChanged();
		}
	}

	private void UpdateRegistration()
	{
		if (IsActive)
			GoalManager.AddLevelGoal(this);
		else
			GoalManager.RemoveLevelGoal(this);
	}

	protected virtual void OnActivationChanged()
	{

	}

	private void OnDisable()
	{
		GoalManager.RemoveLevelGoal(this);
	}

	private void OnEnable()
	{
		UpdateRegistration();
	}

	public virtual void OnSuccess()
	{
		Debug.Log($"{GoalName} is done.");
		GoalSuccessEvent?.Invoke();
		PlaySuccessMessage();
	}

	public virtual void OnFailure()
	{
		GoalFailureEvent?.Invoke();
		PlayFaiureMessage();
	}

	public void PlaySuccessMessage()
	{
		if (SuccessAudioMsgName?.Length > 0)
		{
			AudioManager.TheAudioManager.Play(SuccessAudioMsgName);
		}
	}

	public void PlayFaiureMessage()
	{
		if (FailureAudioMsgName?.Length > 0)
		{
			AudioManager.TheAudioManager.Play(FailureAudioMsgName);
		}
	}

	public virtual IEnumerable<ResourceGoalProgress> GetGoalProgress()
	{
		yield break;
	}
}
