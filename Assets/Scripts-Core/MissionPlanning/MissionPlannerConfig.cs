using System;
using UnityEngine;

namespace Assets.Scripts.MissionPlanning
{
	[CreateAssetMenu(fileName = "MissionPlannerConfig", menuName = "Data/Mission Planner Config", order = 0)]
	public  class MissionPlannerConfig : ScriptableObject
	{
		public ShipTrackInfo[] Configuration;
		public float TimePlayerTracked = 10f;
	}

}
