using Milkman;
using UnityEngine;


public enum PowerUpKind
{
    Shield,
    Health,
    Weapon,
    Time
}

public class PowerUp : MonoBehaviour
{
    public PowerUpKind Kind;
    public float Amount;

    /// <summary>
    /// Optional weapon or other item to add to the recipient.
    /// </summary>
    public GameObject Payload;

    /// <summary>
    /// Instantiated when applied to recipient.
    /// </summary>
    public GameObject UsageEffect;
    public float RadiusFactor = 2.5f;

    private float OurRadius { get; set; }
    public float FadeOutDuration = .5f;
    private float? TimeOfUse = null;
    private Vector3 SmallestScale = new Vector3(.1f, .1f, .1f);
    public bool DestroyOnUse = true;
    public bool DisableRbOnUse = true;

    private void Start()
    {

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            OurRadius = RadiusFactor * sr.bounds.extents.x;
            //var t = sr.sprite.pixelsPerUnit;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (TimeOfUse != null)
            return;

        var thePlayer = collision.gameObject.GetComponent<PlayerShip>();
        if (thePlayer != null)
        {
            ApplyPowerup(thePlayer);
        }
        else
        {
            var consumer = collision.gameObject.GetComponent<ResourceConsumer>();
            ApplyPowerup(consumer);
        }
    }

    private void Update()
    {
        // After use the power-up portal thingy shrinks and is either destroyed or grows again and is 
        // ready for additional use.
        if (TimeOfUse != null)
        {
            var elapsed = Time.time - TimeOfUse.Value;
            Vector3 nextScale = Vector3.Lerp(Vector3.one, SmallestScale, elapsed / FadeOutDuration);

            transform.localScale = nextScale;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localScale = nextScale;
            }

            if (nextScale.x <= .15)
            {
                if (DestroyOnUse)
                {
                    Destroy(gameObject);
                }
                else
                {
                    TimeOfUse = null;
                }
            }
        }
        //else
        //{
        //	Debug.DrawLine(transform.position, transform.position+(transform.up * OurRadius), Color.blue);
        //	RaycastHit2D[] raycastHit2D = Physics2D.CircleCastAll(transform.position, OurRadius, Vector2.zero, 0f);

        //	foreach (RaycastHit2D nextHit in raycastHit2D)
        //	{
        //		if (nextHit.collider.gameObject.CompareTag("Player"))
        //		{
        //			var thePlayer = nextHit.collider.gameObject.GetComponent<PlayerShip>();
        //			ApplyPowerup(thePlayer);
        //			break;
        //		}
        //		else
        //              {
        //			var consumer = nextHit.collider.gameObject.GetComponent<ResourceConsumer>();
        //			ApplyPowerup(consumer);
        //			break;
        //		}
        //	}
        //}
    }

    private void ApplyPowerup(ResourceConsumer consumer)
    {
        if (consumer != null)
        {
            consumer.AdjustResource(Resource.Food, Amount);
            ItemWasUsed();
        }
    }

    private void ItemWasUsed()
    {

        TimeOfUse = Time.time;

        if (DisableRbOnUse)
        {
            var comp = GetComponent<Collider2D>();
            if (comp != null)
            {
                comp.isTrigger = true;
            }
        }
    }

    private void ApplyPowerup(PlayerShip toPlayer)
    {
        if (toPlayer == null)
            return;

        if (UsageEffect != null)
        {
            var effect = GameObject.Instantiate(UsageEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        switch (Kind)
        {
            case PowerUpKind.Time:
                toPlayer.UpdateTimeToLive(Amount);
                break;

            case PowerUpKind.Shield:
                toPlayer.UpdateShield(Payload, Amount);
                break;

            case PowerUpKind.Health:
                toPlayer.AdjustHealth(Amount);
                break;

            case PowerUpKind.Weapon:
                {
                    var asMunition = Payload.GetComponent<Munition>();
                    if (asMunition != null)
                        toPlayer.AddWeapon(asMunition);
                }
                break;
        }

        ItemWasUsed();
    }
}
