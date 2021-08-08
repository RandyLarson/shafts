using UnityEngine;

namespace Assets.Scripts.Munitions
{
	public class TractorBeam : MonoBehaviour
	{
		public float RateOfFire = 1f;
		public BeamController BeamPrototype;
		public Transform MainWeaponSpawn;

		private float NextFireTime = 0f;
		private BeamController ActiveBeam { get; set; }

		void Start()
		{
		}

		void Update()
		{
		}

		public void FireWeapon(GameObject target)
		{
			if (target != null && BeamPrototype != null && MainWeaponSpawn != null && Time.time >= NextFireTime)
			{
				NextFireTime = Time.time + RateOfFire;

				ActiveBeam = Instantiate(BeamPrototype, MainWeaponSpawn.position, MainWeaponSpawn.rotation);
				ActiveBeam.AimAt(target.transform.position);
			}
		}
	}
}