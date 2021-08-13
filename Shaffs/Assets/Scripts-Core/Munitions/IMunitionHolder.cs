using UnityEngine;

public interface IMunitionHolder
{
	void Add(Munition toAdd);
    void Add(GameObject payload);
}
