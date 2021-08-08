using Milkman;
using UnityEngine;


[System.Serializable]
public enum EvolveKind
{
    time,
    health,
    coupling
}

public class Evolve : MonoBehaviour
{
    public PlayerShip IntoForm;
    public GameObject VisualEffect;
    public bool GetSpawnLocationFromGameController;
    public EvolveKind EvolveBy = EvolveKind.health;
    public int HealthAmtToEvolve = 100;
    public int EvolveAfterTime = 10;
    public bool DestroyOnUse = true;
    //public bool DisableRbOnUse = false;

    private HealthPoints Health;
    private bool HasEvolved = false;
    public float FadeOutDuration = .5f;
    private float? TimeOfUse = null;
    private float? TimeOfStart = null;
    private Vector3 SmallestScale = new Vector3(.1f, .1f, .1f);

    // Start is called before the first frame update
    void Start()
    {
        Health = GetComponent<HealthPoints>();
        TimeOfStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasEvolved)
        {
            bool okayToGo = false;

            if ( EvolveBy == EvolveKind.coupling )
            {
                var mayFly = GetComponent<Mayfly>();
                if ( mayFly.CouplingAchieved )
                {
                    okayToGo = true;
                }
            }

            if ((EvolveBy == EvolveKind.time && (Time.time - TimeOfStart > EvolveAfterTime)) || (EvolveBy == EvolveKind.health && Health != null && Health.HP >= HealthAmtToEvolve))
            {
                okayToGo = true;
            }

            if ( okayToGo )
            { 
                HasEvolved = true;
                TimeOfUse = Time.time;

                Transform spawnNewFormLocation = null;
                if (GetSpawnLocationFromGameController)
                    spawnNewFormLocation = GameController.TheController.GetNextSpawnLocation();

                Vector3 spawnLocation = spawnNewFormLocation != null ? spawnNewFormLocation.position : gameObject.transform.position;
                var spawnRotation = spawnNewFormLocation != null ? spawnNewFormLocation.rotation : gameObject.transform.rotation;
                var nextForm = GameObject.Instantiate(IntoForm, spawnLocation, spawnRotation);

                if (VisualEffect != null)
                {

                    var effect = GameObject.Instantiate(VisualEffect, spawnLocation, spawnRotation);
                    GameObject.Destroy(effect, 5);
                }

                GameController.TheController.SwitchPlayer(nextForm);

            }
            else if (TimeOfUse.HasValue)
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

        }
    }
}
