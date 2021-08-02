using UnityEngine;
using Assets.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Munitions;
using static UnityEngine.ParticleSystem;

namespace Milkman
{
	public struct RBHoldingValues
	{
		public float angularDrag;
		public float drag;
		public float gravityScale;
		public float mass;
		public bool useAutoMass;
		public PhysicsMaterial2D physicsMaterial2D;
	}

	//[RequireComponent(typeof(PlayerShieldController))]
	[RequireComponent(typeof(HealthPoints))]
	[RequireComponent(typeof(PickUp))]
	public class PlayerShip : MonoBehaviour, IFreightController
	{
		public float ShipForce = 2000;
		public float ManeuveringForce = 600;
		[Range(.5f, 10f)]
		public float TimeToFullPower = 3f;
		public Vector2 MaxVelocity;
		public float BrakeDampening = 4f;
		public float TurnRate = 80;

		[Range(.1f, 1)]
		public float ActiveBrakingFactor = .8f;

		[Range(.1f, 1)]
		public float PassiveBrakingFactor = .4f;

		/// <summary>
		/// Saved from the rigid body's original drag.
		/// The drag of the ship is altered to brake it when.
		/// </summary>
		private float SavedMovingDrag = 0f;

		public MunitionSlot MainWeapon { get; set; }
		public List<MunitionSlot> WeaponRack { get; set; } = new List<MunitionSlot>();

		public List<Munition> AvailableWeapons;
		public int AvailableWeaponSlots = 3;
		private float NextWeaponChangeAvailableAt = 0;
		public float WeaponChangeCoolDown = .25f;

		public Transform MainWeaponSpawn;

		public GameObject Shield;

		public bool FacingLeft = true;

		[Range(-180,180)]
		public float FacingOffset = 0f;


		public Animator[] engineAnimators;            // Reference to the player's animator component.

		public Rigidbody2D PlayerRigidBody { get; private set; }
		public HealthPoints Health { get; private set; }
		public float TimeToLive = 30;

		public HealthPoints ShieldHealth { get => ShieldController?.ShieldHealth; }
		private ShieldController ShieldController = null;


		private float NextWeaponFireTime { get; set; } = 0;

		private PickUp FreightPicker { get; set; }
		private ParticleSystem MainEngineParticleSystem;

		private void Awake()
		{
			PlayerRigidBody = GetComponent<Rigidbody2D>();
			Health = GetComponent<HealthPoints>();

			if (Shield != null)
				ShieldController = Shield.GetComponent<PlayerShieldController>();

			SavedMovingDrag = PlayerRigidBody.drag;
		}


		private void Start()
		{
			InitializeWeaponRack();
			FreightPicker = GetComponent<PickUp>();
			MainEngineParticleSystem = GetComponentInChildren<ParticleSystem>();
		}

		private void InitializeWeaponRack()
		{
			foreach (Munition ammo in AvailableWeapons)
			{
				AddWeapon(ammo);
			}
			MainWeapon = WeaponRack.FirstOrDefault();
			GameController.TheController.SignalGameEvent(GameConstants.EventPlayerWeaponChange);
		}

		private void OnDestroy()
		{
			GameController.PlayerDestroyed();
		}

		private void OnDrawGizmos()
		{
			//Gizmos.DrawWireSphere(PlayerShipController.CursorToWorldCoordinates, 3);
		}

		public void FireWeapon()
		{
			if (MainWeapon != null && Time.time >= NextWeaponFireTime)
			{
				// Collect statistics and tag the munition as being owned by the player
				// so it can be tracked and tallied.
				Munition payload = Instantiate(MainWeapon.Munition, MainWeaponSpawn.position, transform.rotation);
				payload.IsPlayerMunition = true;
				payload.Count = 0;

				if (payload.gameObject.GetComponent(out Rigidbody2D munitionRB))
				{
					Vector2 launchForce = (Vector2)(payload.LaunchForce * payload.transform.right) + PlayerRigidBody.velocity;
					//Debug.DrawLine(shot.transform.position, 10 * launchForce, Color.yellow, .3f);
					munitionRB.AddForce(launchForce, ForceMode2D.Impulse);
				}

				NextWeaponFireTime = Time.time + payload.FireRate;

				if (MainWeapon.Munition.Capacity > 0)
				{
					MainWeapon.Count--;
					if (MainWeapon.Count == 0)
					{
						DropCurrentWeaponSlot();
					}
				}

				GameController.LevelStatistics.ShotFired();
			}
		}

