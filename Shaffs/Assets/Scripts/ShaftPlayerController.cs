using Assets.Scripts.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class ShaftPlayerController : MonoBehaviour, IMunitionHolder
    {
        //public MapController MapController;
        //public SceneObjects SceneObjects;

        [Tooltip("A gameobject that will be enabled when contact damage is available.")]
        public GameObject ContactDamageArea;

        public GameObject WeaponMountPoint;
        
        private AudioSource AudioSource { get; set; }
        private Rigidbody2D Rigidbody2D { get; set; }
        private Animator WalkingAnimator { get; set; }
        private SpriteRenderer SpriteRenderer { get; set; }
        private LadderClimber LadderClimber { get; set; }
        private Vector2 Movement { get; set; }
        private Vector2 LastMv { get; set; }

        public float Speed = .5f;
        private float Impedence = 1f;
        public int AmmoCount = 20000;
        public PlayerStats PlayerStats;
        public GameStats GameStats;

        public GameObject CurrentAuxArmament;
        public GameObject[] AuxArmaments;

        private int AnimHashIdle { get; set; } = Animator.StringToHash("Idle");
        private int AnimHashShiv { get; set; } = Animator.StringToHash("Shiving");
        private int AnimHashFacingRight { get; set; } = Animator.StringToHash("FacingRight");

        private bool IsFiring { get; set; }
        private float AttackingTime { get; set; } = 0;
        public float DurationOfShiv = 1f;

        private void Awake()
        {
        }

        void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            WalkingAnimator = GetComponent<Animator>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            LadderClimber = GetComponent<LadderClimber>();
            //SceneObjects.TheGameController.WireUpPlayer(gameObject);

            var inventory = GetComponent<InventoryHolder>();
            if (inventory != null)
            {
                PlayerStats.Gold = inventory.Inventory.Materials;
                inventory.OnResourceChanged += Inventory_OnResourceChanged;
            }

            var healthController = GetComponent<HealthPoints>();
            if (healthController != null)
            {
                PlayerStats.Health = healthController.HP;
                healthController.OnHealthChanged += HealthController_OnHealthChanged;
            }
        }

        private void HealthController_OnHealthChanged(GameObject gameObject, float orgValue, float currentValue)
        {
            PlayerStats.Health = currentValue;
        }

        private void Inventory_OnResourceChanged(Resource kind, float newAmt)
        {
            switch (kind)
            {
                case Resource.Material:
                    PlayerStats.Gold = newAmt;
                    break;
                default:
                    break;
            }
        }

        void Update()
        {
        }

        private void FixedUpdate()
        {
            if (GameStats.GameMode != GameMode.Playing)
                return;

            Rigidbody2D.gravityScale = LadderClimber.IsOnLadder ? 0 : 4;

            WalkingAnimator.SetBool(AnimHashIdle, Movement == Vector2.zero && !LadderClimber.IsOnLadder);
            WalkingAnimator.SetBool(AnimHashShiv, IsFiring);

            if (SpriteRenderer != null)
            {
                //SpriteRenderer.flipX = Movement.x < 0;
                //if (math.sign(Movement.x) != math.sign(LastMv.x))
                //Debug.Log($"MovementX: {Movement}");

                if (Movement.x != 0)
                    transform.rotation = Quaternion.Euler(0, Movement.x < 0 ? 180 : 0, 0);
            }

            if (Movement != Vector2.zero)
            {
                Vector2 playerWorldPosition = (Vector2)transform.position + (Vector2)math.abs(Movement);
                //Rigidbody2D.position += Movement * Speed * Impedence;
                Rigidbody2D.position += Movement * Speed;
                if (LadderClimber.IsOnLadder)
                    Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);
            }
            else
            {
                if (LadderClimber.IsOnLadder)
                {
                    Rigidbody2D.velocity = Vector2.zero;
                }
            }


            if ( CurrentAuxArmament == null )
            {
                if (IsFiring)
                {
                    if (Time.time - AttackingTime > DurationOfShiv)
                    {
                        IsFiring = false;
                        //AudioSource.Stop();
                    }
                }
                if (IsFiring && !AudioSource.isPlaying)
                    AudioSource.Play();
                ContactDamageArea.SafeSetActive(IsFiring);
            }
            else
            {
                ContactDamageArea.SafeSetActive(false);

                var itsFireController = CurrentAuxArmament.GetComponent<FireControl>();
                if ( itsFireController != null )
                {
                    //CurrentAuxArmament.SafeSetActive(true);
                    //IsFiring = itsFireController.CanFire();
                    if (IsFiring)
                        itsFireController.Fire();
                }
                IsFiring = false;
            }

            LastMv = Movement;
        }

        public void OnWeapons(InputValue value)
        {
            if (GameStats.GameMode != GameMode.Playing)
                return;

            if ( AuxArmaments.Length > 0 )
            {
                if (CurrentAuxArmament == null)
                    CurrentAuxArmament = AuxArmaments[0];
                else
                    CurrentAuxArmament = null;
            }

            var asMunition = CurrentAuxArmament == null ? null : CurrentAuxArmament.GetComponent<Munition>();
            PlayerStats.CurrentWeaponName = asMunition != null ? asMunition.Identification : "SV";
        }

        public void OnMove(InputValue value)
        {
            if (GameStats.GameMode != GameMode.Playing)
                return;

            Movement = value.Get<Vector2>();
        }

        public void OnFire()
        {
            if (GameStats.GameMode != GameMode.Playing)
                return;

            //            if (AmmoCount > 0)
            {
                //Debug.Log($"Firing. Ammo remaining: {AmmoCount}");
                //SendFire();
                IsFiring = true;
                AttackingTime = Time.time;
            }
            //    else
            //    {
            //        Debug.Log("I'm outta ammo");
            //    }
            //
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

        public void Add(Munition toAdd)
        {
            Add(toAdd.gameObject);
        }

        public void Add(GameObject payload)
        {
            AuxArmaments[0] = GameObject.Instantiate(payload, WeaponMountPoint.transform);
            CurrentAuxArmament = null;
        }
    }

}