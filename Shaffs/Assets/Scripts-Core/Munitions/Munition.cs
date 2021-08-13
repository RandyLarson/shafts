using Assets.Scripts.Extensions;
using Assets.Scripts.Helpers;
using System;
using System.Linq;
using UnityEngine;
public class Munition : MonoBehaviour, IDamageInflictor
{
	[Tooltip("Unique identifier for weapon.")]
	public string Identification;
	[Tooltip("Current inventory, will be removed when zero and capacity is > 0.")]
	public int Count = 0;
	[Tooltip("Maximum number to be carried at one time, 0 for unlimited.")]
	public int Capacity = 0;
	public string[] TargetTags;
	public bool DestroyOnContact = true;
	public bool RequireRigidBody = true;
	public bool EnableTrigger = false;
	public bool ChainDetonation = false;
	public float LaunchForce = 50f;
	public float FireRate = 1;
	public bool IsSingleUse = true;
	public GameObject Explosion = null;
	public GameObject ImpactExplosion = null;

	public AudioSource LoadingAudio;

	/// <summary>
	/// Tracks if this munition was created by the Player, in order to keep statistics.
	/// </summary>
	public bool IsPlayerMunition = false;

	public static RaycastHit2D[] SharedHitPool = new RaycastHit2D[25];

	public bool DamageApplied { get; protected set; } = false;
	protected Damage PotentialDamage { get; set; } = null;

	public void Start()
	{
		PotentialDamage = GetComponent<Damage>();
		HealthPoints ourHP = GetComponent<HealthPoints>();

		if (ourHP)
		{
			ourHP.OnDestroyed += OurGameObject_OnDestroyed;
		}
	}

	private void OurGameObject_OnDestroyed()
	{
		if (!DamageApplied && ChainDetonation)
		{
			ApplyAreaDamage();
		}
	}


	private bool IsValidTarget(Vector3 originPt, GameObject target)
	{
		bool potentialTarget = true;
		if (target.gameObject.CompareTag(GameConstants.Shield))
		{
			return true;
		}

		if (TargetTags == null ||
			TargetTags.Length == 0 ||
			TargetTags.Any(tag => target.CompareTag(tag)))
		{
			potentialTarget = true;
		}

		// Is it shielded? But not a shield itself.
		if (potentialTarget && originPt != Vector3.zero)
		{
			if (0 < Physics2D.LinecastNonAlloc(originPt, target.transform.position, SharedHitPool, GameConstants.LayerMaskShield))
				potentialTarget = false;
		}

		return potentialTarget;
	}

	protected void InflictDamage(GameObject target, float damage)
	{
		DamageApplied = true;

		var hpOther = target.GetComponent<HealthPoints>();
		if (hpOther)
		{
			// Returns true on destroyed.
			// Destroyed => report statistics. We could also report all damage inflicted to tally that.
			bool wasDestroyed = hpOther.AdjustHealthBy(-damage);

			if (IsPlayerMunition)
			{
				GameController.LevelStatistics.DamageInflicted(damage);
				if (wasDestroyed)
					GameController.LevelStatistics.EnemyDestroyed();
			}

		}
	}

	private void InflictDamage(Vector2 atPoint, GameObject other)
	{
		CreateSmoke(atPoint, other, PotentialDamage.DamagePotential);
		InflictDamage(other.gameObject, PotentialDamage.DamagePotential);
	}

	public virtual bool ApplyDamageTo(Vector2 atPoint, GameObject other)
	{
		if (IsValidTarget(Vector3.zero, other))
		{
			InflictDamage(atPoint, other);
			return true;
		}
		return false;
	}

	public virtual void ApplyAreaDamage()
	{
		if (PotentialDamage == null)
			return;

		int numHits = Physics2D.CircleCastNonAlloc(transform.position, PotentialDamage.Radius, transform.position, SharedHitPool, 0f);

		for (int i = 0; i < numHits; i++)
		{
			RaycastHit2D nextHit = SharedHitPool[i];
			if (nextHit.collider.gameObject == gameObject)
				continue;

			if (IsValidTarget(transform.position, nextHit.collider.gameObject))
			{
				InflictDamage(nextHit.point, nextHit.collider.gameObject);
				CreateExplosion(nextHit.point, nextHit.transform.rotation, nextHit.collider.gameObject.transform);
			}
		}
	}

