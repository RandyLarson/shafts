[System.Serializable]
public class ResourceAmount
{
	public Resource Kind;
	public float Amount;
}

[System.Serializable]
public class ResourceGoalAmount : ResourceAmount
{
	/// <summary>
	/// True when the goal is to rid the resources from the inventory.
	/// Example: transporting people away from the station.
	/// </summary>
	public bool IsDepletionGoal = false;
}


public class ResourceGoalProgress
{
	/// <summary>
	/// The kind of resource we are talking about.
	/// </summary>
	public Resource Kind { get; set; }

	/// <summary>
	/// The amount yet available. So the amount that can still be 
	/// produced or the number of people still left that can be rescued.
	/// </summary>
	public float PotentialAmount { get; set; }

	/// <summary>
	/// The amount that must be reached - the number of people that must
	/// be saved or the number of resources that must be delivered/produced/destroyed.
	/// </summary>
	public float RequiredAmount { get; set; }

	/// <summary>
	/// The amount that has successfully been delivered/produced/saved.
	/// </summary>
	public float CurrentAmount { get; set; }

	/// <summary>
	/// The amount that has been lost in delivery/production/rescue.
	/// </summary>
	public float LostAmount { get; set; }
}