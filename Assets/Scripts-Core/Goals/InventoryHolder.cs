using UnityEngine;


public class InventoryHolder : MonoBehaviour, IInventory, IFreightStatusChanged
{
	public Inventory Inventory;

	#region IInventory

	public event InventoryChanged OnResourceChanged
	{
		add
		{
			Inventory.OnResourceChanged += value;
		}

		remove
		{
			Inventory.OnResourceChanged -= value;
		}
	}

	public float GetResource(Resource kind)
	{
		return Inventory.GetResource(kind);
	}

	public void SetResource(Resource kind, float value)
	{
		Inventory.SetResource(kind, value);
	}


	public void AdjustResource(Resource kind, float amt)
	{
		Inventory.AdjustResource(kind, amt);
	}

	#endregion

	private void Start()
	{
		Inventory.Time = 0f;
		Inventory.ReportResourcesAs(ResourceAction.Potential);
	}

	private void OnDestroy()
	{
		Inventory.ReportResourcesAs(ResourceAction.Lost);
	}

	public void OnFreightStatusChange(Freight whichFreight, ResourceAction freightStatus)
	{
		// We do not act on the `Produced` action - the ItemProducer directly draws down 
		// from the inventory. 
		//if (freightStatus == ResourceAction.Produced)
		//{
		//	AdjustResource(whichFreight.Kind, -whichFreight.Amount);
		//}
	}
}