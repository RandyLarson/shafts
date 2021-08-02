using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Extensions;
using TMPro;
using Assets.Scripts.Munitions;

public class UIWeaponSlot : MonoBehaviour
{
	public MunitionSlot WeaponInSlot;
	public Image WeaponImage;
	public bool IsSelected = false;
	public GameObject SelectedMarker;
	public TextMeshProUGUI Capacity;

	public float UpdateTime { get; set; }

	public void UpdateDisplay()
	{
		SelectedMarker.SafeSetActive(IsSelected);
		Capacity.SafeSetText(WeaponInSlot.Munition.Capacity > 0 ? WeaponInSlot.Count.ToString() : string.Empty);
	}

	public void SetSelected(bool to)
	{
		IsSelected = to;
		UpdateDisplay();
	}

	public void SetSlotItem(MunitionSlot toItem)
	{
		if (toItem == WeaponInSlot)
			return;

		if (toItem == null )
		{
			WeaponImage.sprite = null;
			return;
		}

		// The given item must have a sprite
		if ( toItem.Munition.GetComponent(out SpriteRenderer itemSprite))
		{
			WeaponImage.sprite = itemSprite.sprite;
		}
		WeaponInSlot = toItem;

	}
}
