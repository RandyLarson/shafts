using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipCharacteristics : MonoBehaviour
{
	public float EngineForce;
	public bool AutoForce = true;
	public float MaxSpeed = 100;
	public float TimeToMaxSpeed = 10;

	[Range(.1f, 3f)]
	public float ProximitySlowingFactor = 1.4f;
	public float ManeuveringDrag = .6f;

	/// <summary>
	/// The angle between the ship's velocity and the vector toward the target
	/// at which the 'maneuveringDrag' kicks in. This is used by the move-toward
	/// script when piloting ships toward targets. If the angle (as returned by the dot
	/// product between the ship's velocity and the `aim-at` vector) is less than this
	/// value, drag will be increased to facilitate making the adjustments.
	/// </summary>
	[Range(0, 1)]
	public float ManeuveringAngle = .15f;


	public float DragAtMaxSpeed;
	public float DragMinimum = .2f;

	public float PropulsiveForce => EngineForce;
	public Vector3 LastCourseVector = Vector3.zero;

	public float NextTerrainCheck { get; set; } = 0;
	public float TerrainCheckInterval = .1f;
	public bool ShouldCheckAvoidance { get => Time.time >= NextTerrainCheck; }

	public float SystemsDisabledUntil { get; set; } = 0;
	public bool AreSystemsDisabled { get => Time.time < SystemsDisabledUntil; }

	internal void DisableSystemsUntil(float timeToRenable)
	{
		SystemsDisabledUntil = timeToRenable;
	}

	public void UpdateAvoidanceCheck() => NextTerrainCheck = Time.time + TerrainCheckInterval;

	private void Awake()
	{
		var OurRB = gameObject.GetComponent<Rigidbody2D>();
	}

	private Rigidbody2D OurRB;
	private void Start()
	{
		OurRB = GetComponent<Rigidbody2D>();
		TerrainCheckInterval = Time.fixedDeltaTime * 3;
		if (AutoForce)
		{
			EngineForce = MaxSpeed / TimeToMaxSpeed * OurRB.mass;
			DragAtMaxSpeed = /*.9f**/ EngineForce / OurRB.mass / MaxSpeed * GameController.TheController.AutoForceDragMultiplier;
			//EngineForce *= 1.2f;
			OurRB.drag = 0;
		}
	}

	private void Update()
	{
		if (AreSystemsDisabled)
		{
			OurRB.gravityScale = 1;
		}
		else
		{
			OurRB.gravityScale = 0f;
		}
	}
}
