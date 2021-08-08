using Assets.Scripts.Extensions;
using UnityEngine;

public static class SoundExtensions
{
	public static void Initialize(this Sound whichSound, GameObject parent)
	{
		if (parent == null || whichSound == null)
			return;

		if (whichSound.audioSource == null)
		{
			//if (!parent.GetComponent<AudioSource>(out var audioSource))
			AudioSource	audioSource = parent.AddComponent<AudioSource>();
			whichSound.audioSource = audioSource;
		}

		whichSound.audioSource.clip = whichSound.clip;
		whichSound.audioSource.volume = whichSound.volume;
		whichSound.audioSource.loop = whichSound.loop;
	}

	public static void Initialize(this Sound whichSound, MonoBehaviour parent)
	{
		if (parent == null || whichSound == null)
			return;

		whichSound.Initialize(parent.gameObject);
	}

}
