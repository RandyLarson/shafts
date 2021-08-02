using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using UnityEditor;
using UnityEngine;


[Serializable]
public class InventoryBinder
{
	public bool Enabled;
	/// <summary>
	/// Non-null if resource are to be drawn down from a held inventory someplace.
	/// Items will not be produced if the inventory is 0 for the resource kind.
	/// </summary>
	public GameObject SourceInventoryHolder = null;

	/// <summary>
	/// The Kind of resource being bound to.
	/// </summary>
	public Resource Kind = Resource.Food;

	private IInventory SourceInventory = null;

	public void Initialize()
	{
		SourceInventory = SourceInventoryHolder.GetInterface<IInventory>();
	}

	public bool HasInventory
	{
		get
		{
			return Enabled && SourceInventory?.GetResource(Kind) > 0;
		}
	}

	/// <summary>
	/// Pulls at most the requested amount away from the source.
	/// </summary>
	/// <param name="amtWished"></param>
	/// <returns></returns>
	public float DrawDownResources(float amtWished)
	{
		// Really an error -
		if (!Enabled || SourceInventory == null)
			return 0;

		float amtTaken = Mathf.Min(amtWished, SourceInventory.GetResource(Kind));
		SourceInventory.AdjustResource(Kind, -amtTaken);
		return amtTaken;
	}
}


public class ItemProducer : MonoBehaviour
{
	/// <summary>
	/// An icon texture to display in edit mode.
	/// </summary>
	public Texture GizmoIcon = null;
	private string GizmoIconPath = null;

	public GameObject SpawnPoint;
	public GameObject ProducingEffect;
	public GameObject ToProduce;
	public bool SpawningEnabled = false;
	public float InitialSpawnDelay = 0f;
	public float TimeToProduce = 5f;
	public float SpawnLifetime = 0;
	public float SpawnInterval = 10;
	public int InventoryCount = 50;


	public GameObject SpawnedItem;

	/// <summary>
	/// Enable if the amount of a resource kind should differ from its default amount.
	/// </summary>
	public bool OverrideDefaultResources = false;

	/// <summary>
	/// If enabled, these are the amounts of the kinds of resources per item
	/// produced (each item type has a default amount built into it).
	/// </summary>
	public Inventory ResourceOverrides;

	/// <summary>
	/// External sources of inventory can be drawn down by the item producer in those
	/// cases where the resources is finite. People from a city, or mining resources that 
	/// are made available over time are examples.
	/// </summary>
	public InventoryBinder ExternalInventoryBinder;

	/// <summary>
	/// Each supports the FreightStatusChanged interface so they can 
	/// monitor freight produced by this ItemProducer.
	/// </summary>
	public GameObject[] FreightStatusListeners;

	private float NextSpawnTime = 0f;
	private GameObject ActiveEffect = null;


	float ProductionFinishedTime = float.MaxValue;
	float ItemExpiredTime = 0;

	private void Start()
	{
		ExternalInventoryBinder.Initialize();
		NextSpawnTime = Time.time + InitialSpawnDelay;
	}

	void Update()
	{
		if (ProductionFinishedTime != 0 && Time.time > ProductionFinishedTime)
		{
			FinishItemProduction();
			return;
		}

		if (HasUnclaimedItem() && ItemExpiredTime != 0 && Time.time > ItemExpiredTime)
			ClearItem();

		if (ToProduce != null &&
			!HasUnclaimedItem() &&
			HasInventoryToProduce() &&
			SpawningEnabled &&
			Time.time >= NextSpawnTime)
		{
			NextSpawnTime = Time.time + SpawnInterval;
			InventoryCount--;
			BeginItemProduction();
		}
	}

