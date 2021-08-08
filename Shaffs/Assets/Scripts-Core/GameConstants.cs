using UnityEngine;

public static class GameConstants
{
	public static string PatrolBeacon = "PatrolBeacon";
	public static string CiviAircraft = "CiviAircraft";
	public static string Structures = "Structures";
	public static string MilitaryAircraft = "MilitaryAircraft";
	public static string PatrolTarget = "PatrolTarget";
	public static string Portal = "Portal";
	public static string Player = "Player";
	public static string Mayfly = "Mayfly";
	public static string Alien = "Alien";
	public static string Freight = "Freight";
	public static string Terrain = "Terrain";
	public static string Shield = "Shield";
	public static string Power = "power";
	public static string VVertical = "v-vert";
	public static string HorzThrusterId = "h-thrusterId";
	public static string VertThrusterId = "v-thrusterId";
	public static string MainEngineStatus = "mainEngineStatus";
	public static string SqrVelocity = "sqr-velocity";
	public static string LevelStart = "Level-0.0-";
	public static string Underground = "Underground";
	public static string Default = "Default";

	public static string EventPlayerWeaponChange = "PlayerWeaponChange";
		
	public static int ShieldHitHash = Animator.StringToHash("Impact");
	public static int ShieldDeactivatedHash = Animator.StringToHash("Deactivated");
	public static int ShieldHealthHash = Animator.StringToHash("Health");

	public static int PowerHash = Animator.StringToHash(Power);
	public static int SqrVelocityHash = Animator.StringToHash(SqrVelocity);

	// 0 = no-thrusters
	// 1 = rear-thrusters
	// 2 = front-thrusters
	public static int HorzThrusterHash = Animator.StringToHash(HorzThrusterId);

	// 0 = no-thrusters
	// 1 = bottom-thrusters
	// 2 = top-thrusters
	public static int VertThrusterHash = Animator.StringToHash(VertThrusterId);

	// 0 = main-engine off
	// 1 = main-engine on
	public static int MainEngineStatusHash = Animator.StringToHash(MainEngineStatus);
	public static int VertVelocityHash = Animator.StringToHash(VVertical);

	public static int LayerMaskDefault;
	public static int LayerMaskPatrolBeacon;
	public static int LayerMaskPortal;
	public static int LayerMaskTerrain;
	public static int LayerMaskCivilianShips;
	public static int LayerMaskStructures;
	public static int LayerMaskDefaultAvoid;
	public static int LayerMaskShield;

	public static int SortingLayerUnderground;
	public static int SortingLayerDefault;

	public static void Init()
	{
		LayerMaskPatrolBeacon = 1 << LayerMask.NameToLayer(PatrolBeacon);
		LayerMaskPortal = 1 << LayerMask.NameToLayer(Portal);
		LayerMaskTerrain = 1 << LayerMask.NameToLayer(Terrain);
		LayerMaskShield = 1 << LayerMask.NameToLayer(Shield);
		LayerMaskDefault = 1 << LayerMask.NameToLayer("Default");
		LayerMaskCivilianShips = 1 << LayerMask.NameToLayer(CiviAircraft);
		LayerMaskStructures = 1 << LayerMask.NameToLayer(Structures);
		LayerMaskDefaultAvoid = LayerMaskTerrain | LayerMaskCivilianShips | LayerMaskStructures;

		SortingLayerUnderground = SortingLayer.NameToID(Underground);
		SortingLayerDefault = SortingLayer.NameToID(Default);
	}

}
