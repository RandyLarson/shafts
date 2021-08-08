using UnityEngine;
using UnityEngine.EventSystems;

public class UiHandler : MonoBehaviour, IPointerClickHandler
{
	public GameController _gameController;
	public string _parameter;

	public void OnPointerClick(PointerEventData eventData)
	{
		_gameController.RestorePlayerAndContinue();
	}

}
