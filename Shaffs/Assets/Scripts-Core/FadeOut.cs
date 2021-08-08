using Assets.Scripts.Extensions;
using UnityEngine;

public delegate void LevelIntroComplete();

public class FadeOut : MonoBehaviour
{

    [Tooltip("A delay after StartFade has been signaled.")]
    public float BeginFadeAfter = 5;
    [Tooltip("The time the fade/transition portion will take.")]
    public float FadeOutDuration = 4;
    public bool AutoFade = true;
    public bool DestroyWhenFaded = false;
    public bool DisableWhenFaded = true;

    public event LevelIntroComplete OnLevelIntroComplete;

    private float FadeOutStart { get; set; } = 0;
    private bool IsFading = false;

    public void StartFade()
    {
        FadeOutStart = Time.time;
        IsFading = true;
    }

    private void Reset()
    {
        IsFading = false;
    }

    void Update()
    {
        if (!IsFading)
            return;

        if (Time.time - FadeOutStart > BeginFadeAfter)
        {
            float scale = Mathf.Lerp(1, 0, (Time.time - (FadeOutStart+BeginFadeAfter)) / FadeOutDuration);
            gameObject.transform.localScale = Vector3.one * scale;
            gameObject.transform.rotation = Quaternion.Euler(0, 360 * scale, 0);

            if (Time.time > FadeOutStart + BeginFadeAfter + FadeOutDuration)
            {
                OnLevelIntroComplete?.Invoke();
                Reset();

                if (DisableWhenFaded)
                    gameObject.SafeSetActive(false);
                else if (DestroyWhenFaded)
                    Destroy(this);
            }
        }
    }
}
