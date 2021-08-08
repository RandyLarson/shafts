using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectCollection
{
    public List<GameObject> Members { get; private set; } = new List<GameObject>();

    public void RememberObject(GameObject go)
    {
        if (!Members.Contains(go))
            Members.Add(go);
    }
    public void ForgetObject(GameObject go)
    {
        Members.Remove(go);
    }


	public void PruneNullTargets()
	{
		Members.Where(t => t.gameObject == null)
			.ToList()
			.ForEach(t =>
			{
				Members.Remove(t);
			});
	}
}

