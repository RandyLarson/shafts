using UnityEngine;

public class MidGameMenuController : MonoBehaviour
{
    void Update()
    {
        
    }

	private void OnEnable()
	{
		GameController.TheController.SetTimeScale(TimeScale.Paused);
	}

	private void OnDisable()
	{
		GameController.TheController.EnterGamePhase(GameState.Play);
	}
}
