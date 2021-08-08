using System;
using System.Collections.Generic;
using UnityEngine;

namespace Milkman
{
	public interface IUpdatable
	{
		void DoUpdate();
	}

	public class ItemManager<Face> : MonoBehaviour where Face : class, IUpdatable
	{
		private List<Face> RemovedItems = new List<Face>();
		private Face[] KnownItems;
		private int iFreeIndex = 0;

		public ItemManager()
		{
			KnownItems = new Face[10000];
			iFreeIndex = 0;
		}

		public void AddItem(Face toAdd)
		{
			if (iFreeIndex < KnownItems.Length)
			{
				KnownItems[iFreeIndex] = toAdd;
				iFreeIndex++;
			}
		}

		public void RemoveItem(Face toRemove)
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
					if (KnownItems[i] != null)
						KnownItems[i].DoUpdate();
					else
						KnownItems[i] = null;
				}
				catch (Exception)
				{
					KnownItems[i] = null;
				}
			}
		}
	}

}
