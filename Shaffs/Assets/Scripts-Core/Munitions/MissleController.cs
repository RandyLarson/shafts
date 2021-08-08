using UnityEngine;
using System.Linq;

public class MissleController : MonoBehaviour
{

	public float RateOfFire = 1f;
	public Munition MainWeapon;
	public float LaunchForce = 50;
	public float RotationSpeed = 5;
	public float RadarDistance = 30f;
	public Transform MainWeaponSpawn;
	public string[] TargetTags;
	public int Inventory = 30;
	private Rigidbody2D ourRigidBody;

	private float nextFire = 0f;

	// Use this for initialization
	void Start()
	{
		ourRigidBody = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		if ((Inventory == -1 || Inventory > 0) && Time.time > nextFire)
		{
			GameObject target = ScanForTargets();
			FireWeapon(target);
		}
	}

	private GameObject ScanForTargets()
	{
		var targets = Physics2D.OverlapCircleAll(gameObject.transform.position, RadarDistance)
			.Where(other => TargetTags.Contains(other.gameObject.tag))
			.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position));

		if ( targets.Any() )
			return targets.First().gameObject;

		return null;
	}

	public void FireWeapon(GameObject target)
	{
		if (target != null && MainWeapon != null && MainWeaponSpawn != null && Time.time >= nextFire)
		{
			nextFire = Time.time + RateOfFire;
			Inventory--;

			var shot = Instantiate(MainWeapon, MainWeaponSpawn.position, MainWeaponSpawn.rotation);
			var guidedController = shot.GetComponent<GuidedProjectileController>();
			if ( guidedController)
			{
				guidedController.target = target;
			}

			var rb = shot.GetComponent<Rigidbody2D>();
			if (rb)
			{
				Vector2 addForce = shot.transform.up * MainWeapon.LaunchForce;
				rb.AddForce(addForce, ForceMode2D.Impulse);
			}

			var aud = GetComponent<AudioSource>();
			if (aud)
				aud.Play();
		}
	}

}
