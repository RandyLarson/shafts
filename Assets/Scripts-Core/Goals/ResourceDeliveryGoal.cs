
using System.Collections.Generic;

public class ResourceDeliveryGoal : GoalBase, IFreightStatusChanged
{
	public InventoryHolder InventorySource;
	public Resource Kind;
	public float AmtToDeliver = -1;
	public float AmtToProduce = -1;
	public float MaxDestroyed = -1;

	public float AmtProduced = 0;
	public float AmtDelivered = 0;
	public float AmtDestroyed = 0;

	public ResourceDeliveryGoal()
	{
		Name = "Delivery";
	}

	private float AmtAvailable { get => AmtProduced - AmtDelivered - AmtDestroyed; }

	override public GoalStatus GetGoalStatus(out string message)
	{
		if ( MaxDestroyed != -1 && AmtDestroyed > MaxDestroyed)
		{
			message = FailureMessage;
			return GoalStatus.Failed;
		}

		if ( (AmtToProduce == -1 || AmtProduced > AmtToProduce) &&
			 (AmtToDeliver == -1 || AmtDelivered >= AmtToDeliver) )
		{
			message = SuccessMessage;
			return GoalStatus.Successful;
		}

		// Is there potential for the goal to still be a success?
		float amtInInventory = InventorySource != null ? ((IInventory)InventorySource).GetResource(Kind) : 0;
		if (amtInInventory + AmtAvailable < AmtToDeliver-AmtDelivered)
		{
			message = FailureMessage;
			return GoalStatus.Failed;
		}

		message = UnresolvedMessage;
		return GoalStatus.Unresolved;
	}

	ResourceGoalProgress ProgressReport = new ResourceGoalProgress();

	public override IEnumerable<ResourceGoalProgress> GetGoalProgress()
	{
		ProgressReport.Kind = Kind;
		ProgressReport.PotentialAmount = InventorySource.GetResource(Kind);
		ProgressReport.CurrentAmount = AmtDelivered;
		ProgressReport.RequiredAmount =
			AmtToProduce != -1 ? AmtToProduce :
			AmtToDeliver != -1 ? AmtToDeliver :
			0;
		ProgressReport.LostAmount = AmtDestroyed;

		yield return ProgressReport;
	}

	public void OnFreightStatusChange(Freight freight, ResourceAction freightStatus)
	{
		if (freight.Kind == Kind)
		{
			switch (freightStatus)
			{
				case ResourceAction.Produced:
					AmtProduced += freight.Amount;
					break;
				case ResourceAction.Lost:
					AmtDestroyed += freight.Amount;
					break;
				case ResourceAction.Delivered:
					AmtDelivered += freight.Amount;
					break;
			}
		}
	}
}