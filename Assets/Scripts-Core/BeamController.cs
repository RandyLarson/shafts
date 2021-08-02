using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Extensions;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamController : MonoBehaviour
{
	public GameObject BeamImpactObject;
	LineRenderer TheBeam { get; set; }

	public float RetargetSpeed = 10;
	public bool FadeOut { get; set; } = false;


	private Vector2 SavedLineWidths { get; set; } = new Vector2();
	public Vector3? NextTarget { get; set; }
	private Vector3? NextPosition1Offset { get; set; }
	private BoxCollider2D BeamCollider { get; set; }

	private void Awake()
	{
		TheBeam = GetComponent<LineRenderer>();
		BeamCollider = GetComponent<BoxCollider2D>();
		SavedLineWidths = new Vector2(TheBeam.startWidth, TheBeam.endWidth);
	}

	void Update()
	{
		if (NextTarget.HasValue)
		{
			if (TheBeam.GetPosition(1) == NextTarget.Value)
			{
				NextTarget = null;
			}
			else
			{
				Vector3 currentPos = TheBeam.GetPosition(1);
				Vector3 nextPos = Vector3.Lerp(currentPos, NextPosition1Offset.Value, Time.deltaTime * RetargetSpeed);
				TheBeam.SetPosition(1, nextPos);
				BeamImpactObject.transform.position = NextTarget.Value;
				AdjustCollider();
			}
		}

		if (FadeOut)
		{
			TheBeam.startWidth = Mathf.Lerp(TheBeam.startWidth, 0, Time.deltaTime * RetargetSpeed);
			TheBeam.endWidth = Mathf.Lerp(TheBeam.endWidth, 0, Time.deltaTime * RetargetSpeed);

			if (TheBeam.startWidth == 0 || TheBeam.endWidth == 0)
			{
				TheBeam.gameObject.SafeSetActive(false);
				FadeOut = false;
			}
		}
	}

	private void AdjustCollider()
	{
		Vector3 beamGeometry = TheBeam.GetPosition(1) - TheBeam.GetPosition(0);
		BeamCollider.offset = new Vector2(0, beamGeometry.y/2);
		BeamCollider.size = new Vector2((TheBeam.endWidth + TheBeam.startWidth) / 2, Math.Abs(beamGeometry.y));
	}

	public bool Active
	{
		get
		{
			return TheBeam.gameObject.activeSelf;
		}
		set
		{
			// Fade out?
			if (value == false)
			{
				FadeOut = true;
			}
			else
			{
				TheBeam.gameObject.SafeSetActive(true);
				TheBeam.startWidth = SavedLineWidths.x;
				TheBeam.endWidth = SavedLineWidths.y;
			}
		}
	}

	internal void TurnOff()
	{
		TheBeam.enabled = false;
		BeamCollider.enabled = false;
		BeamImpactObject.SafeSetActive(false);
	}

	internal void TurnOn()
	{
		TheBeam.enabled = true;
		BeamCollider.enabled = true;
		BeamImpactObject.SafeSetActive(true);
	}

	public void AimAt(Vector3 targetPoint, bool lerpIt = true)
	{
		if (lerpIt)
		{
			// This is relative to position 0
			NextPosition1Offset = targetPoint - transform.position;
		}
		else
		{
			TheBeam.SetPosition(1, targetPoint - transform.position);
			BeamImpactObject.transform.position = targetPoint;
			AdjustCollider();
		}
	}
}
