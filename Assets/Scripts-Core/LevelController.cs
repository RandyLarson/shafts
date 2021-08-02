using System;
using System.Linq;
using Assets.Scripts.Extensions;
using Assets.Scripts.MissionPlanning;
using Milkman;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SurvivalKind
{
	all,
	majority,
	atLeastOne
}


public class LevelController : MonoBehaviour
{
	public ActivationPofile[] OnLevelActivation;
	public EventActionProfile[] OnGameControllerEvent;

	/// <summary>
	///  Non-null if a new player ship is to be used.
	/// </summary>
	public PlayerShip Player;
	public bool UsePlayerAlways = false;

	/// <summary>
	/// True only for the start-menu scene.
	/// </summary>
	public bool IsStartScene = false;
	public float MinimumPatrolAltitude = 0;
	public Rect LevelBounds;
	public LevelAnnotation LevelDescription;
	public EndLevelAnnotation EndLevelDescription;
	public MissionPlannerConfig MissionPlannerConfig;
	public StatisticsUIConfig StatisticsUIConfig;

	LevelController()
	{
	}


	private void Awake()
	{
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void Start()
	{
		GameController.TheController.OnGameEvent += TheController_GameEvent;
	}

	private void TheController_GameEvent(string eventName, float eventArg)
	{
		foreach ( var eventTrigger in OnGameControllerEvent)
		{
			if (eventTrigger.EventName.EqualsIgnoreCase(eventName))
			{
				eventTrigger?.TriggeredEvent?.Invoke();
			}
		}
	}

	private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadMode)
	{
		if (OnLevelActivation?.Length > 0)
		{
			foreach (var profile in OnLevelActivation)
			{
				profile.SceneLoaded(loadedScene.name);
			}
		}
	}

	public void AddPlayerMessage(string content)
	{
		MessageContoller.AddMessage(content);
	}

	/// <summary>
	/// The GameController is the central message dispatcher. Objects can subscribe there.
	/// This method relays the call to the GameController (from visually configured items in the scene (which
	/// cannot reference the game controller directly)).
	/// </summary>
	/// <param name="named">A well-known game milestone/event name.</param>
	public virtual void SignalGameEvent(string name)
	{
		GameController.TheController.SignalGameEvent(name, 0);
	}

	public void WireUpLevel()
	{
		Debug.Log("Wiring Level Controller for " + gameObject.scene.name);

		if (MissionPlannerConfig != null)
		{
			MissionTracker.PlannerConfig = MissionPlannerConfig;
		}

		//if (gameObject.GetComponent<TimeGoal>(out var localTimeGoal))
		//{
		//	TimeGoal levelTimer = GameUIController.Controller.LevelTimer;
		//	if (levelTimer != null && levelTimer.Visualize)
		//	{
		//		levelTimer.SetFrom(localTimeGoal);
		//	}
		//}

		//GameUIController.Controller.LevelIntroController.Details = LevelDescription;
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(LevelBounds.center, LevelBounds.size);

		Vector2 box2 = new Vector2(LevelBounds.size.x - 2, LevelBounds.size.y - 2);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(LevelBounds.center, box2);
	}
}
