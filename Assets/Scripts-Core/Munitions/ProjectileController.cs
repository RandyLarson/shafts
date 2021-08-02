using UnityEngine;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Extensions;

public class ProjectileController : MonoBehaviour
{

	public float RateOfFire = .3f;
	public bool UseMinY = true;
	public float MinY = -15;
	public Munition Payload;
	public int Inventory = 50;
	private float PayloadMass;
	public float RadarDistance = 150f;
	public Transform SpawnPoint;
	public string[] TargetTags;
	public string TargetLayerName;

	[Tooltip("The experience of the operator -- how good are they at targeting.")]
	[Range(0,1)]
	public float AimProficiency = .5f;

	// Time interval to scan for new targets.
	public float ScanInterval = .1f;
	private float NextScanAt = 0f;
	public Collider2D CurrentTarget;

	private float nextFire = 0f;
	private Collider2D[] ScannedTargets;
	private int TargetLayerMask = -1;

	public bool HasInventory => Inventory == -1 || Inventory > 0;

	public System.Func<GameObject, bool> TargetQualifier { get; set; } = null;

	void Start()
	{
		ScannedTargets = new Collider2D[100];

		if (Payload != null)
		{
			var payloadRB = Payload.GetComponent<Rigidbody2D>();
			PayloadMass = payloadRB.mass;
		}

		if (TargetLayerName?.Length > 0)
			TargetLayerMask = 1 << LayerMask.NameToLayer(TargetLayerName);

		NextScanAt = Time.time + ScanInterval;
	}

	void Update()
	{
		if (Payload != null && HasInventory)
		{
			if (Time.time >= NextScanAt)
			{
				CurrentTarget = ScanForTarget();
				NextScanAt = Time.time + ScanInterval;
			}

			if (CurrentTarget != null)
				FireWeapon();
		}
	}

	private Collider2D ScanForTarget()
	{
		int hits = 0;
		if (TargetLayerMask >= 0)
		{
			hits = Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, RadarDistance, ScannedTargets, TargetLayerMask);
		}
		else
		{
			hits = Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, RadarDistance, ScannedTargets);
		}

		var byKindAndDistance = ScannedTargets
			.Where((other, idx) => idx < hits && TargetTags.Contains(other.gameObject.tag))
			.Where(other => VetTarget(other))
			.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position));

		foreach (var potential in byKindAndDistance)
		{
			if (Aim(potential))
				return potential;
		}

		return null;

		//var targets = Physics2D.OverlapCircleAll(gameObject.transform.position, RadarDistance)
		//	.Where(other => TargetTags.Contains(other.gameObject.tag))
		//	.OrderBy(other => Vector2.Distance(gameObject.transform.position, other.transform.position))
		//	.ToArray();

		//return targets;

		//return targets.FirstOrDefault()?.gameObject;
	}

	public virtual bool VetTarget(Collider2D other)
	{
		return TargetQualifier != null ? TargetQualifier(other.gameObject) : true;
	}

	private bool Aim(Collider2D target)
	{
		bool okToFire = false;

		if (null != target)
		{

			okToFire = true;
			Vector3 aimAt = target.transform.position;

			// Point directly at the target, then determine the intercept offset and re-aim.
			gameObject.transform.up = aimAt - SpawnPoint.transform.position;

			// The proficiency of the operator affects how well they measure the target's velocity.
			Vector2 targetVelocity = target.attachedRigidbody != null ? target.attachedRigidbody.velocity : Vector2.zero;
			float potentialError = (1f-AimProficiency)/2f;
			Vector2 velocityError = targetVelocity * (1 + Random.Range(-potentialError, potentialError));

			float potentialPosError = (1f - AimProficiency) / 15f;
			Vector2 positionWithError = aimAt * (1 + Random.Range(-potentialPosError, potentialPosError));


			aimAt = AimingHelpers.FirstOrderIntercept(SpawnPoint.transform.position, Vector3.zero, Payload.LaunchForce / PayloadMass, positionWithError, velocityError);
			Vector3 spawnAim = aimAt - SpawnPoint.transform.position;
			spawnAim.z = 0;

			if (UseMinY)
			{
				if (MinY < 0)
					spawnAim.y = Mathf.Min(spawnAim.y, MinY);
				else
					spawnAim.y = Mathf.Max(spawnAim.y, MinY);
			}

			gameObject.transform.up = spawnAim;
			//Debug.DrawRay(SpawnPoint.position, gameObject.transform.up * 50 , Color.magenta);

			if (UseMinY && MinY == spawnAim.y)
			{
				okToFire = false;
			}
			else
			{
				// Check for obstructions (self or like-kind items)
				float rayRange = 40f;
				RaycastHit2D hit;
				Debug.DrawRay(SpawnPoint.position, gameObject.transform.up * rayRange, Color.green);
				//hit = Physics2D.Raycast(SpawnPoint.position, gameObject.transform.up, rayRange);

				// | | The radius of the circle should be the radius of the munition's collider so we can
				// properly detect self hits and occluded hits.
				hit = Physics2D.CircleCast(SpawnPoint.position, .4f, gameObject.transform.up, rayRange);
				if (hit.rigidbody != null)
				{

					// Self or same-team tag or terrain
					// Or maybe not in the target tag list if the list is non-empty?
					if (hit.rigidbody.gameObject == gameObject || hit.rigidbody.gameObject == transform.parent.gameObject)
					{
						okToFire = false;
						Debug.DrawLine(SpawnPoint.position, hit.point, Color.red);
					}
					else if (hit.rigidbody.CompareTag(GameConstants.Terrain) ||
						(gameObject.tag.Length > 0 && hit.rigidbody.gameObject.tag == gameObject.tag))
					{
						okToFire = false;
						Debug.DrawLine(SpawnPoint.position, hit.point, Color.red);
					}
				}
			}


			return okToFire;
		}

		return false;
	}


	public void FireWeapon()
	{
		if (HasInventory && Payload != null && Time.time >= nextFire)
		{
			if (Inventory > 0 )
				Inventory--;

			nextFire = Time.time + RateOfFire;

			var shot = Instantiate(Payload, SpawnPoint.position, SpawnPoint.rotation);
			var rb = shot.GetComponent<Rigidbody2D>();
			if (rb)
			{
				Vector2 addForce = SpawnPoint.transform.up * Payload.LaunchForce;
				rb.AddForce(addForce, ForceMode2D.Impulse);
			}

			if (gameObject.GetComponent<AudioSource>(out var aud) && aud != null)
				aud.Play();
		}
	}

}

