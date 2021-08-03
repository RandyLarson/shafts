using Milkman;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
	[Tooltip("The amount of damage inflicted.")]
	public float DamageAmt = 10;
	[Tooltip("The interval between damage being inflicted.")]
	public float DamageInterval = 1;

	[Tooltip("Spawned when damage inflicted")]
	public GameObject DamageIndicator;

	private float LastInflictedDamageAt = 0;

	private void Start()
	{
		LastInflictedDamageAt = 0;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//var player = collision.gameObject.GetComponent<PlayerShip>();
		//if ( player != null )

		if (collision.gameObject.CompareTag(GameConstants.Player))
			InfictDamage(collision, collision.gameObject);
	}

    private void OnCollisionStay2D(Collision2D collision)
    {
		if (collision.gameObject.CompareTag(GameConstants.Player))
			InfictDamage(collision, collision.gameObject);
	}
	
	private void InfictDamage(Collision2D collision, GameObject other)
	{
		if (Time.time - LastInflictedDamageAt > DamageInterval)
		{
			var hpOther = other.gameObject.GetComponent<HealthPoints>();
			if (hpOther)
			{
				LastInflictedDamageAt = Time.time;
				if ( DamageIndicator != null )
                {
					var visual = Instantiate(DamageIndicator, collision.contacts[0].point, other.transform.rotation);
					Destroy(visual, 2);
                }

				hpOther.AdjustHealthBy(-DamageAmt);
			}
		}
	}
}
