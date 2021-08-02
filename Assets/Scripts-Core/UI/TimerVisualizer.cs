using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;

public class TimerVisualizer : MonoBehaviour
{
	public string Label;
	public string TimerValue;

	public TextMeshProUGUI TimerUI;

	public TextMeshProUGUI TimeVisualizer => TimerUI;

	public void SetLabel(string to)
	{
		Label = to;
		RefreshVisual();
	}

	public void SetTimer(string to)
	{
		TimerValue = to;
		RefreshVisual();
	}

	public void RefreshVisual()
	{
		if (TimerUI.IsValidGameobject())
		{
			string content = Label;
			if (!string.IsNullOrWhiteSpace(content))
				content += " ";
			content += TimerValue;

			TimerUI.text = content;
		}
	}

	public bool Visualize
	{
		get
		{
			if (TimerUI.IsValidGameobject())
				return TimerUI.IsActive();
			return false;
		}

		set
		{
			gameObject.SafeSetActive(value);
		}
	}


}
