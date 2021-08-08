using System;
using System.Linq;
using Assets.Scripts.Extensions;
using UnityEngine;

[Serializable]
public class CriticalCollection
{
	public GameObject[] Items;
	public SurvivalKind Condition;
	public string FailureMessage;
	public string RelevantUntilLevel;

	public CriticalCollection()
	{
		IsRelevant = true;
	}

	/// <summary>
	/// True if the criteria is still valid. IsFailure will return false if IsRelevant is false.
	/// </summary>
	public bool IsRelevant { get; set; }

	public bool IsFailure(out string reason)
	{
		reason = null;

		if (!IsRelevant)
			return false;

		bool itFailed = false;
		switch (Condition)
		{
			case SurvivalKind.all:
				itFailed = Items.Any(item => item == null);
				break;
			case SurvivalKind.atLeastOne:
				itFailed = Items.All(item => item == null);
				break;
			case SurvivalKind.majority:
				itFailed = Items.Count(item => item == null) > Items.Count();
				break;
		}

		if (itFailed)
		{
			reason = FailureMessage;
		}
		return itFailed;
	}

	internal void SceneLoaded(string name)
	{
		if (RelevantUntilLevel.EqualsIgnoreCase(name))
			IsRelevant = false;
	}
}
