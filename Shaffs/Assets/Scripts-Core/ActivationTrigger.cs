using Assets.Scripts.Extensions;
using UnityEngine;


[RequireComponent(typeof(TagDomain))]
public class ActivationTrigger : MonoBehaviour
{
    public GameObject SpawnOnActivation;
    public GameObject EnableOnActivation;

    public float AutoDestroySpawnIn = 2;
    public Transform SpawnLocation;

    public GameObject SpawnOnInitialTrigger;
    public float AutoDestroyInitialSpawnIn = 2;

    public GameObject SpawnOnTriggerReset;
    public float AutoDestroyTriggerResetSpawnIn = 2;
    public float ResetTriggerIn = 0;
    public int MaxNumberOfActiveSpawnedItems = 1;
    public int MaxTotalSpawns = 0;
    private int NumTotalSpawns = 0;

    public float DelayFromTriggerToActivation = 5f;
    public float? WhenActivatedLast = null;
    public bool ProcessedTimesUp = false;

    private GameObjectCollection SpawnedItems = new GameObjectCollection();


    private ITagDomain TagDomain { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        TagDomain = GetComponent<ITagDomain>();
    }

    void Update()
    {
        CheckForTriggerEffect();
    }

    private void CheckForTriggerEffect()
    {
        if (WhenActivatedLast == null)
            return;

        if (ProcessedTimesUp)
        {
            SpawnedItems.PruneNullTargets();

            if (ResetTriggerIn > 0 &&
                NumTotalSpawns < MaxTotalSpawns &&
                Time.time > (WhenActivatedLast.Value + ResetTriggerIn) &&
                SpawnedItems.Members.Count < MaxNumberOfActiveSpawnedItems)
            {
                ResetTrigger();
            }
            return;
        }

        if (Time.time - WhenActivatedLast >= DelayFromTriggerToActivation)
        {
            ProcessedTimesUp = true;
            EnableOnActivation.SafeSetActive(true);

            if (SpawnOnActivation.SafeInstantiate<Transform>(SpawnLocation != null ? SpawnLocation.position : transform.position, out Transform created, AutoDestroySpawnIn))
            {
                SpawnedItems.RememberObject(created.gameObject);
                NumTotalSpawns++;
            }
        }
    }

    private void OnTriggerTripped()
    {
        WhenActivatedLast = Time.time;
        SpawnOnInitialTrigger.SafeInstantiate(transform.position, out GameObject created, AutoDestroyInitialSpawnIn);
    }

    private void ResetTrigger()
    {
        WhenActivatedLast = null;
        ProcessedTimesUp = false;
        SpawnOnTriggerReset.SafeInstantiate(transform.position, out GameObject created, AutoDestroyTriggerResetSpawnIn);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (WhenActivatedLast != null || ProcessedTimesUp)
            return;

        bool doActivation = true;
        if (TagDomain != null && TagDomain.TheDomain.Length > 0 && !TagDomain.IsInDomain(collision.gameObject))
            doActivation = false;

        if (doActivation)
        {
                OnTriggerTripped();
        }
    }
}
