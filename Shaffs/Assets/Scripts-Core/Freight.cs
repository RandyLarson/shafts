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


public class Freight : MonoBehaviour, IFreight
{

    public Resource Kind;
    public float Amount = 20f;
    public bool DestroyAfterConsumed = false;

    public event Action<Freight, ResourceAction> OnFreightStatusChanged;

    public bool WasDelivered { get; set; } = false;
    Resource IFreight.Kind { get => Kind; set => Kind=value; }
    float IFreight.Amount { get => Amount; set => Amount = value; }

    private void Start()
    {
        OnFreightStatusChanged?.Invoke(this, ResourceAction.Produced);
    }

    public void Consumed()
    {
        WasDelivered = true;
        OnFreightStatusChanged?.Invoke(this, ResourceAction.Delivered);
        if (DestroyAfterConsumed)
            GameObject.Destroy(gameObject);
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
