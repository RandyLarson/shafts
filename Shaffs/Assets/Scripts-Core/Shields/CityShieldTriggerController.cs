using UnityEngine;

public class CityShieldTriggerController : MonoBehaviour
{
	public CityShieldController PhysicalShieldController;

	private void Start()
	{
		if (PhysicalShieldController == null)
		{
			PhysicalShieldController = transform.parent.GetComponent<CityShieldController>();
			Collider2D outerCollider = PhysicalShieldController.GetComponent<Collider2D>();
			Physics2D.IgnoreCollision(outerCollider, GetComponent<Collider2D>());
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (PhysicalShieldController == null || !PhysicalShieldController.IsShieldActive)
			return;

		if (other.CompareTag("Freight") || other.CompareTag("Player"))
		{
			PhysicalShieldController.UpdateShieldStatus(false);
			PhysicalShieldController.EnableShieldsAt = Time.time + 1f;
		}
	}
}
