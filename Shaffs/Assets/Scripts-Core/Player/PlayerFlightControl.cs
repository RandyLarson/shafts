using System;
using UnityEngine;

namespace Milkman
{
	public class PlayerFlightControl
	{
		public Vector3 LastMousePosition { get; set; } = Vector3.zero;

		Vector2 innerControlVector;
		Vector2 innerMainEngineVector;
		Vector2 innerManeuveringVector;

		/// <summary>
		/// Each component is the range of [-1,1]
		/// </summary>
		public Vector2 FlightControlVector { get => innerControlVector; private set => innerControlVector = value; }
		public float FlightControlX { get => innerControlVector.x; set => innerControlVector.x = value; }
		public float FlightControlY { get => innerControlVector.y; set => innerControlVector.y = value; }

		/// <summary>
		/// A measure of degrees for the heading. The heading is relative to the facing.
		/// </summary>
		public float Heading { get; set; }

		/// <summary>
		/// A measure of the main engine power. The X component of the main engine vector is used to determine the throttle.
		/// </summary>
		public Vector2 MainEngineVector { get => innerMainEngineVector; private set => innerMainEngineVector = value; }
		public float MainEngineVectorX {
			get => innerMainEngineVector.x;
			set {
				if (value == 0 || innerMainEngineVector.x == 0)
					TimeOfAccelerationStart = Time.time;

				innerMainEngineVector.x = value;
			}
		}
		/// <summary>
		/// The Y component of the main engine will always be 0, unless some kind of vertical component is added to the main
		/// engine. Otherwise, main thrust is applied to the 'right' vector of the ship. 
		/// </summary>
		public float MainEngineVectorY { get => innerMainEngineVector.y; set => innerMainEngineVector.y = value; }

		/// <summary>
		/// Maneuvering jets
		/// </summary>
		public Vector2 ManeuveringVector { get => innerManeuveringVector; private set => innerManeuveringVector = value; }
		public float ManeuveringThrustX { get => innerManeuveringVector.x; set => innerManeuveringVector.x = value; }
		public float ManeuveringThrustY { get => innerManeuveringVector.y; set => innerManeuveringVector.y = value; }

		public float TimeOfAccelerationStart { get; set; } = 0f;
		public float TimeSinceAccelerationStart { get => Time.time - TimeOfAccelerationStart; }
		public void ResetTimeOfAccelerationStart() => TimeOfAccelerationStart = 0;
		public float PercentToFullPower(float timeToMaxPower)
		{
			return Mathf.Clamp01(TimeSinceAccelerationStart / timeToMaxPower);
		}

		public PlayerFlightControl()
		{
			Init();
		}

		internal void Init()
		{
			DoFireWeapon = false;
			DoFreightDrop = false;
			DoWeaponCycleDirection = 0;
			ZeroFlightControls();
			ZeroManeuveringThrusters();
		}

		/// <summary>
		/// Percent of throttle engaged [-1,1]
		/// </summary>
		public float MainThrottle
		{
			get => MainEngineVector.magnitude;
		}
		public float ManeuveringThrottle
		{
			get => ManeuveringVector.magnitude;
		}

		public bool DoFireWeapon { get; set; } = false;
		public bool DoFreightDrop { get; set; } = false;
		public int DoWeaponCycleDirection { get; set; } = 0;
		public bool ApplyAirBrake { get; internal set; } = false;
		public bool DoWeaponDisposal { get; internal set; } = false;

		internal void ZeroFlightControls()
		{
			Heading = 0;
			innerControlVector.x = 0;
			innerControlVector.y = 0;
			innerMainEngineVector.x = 0;
			innerMainEngineVector.y = 0;

			//ZeroManeuveringThrusters();
		}

		internal void LevelFlightControls()
		{
			Heading = 0;
		}

		internal void ZeroManeuveringThrusters()
		{
			innerManeuveringVector.x = 0;
			innerManeuveringVector.y = 0;
		}

		internal void ApplyBrakingFactor(float factor)
		{
			MainEngineVectorX += -MainEngineVectorX * factor;
			// Do not level out the ship immediately.
			//FlightControls.ZeroFlightControls();
			ZeroManeuveringThrusters();
		}
	}
}
