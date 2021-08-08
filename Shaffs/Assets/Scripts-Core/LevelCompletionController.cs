using System.Linq;
using TMPro;
using UnityEngine;



public class LevelCompletionController : LevelIntroductionController
{
	private EndLevelAnnotation EndLevelDetails;
	protected override GameState ControllerKind => GameState.NextLevel;

	public new EndLevelAnnotation Details
	{
		get => EndLevelDetails;
		set
		{
			EndLevelDetails = value;
			base.Details = value;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if ( EndLevelDetails != null )
		{
			EndLevelDetails.Clear();
		}
	}

	private void OnEnable()
	{
		Activate();
	}

	private void OnDisable()
	{
		
	}

	public override void Dismiss()
	{
		LoadNextLevel();
		GameController.TheController.LevelDetailsDismissed(ControllerKind);
		gameObject.SetActive(false);
	}

	public void LoadNextLevel()
	{
		if (EndLevelDetails?.NextLevelOverride?.Any() == true)
		{
			GameController.TheController.JumpToScene(EndLevelDetails.NextLevelOverride);
		}
		else
		{
			GameController.TheController.ProgressToNextScene(true);
		}
	}
}
