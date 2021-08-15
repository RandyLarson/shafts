using Assets.Scripts.Extensions;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    public GameObject ToSpawnUponDestruction;
    public GameObject[] SpawnCollection;
    public bool DoSpawnItem = true;

    [Tooltip("Time after spawning to self-destruct. Set to 0 for no destruction.")]
    public float LifetimeOfSpawned = 1;

    private void OnDestroy()
    {
        if (DoSpawnItem)
        {
            ToSpawnUponDestruction.SafeInstantiate<GameObject>(transform.position, out GameObject gameObject, LifetimeOfSpawned);
            if ( SpawnCollection.Length > 0 )
            {
                for (int i=0; i< SpawnCollection.Length; i++)
                {
                    SpawnCollection[i].SafeInstantiate(transform.position, out GameObject theSpawn, LifetimeOfSpawned);
                }
            }
        }
    }
}
