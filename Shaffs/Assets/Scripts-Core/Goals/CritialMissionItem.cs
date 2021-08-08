using System;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Goals
{
	public class CritialMissionItem : MonoBehaviour
	{
		public bool IsCritical = true;
		public string CriticalUntilLevel = null;
		public string Name = "Critical building";

		/// <summary>
		/// Displayed when this item is destroyed while still critical.
		/// </summary>
		public string DestroyedMessage = string.Empty;

		CritialMissionItem()
		{
		}

		private void Awake()
		{
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode mode)
		{
			if (CriticalUntilLevel != null)
			{
				if (CriticalUntilLevel.EqualsIgnoreCase(loadedScene.name))
					IsCritical = false;
			}
		}
	}
}
