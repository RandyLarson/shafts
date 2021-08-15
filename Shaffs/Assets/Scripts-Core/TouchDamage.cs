using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TagDomain))]
public class TouchDamage : MonoBehaviour
{
    [Tooltip("The amount of damage inflicted.")]
    public float DamageAmt = 10;
    [Tooltip("The interval between damage being inflicted.")]
    public float DamageInterval = 1;
    public bool DamageOnlyWhenMoving = false;

    [Tooltip("Spawned when damage inflicted")]
    public GameObject DamageIndicator;

    private GameObjectCollection Targets = new GameObjectCollection();

    private float LastInflictedDamageAt = 0;

    private TagDomain TagDomain;
    private Rigidbody2D OurRb { get; set; }

    private void Start()
    {
        LastInflictedDamageAt = 0;
        TagDomain = GetComponent<TagDomain>();
        OurRb = GetComponent<Rigidbody2D>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (TagDomain.IsInDomain(other))
        {
            if (Time.time == LastInflictedDamageAt || Time.time - LastInflictedDamageAt > DamageInterval)
            {
                InflictDamageUpon(other, other.transform.position);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TestForRemembering(collision.gameObject);
    }

    private void TestForRemembering(GameObject toTest)
    {
        if (DamageOnlyWhenMoving == true && OurRb != null && OurRb.velocity == Vector2.zero)
            return;

        if (toTest != gameObject && (TagDomain == null || TagDomain.IsInDomain(toTest)))
            Targets.RememberObject(toTest);
    }

    private void TestForClearing()
    {
        if (Targets.Members.Count == 0)
            return;

        if (DamageOnlyWhenMoving == true && OurRb != null && OurRb.velocity == Vector2.zero)
        {
            Targets.Clear();
        }
    }

    private void TestForForgettion(GameObject toTest)
    {
        if (Targets.Members.Count == 0)
            return;

        TestForClearing();

        if (TagDomain != null && toTest != gameObject && TagDomain.IsInDomain(toTest))
            Targets.ForgetObject(toTest);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TestForForgettion(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TestForRemembering(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TestForForgettion(collision.gameObject);
    }

    void Update()
    {
        TestForClearing();
        if (Targets.Members.Count > 0 && Time.time - LastInflictedDamageAt > DamageInterval)
        {

            bool doPrune = false;
            for (int i = 0; i < Targets.Members.Count; i++)
            {
                GameObject go = Targets.Members[i];
                if (go != null)
                    InflictDamageUpon(go, go.transform.position);
                else
                    doPrune = true;
            }

            if (doPrune)
                Targets.PruneNullTargets();
        }
    }

    private void InflictDamageUpon(GameObject target, Vector2 contactPt)
    {
        var hpOther = target.GetComponent<HealthPoints>();
        if (hpOther)
        {
            LastInflictedDamageAt = Time.time;
            if (DamageIndicator != null)
            {
                var visual = Instantiate(DamageIndicator, contactPt, target.transform.rotation);
                Destroy(visual, 2);
            }

            hpOther.AdjustHealthBy(-DamageAmt);
        }
    }
}


