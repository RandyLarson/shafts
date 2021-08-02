using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class ShaftPlayerController : MonoBehaviour
    {
        //public PlayerStats PlayerStats;

        //public MapController MapController;
        //public SceneObjects SceneObjects;

        private Rigidbody2D Rigidbody2D { get; set; }

        private Vector2 Movement { get; set; }
        public float Speed = .5f;
        private float Impedence = 1f;
        public int AmmoCount = 20;

        private bool IsFiring { get; set; }

        private void Awake()
        {
        }

        void Start()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            //SceneObjects.TheGameController.WireUpPlayer(gameObject);
        }

        void Update()
        {
        }

        private void FixedUpdate()
        {
            if (Movement != Vector2.zero)
            {
                Vector2 playerWorldPosition = (Vector2)transform.position + Movement;
                //TerrainType terrainType = MapController.GetTerrainFromPosition(playerWorldPosition);

                //Impedence = terrainType != null ? terrainType.MovementFactor : 0;
                //Rigidbody2D.position += Movement * Speed * Impedence;
                Rigidbody2D.position += Movement * Speed;
            }

            if (IsFiring)
            {
                IsFiring = false;
            }
        }

        public void OnMove(InputValue value)
        {
            Movement = value.Get<Vector2>();
        }

        public void OnFire()
        {
            if (AmmoCount > 0)
            {
                Debug.Log($"Firing. Ammo remaining: {AmmoCount}");
                SendFire();
                IsFiring = true;
            }
            else
            {
                Debug.Log("I'm outta ammo");
            }
        }

        private void SomebodyIsOutOfAmmo()
        {
            Debug.Log($"The server reports that somebody is out of ammo.");

        }

        private void OutOfAmmo()
        {
            Debug.Log($"The server tells me that I'm outta ammo. I think I have {AmmoCount}");
        }

        private void SendFire()
        {
            AmmoCount--;
            if (AmmoCount == 0)
            {
                OutOfAmmo();
                SomebodyIsOutOfAmmo();
            }
        }
    }

}