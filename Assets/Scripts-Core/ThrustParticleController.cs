using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustParticleController : MonoBehaviour
{
	public float NonIdleLifetime = .1f;
	public float IdleThreshold = 4;
	private float InitialLifetime = .1f;
	private float SquaredIdleThreshold = 1f;

	private ParticleSystem MainEngineParticleSystem;
	private Rigidbody2D ShipBody;
	void Start()
	{
		MainEngineParticleSystem = GetComponent<ParticleSystem>();
		ShipBody = GetComponentInParent<Rigidbody2D>();

		if (MainEngineParticleSystem != null)
		{
			var mainTraits = MainEngineParticleSystem.main;
			InitialLifetime = mainTraits.startLifetime.constant;
			SquaredIdleThreshold = IdleThreshold * IdleThreshold;
		}
	}

	void Update()
	{
		if (MainEngineParticleSystem != null)
		{
			var mainTraits = MainEngineParticleSystem.main;
			if (ShipBody != null && ShipBody.velocity.sqrMagnitude <= IdleThreshold)
			{
				/// var pSize = new ParticleSystem.MinMaxCurve(1);
				// mainTraits.startSizeX = pSize;
				// mainTraits.startSizeY = pSize;

				mainTraits.startLifetime = InitialLifetime;
			}
			else
			{
				mainTraits.startLifetime = NonIdleLifetime;
			}

			//EmissionModule emissionModule = MainEngineParticleSystem.emission;
			//emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Max(.15f, mainEngineThrottle) * 10f);

		}
	}
}
