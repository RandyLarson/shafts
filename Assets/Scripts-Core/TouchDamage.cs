using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [Tooltip("The amount of damage inflicted.")]
    public float DamageAmt = 10;
    [Tooltip("The interval between damage being inflicted.")]
    public float DamageInterval = 1;

    [Tooltip("Spawned when damage inflicted")]
    public GameObject DamageIndicator;

    private GameObjectCollection<GameObject> Targets = new GameObjectCollection<GameObject>();

    private float LastInflictedDamageAt = 0;

    private void Start()
    {
        LastInflictedDamageAt = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //var player = collision.gameObject.GetComponent<PlayerShip>();
        //if ( player != null )

        //if (collision.gameObject.CompareTag(GameConstants.Player))
        //    InflictDamageUpon(collision.gameObject, collision.contacts[0].point);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.Player))
            InflictDamageUpon(collision.gameObject, collision.contacts[0].point);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.Player))
            Targets.RememberObject(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.Player))
            Targets.ForgetObject(collision.gameObject);
    }

    void Update()
    {
        foreach (GameObject go in Targets.Members)
        {
            InflictDamageUpon(go, go.transform.position);
        }
    }

    private void InflictDamageUpon(GameObject target, Vector2 contactPt)
    {
        if (Time.time - LastInflictedDamageAt > DamageInterval)
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
}


