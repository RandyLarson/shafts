using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageVisualization : MonoBehaviour
{
	public GameObject Visual;
	public GameObject VisualInstance { get; set; }
	public ParticleSystem VisualParticleSystem { get; set; }

	HealthPoints ParentHP;

	public float FadeOutDuration = 7;
	private float FadeOutStart { get; set; } = 0;
	private float DamageAccumulated { get; set; } = 0;

	private void OnDestroy()
	{
	}
	
	void Start()
	{
		ParentHP = transform.parent.GetComponent<HealthPoints>();
		if (ParentHP != null)
		{
			ParentHP.OnDestroyed += ParentHP_OnDestroyed;
			ParentHP.OnHealthChanged += ParentHP_OnHealthChanged;
		}
	}

	private void ParentHP_OnHealthChanged(GameObject gameObject, float orgValue, float currentValue)
	{
		DamageAccumulated = orgValue - currentValue;
		DamageAdjusted();
	}

	private void ParentHP_OnDestroyed()
	{	
		gameObject.transform.SetParent(null);
		FadeOutStart = Time.time;
		Destroy(this, FadeOutDuration + .5f);

		if(VisualParticleSystem)
		{
			VisualParticleSystem.Stop();
		}
	}

	void Update()
	{
		if (VisualInstance != null && FadeOutStart > 0 && Time.time >= FadeOutStart)
		{
			float a = Mathf.Lerp(1, 0, (Time.time - FadeOutStart) / FadeOutDuration);
			//VisualInstance.transform.localScale *= a;

			//var toFade = this;
			//{
			//	toFade.item.color = new Color(toFade.org.r, toFade.org.g, toFade.org.b, a);

			//	if (a <= .05)
			//	{
			//		Destroy(this);
			//	}
			//}
		}

		if (Time.time > FadeOutStart + FadeOutDuration)
		{
			Destroy(VisualInstance);
			VisualInstance = null;
			VisualParticleSystem = null;
		}
	}

	internal void DamageAdjusted()
	{
		if ( VisualInstance == null )
		{
			AddDamage(Vector2.zero, DamageAccumulated);
		}
	}

	internal void AddDamage(Vector2 atPoint, float damageDone)
	{
		if (DamageAccumulated == 0)
			DamageAccumulated = damageDone;

		if (VisualInstance == null)
		{
			if (Visual == null)
				return;

			VisualInstance = Instantiate(Visual, atPoint, transform.rotation, gameObject.transform);
			VisualParticleSystem = VisualInstance.GetComponent<ParticleSystem>();
		}

		float destroyAfter;
		if (gameObject.CompareTag(GameConstants.Shield))
			destroyAfter = 2;
		else
			destroyAfter = 5 + DamageAccumulated / 2;

		VisualInstance.transform.localScale = Vector3.one + (Vector3.one * DamageAccumulated / 10);
		FadeOutStart = Time.time + destroyAfter;
	}
}
