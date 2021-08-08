using Assets.Scripts;
using Assets.Scripts.Extensions;
using UnityEngine;

public class GuidedProjectileController : MonoBehaviour
{

	public GameObject target;
	public float Force;
	public float FuelLifetime = 10;
	public bool DestructOnNoFuel = false;

	private Rigidbody2D ourRB;
	private Rigidbody2D targetRB;
	private float BirthTime = 0f;

	// Use this for initialization
	void Start()
	{
		ourRB = GetComponent<Rigidbody2D>();
		target.GetComponent(out targetRB);
		BirthTime = Time.time;
	}

	private void OnDestroy()
	{
		//Destroy(gameObject);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (target == null || Time.time - BirthTime > FuelLifetime)
		{
			if (DestructOnNoFuel)
			{
				Destroy(gameObject);
			}
			else
			{
				// Point down and let gravity take it.
				ourRB.gravityScale += .5f;

				if (gameObject.GetComponentInChildren<TrailRenderer>(out var renderer))
				{
					renderer.gameObject.SetActive(false);
				}
			}
			return;
		}
		try
		{
			Vector3 aimAt = AimingHelpers.FirstOrderIntercept(transform.position, Vector3.zero, ourRB.velocity.magnitude, target.transform.position, targetRB.velocity);

			// Rotate about the `forward` (z) axis toward the given target.
			//var translatedTarget = aimAt - transform.position;
			//float delta = Vector3.SignedAngle(transform.position, aimAt, transform.forward);
			//if (Mathf.Abs(delta) < 6)
			//	return;

			//Vector3 diff = aimAt - transform.position;
			//diff.Normalize();

			//float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
			//transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);

			transform.up = aimAt - transform.position;

			//Debug.DrawRay(transform.position, transform.up * 5, Color.green, .2f);
			//Debug.DrawRay(transform.position, ourRB.velocity, Color.red, .2f);
			//Debug.DrawRay(transform.position, transform.right * 5, Color.yellow, .2f);

			var forceFwd = Force * Time.deltaTime * transform.up;
			//Debug.DrawRay(transform.position, forceFwd, Color.blue, .2f);

			ourRB.AddForce(forceFwd);
			//ourRB.AddForce(transform.up - transform.right, ForceMode2D.Impulse);
			//ourRB.AddForce(-ourRB.velocity * 10);

		}
		catch (System.Exception)
		{
			Destroy(gameObject);
		}
	}
}
