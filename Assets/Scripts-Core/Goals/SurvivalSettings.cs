
using System;

[Serializable]
public class SurvivalSettings
{
	public bool MustSurvive = false;
	public string UntilLevel = null;
	public bool CanStarve = false;
	public float StarvationInterval = 10f;
}
