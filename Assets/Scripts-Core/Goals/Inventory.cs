using System;
using UnityEngine;

[Serializable]
public class Inventory : IInventory
{
	public float _Food = 0f;
	public float _Materials = 0f;
	public float _Shield = 0f;
	public float _Time = 0f;
	public float _People = 0f;

	public Inventory()
	{
		Food = 0f;
		Materials = 0f;
		Shield = 0f;
		Time = 0f;
		People = 0f;
	}

	public float GetResource(Resource kind)
	{
		switch (kind)
		{
			case Resource.Shield:
				return Shield;
			case Resource.Food:
				return Food;
			case Resource.Material:
				return Materials;
			case Resource.Time:
				return Time;
			case Resource.People:
				return People;
			default:
				throw new ArgumentException(string.Format("Unknown resource kind `{0}`given.", kind));
		}
	}

	public void SetResource(Resource kind, float value)
	{
		switch (kind)
		{
			case Resource.Shield:
				Shield = Mathf.Max(0f, value);
				break;
			case Resource.Food:
				Food = Mathf.Max(0f, value);
				break;
			case Resource.Material:
				Materials = Mathf.Max(0f, value);
				break;
			case Resource.Time:
				Time = Mathf.Max(0f, value);
				break;
			case Resource.People:
				People = Mathf.Max(0f, value);
				break;
			default:
				throw new ArgumentException(string.Format("Unknown resource kind `{0}` given.", kind));

		}
	}

	public void AdjustResource(Resource kind, float amt)
	{
		var initialAmt = GetResource(kind);
		SetResource(kind, Mathf.Max(0f, initialAmt + amt));
	}

	private event InventoryChanged OnInventoryChanged;

	private void ReportResourceToStatistics(Resource kind, ResourceAction reportingKind, float amount)
	{
		GameController.LevelStatistics?.AdjustResourceStatistic(kind, reportingKind, amount);
	}

	public void ReportResourcesAs(ResourceAction resourceAction)
	{
		ReportResourceToStatistics(Resource.Food, resourceAction, Food);
		ReportResourceToStatistics(Resource.Material, resourceAction, Materials);
		ReportResourceToStatistics(Resource.Shield, resourceAction, Shield);
		ReportResourceToStatistics(Resource.People, resourceAction, People);
		ReportResourceToStatistics(Resource.Time, resourceAction, Time);
	}

	private void SignalInventoryChanged(Resource kind, float amt)
	{
		OnInventoryChanged?.Invoke(kind, amt);

		// We could report this action to the overall statistics, but we don't know why this change occurred.
		// We may not want to -- drawing down of inventory is by something else (an item producer). This something
		// reports the creation of a resource. We can't report resources as destroyed here.
		//
		// So, we can report general activity only. This will drive those items that do their own interrogation 
		// of items (the goal display mechanism).
		GameController.LevelStatistics?.GeneralInventoryUpdate();
	}

	public event InventoryChanged OnResourceChanged
	{
		add
		{
			OnInventoryChanged += value;
		}

		remove
		{
			OnInventoryChanged -= value;
		}
	}

	public float Food
	{
		get
		{
			return _Food;
		}

		set
		{
			if (value != _Food)
			{
				_Food = value;
				SignalInventoryChanged(Resource.Food, _Food);
			}
		}
	}

	public float Materials
	{
		get
		{
			return _Materials;
		}

		set
		{
			if (_Materials != value)
			{
				_Materials = value;
				SignalInventoryChanged(Resource.Material, _Materials);
			}
		}
	}

	public float Shield
	{
		get
		{
			return _Shield;
		}

		set
		{
			if (_Shield != value)
			{
				_Shield = value;
				SignalInventoryChanged(Resource.Shield, _Shield);
			}
		}
	}
	public float Time
	{
		get
		{
			return _Time;
		}

		set
		{
			if (_Time != value)
			{
				_Time = value;
				SignalInventoryChanged(Resource.Time, _Time);
			}
		}
	}
	public float People
	{
		get
		{
			return _People;
		}

		set
		{
			if (_People != value)
			{
				_People = value;
				SignalInventoryChanged(Resource.People, _People);
			}
		}
	}
}
