using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct LevelInfo
{
	public int LevelNum;
	public int ChapterNum;
}

/// <summary>
/// Generic configuration for goals.
/// </summary>
public class GoalConfig : MonoBehaviour
{

	public LevelInfo ValidFrom;
	public LevelInfo ValidUntil;
}
