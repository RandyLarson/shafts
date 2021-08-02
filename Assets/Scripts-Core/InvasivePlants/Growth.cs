using Assets.Scripts.InvasivePlants;
using UnityEngine;

public class Growth : MonoBehaviour, IInvasivePlant
{
	public GameObject GrowsInto = null;
	public float TimeUntilNextGrowthPhase = 3f;
	private float GrowsAtTime = 0f;

	void Start ()
	{
		PlantManager.AddPlant(this);
		GrowsAtTime = Time.time + TimeUntilNextGrowthPhase;
	}

	private void OnDestroy()
	{
		PlantManager.RemovePlant(this);
	}

	private GameObject CreateNextGrowthPhase(GameObject whatToCreate)
	{
		// Clone it and give it a little pop so it spreads
		var createdItem = Instantiate<GameObject>(whatToCreate, gameObject.transform.position, gameObject.transform.rotation);
		createdItem.transform.position = transform.position;
		return createdItem;
	}

	private void AdvanceToNextGrowthPhase()
	{
		if (GrowsInto == null)
			return;

		// Next entity
		GameObject nextPhase = CreateNextGrowthPhase(GrowsInto);

		// Remove self
		Destroy(gameObject);
	}

	public void DoLiving()
	{
		if (GrowsInto != null && Time.time >= GrowsAtTime)
		{
			AdvanceToNextGrowthPhase();
		}
	}
}
