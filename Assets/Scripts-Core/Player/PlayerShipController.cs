using System;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Milkman
{

	public class PlayerShipController : MonoBehaviour
	{
		public Joystick playerJoystick;
		public ButtonJoystick weaponsControl;
		public ButtonJoystick freightControl;


		private PlayerFlightControl FlightControls { get; set; } = new PlayerFlightControl();

		private void Start()
		{
		}

		private void Awake()
		{
		}

		private void OnEnable()
		{
			InitPlayerInputSystem();
		}


		private void InitPlayerInputSystem()
		{
			FlightControls = new PlayerFlightControl();
		}

		Range mouseXRange = new Range(0, 0);
		Range mouseYRange = new Range(0, 0);

		public static Vector3 CursorToWorldCoordinates
		{
			get
			{
				var pointerPos = Pointer.current.position.ReadValue();
				return GameUIController.Controller.MainCamera.ScreenToWorldPoint(
					new Vector3(pointerPos.x,
					pointerPos.y,
					0));
			}
		}

		private void GatherInputMetrics(System.Text.StringBuilder sb)
		{
			sb.Length = 0;
			sb.AppendLine($"Mouse-Delta-X: {Input.GetAxis("MouseX")} Y: {Input.GetAxis("MouseY")}");
			sb.AppendLine($"Horizontal: {Input.GetAxis("Horizontal")}");
			sb.AppendLine($"Vertical: {Input.GetAxis("Vertical")}");
			sb.AppendLine($"Fire-1: {Input.GetButton("Fire1")}");
			sb.AppendLine($"Fire-2: {Input.GetButton("Fire2")}");
			sb.AppendLine($"Fire-3: {Input.GetButton("Fire3")}");
			sb.AppendLine($"MousePosition: {Input.mousePosition.x}, {Input.mousePosition.y}");
			sb.AppendLine($"MouseScroll: {Input.mouseScrollDelta}");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="thrustInput">Ranging between -1 .. 1 </param>
		private void ApplyThrustToFlightControls(float thrustInput, bool negativeThrustIsBrake)
		{
			if (thrustInput != 0)
			{
				FlightControls.ZeroManeuveringThrusters();
				FlightControls.MainEngineVectorX = Math.Max(0, FlightControls.MainEngineVectorX + thrustInput);

				if (negativeThrustIsBrake)
					FlightControls.ApplyAirBrake = thrustInput < 0;
			}
			else
			{
				if (FlightControls.MainEngineVectorX < .1f)
					FlightControls.MainEngineVectorX = 0;
				else
					FlightControls.ApplyBrakingFactor(GameController.ThePlayer.PassiveBrakingFactor);
			}
		}

		private void AssessMouseFirstInput(bool preferMouseForFlightControl)
		{
			Vector3 cursorWorldPos = CursorToWorldCoordinates;

			//
			// Pitch/facing of ship
			//
			float theta = 0;
			float headingInputX = 0;
			float headingInputY = 0;

			// **Mouse**
			// The 'stick' position is the relative position of the mouse to the player\
			if (preferMouseForFlightControl)
			{
				Vector3 cursorOffset = cursorWorldPos - GameController.ThePlayer.transform.position;

				Debug.DrawLine(GameController.ThePlayer.transform.position, GameController.ThePlayer.transform.position+cursorOffset, Color.red);

				float r = Vector2.Distance(cursorWorldPos, GameController.ThePlayer.transform.position);
				theta = Vector2.Angle(new Vector2(cursorOffset.x, 0), cursorOffset);
				var rad = Mathf.Deg2Rad * theta;
				headingInputX = Mathf.Cos(rad) * r;
				headingInputY = Mathf.Sin(rad) * r;

				// Accommodate for how the facing of the sprite is
				// build - this is for left-facing sprites.
				// For sprites built right-facing, these `1`s should be 
				// swapped to `-1`s.
				if (Mathf.Sign(cursorOffset.x) == 1)
				{
					headingInputX = -headingInputX;
				}
				if (Mathf.Sign(cursorOffset.y) == 1)
				{
					headingInputY = -headingInputY;
					theta = -theta;
				}
			}
			else
			{
				// **Game Pad**
				// These are percentages, just like the on-screen joystick.
				var gamepad = Gamepad.current;
				if ( gamepad != null)
                {

				//headingInputX = gamepad.leftStick.x.GetAxis("HeadingXComp");
				//headingInputY = Input.GetAxis("HeadingYComp");
                }

				//if (Mathf.Abs(headingInputX) < .1)
				//	headingInputX = 0;

				//if (Mathf.Abs(headingInputY) < .1)
				//	headingInputY = 0;

				theta = CalculateHeadingFromStickPercentages(headingInputX, headingInputY);

				DiagnosticController.Add($"HeadingXComp: {headingInputX}");
				DiagnosticController.Add($"HeadingYComp: {headingInputY}");
			}

			FlightControls.Heading = theta;
			FlightControls.FlightControlX = headingInputX;
			FlightControls.FlightControlY = headingInputY;

			var thePointer = Pointer.current;
			var mousePos = thePointer.position;

			float thrusterHInput = 0; // Input.GetAxis("Horizontal");
			float thrusterVInput = 0; // Input.GetAxis("Vertical");
			float mainEngineInput = Math.Min(1, Keyboard.current.leftShiftKey.ReadValue() + Keyboard.current.rightShiftKey.ReadValue()); //  0; //  Input.GetAxis("Thrust");
			bool doDrop = false; //  Input.GetButtonDown("DropPayload");
			bool applyBrake = false; //  Input.GetButton("Brake");
			bool levelFlight = false; //  Input.GetButton("LevelFlight");
			bool doFireWeapon = false; //  Input.GetButton("Fire1");
			bool doCycleWeaponToNext = false; //  Input.GetButtonDown("ChangeWeaponNext");
			bool doCycleWeaponToPrev = false; //  Input.GetButtonDown("ChangeWeaponPrev");
			bool doWeaponDisposal = false; //  Input.GetButtonDown("DropCurrentWeapon");
			float cycleWeaponAxis = 0; //  Input.GetAxis("ChangeWeapon");

			//
			// Main-Engine thrust -- apply to ship.
			//
			ApplyThrustToFlightControls(mainEngineInput, true);

            // Diagnostics..
#if false
            DiagnosticController.Add($"MouseX: {Input.GetAxis("MouseX")}");
            DiagnosticController.Add($"MouseY: {Input.GetAxis("MouseY")}");
            DiagnosticController.Add($"Thrust: {mainEngineInput}");
            DiagnosticController.Add($"DoDrop: {doDrop}");
            DiagnosticController.Add($"DoCycleWeapon-Next: {doCycleWeaponToNext}");
            DiagnosticController.Add($"DoCycleWeapon-Prev: {doCycleWeaponToPrev}");
            DiagnosticController.Add($"ApplyBrake: {applyBrake}");

            //DiagnosticController.Add($"Theta: {theta}");
            //DiagnosticController.Add($"Rad: {rad}");
            DiagnosticController.Add($"Theta: {theta}");
            DiagnosticController.Add($"HeadingX: {headingInputX}");
            DiagnosticController.Add($"HeadingY: {headingInputY}");
            DiagnosticController.Add($"ThrustH: {thrusterHInput}");
            DiagnosticController.Add($"ThrustY: {thrusterVInput}");

#endif
            // Debug.DrawLine(GameController.ThePlayer.transform.position, new Vector3(GameController.ThePlayer.transform.position.x + cx, GameController.ThePlayer.transform.position.y + cy, 0), Color.green);
            // Debug.DrawLine(GameController.ThePlayer.transform.position, GameController.ThePlayer.transform.position + (Vector3)(FlightControls.FlightControlVector), Color.green);
            // ..Diagnostics

            FlightControls.MainEngineVectorX = Mathf.Clamp(FlightControls.MainEngineVectorX, -1, 1);
			FlightControls.MainEngineVectorY = Mathf.Clamp(FlightControls.MainEngineVectorY, -1, 1);
			FlightControls.FlightControlX = Mathf.Clamp(FlightControls.FlightControlX, -1, 1);
			FlightControls.FlightControlY = Mathf.Clamp(FlightControls.FlightControlY, -1, 1);

			if (applyBrake)
			{
				FlightControls.ApplyAirBrake = applyBrake;
				FlightControls.ApplyBrakingFactor(GameController.ThePlayer.ActiveBrakingFactor);
			}

			if (levelFlight)
			{
				FlightControls.LevelFlightControls();
			}

			//  Maneuvering thrusters:
			if (thrusterHInput != 0 || thrusterVInput != 0)
			{
				FlightControls.ZeroFlightControls();
			}

			FlightControls.ManeuveringThrustX = thrusterHInput;
			FlightControls.ManeuveringThrustY = thrusterVInput;

			FlightControls.DoWeaponCycleDirection = doCycleWeaponToNext ? 1 : doCycleWeaponToPrev ? -1 : 0;
			FlightControls.DoWeaponDisposal = doWeaponDisposal;
			FlightControls.DoFreightDrop = doDrop;
			FlightControls.DoFireWeapon = doFireWeapon;

			// Check the input axis (mouse-wheel by default)
			if (FlightControls.DoWeaponCycleDirection == 0)
			{
				DiagnosticController.Add($"DoCycleWeapon-Wheel: {cycleWeaponAxis}");

				FlightControls.DoWeaponCycleDirection = (int)Math.Round(cycleWeaponAxis);
				DiagnosticController.Add($"DoCycleWeapon-Final: {FlightControls.DoWeaponCycleDirection}");
			}

			// At the beginning of a level or segment, we don't want the ship to pitch toward
			// the mouse until any flight control is used (mouse or keyboard)
			bool kbdCanZero = false;
			//if (Mathf.Abs(GameController.ThePlayer.PlayerRigidBody.velocity.x) < 3 &&
			//	Mathf.Abs(GameController.ThePlayer.PlayerRigidBody.velocity.y) < 3 &&
			//	mainEngineInput == 0 &&
			//	thrusterVInput == 0)
			//{
			//	kbdCanZero = true;
			//}

			bool mouseCanZero = false;
			if (FlightControls.LastMousePosition == Vector3.zero ||
				FlightControls.LastMousePosition == cursorWorldPos)
			{
				mouseCanZero = true;
			}

			DiagnosticController.Add($"ZeroKbd: {kbdCanZero}");
			DiagnosticController.Add($"ZeroMse: {mouseCanZero}");

			if (kbdCanZero && mouseCanZero)
				FlightControls.Heading = 0;

			FlightControls.LastMousePosition = cursorWorldPos;
		}

		Vector2 HeadingVectorTmp = new Vector2();

		/// <summary>
		/// Give horizontal and vertical input in the range of [-1,1], returns the heading.
		/// Used for converting touch screen and game pad joystick input into a ship heading.
		/// </summary>
		/// <param name="horizonal"></param>
		/// <param name="vertical"></param>
		/// <returns>The resulting heading.</returns>
		private float CalculateHeadingFromStickPercentages(float horizonal, float vertical)
		{
			HeadingVectorTmp.x = horizonal;
			HeadingVectorTmp.y = vertical;

			var heading = Vector2.Angle(horizonal < 0 ? Vector2.left : Vector2.right, HeadingVectorTmp);
			var rad = Mathf.Deg2Rad * heading;

			if (vertical < 0)
				heading = -heading;

			return heading;
		}

		private void AssessTouchScreenInput()
		{
			if (playerJoystick != null)
			{
				DiagnosticController.Add($"Horz: {playerJoystick.Horizontal}");
				DiagnosticController.Add($"Vert: {playerJoystick.Vertical}");

				var heading = CalculateHeadingFromStickPercentages(playerJoystick.Horizontal, playerJoystick.Vertical);

				DiagnosticController.Add($"Heading: {heading}");
				DiagnosticController.Add($"CX: {playerJoystick.Horizontal}");
				DiagnosticController.Add($"CY: {playerJoystick.Vertical}");

				// This should be the abs of the horizontal input, since we are equating the 
				// distance to throttle. But this could also be the magnitude of the vector
				// created by the horizontal and vertical components of the joystick.
				ApplyThrustToFlightControls(Mathf.Abs(playerJoystick.Horizontal), false);

				// Vertical movement goes here
				FlightControls.MainEngineVectorY = 0;

				FlightControls.FlightControlX = playerJoystick.Horizontal;
				FlightControls.FlightControlY = playerJoystick.Vertical;
				FlightControls.Heading = heading;


				FlightControls.MainEngineVectorX = Mathf.Clamp(FlightControls.MainEngineVectorX, -1, 1);
				FlightControls.MainEngineVectorY = Mathf.Clamp(FlightControls.MainEngineVectorY, -1, 1);
				FlightControls.FlightControlX = Mathf.Clamp(FlightControls.FlightControlX, -1, 1);
				FlightControls.FlightControlY = Mathf.Clamp(FlightControls.FlightControlY, -1, 1);

				Debug.DrawLine(GameController.ThePlayer.transform.position, GameController.ThePlayer.transform.position + (Vector3)(FlightControls.FlightControlVector * 20), Color.green);

			}

			// Read the input in Update so button presses aren't missed.
			if (freightControl != null && freightControl.IsPressed)
				FlightControls.DoFreightDrop = true;

			if (weaponsControl != null && weaponsControl.IsPressed)
				FlightControls.DoFireWeapon = true;
		}


		private void Update()
		{
			if (GameController.ThePlayer == null)
				return;

			if (GameController.TheController.GamePhase != GameState.Play && EventSystem.current.IsPointerOverGameObject())
				return;

			switch (GameController.TheController.PlayerInputMode)
			{
				case PlayerInputStyle.TouchScreen:
					AssessTouchScreenInput();
					break;
				case PlayerInputStyle.Keyboard:
					AssessMouseFirstInput(true);
					break;

				case PlayerInputStyle.GamePad:
					AssessMouseFirstInput(false);
					break;
			}

			if (FlightControls.DoFreightDrop)
			{
				GameController.ThePlayer.AttemptDrop();
				FlightControls.DoFreightDrop = false;
			}

			if (FlightControls.DoFireWeapon)
			{
				GameController.ThePlayer.FireWeapon();
				FlightControls.DoFireWeapon = false;
			}

			if (FlightControls.DoWeaponDisposal)
			{
				GameController.ThePlayer.DropCurrentWeaponSlot();
			}

			if (FlightControls.DoWeaponCycleDirection != 0)
			{
				GameController.ThePlayer.CycleMainWeapon(FlightControls.DoWeaponCycleDirection);
				FlightControls.DoWeaponCycleDirection = 0;
			}
		}

		private void FixedUpdate()
		{
			if (GameController.ThePlayer == null)
				return;

			GameController.ThePlayer.ApplyFlightControls(FlightControls);

			//if (FlightControls.DoFireWeapon)
			//{
			//	GameController.ThePlayer.FireWeapon();
			//	FlightControls.DoFireWeapon = false;
			//}
		}
	}
}
