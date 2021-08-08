using UnityEngine;
using Assets.Scripts.Extensions;

namespace Milkman
{
	public class Gatherer : MonoBehaviour
	{
		public TagDomain ThingsToGather;

		private IInventory HostInventory { get; set; }

		private void Start()
		{
			HostInventory = GetComponent<InventoryHolder>();
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (HostInventory == null)
				return;

			AttemptPickup(collision.gameObject);
		}


		public void AttemptPickup(GameObject toPickup)
		{
			if ( ThingsToGather.IsInDomain(toPickup) )
				GatherItem(toPickup);
		}


		public void GatherItem(GameObject toCarry)
		{
			var freight = toCarry.GetComponent<Freight>();
			if ( freight != null )
            {
				HostInventory.AdjustResource(freight.Kind, freight.Amount);
				freight.Consumed();
            }
		}

	}
}