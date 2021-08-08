using System;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum ActivateAction
{
	None,
	Activate,
	DeActivate
}

public enum GoalAction
{
	None,
	Mandatory,
	NonMandatory
}

[Serializable]
public class ActivationPofile
{
	public string TargetLevel;
//	public ActivateAction Activation = ActivateAction.None;
//	public GoalAction GoalAction = GoalAction.None;
	public UnityEvent TriggeredEvent;
//	public GameObject[] TargetObjects = null;


	public void SceneLoaded(string levelName)
	{
		if (levelName.EqualsIgnoreCase(TargetLevel))
		{
			if (TriggeredEvent != null)
				TriggeredEvent.Invoke();

			//if (TargetObjects != null)
			//{
			//	foreach (var go in TargetObjects)
			//	{
			//		if (Activation != ActivateAction.None)
			//			go.SetActive(Activation == ActivateAction.Activate ? true : false);
			//		if (GoalAction != GoalAction.None && go.GetInterface<IGameGoal>(out var goalFace))
			//			goalFace.MandatoryForSuccess = GoalAction == GoalAction.Mandatory ? true : false;
			//	}
			//}
		}
	}

}


[Serializable]
public class EventActionProfile
{
	public string EventName;
	public UnityEvent TriggeredEvent;
}

public class ActivateByLevel : MonoBehaviour
{
	public ActivationPofile[] Profiles;


	void Awake()
	{
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void SceneManager_sceneLoaded(Scene theScene, LoadSceneMode loadMode)
	{
		if (Profiles?.Length > 0)
		{
			foreach (var profile in Profiles)
			{
				profile.SceneLoaded(theScene.name);
			}
		}
	}
}
