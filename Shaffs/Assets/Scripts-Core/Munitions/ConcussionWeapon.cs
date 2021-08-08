using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.Munitions
{
	public class ConcussionWeapon : Munition
	{
		[Tooltip("True to enable potential freight drop")]
		public bool ForceFreightRelease = true;

		[Tooltip("Anything carried by target will possibly drop it")]
		[Range(0, 1)]
		public float ChanceToDislodgeFreight = .3f;

		[Tooltip("Flight control systems of target will be disabled for this duration.")]
		public float DisablesTargetDuration = 0f;

		protected override void HandleCollision(Vector2 atContact, Collider2D other)
		{
			base.HandleCollision(atContact, other);

			if (DamageApplied)
			{
				if (gameObject.GetComponent(out Rigidbody2D weaponBody))
				{
					Rigidbody2D otherBody = other.gameObject.GetComponent<Rigidbody2D>();
					if (null == otherBody)
					{
						otherBody = other.gameObject.GetComponentInParent<Rigidbody2D>();
					}

					if (otherBody != null)
					{
						otherBody.AddForce(weaponBody.velocity, ForceMode2D.Impulse);
					}
				}

				if (ForceFreightRelease && other.gameObject.GetInterfaceInChildren(out IFreightController controller))
				{
					if (Random.value < ChanceToDislodgeFreight)
						controller.DropFreight();
				}

				if (DisablesTargetDuration > 0 && other.gameObject.GetInterfaceInChildren(out ShipCharacteristics shipsCharacteristics))
				{
					shipsCharacteristics.DisableSystemsUntil(Time.time + DisablesTargetDuration);
				}
			}
		}

	}
}
