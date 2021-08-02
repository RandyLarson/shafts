using Milkman;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    female,
    male
}

public class Mayfly : MonoBehaviour
{
    private static int id = 0;

    public Gender Gender;
    public int CouplingCount = 0;
    public int MaxCouplingCount = 5;
    public GameObject Egg;
    public GameObject CouplingVisual;
    public GameObject CantCoupleVisual;
    public bool DieOfOldAge = true;
    private bool IsDone = false;
    public float EggSpawnForce = 1f;

    public int Id;
    private List<int> Partners;
    private HealthPoints HealthPoints;
    public Animation Animation;

    public bool CouplingAchieved { get => CouplingCount >= MaxCouplingCount;  }
    public Age Age { get; set; }
    public PlayerShip Player { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Id = ++id;
        Partners = new List<int>();
        HealthPoints = GetComponent<HealthPoints>();
        Age = GetComponent<Age>();
        Player = GetComponent<PlayerShip>();

        MaxCouplingCount = (GameController.TheController.CurrentLevel * GameController.TheController.CouplingDeltaPerLevel) + GameController.TheController.CouplingBase;

        //if (Player != null)
        //    Player.TimeToLive -= 2 * GameController.TheController.CurrentLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDone && CouplingCount >= MaxCouplingCount)
        {
            IsDone = true;
            // Spawn eggs
            //for (int i = 0; i < CouplingCount; i++)
            {
                if (Egg != null)
                {
                    var spawnedEgg = InstantiateFor(Egg, 40);
                    var erb = spawnedEgg.GetComponent<Rigidbody2D>();
                    if (erb)
                    {
                        Vector2 addForce = spawnedEgg.transform.up * EggSpawnForce;
                        erb.AddForce(addForce, ForceMode2D.Impulse);
                    }
                }
            }

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.angularDrag = .1f;
                rb.drag = .5f;
                rb.gravityScale = 1.2f;
            }

            var an = GetComponentInChildren<Animator>();
            if (an != null)
                an.enabled = false;

            var pf = GetComponent<PathFollower>();
            if (pf != null)
                pf.ClearWaypoints();

            Age.BornTime = Time.time - 6;
        }
        else if (DieOfOldAge && Age.CurrentAge > Age.LifeTime)
        {
            if (HealthPoints != null)
                HealthPoints.AdjustHealthBy(-HealthPoints.HP * 2);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.Mayfly))
        {
            var otherMayfly = collision.gameObject.GetComponent<Mayfly>();
            if (otherMayfly != null)
            {
                bool didCouple = false;
                if (otherMayfly.CouplingCount < otherMayfly.MaxCouplingCount)
                {
                    if (otherMayfly.Gender != Gender) // && otherMayfly.CouplingCollider == collision.collider)
                    {
                        if (!Partners.Contains(otherMayfly.Id))
                        {
                            CouplingCount++;
                            Partners.Add(otherMayfly.Id);
                            didCouple = true;
                        }
                    }
                }

                if (didCouple)
                    InstantiateFor(CouplingVisual);
                else
                    InstantiateFor(CantCoupleVisual);
            }
        }
    }

    private GameObject InstantiateFor(GameObject toCreate, float time = 2)
    {
        if (toCreate != null)
        {
            var visual = Instantiate(toCreate, gameObject.transform.position, gameObject.transform.rotation);
            if (time > 0)
                Destroy(visual, time);
            return visual;
        }
        return null;
    }

}
