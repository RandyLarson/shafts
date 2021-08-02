using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI;

public static class GoalManager
{
	private static List<IGameGoal> Goals = new List<IGameGoal>();

	public static void GatherGoalDiagnostics(bool onlyMandatory = true)
	{
		foreach (IGameGoal goal in Goals)
		{
			if (onlyMandatory && !goal.IsMandatoryGoal)
				continue;

			GoalStatus itsStatus = GoalStatus.Successful;
			string itsStatusMsg = null;

			// If a goal has been destroyed
			if (goal == null)
			{
				itsStatus = GoalStatus.Failed;
				DiagnosticController.Add($"G: 'Goal Destroyed' // 'M' - {itsStatus}");
			}
			else
			{
				itsStatus = goal.GetGoalStatus(out itsStatusMsg);
				DiagnosticController.Add($"G: {goal.GoalName} // {(goal.IsMandatoryGoal ? "M" : "N")} - {itsStatus}");
			}
		}
	}

	public static IEnumerable<IGameGoal> GetMandatoryGoals()
	{
		return Goals.Where(goal => goal.IsMandatoryGoal);
	}

	public static (GoalStatus status, string reason) OverallStatus
	{
		get
		{
			GoalStatus overallStatus = GoalStatus.Successful;
			string overallStatusMessage = "Sad villagers :(";

			foreach (IGameGoal goal in Goals)
			{
				if (!goal.IsMandatoryGoal)
					continue;

				GoalStatus itsStatus = GoalStatus.Successful;
				string itsStatusMsg = null;

				// If a goal has been destroyed
				if (goal == null)
				{
					itsStatus = GoalStatus.Failed;
					DiagnosticController.Add($"G: 'Goal Destroyed' // 'M' - {itsStatus}");
				}
				else
				{
					itsStatus = goal.GetGoalStatus(out itsStatusMsg);
					DiagnosticController.Add($"G: {goal.GoalName} // {(goal.IsMandatoryGoal ? "M" : "N")} - {itsStatus}");
				}

				if (itsStatus == GoalStatus.Failed)
				{
					overallStatus = GoalStatus.Failed;
					if (itsStatusMsg != null)
						overallStatusMessage = itsStatusMsg;
					break;
				}
				else if (itsStatus == GoalStatus.Unresolved)
				{
					overallStatus = GoalStatus.Unresolved;
				}
			}

			return (overallStatus, overallStatusMessage);
		}
	}


	internal static void ResetGoalList()
	{
		Goals.Clear();
	}


	public static void AddLevelGoal(IGameGoal gameGoal)
	{
		if (!Goals.Contains(gameGoal))
			Goals.Add(gameGoal);
	}

	public static void RemoveLevelGoal(IGameGoal gameGoal)
	{
		if (Goals.Contains(gameGoal))
			Goals.Remove(gameGoal);
	}

}
