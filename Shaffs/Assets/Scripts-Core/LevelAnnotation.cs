using System;
using UnityEngine;

[Serializable]
public class LevelAnnotation
{
	[Multiline]
	public string LevelTitle;
	[Multiline]
	public string OverallGoal;

	public string StartPromptOverride;

	[HideInInspector]
	public float Lifetime = 0f;

	public bool HasContent { get => LevelTitle?.Length > 0; }

	public virtual void Clear()
	{
		LevelTitle = string.Empty;
		OverallGoal = string.Empty;
		StartPromptOverride = string.Empty;
	}

}

[Serializable]
public class EndLevelAnnotation : LevelAnnotation
{
	public string NextLevelOverride;
	public override void Clear()
	{
		base.Clear();
		NextLevelOverride = string.Empty;
	}
}
