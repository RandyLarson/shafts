using System;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Extensions;

[Serializable]
public class Sound
{
	public string name;
	public AudioClip clip;
	public bool loop;

	[Range(0f, 1f)]
	public float volume = .5f;
	[Range(.1f, 3f)]
	public float pitch = 1;

	[HideInInspector]
	public AudioSource audioSource;
}

public class AudioManager : MonoBehaviour
{
	public static AudioManager TheAudioManager { get; set; }

	public Sound[] Sounds;

	public void Start()
	{
	}

	void Awake()
	{
		if (TheAudioManager == null)
		{
			TheAudioManager = this;
		}
		else
		{
			Destroy(this);
		}

		foreach (var snd in Sounds)
		{
			if ( snd.clip != null )
				snd.Initialize(this);
		}
	}

	public AudioSource Get(string named)
	{
		var found = Sounds.FirstOrDefault(snd => snd.name.EqualsIgnoreCase(named));
		if ( found != null )
			return found.audioSource;
		return null;
	}

	public void Play(string named)
	{
		var found = Sounds.FirstOrDefault(snd => snd.name.EqualsIgnoreCase(named));
		if ( found != null && found.audioSource != null )
		{
			found.audioSource?.Play();
		}
		else
		{
 			Debug.LogWarning("Unable to find sound `" + named.LogValue() + "`.");
		}

	}

}
