using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TagDomain))]
public class TouchDamage : MonoBehaviour
{
    [Tooltip("The amount of damage inflicted.")]
    public float DamageAmt = 10;
    [Tooltip("The interval between damage being inflicted.")]
    public float DamageInterval = 1;

    [Tooltip("Spawned when damage inflicted")]
    public GameObject DamageIndicator;

    private GameObjectCollection Targets = new GameObjectCollection();

    private float LastInflictedDamageAt = 0;

    private TagDomain TagDomain;

    private void Start()
    {
        LastInflictedDamageAt = 0;
        TagDomain = GetComponent<TagDomain>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (TagDomain.IsInDomain(other))
            InflictDamageUpon(other, other.transform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)

    {
        if (TagDomain.IsInDomain(collision.gameObject))
            Targets.RememberObject(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (TagDomain.IsInDomain(collision.gameObject))
            InflictDamageUpon(collision.gameObject, collision.contacts[0].point);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (TagDomain.IsInDomain(collision.gameObject))
            Targets.ForgetObject(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TagDomain.IsInDomain(collision.gameObject))
            Targets.RememberObject(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (TagDomain.IsInDomain(collision.gameObject))
            Targets.ForgetObject(collision.gameObject);
    }

    void Update()
    {
        if (Time.time - LastInflictedDamageAt > DamageInterval)
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


