using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Extensions;
using System.Linq;
using Assets.Scripts.Munitions;

public class UIWeaponsController : MonoBehaviour
{
	public UIWeaponSlot Prototype;
	public List<UIWeaponSlot> ActiveUIElements;
	public List<UIWeaponSlot> InActiveUIElements;

	private void Start()
	{
		GameController.TheController.OnGameEvent += TheController_OnGameEvent;
		GameController.LevelStatistics.OnStatisticsChanged += LevelStatistics_OnStatisticsChanged;
		DeactivateInActives();
		UpdateDisplay();
	}

	private void LevelStatistics_OnStatisticsChanged()
	{
		UpdateDisplay();
	}

	private void TheController_OnGameEvent(string eventName, float _)
	{
		if (eventName == GameConstants.EventPlayerWeaponChange)
		{
			UpdateDisplay();
		}
	}

	public void DeactivateInActives()
	{
		foreach ( UIWeaponSlot item in InActiveUIElements )
		{
			item.SafeSetActive(false);
		}
	}

	public void UpdateDisplay()
	{
		if (GameController.ThePlayer != null)
		{
			foreach (MunitionSlot munition in GameController.ThePlayer.WeaponRack)
			{
				var uiElement = GetExistingUIFor(munition);
				if (uiElement == null)
				{
					uiElement = CreateDisplayFor(munition);
					ActiveUIElements.Add(uiElement);
				}
				uiElement.UpdateTime = Time.time;
				uiElement.SetSelected(GameController.ThePlayer.MainWeapon == munition);
			}
		}
		PruneOrphanUIElements();
	}

	void PruneOrphanUIElements()
	{
		var toPrune = ActiveUIElements
			.Where(member => member.UpdateTime != Time.time)
			.ToArray();

		foreach (var pruneMe in toPrune)
		{
			DecommissionUIFor(pruneMe);
		}
	}

	UIWeaponSlot GetExistingUIFor(MunitionSlot seeking)
	{
		return ActiveUIElements.FirstOrDefault(existing => existing.WeaponInSlot == seeking);
	}

	void DecommissionUIFor(UIWeaponSlot toHide)
	{
		toHide.SafeSetActive(false);
		ActiveUIElements.Remove(toHide);
		InActiveUIElements.Add(toHide);
		
		// If not saving items:
		//toHide.gameObject.transform.SetParent(null);
		//toHide.DestroyGameObject();
	}

	UIWeaponSlot CreateDisplayFor(MunitionSlot munition)
	{
		// Find an inactive controller and use that.
		// Assign over munition for it to hang onto and get the icon from.
		if (InActiveUIElements.Count > 0)
		{
			UIWeaponSlot nextSlot = InActiveUIElements.First();
			InActiveUIElements.Remove(nextSlot);
			nextSlot.SetSlotItem(munition);
			nextSlot.SafeSetActive(true);
			return nextSlot;
		}
		else
		{
			// Create a weapon slot holder and assign over munition for
			// it to hang onto and get the icon from.
			UIWeaponSlot slotItem = GameObject.Instantiate(Prototype);
			slotItem.SetSlotItem(munition);
			slotItem.transform.SetParent(transform);
			slotItem.gameObject.SetActive(true);
			return slotItem;
		}
	}

}
