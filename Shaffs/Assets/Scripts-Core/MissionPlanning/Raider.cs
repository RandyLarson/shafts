using System;
using System.Collections.Generic;
using Assets.Scripts.Munitions;
using UnityEngine;

[RequireComponent(typeof(MissionPlanner))]
class Raider : MonoBehaviour
{
	[Tooltip("The tool used to capture ")]
	public TractorBeam CaptureWeapon = null;

	private MissionPlanner MissionPlanner { get; set; } = null;

	private void Start()
	{
		MissionPlanner = GetComponent<MissionPlanner>();
	}

	private void Update()
	{
		AttemptCapture();
	}

	private void AttemptCapture()
	{
		// We are looking for freight being carried by the target craft.
		if (MissionPlanner.CurrentTarget == null)
			return;

		Freight targetFreight =  MissionPlanner.CurrentTarget.gameObject.GetComponentInChildren<Freight>();
		if ( targetFreight != null && CaptureWeapon != null)
		{
			CaptureWeapon.FireWeapon(targetFreight.gameObject);
		}

	}
}
