using Assets.Scripts.Extensions;
using UnityEngine;


[RequireComponent(typeof(TagDomain))]
public class ActivationTrigger : MonoBehaviour
{
    [Tooltip("What to spawn on triggering; may be empty if we are not spawning and item.")]
    public GameObject SpawnOnActivation;
    [Tooltip("Where to spawn items; may be empty if we are not spawning and item.")]
    public Transform SpawnLocation;
    [Tooltip("Destroy spawned items in this time; 0 for never.")]
    public float AutoDestroySpawnIn = 2;

    public GameObject[] EnableOnActivation;
    public GameObject[] DisableOnActivation;

    public float DelayFromTriggerToActivation = 5f;

    [Tooltip("Something to spawn when the timer starts, (e.g. sound-effect)")]
    public GameObject SpawnOnInitialTrigger;
    [Tooltip("When to auto-destroy that the initial trigger item; 0 for never.")]
    public float AutoDestroyInitialSpawnIn = 2;

    public int MaxNumberOfActiveSpawnedItems = 1;
    public float ResetTriggerIn = 0;
    public int MaxTotalSpawns = 0;

    public GameObject SpawnOnTriggerReset;
    public float AutoDestroyTriggerResetSpawnIn = 2;

    private float? WhenActivatedLast = null;
    private bool ProcessedTimesUp = false;
    private GameObjectCollection SpawnedItems = new GameObjectCollection();
    private int NumTotalSpawns = 0;


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
            DisableOnActivation.SafeSetActive(false);

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
