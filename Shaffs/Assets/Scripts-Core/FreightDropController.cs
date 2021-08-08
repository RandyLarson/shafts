using System.Linq;
using UnityEngine;

public class FreightDropController : MonoBehaviour
{
	public Resource[] AcceptedResources;

	public GameObject healthEffect;
	public IResourceConsumer freightRecipient { get; set; }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag != "Freight")
			return;

		var freightInfo = other.GetComponent<Freight>();
		if (freightInfo && (AcceptedResources.Length == 0 || AcceptedResources.Contains(freightInfo.Kind)))
		{
			if (healthEffect != null)
				Instantiate<GameObject>(healthEffect, other.transform.position, other.transform.rotation);

			if (freightRecipient != null)
				freightRecipient.AdjustResource(freightInfo.Kind, freightInfo.Amount);

			FreightDelivered(freightInfo);
		}
	}

	protected virtual void FreightDelivered(Freight freight)
	{
		freight.WasDelivered = true;
		Destroy(freight.gameObject);
	}
}
