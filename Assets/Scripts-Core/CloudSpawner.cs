using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Extensions;
using Milkman;
using UnityEngine;


[System.Serializable]
public class Clouds
{
	public GameObject[] members;
	public GameObject RandomObject
	{
		get
		{
			return members[Random.Range(0, members.Length)];
		}
	}
}

//public static class CloudManager
//{
//	public static ItemManager<IUpdatable> Clouds;
//}

public class CloudSpawner : MonoBehaviour
{
	public float direction = 1f;
	public float verticalSpawnRange = 100f;
	public float horizontalSpawnRange = 1000f;
	public Clouds spawnable;
	public bool AutoSpawn = true;
	public bool BiDirectionalSpawning = false;
	public float interval = 9f;
	private float lastSpawnTime = 0f;
	public float InitialDensity = 20;

	private void Start()
	{
		StartCoroutine(SpawnCoRoutine());
	}

	private IEnumerator SpawnCoRoutine()
	{
		for (int i=0; i<InitialDensity; i++)
		{
			SpawnItem(gameObject.transform, gameObject.transform.rotation);
			yield return null;
		}

		if (!AutoSpawn)
			yield break;

		while (true)
		{
			if ((Time.time - lastSpawnTime) >= interval)
			{
				lastSpawnTime = Time.time;
				SpawnItem(gameObject.transform, gameObject.transform.rotation);
			}
			yield return null;
		}
	}

	//public void SpawnItem(Transform parent, Vector3 itemFacing)
	//{
	//	SpawnItem(parent, Quaternion.Euler(0, (itemFacing == Vector3.left ? 180 : 0), 0));
	//}

	public void SpawnItem(Transform parent, Quaternion itemRotation)
	{
		float vertOffset = Random.Range(-verticalSpawnRange, verticalSpawnRange);
		float horzOffset = Random.Range(BiDirectionalSpawning ? -horizontalSpawnRange:0, horizontalSpawnRange);

		Vector3 spawnPos = transform.position + new Vector3(transform.right.x * horzOffset, vertOffset, 0);

		var spawned = Instantiate(spawnable.RandomObject, spawnPos, itemRotation, parent.transform);

		if (spawned.GetComponent<DestroyByTime>(out var destroyByTime))
		{
			destroyByTime.randomBeforeLifetime = true;
			destroyByTime.lifetimeRange = new Range(200, 1000);
		}

		// Do we want to manage all the clouds from a single update routine? Possibly:
		//CloudManager.Clouds.AddItem(spawned);
	}
}
