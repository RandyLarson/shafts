using UnityEngine;
using Assets.Scripts.Extensions;

namespace Milkman
{
	public class PickUp : MonoBehaviour, IFreightController
	{
		public Transform CarryPoint;
		public GameObject Carrying = null;
		public string TagToPickup = GameConstants.Freight;

		[Tooltip("A multiplier on the mass of carried objects (.5 => freight's mass is halved while being carried")]
		[Range(0, 1)]
		public float CarryDensityModifier = .4f;

		private GameObject LastDroppedItem { get; set; }
		private float RecaptureTime { get; set; } = 0;

		public bool IsCarryingFreight => Carrying != null;

		public Rigidbody2D HostRigidBody { get; private set; }
		private RBHoldingValues? CarryingRigidBodyHolder = null;

		private void Start()
		{
			HostRigidBody = GetComponent<Rigidbody2D>();
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.CompareTag(TagToPickup))
				AttemptPickup(collision.gameObject);
		}


		public void AttemptPickup(GameObject toPickup)
		{
			CarryItem(toPickup);
		}


		public void CarryItem(GameObject toCarry)
		{
			if (Carrying != null)
				return;

			if (toCarry == LastDroppedItem && Time.time < RecaptureTime)
				return;

			// Want to mount the item to the ship at the carry point
			// Make it a child of this game object
			var itsRB = toCarry.GetComponent<Rigidbody2D>();
			if (itsRB != null)
			{
				CarryingRigidBodyHolder = new RBHoldingValues()
				{
					physicsMaterial2D = itsRB.sharedMaterial,
					angularDrag = itsRB.angularDrag,
					drag = itsRB.drag,
					gravityScale = itsRB.gravityScale,
					mass = itsRB.mass,
					useAutoMass = itsRB.useAutoMass
				};
				Destroy(itsRB);
			}

			toCarry.transform.SetPositionAndRotation(CarryPoint.position, transform.rotation);
			toCarry.transform.SetParent(CarryPoint);
			Carrying = toCarry;

			// Density can only be changed after their is a parent with auto-mass, I think.
			if (toCarry.GetComponents<Collider2D>(out var collider2Ds))
			{
				foreach (var aCollider in collider2Ds)
					aCollider.density = aCollider.density * CarryDensityModifier;
			}
		}

		public void DropFreight()
		{
			if (Carrying == null)
				return;

			try
			{
				LastDroppedItem = Carrying;
				RecaptureTime = Time.time + 2f;

				Carrying.transform.SetParent(null);
				var theirRB = Carrying.GetComponent<Rigidbody2D>();
				if (null == theirRB)
				{
					theirRB = Carrying.AddComponent<Rigidbody2D>();
					if (CarryingRigidBodyHolder != null)
					{
						theirRB.angularDrag = CarryingRigidBodyHolder.Value.angularDrag;
						theirRB.drag = CarryingRigidBodyHolder.Value.drag;
						theirRB.gravityScale = CarryingRigidBodyHolder.Value.gravityScale;
						theirRB.mass = CarryingRigidBodyHolder.Value.mass;
						theirRB.useAutoMass = CarryingRigidBodyHolder.Value.useAutoMass;
						theirRB.sharedMaterial = CarryingRigidBodyHolder.Value.physicsMaterial2D;
					}
				}

				if (Carrying.GetComponent(out Collider2D theirCollider))
					theirCollider.density /= CarryDensityModifier;


				if (theirRB != null)
				{
					Vector3 ourVelocity = new Vector2(HostRigidBody.velocity.x, HostRigidBody.velocity.y);
					//					theirRB.AddForce(ourVelocity + -transform.up * 4, ForceMode2D.Impulse);
					theirRB.velocity = ourVelocity;
					theirRB.AddForce(-transform.up * 10, ForceMode2D.Impulse);
				}
			}
			finally
			{
				Carrying = null;
				CarryingRigidBodyHolder = null;
			}
		}

	}
}