using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Range
{
	public float min = .1f;
	public float max = 3f;
	public Range(float iMin, float iMax)
	{
		min = iMin;
		max = iMax;
	}
}

public class Launcher : MonoBehaviour
{
	[Tooltip("An item will be chosen from the bag to launch each time.")]
	public GameObject[] LaunchBag;

	public int MaxNumToLaunch = 10;

	public float LaunchingBeginsAt = 0f;
	public float LaunchingEndsAt = 0f;
	public Range LaunchInterval;

	[Tooltip("The vector to launch toward. The magnitude will be used to calculate force if LaunchForce is 0.")]
	public GameObject LaunchVector;

	[Tooltip("Apply force as an impulse. There isn't much need to not at this point.")]
	public bool impulse = true;

	[Tooltip("Non-zero - will apply this force instead of the vector formed with the launch vector.")]
	public float LaunchForce = 0;

	private float NextLaunchTime = 0f;
	private int NumLaunched = 0;

	// Use this for initialization
	void Start()
	{
		NextLaunchTime = LaunchingBeginsAt;
	}

	void Update()
	{
		if  ( (MaxNumToLaunch > 0 && NumLaunched >= MaxNumToLaunch) || (LaunchingEndsAt > 0 && Time.time > LaunchingEndsAt))
		{
			GameObject.Destroy(gameObject);
			return;
		}

		if (Time.time > LaunchingBeginsAt && Time.time > NextLaunchTime)
		{
			NumLaunched++;
			NextLaunchTime = Time.time + Random.Range(LaunchInterval.min, LaunchInterval.max);

			var spawnObject = Instantiate(LaunchBag[Random.Range(0, LaunchBag.Length)]);
			spawnObject.transform.position = transform.position;

			var spawnedBody = spawnObject.GetComponent<Rigidbody2D>();

			Vector3 lVector = (LaunchVector != null ? LaunchVector.transform.position - transform.position : transform.up);

			if (LaunchForce != 0 )
            {
				lVector = lVector.normalized * LaunchForce;
            }
			
			spawnedBody.AddForce(lVector, impulse ? ForceMode2D.Impulse : ForceMode2D.Force);
		}
	}
}