		public void AttemptPickup(GameObject toPickup) => FreightPicker.CarryItem(toPickup);
		public void AttemptDrop() => FreightPicker.DropFreight();
		public void DropFreight() => FreightPicker.DropFreight();
		public bool IsCarryingFreight { get => FreightPicker.IsCarryingFreight; }



		bool noLerp = false;

		public void ApplyFlightControls(PlayerFlightControl flightControl)
		{
			//DiagnosticController.Add($"Main-Throttle: {flightControl.MainThrottle}");
			//DiagnosticController.Add($"Main-Vector: {flightControl.MainEngineVector}");
			//DiagnosticController.Add($"Man-Throttle: {flightControl.ManeuveringThrottle}");
			//DiagnosticController.Add($"Man-Vector: {flightControl.ManeuveringVector}");
			//DiagnosticController.Add($"Flight-Vector: {flightControl.FlightControlVector}");
			//DiagnosticController.Add($"Heading: {flightControl.Heading}");
			//DiagnosticController.Add($"Velocity: {PlayerRigidBody.velocity}");

			if (flightControl.ApplyAirBrake)
				PlayerRigidBody.drag = BrakeDampening;
			else
				PlayerRigidBody.drag = SavedMovingDrag;

			var currentRotation = gameObject.transform.rotation.eulerAngles.z;

			float yRotation = FacingLeft ? 0: 180;

			// The angle of the joystick with respect to our facing.
			var dstAngle = flightControl.Heading;

			//DiagnosticController.Add($"Start-Z: {currentRotation}");
			//DiagnosticController.Add($"Dst Angle: {dstAngle}");

			if (FacingLeft)
			{
				if (flightControl.FlightControlX < 0)
					dstAngle = 180 - dstAngle;
				//if (flightControl.FlightControlX > 0)
				//	dstAngle = 180 - dstAngle;
			}
			else
			{
				if (flightControl.FlightControlX < 0)
					dstAngle = 180 - dstAngle;
			}

			dstAngle += GameController.ThePlayer.FacingOffset;

			//var ultimateRotation = Quaternion.Euler(0f, yRotation, dstAngle);
			//var stepRotation = Quaternion.Slerp(transform.rotation, ultimateRotation, TurnRate * Time.deltaTime);
			//gameObject.transform.rotation = stepRotation;

			float nextAngle = noLerp ? dstAngle : Mathf.LerpAngle(currentRotation, dstAngle, TurnRate * Time.deltaTime);
			var nextRotation = Quaternion.Euler(0f, yRotation, nextAngle);
			gameObject.transform.rotation = nextRotation;
			noLerp = false;

            //DiagnosticController.Add($"Dst Angle: {dstAngle}");
            //DiagnosticController.Add($"Turn Rate: {TurnRate}");
            //DiagnosticController.Add($"Final X: {gameObject.transform.rotation.eulerAngles.x}");
            //DiagnosticController.Add($"Final Y: {gameObject.transform.rotation.eulerAngles.y}");
            //DiagnosticController.Add($"Final Z: {gameObject.transform.rotation.eulerAngles.z}");
            //DiagnosticController.Add($"Facing L: {FacingLeft}");

            //// 135 / 270
            //if (Mathf.DeltaAngle(nextAngle, dstAngle) < 10 &&
            //    gameObject.transform.rotation.eulerAngles.z > 160 && gameObject.transform.rotation.eulerAngles.z < 200)
            //{
            //    FacingLeft = !FacingLeft;
            //    noLerp = true;
            //}

            // Main engine throttle
            float mainEngineThrottle = flightControl.MainThrottle;
			float hForceNet = ShipForce * mainEngineThrottle * Time.deltaTime * flightControl.PercentToFullPower(TimeToFullPower);
			float playerSqrVelocity = PlayerRigidBody.velocity.sqrMagnitude;

			// Default thrust vector is 'right', which is main-engine thrust.
			// The maneuvering jets may be active. They'll change the trust vector.
			Vector3 thrustVector = Vector3.zero;

			// The maneuvering jets can be active if the main engine is not.
			if (flightControl.MainThrottle != 0)
			{
				thrustVector = GameController.ThePlayer.FacingOffset == 0 ? -transform.right : transform.up;
			}
			else if (flightControl.ManeuveringThrottle != 0)
			{
				hForceNet = ManeuveringForce * flightControl.ManeuveringThrottle * Time.deltaTime;
				if (flightControl.ManeuveringThrustY != 0)
				{
					thrustVector = flightControl.ManeuveringThrustY > 0 ? transform.up : -transform.up;
				}

				if (flightControl.ManeuveringThrustX != 0)
				{
					// Switch the vector if we are facing left.
					// The input controller doesn't know about facing.
					if (FacingLeft)
						thrustVector = thrustVector + ((flightControl.ManeuveringThrustX > 0) ? -transform.right : transform.right);
					else
						thrustVector = thrustVector + ((flightControl.ManeuveringThrustX > 0) ? transform.right : -transform.right);
				}
			}

			// Debug.DrawLine(transform.position, transform.position + (thrustVector * hForceNet), Color.blue);

			// Limit the velocity - if we are  at the max, zero out that vector.
			// This should cause 0 force to be applied along that axis.
			if (PlayerRigidBody.velocity.x < -MaxVelocity.x ||
				PlayerRigidBody.velocity.x > MaxVelocity.x)
			{
				thrustVector.x = 0;
			}

			if (PlayerRigidBody.velocity.y < -MaxVelocity.y ||
				PlayerRigidBody.velocity.y > MaxVelocity.y)
			{
				thrustVector.y = 0;
			}

			PlayerRigidBody.AddForce(thrustVector * hForceNet, ForceMode2D.Impulse);

			// Obey the bounds of the level

			if (!GameController.TheController.LevelBoundary.Contains(transform.position))
			{

				Vector3 clampedPosition = new Vector3(
					Mathf.Clamp(transform.position.x, GameController.TheController.LevelBoundary.xMin, GameController.TheController.LevelBoundary.xMax),
					Mathf.Clamp(transform.position.y, GameController.TheController.LevelBoundary.yMin, GameController.TheController.LevelBoundary.yMax),
					transform.position.z);
				transform.position = clampedPosition;
			}

			// 0 = no-thrusters
			// 1 = rear-thrusters
			// 2 = front-thrusters
			int horzThrusterStatus = 0;

			// 0 = no-thrusters
			// 1 = bottom-thrusters
			// 2 = top-thrusters
			int vertThrusterStatus = 0;

			if (flightControl.ManeuveringThrustX > 0)
				horzThrusterStatus = FacingLeft ? 2 : 1;
			else if (flightControl.ManeuveringThrustX < 0)
				horzThrusterStatus = FacingLeft ? 1 : 2;

			if (flightControl.ManeuveringThrustY < 0)
				vertThrusterStatus = 2;
			else if (flightControl.ManeuveringThrustY > 0)
				vertThrusterStatus = 1;

			// False=>Off, if no throttle & some thruster activity.
			bool mainThrusterStatus = mainEngineThrottle != 0 || (horzThrusterStatus == 0 && vertThrusterStatus == 0);

			// Inform the UI components of the engines active
			foreach (var animator in engineAnimators)
			{
				animator.SetFloat(GameConstants.PowerHash, mainEngineThrottle);
				animator.SetFloat(GameConstants.VertVelocityHash, flightControl.MainEngineVectorY);
				animator.SetFloat(GameConstants.SqrVelocityHash, playerSqrVelocity);

				animator.SetInteger(GameConstants.HorzThrusterHash, horzThrusterStatus);
				animator.SetInteger(GameConstants.VertThrusterHash, vertThrusterStatus);
				animator.SetBool(GameConstants.MainEngineStatusHash, mainThrusterStatus);
			}

			if (MainEngineParticleSystem != null)
			{
				//EmissionModule emissionModule = MainEngineParticleSystem.emission;
				//emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Max(.15f, mainEngineThrottle) * 10f);

				// var mainTraits = MainEngineParticleSystem.main;
				// var pSize = new ParticleSystem.MinMaxCurve(Mathf.Max(.15f, mainEngineThrottle) * 10f);
				// mainTraits.startSizeX = pSize;
				// mainTraits.startSizeY = pSize;
			}
		}


