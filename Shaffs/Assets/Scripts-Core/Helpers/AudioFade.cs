
using System.Collections;
using UnityEngine;

public static class AudioFadeOut
{

	public static IEnumerator FadeOut(this AudioSource audioSource, float FadeTime)
	{
		float startVolume = audioSource.volume;

		while (audioSource.volume > 0)
		{
			audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

			yield return null;
		}

		audioSource.Stop();
		audioSource.volume = startVolume;
	}

	public static void DoAudioFade(AudioSource audioSource, float fadeTime = 1f)
	{
	}

}