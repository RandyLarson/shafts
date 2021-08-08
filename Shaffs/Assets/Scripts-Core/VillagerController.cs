using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerController : MonoBehaviour
{
	private VillageController parentVillage;
	private Animator villagerAnimator;
	private Rigidbody2D villagerRB;
	//Renderer villagerRenderer;
	//private float? TimeDown = null;
	public float WaitTime = 3f;
	public float RunSpeed = 2f;
	public int Facing = 1;
	public GameObject RunToward;
	public bool IsAJumper = true;
	public bool LooksUp = true;
	private bool isObstructed = false;
	private float? waitUntilTime = null;


	int currentSpeedID = Animator.StringToHash("CurrentSpeed");
	int lookingSkywardID = Animator.StringToHash("LookingSkyward");
	int isObstructedID = Animator.StringToHash("IsObstructed");
	int jumpID = Animator.StringToHash("Jump");

	private bool IsFacingRight {  get { return Facing > 0; } }

	// Use this for initialization
	void Start()
	{
		villagerAnimator = GetComponent<Animator>();
		villagerRB = GetComponent<Rigidbody2D>();
		//villagerRenderer = GetComponent<Renderer>();
		FindParentVillage();
	}

	private void FindParentVillage()
	{
		if (null == parentVillage)
			parentVillage = GetComponentInParent<VillageController>();
	}

	private void OnEnable()
	{
		FindParentVillage();
		// parentVillage.VillagerAlive(this);
	}

	private void OnDisable()
	{
		FindParentVillage();
		// parentVillage.VillagerDead(this);
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		Facing *= -1;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	/// <summary>
	/// Right is positive.
	/// </summary>
	/// <param name="leftOrRight"></param>
	private void Face(float leftOrRight)
	{
		// Right is positive
		if (leftOrRight >= 0 && Facing < 0)
			Flip();
		else if (leftOrRight < 0 && Facing > 0)
			Flip();
	}


	public void Starve()
	{
		Destroy(gameObject);
	}

	void Update()
	{
		if (RunToward != null && (waitUntilTime.HasValue == false || Time.time > waitUntilTime))
		{
			// Flip the villager depending upon the orientation of it to the target object
			Face(RunToward.transform.position.x - transform.position.x);

			// Max move delta, contrained by obstructions
			float maxMvDelta = MaximumAllowedMvDistance(IsFacingRight, Time.unscaledDeltaTime * RunSpeed);
			float nextSpeed = 0;
			if (maxMvDelta > 0)
			{
				isObstructed = false;
				// There something to move toward - `RunToward`
				var newPos = Vector2.MoveTowards(transform.position, RunToward.transform.position, maxMvDelta);
				var posDelta = newPos - (Vector2)transform.position;
				transform.position = new Vector2(newPos.x, transform.position.y);

				// Multiplying by 10 to make it easier writing conditions.
				// Debug.Log("Name: " + name + "PosDelta: " + posDelta.x);
				nextSpeed = 10 * RunSpeed * Mathf.Abs(posDelta.x);
			}
			else
			{
				nextSpeed = 0;
				isObstructed = true;
				waitUntilTime = Time.time + WaitTime;
				//Debug.Log(name + " Obstructed.");
			}

			//Debug.Log(transform.name + " nextspeed: " + nextSpeed);
			villagerAnimator.SetBool(isObstructedID, isObstructed);
			villagerAnimator.SetFloat(currentSpeedID, nextSpeed);
			villagerAnimator.SetBool(lookingSkywardID, LooksUp);
			if (IsAJumper)
				villagerAnimator.SetTrigger(jumpID);
		}
	}

	private float MaximumAllowedMvDistance(bool toRight, float wishedDistance)
	{
		return wishedDistance;

	//	float dxBuffer = 2*villagerRenderer.bounds.size.x;
	//	Vector2 org = new Vector2(transform.position.x, transform.position.y + 3);
	//	Vector2 org2 = new Vector2(transform.position.x, transform.position.y + 5);

	//	//Vector2 org3 = new Vector2(transform.position.x, transform.position.y);
	//	//Vector2 direction = new Vector2(5,5); // new Vector2(org3.x + (toRight ? 3 : -3), org3.y);

	//	// Collider2D villagerCollider = GetComponent<Collider2D>();
	//	//		Debug.DrawRay(org, Vector2.right * dxBuffer, Color.blue, 1f);
	//	//		Debug.DrawRay(org2, Vector2.left * dxBuffer, Color.red, 1f);

	//	//Debug.DrawRay(org, toRight ? Vector2.right : Vector2.left * dxBuffer, Color.blue, .2f);
	//	var hits =  Physics2D.RaycastAll(org, toRight ? Vector2.right : Vector2.left, villagerRenderer.bounds.size.x * 2);
	//	if (hits != null)
	//	{
	//		foreach (var hit in hits)
	//		{
	//			if (hit.collider != null)
	//			{
	//				if (hit.collider.gameObject != gameObject && (hit.collider.CompareTag("Villager") || hit.collider.CompareTag("VillagerBoundary")))
	//				{
	//					float distance = Mathf.Abs(hit.point.x - transform.position.x);
	//					float allowedDX = Mathf.Clamp(wishedDistance, 0, distance-dxBuffer);
	//					//Debug.Log(name + " found " + hit.collider.name + " lookingR: " + toRight + " at distance: " + distance + " wDX: " + wishedDistance + " aDX: " + allowedDX);
	//					return allowedDX;
	//				}
	//			}
	//		}
	//	}
	//	//Debug.Log("No obstructions found " + name + " lookingR: " + toRight + " wDX: " + wishedDistance + " aDX: " + wishedDistance);
	//	return wishedDistance;
	}
}
