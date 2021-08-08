using System;
using Assets.Scripts.Extensions;
using Assets.Scripts.Helpers;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
	public float TimeToDestruction = 10f;
	private float TimeOfDestruction;
	public IDamageInflictor DamageApplyer;

	void Start()
	{
		TimeOfDestruction = Time.time + TimeToDestruction;
		DamageApplyer = gameObject.GetComponent<IDamageInflictor>();
	}

	void Update()
	{
		if (Time.time > TimeOfDestruction)
		{
			if (DamageApplyer != null)
			{
				DamageApplyer.ApplyAreaDamage();
				DamageApplyer.DestroySelf();
			}
			else if ( gameObject.GetComponent<HealthPoints>(out HealthPoints itsHP))
			{
				itsHP.AdjustHealthBy(-itsHP.HP);
			}
		}
	}
}
