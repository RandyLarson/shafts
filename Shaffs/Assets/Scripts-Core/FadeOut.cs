using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void LevelIntroComplete();

public class FadeOut : MonoBehaviour
{

    [Tooltip("A delay after StartFade has been signaled.")]
    public float BeginFadeAfter = 5;
    [Tooltip("The time the fade/transition portion will take.")]
    public float FadeOutDuration = 4;
    public bool AutoFade = false;
    public bool DestroyWhenFaded = false;
    public bool DisableWhenFaded = true;
    public GameObject ContinueButton = null;
    public event LevelIntroComplete OnLevelIntroComplete;
    private float FadeOutStart { get; set; } = 0;
    private bool IsFading = false;

    public void Start()
    {
        //var images = GetComponentsInChildren<Image>();
        //var text = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
    }

    public void Initialize()
    {
        SetTimeScale(TimeScale.SuperSlow);
        gameObject.transform.localScale = Vector3.one;

        if (AutoFade)
            StartFade();
        else
            ShowButton(true);
    }

    private void ShowButton(bool showIt)
    {
        ContinueButton.SafeSetActive(showIt);
    }

    public void StartFade()
    {
        ShowButton(false);
        FadeOutStart = Time.time;
        IsFading = true;
    }

    static public void SetTimeScale(TimeScale setTo)
    {
        float newScale = 1;
        switch (setTo)
        {
            case TimeScale.SuperSlow:
                newScale = .05f;
                break;
            case TimeScale.Slow:
                newScale = .20f;
                break;
            case TimeScale.Normal:
                newScale = 1f;
                break;
            case TimeScale.Paused:
                newScale = 0f;
                break;
        }

        Time.timeScale = newScale;
        // Adjust fixed delta time according to timescale
        // The fixed delta time will now be 0.02 frames per real-time second
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void Reset()
    {
        IsFading = false;
        SetTimeScale(TimeScale.Normal);
    }

    void Update()
    {
        if (!IsFading)
            return;

        var beginFadeAfterAdj = BeginFadeAfter * Time.timeScale;
        var fadeDurationAdj = FadeOutDuration * Time.timeScale;
        var endFadeAtAdj = FadeOutStart + beginFadeAfterAdj + fadeDurationAdj;

        if (Time.time - FadeOutStart > beginFadeAfterAdj)
        {
            float scale = Mathf.Lerp(1, 0, (Time.time - (FadeOutStart+ beginFadeAfterAdj)) / fadeDurationAdj);
            gameObject.transform.localScale = Vector3.one * scale;

            if (Time.time > endFadeAtAdj)
            {
                OnLevelIntroComplete?.Invoke();
                Reset();

                if (DisableWhenFaded)
                    gameObject.SafeSetActive(false);
                else if (DestroyWhenFaded)
                    Destroy(this);
            }
        }
        else
        {
            gameObject.transform.localScale = Vector3.one;
        }
    }
}