	public virtual void CreateExplosion(Vector2 point, Quaternion rotation, Transform parent)
	{
		if (Explosion != null && ShouldInstantiateMunitionExplosion())
		{
			var explosion = Instantiate(Explosion, point, rotation, parent);
			GameObject.Destroy(explosion, 2);
		}
	}

	internal void PlayLoadingAudio()
	{
		if (LoadingAudio != null)
		{
			LoadingAudio.Play();
		}
		else
		{
			AudioManager.TheAudioManager.Play("WeaponChanged");
		}
	}


	protected virtual void HandleCollision(Vector2 atContact, Collider2D other)
	{
		DamageApplied = false;

		if (DestroyOnContact == false || other == null || other.gameObject == null)
			return;

		if (PotentialDamage != null)
		{
			if (PotentialDamage.Radius > 0)
				ApplyAreaDamage();
			else
				ApplyDamageTo(atContact, other.gameObject);
		}

		if (ImpactExplosion != null)
		{
			GameObject impactObject = Instantiate(ImpactExplosion, atContact, transform.rotation);
			Destroy(impactObject, 3);
		}

		CreateExplosion(atContact, transform.rotation, other.gameObject.transform);

		if (DamageApplied && IsSingleUse)
		{
			DestroySelf();
		}
	}

	virtual public bool ShouldInstantiateMunitionExplosion()
	{
		return DamageApplied && IsSingleUse;
	}

	private void CreateSmoke(Vector2 atPoint, GameObject parent, float damageDone)
	{
		if (GameController.TheController.SmokeEffect == null)
			return;

		DamageVisualization existingDamage;

		if (!parent.GetComponentInChildren(out existingDamage))
		{
			existingDamage = Instantiate(GameController.TheController.SmokeEffect, atPoint, Quaternion.Euler(-90, 0, 0), parent.transform);
			existingDamage.AddDamage(atPoint, damageDone);
		}
		else
		{
			existingDamage.AddDamage(atPoint, damageDone);
		}


		//float destroyAfter;
		//if (parent.CompareTag(GameConstants.Shield))
		//	destroyAfter = 2;
		//else
		//	destroyAfter = 5 + damageDone / 2;

		//existingDamage.transform.localScale = Vector3.one * damageDone / 10;
		//Destroy(existingDamage, destroyAfter);
	}


	//private void CreateSmoke(Collision2D theCollision, float damageDone)
	//{
	//	if (null == theCollision)
	//		return;

	//	for (int i = 0; i < theCollision.contactCount; i++)
	//	{
	//		ContactPoint2D theContact = theCollision.GetContact(i);
	//		CreateSmoke(theContact.point, theContact.collider.gameObject, damageDone);
	//	}

	//}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Vector2 contactPoint = Vector2.zero;

		if (0 < collision.contactCount)
		{
			ContactPoint2D theContact = collision.GetContact(0);
			contactPoint = theContact.point;
		}

		HandleCollision(contactPoint, collision.collider);
	}

	ContactPoint2D[] TriggerContacts = new ContactPoint2D[10];

	protected void OnTriggerEnter2D(Collider2D collision)
	{
		HandleTriggerCollision(collision);
	}


	protected virtual void HandleTriggerCollision(Collider2D collision)
	{
		if (!EnableTrigger || (RequireRigidBody && collision.attachedRigidbody == null))
			return;

		Vector2 contactPoint = Vector2.zero;

		var ourCollider = GetComponent<Collider2D>();
		if (ourCollider != null)
		{
			RaycastHit2D[] hits = new RaycastHit2D[20];
			int numHits = ourCollider.Cast(Vector2.zero, hits);
			if (numHits > 0)
			{
				foreach (var hit in hits)
				{
					if (hit.collider == collision)
					{
						contactPoint = hit.point;
						break;
					}
				}
			}
		}

		HandleCollision(contactPoint, collision);
	}


	public void DestroySelf()
	{
		Destroy(gameObject);
	}
}
