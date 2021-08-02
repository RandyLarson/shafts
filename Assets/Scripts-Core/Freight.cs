using System;
using UnityEngine;


public interface IFreightStatusChanged
{
	void OnFreightStatusChange(Freight whichFreight, ResourceAction freightStatus);
}

public interface IFreightController
{
	void DropFreight();
	bool IsCarryingFreight { get; }
}

public class Freight : MonoBehaviour
{

	public Resource Kind;
	public float Amount = 20f;

	public event Action<Freight, ResourceAction> OnFreightStatusChanged;

	public bool WasDelivered { get; set; } = false;

	private void Start()
	{
		OnFreightStatusChanged?.Invoke(this, ResourceAction.Produced);
	}

	private void OnDestroy()
	{
		try
		{
			OnFreightStatusChanged?.Invoke(this, WasDelivered ? ResourceAction.Delivered : ResourceAction.Lost);

		}
		catch (Exception caught)
		{
			Debug.LogError($"Exception: {caught.Message}");
		}
	}
}
