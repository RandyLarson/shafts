using UnityEngine;

public class CloudDrift : MonoBehaviour
{
	private float Speed = 1;

	private void Awake()
	{
		Speed = Random.Range(.1f, 1);
	}

	void FixedUpdate()
	{
		var delta = transform.right * Speed * Time.fixedDeltaTime;
		gameObject.transform.position = new Vector3(gameObject.transform.position.x + delta.x, gameObject.transform.position.y);
	}
}
