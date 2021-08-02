using UnityEngine;

internal interface IItemProducer
{
	/// <summary>
	/// Begin the process of production. The timeToProduce is the time to materialize.
	/// </summary>
	/// <param name="whatToProduce">The object to make available.</param>
	/// <param name="timeToProduce">The time for it to materialize</param>
	/// <param name="lifeTime">How long it lasts, 0 for infinite</param>
	void ProduceItem(GameObject whatToProduce, float timeToProduce, float lifeTime = 0);
	/// <summary>
	/// Does the producer currently have an item ready for pickup?
	/// </summary>
	/// <returns></returns>
	bool HasUnclaimedItem();
	/// <summary>
	/// Remove any item present.
	/// </summary>
	void ClearItem();
}