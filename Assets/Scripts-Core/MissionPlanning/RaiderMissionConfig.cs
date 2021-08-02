using System;
using UnityEngine;

namespace Assets.Scripts.MissionPlanning
{
	public class RaiderMissionConfig : MonoBehaviour, IConfigurator
	{
		public float FreightStealingRadius = 100f;
		public GameObject ExitDestination = null;
		public GameObject MusterLocation = null;

		public Type TargetType { get; set; } = typeof(RaiderMissionController);
		public void ConfigureTarget(Component component)
		{
			RaiderMissionController asController = component as RaiderMissionController;
			if (asController != null)
			{
				asController.FreightStealingRadius = FreightStealingRadius;
				asController.ExitDestination = ExitDestination;
				asController.MusterLocation = MusterLocation;
			}
		}
	}

}
