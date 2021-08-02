using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedSpawn : MonoBehaviour
{
	/// <summary>
	/// Seconds after creation to perform spawn.
	/// </summary>
	public float InitialSpawnTimeOffset = 10f;
	public GameObject WhatToSpawn = null;
	public GameObject ParentToAssign = null;

	public float SpawnInterval = 5f;
	public int TimesToSpawn = 1;

	private float SpawnTime;

    void Start()
    {
		if ( WhatToSpawn == null )
		{
			Debug.LogError("Delay spawn does not contain item to spawn.");
			Destroy(gameObject);
		}

		SpawnTime = Time.time + InitialSpawnTimeOffset;    
    }

    void Update()
    {
		if (TimesToSpawn == 0)
			return;

        if ( Time.time >= InitialSpawnTimeOffset )
		{
			Transform parent = ParentToAssign == null ? null : ParentToAssign.transform;
			var spawnedItem = Instantiate(WhatToSpawn, transform.position, transform.rotation, parent);
			spawnedItem.SetActive(true);
			TimesToSpawn--;
			SpawnTime = Time.time + InitialSpawnTimeOffset;
		}
    }
}
