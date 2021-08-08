using System;
using System.Linq;
using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatGoal : MonoBehaviour
{
	public Resource Kind;
	public Image Icon;
	public TextMeshProUGUI Content;

	public IGameGoal DisplayedGoal { get; set; }
	public float UpdateTime { get; set; }

	public void UpdateDisplay()
	{
		// Based upon the resource type:
		//  * Set Icon
		//  * Set Text content and color
		Icon.gameObject.SetActive(true);
		Content.gameObject.SetActive(true);

		var progressToGoal = DisplayedGoal.GetGoalProgress().FirstOrDefault();
		if (progressToGoal != null)
		{
			Content.SafeSetText($"{progressToGoal.CurrentAmount}/{progressToGoal.RequiredAmount}");
		}
	}

	internal void Clear()
	{
		Content.SafeSetText(string.Empty);
	}
}
