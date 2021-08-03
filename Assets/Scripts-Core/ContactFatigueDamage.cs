using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class ContactFatigueDamage : MonoBehaviour
{
    [Tooltip("The amount of damage inflicted.")]
    public float DamageAmt = 10;
    [Tooltip("The interval between damage being inflicted.")]
    public float DamageInterval = 1;

    [Tooltip("Spawned when damage inflicted")]
    public GameObject DamageIndicator;

    [Tooltip("If non-empty, only fatigue when these tags contact.")]
    public string[] TagDomain;
    public bool ApplyDamageToSelf = true;
    public bool ApplyDamageToOther = true;

    private float LastInflictedDamageAt = 0;

    private void Start()
    {
        LastInflictedDamageAt = 0;
    }

    bool TestGameObjectForApplicability(GameObject gameObject)
    {
        if (gameObject != null)
        {
            if (TagDomain != null && TagDomain.Length > 0)
            {
                return TagDomain.Any(td => gameObject.CompareTag(td));
            }

            return true;
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!TestGameObjectForApplicability(collision.gameObject))
            return;

        InfictDamage(collision, collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!TestGameObjectForApplicability(collision.gameObject))
            return;

        InfictDamage(collision, collision.gameObject);
    }

    private bool ApplyDamage(GameObject go)
    {
        var hpOther = go.GetComponent<HealthPoints>();
        if (hpOther)
        {
            LastInflictedDamageAt = Time.time;
            hpOther.AdjustHealthBy(-DamageAmt);
        }
        return hpOther != null;
    }

    private void InfictDamage(Collision2D collision, GameObject other)
    {
        if (Time.time - LastInflictedDamageAt > DamageInterval)
        {
            bool appliedDamageOther = ApplyDamageToOther && ApplyDamage(other);
            bool appliedDamageSelf = ApplyDamageToSelf && ApplyDamage(gameObject);

            if (appliedDamageOther || appliedDamageSelf)
            {
                if (DamageIndicator != null)
                {
                    var visual = Instantiate(DamageIndicator, collision.contacts[0].point, other.transform.rotation);
                    Destroy(visual, 2);
                }
            }
        }
    }
}