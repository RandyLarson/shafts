using System;
using Assets.Scripts.Extensions;
using UnityEngine;

public delegate void HealthChangedDelegate(GameObject gameObject, float orgValue, float currentValue);

public interface IHealthCallback
{
	event HealthChangedDelegate OnHealthChanged;
}

public class HealthPoints : MonoBehaviour, IHealthCallback
{
	public float HP = 0f;
	public float MaxHP = 0f;
	public bool SynchronizeHPWithInventorItem;
	public GameObject InventoryHolder;
	public Resource InventoryKind;
	public bool LifetimeIsExternallyControlled;
	public bool DestroyAtZero;
	public bool DestroyParentAtZero;
	public bool DisableAtZero;
	public GameObject Explosion;
	public TMPro.TextMeshProUGUI Visualizer;

	private IInventory SynchronizedInventory;

	public event HealthChangedDelegate OnHealthChanged;
	public event Action OnDestroyed;

	private void SignalHealthChanged(float orgHealth, float newHealth)
	{
		OnHealthChanged?.Invoke(gameObject, orgHealth, newHealth);
	}

	private void SignalExternallyDestroyed()
	{
		OnDestroyed?.Invoke();
	}

	public void Start()
	{
		if (SynchronizeHPWithInventorItem && InventoryHolder != null)
		{
			SynchronizedInventory = InventoryHolder.GetInterface<IInventory>();
			if (SynchronizedInventory != null)
			{
				SynchronizedInventory.OnResourceChanged += SynchronizedInventory_ResourceChanged;
				HP = SynchronizedInventory.GetResource(InventoryKind);
			}
		}

		UpdateVisualization();

		if (MaxHP == 0)
		{
			MaxHP = HP;
		}
	}

	public void UpdateVisualization()
	{
		if (Visualizer != null)
		{
			Visualizer.text = "Health ~ " + Mathf.Max(0, HP).ToString("N0");
		}
	}

	public void VisualzeDeath()
    {
		if (Explosion != null)
		{
			var theExplosion = Instantiate(Explosion, transform.position, transform.rotation);
			GameObject.Destroy(theExplosion, 4f);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newValue"></param>
	/// <returns>True if destroyed.</returns>
	protected virtual bool UpdateHeathTo(float newValue)
	{
		float orgHealth = HP;
		HP = Mathf.Min(MaxHP, newValue);

		// Reactivation
		if (HP > 0 && DisableAtZero)
		{
			gameObject.SetActive(true);
		}

		UpdateVisualization();
		SignalHealthChanged(orgHealth, HP);

		if (HP <= 0 && LifetimeIsExternallyControlled)
			return true;

		if (HP <= 0 && (DestroyAtZero || DestroyParentAtZero || DisableAtZero))
		{
			VisualzeDeath();
			if (SynchronizedInventory != null)
			{
				SynchronizedInventory.OnResourceChanged -= SynchronizedInventory_ResourceChanged;
			}

			if (DisableAtZero)
			{
				gameObject.SetActive(false);
			}
			else
			{
				//if (gameObject.GetComponent(out Explodable theExploadable))
				//	theExploadable.Explode();

				if (DestroyAtZero)
				{
					SignalExternallyDestroyed();

					if (DestroyParentAtZero && gameObject.transform.parent != null)
						Destroy(gameObject.transform.parent.gameObject);
					else
						Destroy(gameObject);
				}

				// Was destroyed.
				return true;
			}
		}

		// Not destroyed
		return false;
	}



	protected virtual void SynchronizedInventory_ResourceChanged(Resource kind, float newValue)
	{
		if (kind == InventoryKind)
		{
			UpdateHeathTo(newValue);
		}
	}


	public virtual bool AdjustHealthBy(float amount)
	{
		if (SynchronizedInventory != null)
		{
			float nextValue = amount + SynchronizedInventory.GetResource(InventoryKind);
			SynchronizedInventory.SetResource(InventoryKind, nextValue);
			return false;
		}
		else
		{
			return UpdateHeathTo(HP + amount);
		}
	}

}