		internal void AddWeapon(Munition payload)
		{
			if (payload != null)
			{
				//if (payload.Count == 0 && payload.Capacity > 0)
				//	payload.Count = payload.Capacity;

				MunitionSlot existingMunition = WeaponRack.Find(slot => slot.Munition.Identification == payload.Identification);
				if (existingMunition != null)
				{
					existingMunition.Munition = payload;
					GameController.TheController.SignalGameEvent(GameConstants.EventPlayerWeaponChange);
				}
				else if (WeaponRack.Count < AvailableWeaponSlots)
				{
					MunitionSlot newSlotItem = new MunitionSlot(payload);
					WeaponRack.Add(newSlotItem);
					GameController.TheController.SignalGameEvent(GameConstants.EventPlayerWeaponChange);
				}
				else
				{
					// Message - no more weapon slots.
				}
			}
		}

		internal void DropCurrentWeaponSlot()
		{
			if (NextWeaponChangeAvailableAt <= Time.time)
			{
				if (WeaponRack.Count > 1)
				{
					int dropIndex = WeaponRack.FindIndex(avail => avail == MainWeapon);
					WeaponRack.RemoveAt(dropIndex);
					int newMainIndex = dropIndex % WeaponRack.Count;
					MainWeapon = WeaponRack[newMainIndex];
					GameController.TheController.SignalGameEvent(GameConstants.EventPlayerWeaponChange);
					NextWeaponChangeAvailableAt = Time.time + WeaponChangeCoolDown;
				}
			}
		}

