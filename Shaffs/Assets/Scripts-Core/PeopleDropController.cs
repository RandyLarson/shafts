using UnityEngine;

public class PeopleDropController : FreightDropController
{
	protected override void FreightDelivered(Freight itemDelivered)
	{
		itemDelivered.gameObject.SetActive(false);
	}
}