	private void OnDestroy()
	{
		if (ActiveEffect != null)
		{
			GameObject.Destroy(ActiveEffect);
			ActiveEffect = null;
		}
	}

	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (GizmoIcon != null)
		{
			if (GizmoIconPath == null)
			{
				GizmoIconPath = AssetDatabase.GetAssetPath(GizmoIcon);
				GizmoIconPath = System.IO.Path.GetFileName(GizmoIconPath);
			}

			if (GizmoIconPath != null)
				Gizmos.DrawIcon(gameObject.transform.position, GizmoIconPath, true);
		}
		Gizmos.DrawWireSphere(transform.position, 3);
#endif
	}

	void SignalProductionStart()
	{
		if (ProducingEffect != null && SpawnPoint != null)
		{
			ActiveEffect = Instantiate(ProducingEffect);
			ActiveEffect.transform.position = SpawnPoint.transform.position;
		}
	}

	void SignalProductionFinished()
	{
		if (ActiveEffect != null)
		{
			Destroy(ActiveEffect);
			ActiveEffect = null;
		}
	}

	void SignalItemExpired()
	{
		SignalProductionStart();
		if (ActiveEffect)
			Destroy(ActiveEffect, 1.25f);
	}

	void BeginItemProduction()
	{
		SignalProductionStart();
		ProductionFinishedTime = Time.time + TimeToProduce;
	}

	void FinishItemProduction()
	{
		SignalProductionFinished();
		ProductionFinishedTime = 0;
		SpawnedItem = Instantiate(ToProduce, new Vector3(SpawnPoint.transform.position.x, SpawnPoint.transform.position.y, SpawnPoint.transform.position.z + .1f), SpawnPoint.transform.rotation);
		ItemExpiredTime = (SpawnLifetime != 0) ? Time.time + SpawnLifetime : 0;

		// The inventory comes from either the external inventory source or a fixed amount.
		// The max amount taken is from the standard freight amount built into the item produced, or
		// from the override. If the external source is drawn from, then the max amount may be less if
		// the external source doesn't have that much.
		if (ExternalInventoryBinder.Enabled || OverrideDefaultResources)
		{
			// We may not be producing a freight item.
			Freight currentFreight = SpawnedItem.GetComponent<Freight>();

			if (currentFreight != null)
			{
				float payloadAmt = OverrideDefaultResources ?
					ResourceOverrides.GetResource(currentFreight.Kind) :
					currentFreight.Amount;

				if (ExternalInventoryBinder.Enabled)
					payloadAmt = ExternalInventoryBinder.DrawDownResources(payloadAmt);

				currentFreight.Amount = payloadAmt;

				// Wire up listener for freight status changes.
				// Always the global listener, plus any interested parties.
				currentFreight.OnFreightStatusChanged += GameController.LevelStatistics.OnFreightStatusChange;
				if (FreightStatusListeners != null)
				{
					foreach (var listener in FreightStatusListeners)
						if ( listener.GetInterface( out IFreightStatusChanged eventSink ))
							currentFreight.OnFreightStatusChanged += eventSink.OnFreightStatusChange;
				}
			}
		}
	}

	public void ClearItem()
	{
		if (SpawnedItem != null)
		{
			Destroy(SpawnedItem);
			SpawnedItem = null;
			ItemExpiredTime = 0;
			SignalItemExpired();
		}
	}

	public bool HasInventoryToProduce()
	{
		return ExternalInventoryBinder.HasInventory || (!ExternalInventoryBinder.Enabled && InventoryCount > 0);
	}

	public bool HasUnclaimedItem()
	{
		if (SpawnedItem == null)
		{
			return false;
		}

		if (SpawnedItem.CompareTag("Powerup"))
		{
			// Powerups do not persist beyond collision. It is safe to say that
			// there is an unclaimed item if it is a non-null powerup.
			return true;
		}

		// Freight items persist for longer than they are at the spawn point (they are
		// delivered to a destination). Check to see if there is freight at the spawn point still.
		bool foundItemAtSpawnPoint = false;
		var items = Physics2D.OverlapCircleAll(SpawnPoint.transform.position, 1.5f);
		foreach (var hitItem in items )
		{
			if ( hitItem.CompareTag("Freight") || hitItem.CompareTag("Container"))
			{
				foundItemAtSpawnPoint = true;
				break;
			}
		}

		return foundItemAtSpawnPoint;
	}

	/// <summary>
	/// Initiate the cycle of production.
	/// </summary>
	/// <param name="whatToProduce">The item to produce</param>
	/// <param name="timeToProduce">Delay before item is ready</param>
	/// <param name="lifeTime">Spawned item will be removed after this duration.</param>
	public void ProduceItem(GameObject whatToProduce, float timeToProduce, int inventoryCount = 1, float lifeTime = 0)
	{
		ToProduce = whatToProduce;
		InventoryCount = inventoryCount;
		TimeToProduce = timeToProduce;
		BeginItemProduction();
		SpawnLifetime = lifeTime;
	}
}
