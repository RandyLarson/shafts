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

    private GameObjectCollection<GameObject> Targets = new GameObjectCollection<GameObject>();


    private void Start()
    {
        LastInflictedDamageAt = 0;
    }

    private void Update()
    {
        foreach (var t in Targets.Members)
        {
            InfictDamage(t, t.transform.position);
        }
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

        Targets.RememberObject(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!TestGameObjectForApplicability(collision.gameObject))
            return;

        Targets.ForgetObject(collision.gameObject);
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

    private void InfictDamage(GameObject other, Vector2 contactPt)
    {
        if (Time.time - LastInflictedDamageAt > DamageInterval)
        {
            bool appliedDamageOther = ApplyDamageToOther && ApplyDamage(other);
            bool appliedDamageSelf = ApplyDamageToSelf && ApplyDamage(gameObject);

            if (appliedDamageOther || appliedDamageSelf)
            {
                if (DamageIndicator != null)
                {
                    var visual = Instantiate(DamageIndicator, contactPt, other.transform.rotation);
                    Destroy(visual, 2);
                }
            }
        }
    }
}