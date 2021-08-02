using UnityEngine;

public class SpawnFreight : MonoBehaviour
{

	public bool Active = false;
	public GameObject NextToSpawn;
	public bool OKToSpawn = true;
	public Range SpawnIntervalRange;
	private float NextSpawnTime = 0;
	private float LastSpawnTime = 0f;
	public int InventoryCount = 5;

	// Use this for initialization
	void Start()
	{
		NextSpawnTime = Random.Range(SpawnIntervalRange.min, SpawnIntervalRange.max);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Freight")
		{
			OKToSpawn = false;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Freight")
		{
			OKToSpawn = true;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.time - LastSpawnTime >= NextSpawnTime)
		{
			LastSpawnTime = Time.time;
			if (InventoryCount > 0 && OKToSpawn && NextToSpawn != null)
			{
				NextSpawnTime = Random.Range(SpawnIntervalRange.min, SpawnIntervalRange.max);
				InventoryCount--;
				// Kick off animation for spawn
				// Visualize what spawned
				var sapwned = Instantiate(NextToSpawn, transform.position, transform.rotation);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 2);
	}
}
