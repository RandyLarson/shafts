using System.Linq;

public class CriticalItemGoal : GoalBase
{
	public CriticalCollection[] CriticalItems;
	override public bool IsMandatoryGoal
	{
		get => CriticalItems?.Length > 0;
		set { }
	}

	override public GoalStatus GetGoalStatus(out string message)
	{
		message = null;

		if (CriticalItems == null || CriticalItems.Length == 0)
			return GoalStatus.Successful;

		string failReason = null;
		if (CriticalItems.Any(crit => crit.IsFailure(out failReason)))
		{
			message = failReason;
			return GoalStatus.Failed;
		}

		// Since we are counting critical items only as goals here, we have succeeded
		// until we fail.
		return GoalStatus.Successful;
	}
}
