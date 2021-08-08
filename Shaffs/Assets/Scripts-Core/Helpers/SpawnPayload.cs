using System;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
	[Serializable]
	public class SpawnPayload
	{
		public GameObject WhatToSpawn = null;
		public GameObject SpawnPoint = null;
		public float SpawnRate = .5f;
		public float InventoryCount = 3;
		public float SpawnForce = 0;

		private float? NextSpawnTime = null;

		public bool CanSpawn
		{
			get
			{
				return NextSpawnTime != null && NextSpawnTime <= Time.time;
			}
			set
			{
				if (value == false)
					NextSpawnTime = null;
				else if ( NextSpawnTime == null )
					CalcNextSpawnTime();
			}
		}

		private void CalcNextSpawnTime()
		{
			NextSpawnTime = Time.time + SpawnRate;
		}

		/// <summary>
		/// Spawns a payload item, accelerating it in the 'up' diretion of the spawn point.
		/// Acceleration may be set to 0.
		/// </summary>
		/// <param name="target"></param>
		public void ExecuteSpawn(GameObject target)
		{
			var spawned = UnityEngine.Object.Instantiate<GameObject>(WhatToSpawn, SpawnPoint.transform.position, SpawnPoint.transform.rotation);

			if (target != null)
			{
				var guidedController = spawned.GetComponent<GuidedProjectileController>();
				if (guidedController)
					guidedController.target = target;
			}

			if (SpawnForce > 0)
			{
				var rb = spawned.GetComponent<Rigidbody2D>();
				if (rb)
				{
					Vector2 addForce = -spawned.transform.up * SpawnForce;
					rb.AddForce(addForce, ForceMode2D.Impulse);
				}
			}

			var aud = spawned.GetComponent<AudioSource>();
			if (aud)
				aud.Play();
		}

		/// <summary>
		/// Spawns an inventory item if availalbe and enabled and time has elapsed, override
		/// with the forceSpawn param.
		/// </summary>
		/// <param name="ignoreTime">Override the time span checking component and immediately spawn an item if there is available inventory.</param>
		public void TrySpawn(bool ignoreTime = false)
		{
			if ((ignoreTime || CanSpawn) && InventoryCount > 0 && WhatToSpawn != null)
			{
				ExecuteSpawn(null);
				InventoryCount--;
				CalcNextSpawnTime();

				if ( InventoryCount == 0 )
				{
					CanSpawn = false;
				}
			}
		}
	}
}