		internal void CycleMainWeapon(int doWeaponCycleDirection)
		{
			if (NextWeaponChangeAvailableAt <= Time.time)
			{
				if (WeaponRack.Count > 1)
				{
					int currentIndex = WeaponRack.FindIndex(avail => avail == MainWeapon);
					currentIndex += doWeaponCycleDirection;
					if (currentIndex < 0)
						currentIndex = WeaponRack.Count - 1;
					else if (currentIndex >= WeaponRack.Count)
						currentIndex = 0;

					MainWeapon = WeaponRack[currentIndex];
					MainWeapon.Munition.PlayLoadingAudio();
					GameController.TheController.SignalGameEvent(GameConstants.EventPlayerWeaponChange);
					NextWeaponChangeAvailableAt = Time.time + WeaponChangeCoolDown;
				}
			}
		}

		internal void UpdateTimeToLive(float byAmt)
        {
			TimeToLive += byAmt;
        }

		internal void UpdateShield(GameObject newShield, float amount)
		{
			if (newShield != null && newShield != Shield)
			{
				Shield = newShield;
				ShieldController = Shield?.GetComponent<PlayerShieldController>();
			}
			ShieldController?.AddResource(Resource.Shield, amount);
		}

		internal void AdjustHealth(float amount)
		{
			Health.AdjustHealthBy(amount);
		}

		internal void AdjustTimeToLive(float amount)
		{
			TimeToLive += amount;
		}

		//internal void UpdateWeapon(Munition payload, float amount)
		//{
		//	if (payload != null)
		//		MainWeapon = payload;
		//}


	}
}
