using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
	public GameObject[] ThingsToLaunch;
	public float Acceleration = 100f;
	public float LaunchingBeginsAt = 0f;
	public float LaunchingEndsAt = 0f;
	public Range LaunchInterval;
	public Range XRange = new Range(10, 170);
	public Range YRange = new Range(-1, 1);
	public bool impulse = false;

	private float NextLaunchTime = 0f;

	// Use this for initialization
	void Start()
	{
		NextLaunchTime = LaunchingBeginsAt;
	}

	void Update()
	{
		if  (LaunchingEndsAt > 0 && Time.time > LaunchingEndsAt)
		{
			GameObject.Destroy(gameObject);
			return;
		}

		if (Time.time > LaunchingBeginsAt && Time.time > NextLaunchTime)
		{
			NextLaunchTime = Time.time + Random.Range(LaunchInterval.min, LaunchInterval.max);

			var spawnObject = Instantiate(ThingsToLaunch[Random.Range(0, ThingsToLaunch.Length)]);
			spawnObject.transform.position = transform.position;

			var spawnedBody = spawnObject.GetComponent<Rigidbody2D>();

			float lx = Random.Range(XRange.min, XRange.max);
			float ly = Random.Range(YRange.min, YRange.max);

			Vector3 f = new Vector3(spawnedBody.mass * Acceleration * lx, spawnedBody.mass * Acceleration * ly, 0);
			spawnedBody.AddForce(f, impulse ? ForceMode2D.Impulse : ForceMode2D.Force);
		}
	}
}
