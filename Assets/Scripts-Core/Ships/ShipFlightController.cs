using Assets.Scripts.Helpers;
using System.Linq;
using UnityEngine;

public class ShipFlightController : MonoBehaviour
{

	public GameObject TargetObject;
	public float StandOffDistance = 10;
	public float RadarDistance = 100f;
	public bool SwitchToClosestTarget = false;
	public float Force = 100;
	public string[] TargetTags;

	private float OurMass;
	private float OurMaxSpeedSquared;
	Rigidbody2D OurRB;

	void Start()
	{
		OurRB = GetComponent<Rigidbody2D>();
		OurMass = OurRB.mass;

		ShipCharacteristics characteristics = GetComponent<ShipCharacteristics>();
		if (characteristics != null)
		{
			OurMaxSpeedSquared = characteristics.MaxSpeed * characteristics.MaxSpeed;
		}
	}

	void Update()
	{
		if (TargetObject == null || SwitchToClosestTarget)
		{
			var nextTarget = ScanningHelpers.ScanForTargets(gameObject, RadarDistance, TargetTags).FirstOrDefault();

			if (nextTarget != null && TargetObject != null && nextTarget != TargetObject)
			{
				var dxNew = Mathf.Abs(Vector2.Distance(nextTarget.transform.position, transform.position));
				var dxCur = Mathf.Abs(Vector2.Distance(TargetObject.transform.position, transform.position));

				if (dxNew < dxCur)
					TargetObject = nextTarget;
			}
		}

		if (null == TargetObject)
			return;

		MoveToward.MoveToIntercept(transform, OurRB, Force, OurMaxSpeedSquared, StandOffDistance, -1f, 0f, TargetObject);
		AutoStabalizer.StabalizeRotation(transform);
	}
}
