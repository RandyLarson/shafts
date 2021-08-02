using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.InvasivePlants
{
	class PlantManager : MonoBehaviour
	{
		private static List<IInvasivePlant> RemovedItems = new List<IInvasivePlant>();
		private static IInvasivePlant[] KnownPlants;
		private static int iFreeIndex = 0;

		static PlantManager()
		{
			KnownPlants = new IInvasivePlant[10000];
			iFreeIndex = 0;
		}

		public static void AddPlant(IInvasivePlant toAdd)
		{
			if (iFreeIndex < KnownPlants.Length)
			{
				KnownPlants[iFreeIndex] = toAdd;
				iFreeIndex++;
			}
		}

		public static void RemovePlant(IInvasivePlant toRemove)
		{
			// Need to find the item and remove it from the array at some point.
			RemovedItems.Add(toRemove);
		}

		private void Update()
		{
			for (int i = 0; i < iFreeIndex; i++)
			{
				try
				{
					if (KnownPlants[i] != null)
						KnownPlants[i].DoLiving();

				}
				catch (Exception)
				{
					KnownPlants[i] = null;
				}
			}
		}
	}
}
