using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Extensions;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
	public GameObject ContinueSavedGameControl;

	private void OnEnable()
	{
		bool canContinue = GameController.TheController?.CanCurrentPlayerContinueSavedGame() ?? false;
		ContinueSavedGameControl.SafeSetActive(canContinue);
	}
}
