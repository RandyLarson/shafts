using UnityEngine;

public class DestroyByTime : MonoBehaviour {

	// Use this for initialization
	public Range lifetimeRange;
	public bool randomBeforeLifetime = false;
	private float bornAt = 0f;

	private void Start()
	{
		Destroy(gameObject, randomBeforeLifetime ? Random.Range(lifetimeRange.min, lifetimeRange.max) : lifetimeRange.min);
		bornAt = Time.time;	
	}
}
