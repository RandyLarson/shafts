using System.Collections.Generic;

public enum GoalStatus
{
	Unresolved,
	Successful,
	Failed,
	Inactive
}

public interface IGameGoal
{
	string GoalName { get; }
	GoalStatus GetGoalStatus(out string message);
	bool IsMandatoryGoal { get; set;  }

	IEnumerable<ResourceGoalProgress> GetGoalProgress();
}
