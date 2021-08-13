using UnityEngine;

[RequireComponent(typeof(Munition))]
public class FireControl : MonoBehaviour
{
    public GameObject MunitionToInstantiate;

    private float LastFireTime = 0;
    private Munition Munition;
    private GameObject ActiveMunition;

    public void Start()
    {
        Munition = GetComponent<Munition>();
    }

    public void Fire()
    {
        if (CanFire())
        {
            if (MunitionToInstantiate != null)
            {
                ActiveMunition = GameObject.Instantiate(MunitionToInstantiate, gameObject.transform);
                Munition.Capacity--;
                LastFireTime = Time.time;
            }
        }
    }

    public bool CanFire()
    {
        if (ActiveMunition != null)
            return false;

        return Munition.Capacity != 0 && Time.time - LastFireTime > Munition.FireRate;
    }
}
