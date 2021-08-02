using Assets.Scripts.InvasivePlants;
using UnityEngine;

public class Spawner : MonoBehaviour, IInvasivePlant
{

	/// <summary>
	/// Time between cloning of the seed.
	/// </summary>
	public float MultiplyRate = 5f;
	public float NumCloneTimes = 1;
	public float SpreadAcceleration = 3f;
	public GameObject WhatToClone = null;

	private float? NextMutiplyTime = 0;

	void Start()
	{
		NextMutiplyTime = Time.time + MultiplyRate;
		PlantManager.AddPlant(this);
	}

	private void OnDestroy()
	{
		PlantManager.RemovePlant(this);
	}

	private GameObject Create(GameObject whatToCreate)
	{
		// Clone it and give it a little pop so it spreads
		var createdItem = Instantiate<GameObject>(whatToCreate, gameObject.transform.position, gameObject.transform.rotation);
		createdItem.transform.position = transform.position;
		return createdItem;
	}

	private void DoClone()
	{
		if (null == WhatToClone)
			return;

		// Clone it and give it a little pop so it spreads
		var cloned = Create(WhatToClone);
		var itsGrowth = cloned.GetComponent<Spawner>();
		if (itsGrowth)
			itsGrowth.NumCloneTimes--;

		var spawnedBody = cloned.GetComponent<Rigidbody2D>();

		float lx = Random.Range(-10, 10);
		float ly = Random.Range(-10, 10);
		cloned.transform.position = new Vector3(transform.position.x, transform.position.y);

		Vector3 f = new Vector3(spawnedBody.mass * SpreadAcceleration * lx, spawnedBody.mass * SpreadAcceleration * ly, 0);
		spawnedBody.AddForce(f, ForceMode2D.Impulse);

	}

	public void DoLiving()
	{
		if (NumCloneTimes > 0 && null != NextMutiplyTime && Time.time >= NextMutiplyTime)
		{
			NumCloneTimes--;
			DoClone();
			NextMutiplyTime = Time.time + MultiplyRate;
		}
	}
}
