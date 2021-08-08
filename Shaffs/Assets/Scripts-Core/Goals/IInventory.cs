using UnityEngine;

public delegate void InventoryChanged(Resource kind, float newAmt);


public interface IInventoryChanged
{
	event InventoryChanged OnResourceChanged;
}

public interface IInventory : IInventoryChanged
{
	float GetResource(Resource kind);
	void SetResource(Resource kind, float value);
	void AdjustResource(Resource kind, float amt);
}

