using UnityEngine;

public class CityShieldController : ShieldController
{
	private void EvaluateCollision(Collider2D other)
	{
		if (ShieldAnimator.GetBool(GameConstants.ShieldDeactivatedHash) || other.tag == "Villager" || other.tag == "Boundary")
			return;

		if (other.tag == "Freight" || other.tag == "Player" || other.tag == "CiviAircraft")
		{
			UpdateShieldStatus(false);
			EnableShieldsAt = Time.time + 1f;
		}
		else
		{
			if (explosion != null)
				Instantiate<GameObject>(explosion, other.transform.position, other.transform.rotation);

			ShieldAnimator.SetTrigger(GameConstants.ShieldHitHash);
			UpdateShieldStatus();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		EvaluateCollision(collision.collider);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		EvaluateCollision(other);
	}
}
