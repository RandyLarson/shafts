using UnityEngine;
using Assets.Scripts.Extensions;

public class DrillingBeam : MonoBehaviour
{
	public GameObject TheDrill;
	public float MaxDrillRange = 50;
	public bool IsDrilling = false;
	public float DrillingDuration = 10;
	public float DrillingCoolDown = 8;

	public AudioSource DrillAudio;

	private float NextDrillingTime { get; set; } = 0;
	private float DrillingStopTime { get; set; }
	private BeamController DrillingBeamController { get; set; }

	private void Start()
	{
		DrillingBeamController = TheDrill.GetComponent<BeamController>();
		if (IsDrilling == false)
		{
			TurnOffDrill();
		}
	}

	private void SetNextDrillingTime()
	{
		NextDrillingTime = Time.time + Random.Range(DrillingCoolDown / 2, DrillingCoolDown);
	}

	// Update is called once per frame
	void Update()
	{
		if (IsDrilling && DrillingStopTime <= Time.time)
		{
			TurnOffDrill();
		}
		else if (IsDrilling == false && NextDrillingTime <= Time.time)
		{
			TurnOnDrill();
		}

		if (IsDrilling)
		{
			// Ensure that we make contact with the ground.
			RaycastHit2D terrainCast = Physics2D.Raycast(TheDrill.transform.position, Vector2.down, MaxDrillRange, GameConstants.LayerMaskTerrain);
			if (terrainCast.collider == null)
			{
				TurnOffDrill();
			}
			else
			{
				// Adjust the length of the beam.
				DrillingBeamController.AimAt(new Vector2(terrainCast.point.x, terrainCast.point.y), false);
			}
		}
	}


	private void TurnOnDrill()
	{
		IsDrilling = true;
		DrillingStopTime = Time.time + DrillingDuration;
		DrillingBeamController.TurnOn();

		DrillAudio.Play();
	}

	private void TurnOffDrill()
	{
		IsDrilling = false;
		SetNextDrillingTime();
		DrillingBeamController.TurnOff();
		StartCoroutine(DrillAudio.FadeOut(1f));
	}
}
