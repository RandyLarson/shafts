using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
	[Tooltip("The base amount of damage to inflict/take on collision, adjusted by VelocityMultiplier.")]
	public float DamageRate = 10;

	[Tooltip("Percentage of velocity to multiply the DamageRate by.")]
	public float VelocityMultiplier = .1f;

	HealthPoints Hp;
	Rigidbody2D OurRigidBody;

	private void Start()
	{
		Hp = GetComponent<HealthPoints>();
		OurRigidBody = GetComponent<Rigidbody2D>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		CheckCollision(collision.collider);
	}

	//private void OnTriggerEnter2D(Collider2D collision)
	//{
	//	CheckCollision(collision);
	//}

	private void CheckCollision(Collider2D collision)
	{
		if (collision.CompareTag(GameConstants.Player) || collision.CompareTag(GameConstants.Terrain))
			InfictDamage(collision.gameObject);
	}

	private void InfictDamage(GameObject other)
	{
		if (Hp == null || OurRigidBody == null)
			return;

		// Augment the damage rate by the relative velocities of the colliding objects.
		var otherRB = other.GetComponent<Rigidbody2D>();
		if (otherRB != null)
		{
			Vector2 relativeVelocity = otherRB.velocity - OurRigidBody.velocity;
			float damageToApply = DamageRate * relativeVelocity.magnitude * VelocityMultiplier;

			var hpOther = other.gameObject.GetComponent<HealthPoints>();
			if (hpOther)
			{
				hpOther.AdjustHealthBy(-damageToApply);
			}

			Hp.AdjustHealthBy(-damageToApply);
		}
	}
}
