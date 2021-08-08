using System.Linq;
using System.Text;
using Assets.Scripts.Extensions;
using Assets.Scripts.Goals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VillageController : GoalBase, IHealthCallback
{
	public SurvivalSettings SurvivalSettings;
	public GameObject StarvationVisualization;
	public string VillageName = string.Empty;

	private float FoundingTime = 0f;
	private GameObject[] HighValueItems = null;
	private (GameObject gameObj, string failMsg)[] CriticalItems = null;
	private IGameGoal[] VillageGoals = null;
	public GameObject[] InventoryAdjusters;

	private float? StarvationTimeAt = null;

	public event HealthChangedDelegate OnHealthChanged;

	private void Awake()
	{
		Name = VillageName;
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void Item_OnHealthChanged(GameObject gameObject, float orgValue, float currentValue)
	{
		OnHealthChanged?.Invoke(gameObject, orgValue, currentValue);
	}

	void Start()
	{
		FoundingTime = Time.time;

		// Find all the buildings and non-shield items of value in the village.
		var hpItems = GetComponentsInChildren<HealthPoints>()
			.Where(item => !item.gameObject.CompareTag("Shield"));

		// Register with each building's HP callback so we can monitor aggression against the city
		foreach (var item in hpItems)
		{
			item.OnHealthChanged += Item_OnHealthChanged;
		}

		HighValueItems = hpItems
			.Select(item => item.gameObject)
			.ToArray();

		// Critical items may change their state over time, becoming non-critical when
		// certain levels are loaded or the like.
		CriticalItems = GetComponentsInChildren<CritialMissionItem>()
			.Where(ci => ci.IsCritical)
			.Select(ci => (ci.gameObject, ci.DestroyedMessage))
			.ToArray();

		VillageGoals = gameObject
			.GetInterfacesInChildren<IGameGoal>()
			.ToArray();

		foreach (var adjSource in InventoryAdjusters)
		{
			var changeFace = adjSource.GetInterface<IInventoryChanged>();
			if (changeFace != null)
				changeFace.OnResourceChanged += ChangeFace_ResourceChanged;
		}

	}


	private void SceneManager_sceneLoaded(Scene whichScene, LoadSceneMode mode)
	{
		if (SurvivalSettings.MustSurvive && SurvivalSettings.UntilLevel != null && SurvivalSettings.UntilLevel.EqualsIgnoreCase(whichScene.name))
			SurvivalSettings.MustSurvive = false;
	}

	private void OnDestroy()
	{
		foreach (var adjSource in InventoryAdjusters)
		{
			var changeFace = adjSource.GetInterface<IInventoryChanged>();
			if (changeFace != null)
				changeFace.OnResourceChanged -= ChangeFace_ResourceChanged;
		}
	}

	void Update()
	{
		if (StarvationTimeAt != null && Time.time >= StarvationTimeAt)
		{
			DestroyBuilding();
			StarvationTimeAt = Time.time + SurvivalSettings.StarvationInterval;
		}

	}

	private void ChangeFace_ResourceChanged(Resource kind, float newValue)
	{
		// We care about the overall food content for the village.
		// Starvation is  if we are at 0.
		if (kind == Resource.Food)
		{
			if (newValue <= 0 && SurvivalSettings.CanStarve)
			{
				if (StarvationTimeAt == null)
				{
					StarvationTimeAt = Time.time + SurvivalSettings.StarvationInterval;
				}
			}
			else
			{
				StarvationTimeAt = null;
			}

		}
	}

	public bool IsVillageAlive
	{
		get
		{
			return HighValueItems?.Any(item => item != null) == true;
		}
	}




	override public bool IsMandatoryGoal
	{
		get
		{
			return SurvivalSettings.MustSurvive;
		}
		set
		{
			SurvivalSettings.MustSurvive = value;
		}
	}


	override public GoalStatus GetGoalStatus(out string statusMessage)	
	{
		statusMessage = string.Empty;

		if (CriticalItems != null)
		{
			foreach (var criticalItem in CriticalItems)
			{
				// Will be null if it has been destroyed.
				if (criticalItem.gameObj == null)
				{
					statusMessage = criticalItem.failMsg;
					return GoalStatus.Failed;
				}
			}
		}

		if (SurvivalSettings?.MustSurvive == true && !IsVillageAlive)
		{
			statusMessage = $"{VillageName} was destroyed";
			return GoalStatus.Failed;
		}

		GoalStatus overallStats = GoalStatus.Successful;

		if (VillageGoals != null)
		{
			foreach (var goal in VillageGoals)
			{
				if (goal == (IGameGoal)this)
					continue;

				GoalStatus itsStatus = goal.GetGoalStatus(out statusMessage);
				if (goal.IsMandatoryGoal)
				{
					if (itsStatus == GoalStatus.Failed)
					{
						overallStats = GoalStatus.Failed;
						break;
					}
					else if (itsStatus == GoalStatus.Unresolved)
					{
						overallStats = GoalStatus.Unresolved;
					}
				}
			}
		}

		return overallStats;
	}

	private void DestroyBuilding()
	{
		foreach (var hvi in HighValueItems)
		{
			if (hvi != null)
			{
				GameObject.Destroy(hvi);
				if (null != StarvationVisualization)
				{
					var go = GameObject.Instantiate(StarvationVisualization, hvi.transform);
					Destroy(go, 3f);
				}

				break;
			}
		}
	}

	private void DestroyVillage()
	{
		// Any remaining villagers are either destroyed or wander openly
		// Change image to rubble
		// Play explosion animation

		// Take down any beacons that are used to detect the target.
		var beacons = GetComponents<PatrolBeacon>();
		foreach (var beacon in beacons)
		{
			beacon.IsActive = false;
			Destroy(beacon);
		}
	}
}
