using Assets.Scripts.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Extensions;

public class AutoShipStabalizer : MonoBehaviour
{
	private void Start()
	{
		if (gameObject.GetComponent<CivilianFlightControl>(out CivilianFlightControl controller))
		{
			if (controller.RotateShipDuringFlight)
			{
				Destroy(this);
			}
		}
	}
	private void Update()
	{
		AutoStabalizer.StabalizeRotation(transform);
	}
}
