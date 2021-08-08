using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.Munitions
{
	public class BeamWeapon : Munition
	{
		private GameObject ImpactExplosionInstance { get; set; }

		public override bool ShouldInstantiateMunitionExplosion()
		{
			return DamageApplied;
		}

		protected override void HandleCollision(Vector2 atContact, Collider2D other)
		{
			if (other == null || other.gameObject == null)
				return;

			other.attachedRigidbody.AddForce(-other.attachedRigidbody.velocity * .9f, ForceMode2D.Impulse);

			if (PotentialDamage != null)
			{
				if (PotentialDamage.Radius > 0)
					ApplyAreaDamage();
				else
					ApplyDamageTo(atContact, other.gameObject);
			}

			if (ImpactExplosionInstance == null)
			{
				if (ImpactExplosion != null)
				{
					ImpactExplosionInstance = Instantiate(ImpactExplosion, atContact, transform.rotation);
				}
			}
			else
			{
				ImpactExplosionInstance.transform.position = atContact;
			}
		}

		internal void SetTargetPosition(Vector3 position)
		{
		}

		private void OnTriggerStay2D(Collider2D collision)
		{
			if (!IsSingleUse)
				HandleTriggerCollision(collision);
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (ImpactExplosionInstance != null)
			{
				Destroy(ImpactExplosionInstance);
			}
		}

	}
}
