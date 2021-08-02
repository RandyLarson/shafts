using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInterlude : MonoBehaviour
{
	public LevelAnnotation Details;

	private void OnEnable()
	{
		GameController.TheController.ActivateLevelInterlude(Details);
	}
	
}
