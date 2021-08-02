using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using UnityEngine;


public class ProjectGoal : GoalBase, IResourceConsumer, IInventory
{
	public Inventory Inventory;
	public ResourceVisualizer Visualizer;
	public ResourceGoalAmount[] Requirements;
	public GameObject[] ItemReward;
	public FreightDropController FreightDrop;
	public bool RewardsStartEnabled = false;
	public bool DisablePortalAfterSuccess = true;
	public bool DisplayInventoryAfterSuccess = true;

	private IResourceVisualizer VisualizerFace;
	private List<IResourceConsumer> SubConsumers;

	private bool ProjectComplete { get; set; }

	#region IInventory

	event InventoryChanged IInventoryChanged.OnResourceChanged
	{
		add
		{
			Inventory.OnResourceChanged += value;
		}

		remove
		{
			Inventory.OnResourceChanged -= value;
		}
	}


	float IInventory.GetResource(Resource kind)
	{
		return Inventory.GetResource(kind);
	}

	public void SetResource(Resource kind, float value)
	{
		Inventory.SetResource(kind, value);

		if (SubConsumers?.Any() == true)
		{
			foreach (var consumer in SubConsumers)
			{
				consumer?.SetResource(kind, value);
			}
		}

		VisualizerFace?.InventoryChanged();
	}


	public void AdjustResource(Resource kind, float amt)
	{
		float preAmt = Inventory.GetResource(kind);
		Inventory.AdjustResource(kind, amt);
		if (preAmt != Inventory.GetResource(kind))
		{
			if (SubConsumers?.Any() == true)
			{
				foreach (var consumer in SubConsumers)
				{
					if (consumer != null)
						consumer.AdjustResource(kind, amt);
				}
			}

			VisualizerFace?.InventoryChanged();
		}
	}

	#endregion

	public ProjectGoal()
	{
		Name = "Project";
	}

	private void Awake()
	{
	}

	private void Start()
	{
		if (FreightDrop != null)
		{
			FreightDrop.freightRecipient = this;
		}

		Inventory.Time = 0f;

		SubConsumers = new List<IResourceConsumer>();
		foreach (var reward in ItemReward)
		{
			if (reward != null)
			{
				reward.SetActive(RewardsStartEnabled);

				if( reward.GetInterface<IResourceConsumer>(out var asConsumer))
					SubConsumers.Add(asConsumer);
			}
		}

		if (Visualizer != null)
		{
			VisualizerFace = Visualizer;
			if (VisualizerFace != null)
			{
				VisualizerFace.Requirements = Requirements;
				VisualizerFace.Inventory = Inventory;
			}
		}

		// ??
		//// Report requirements to game stat
		//if ( IsMandatory )
		//{
		//	GameController.LevelStatistics.AdjustResourceStatistic
		//}
	}



	private void Update()
	{
		if (ProjectComplete == false && GetGoalStatus(out string _) == GoalStatus.Successful)
		{
			ProjectComplete = true;
			ActivateRewards(true);
			OnSuccess();

			if (DisablePortalAfterSuccess && FreightDrop != null)
			{
				FreightDrop.gameObject.SetActive(false);
			}

			if (DisplayInventoryAfterSuccess)
			{
				Visualizer.DisplayRequirements = false;
				Visualizer.InventoryChanged();
			}
			else
			{
				Visualizer.gameObject.SetActive(false);
			}
		}
	}

	private void ChangeFace_ResourceChanged(Resource kind, float newValue)
	{
		AdjustResource(kind, newValue);
	}

	override public GoalStatus GetGoalStatus(out string statusMessage)
	{
		statusMessage = string.Empty;
		if (null == Requirements || ProjectComplete)
			return GoalStatus.Successful;

		GoalStatus overallStatus = GoalStatus.Successful;
		foreach (var req in Requirements)
		{
			GoalStatus thisGoalStatus = GoalStatus.Successful;

			float currentAmount = Inventory.GetResource(req.Kind);
			if (req.IsDepletionGoal)
				thisGoalStatus = currentAmount <= req.Amount ? GoalStatus.Successful : GoalStatus.Unresolved;
			else
				thisGoalStatus = currentAmount >= req.Amount ? GoalStatus.Successful : GoalStatus.Unresolved;

			if (thisGoalStatus == GoalStatus.Unresolved)
			{
				overallStatus = thisGoalStatus;
				break;
			}
		}

		return overallStatus;
	}


	public override IEnumerable<ResourceGoalProgress> GetGoalProgress()
	{
		foreach (var req in Requirements)
		{ 
			ResourceGoalProgress ProgressReport = new ResourceGoalProgress();
			ProgressReport.Kind = req.Kind;
			ProgressReport.PotentialAmount = 0;
			ProgressReport.CurrentAmount = Inventory.GetResource(req.Kind);
			ProgressReport.RequiredAmount = req.Amount;
			ProgressReport.LostAmount = 0;

			yield return ProgressReport;
		}
	}


	internal void ActivateRewards(bool activateThem)
	{
		if (ItemReward != null)
		{
			foreach (var reward in ItemReward)
			{
				if (reward != null)
					reward.SetActive(activateThem);
			}
		}
	}

}