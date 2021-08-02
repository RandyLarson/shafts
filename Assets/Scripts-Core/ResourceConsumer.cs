using Assets.Scripts.Extensions;
using UnityEngine;

public class ResourceConsumer : MonoBehaviour//, IInventorChanged
{
	public GameObject InventorySource;
	public Resource Kind;
	public bool Enabled = true;
	public float ConsumptionInterval = 3f;
	public float AmtConsumed = 5f;

	private float NextConsumptionTime;
	private bool LastKnownEnabled;

	private IInventory InventoryToUpdate;

	private void Start()
	{
		LastKnownEnabled = Enabled;
		NextConsumptionTime = Time.time + ConsumptionInterval;
		if (InventorySource != null)
			InventoryToUpdate = InventorySource.GetInterface<IInventory>();
		else
			InventoryToUpdate = gameObject.GetInterface<IInventory>();
	}

	private void Update()
	{
		if (null == InventoryToUpdate)
			return;

		if ( LastKnownEnabled != Enabled )
		{
			LastKnownEnabled = Enabled;
			NextConsumptionTime = Time.time + ConsumptionInterval;
		}

		if ( Enabled && NextConsumptionTime <= Time.time)
		{
			AdjustResource(Kind, -AmtConsumed);
			NextConsumptionTime = Time.time + ConsumptionInterval;
		}
	}

	public void AdjustResource(Resource kind, float amount)
	{
		if (InventoryToUpdate != null)
			InventoryToUpdate.AdjustResource(kind, amount);
	}

}
