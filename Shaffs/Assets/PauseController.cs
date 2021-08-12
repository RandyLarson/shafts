using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    private void OnEnable()
    {
        FadeOut.SetTimeScale(TimeScale.Paused);
    }

    private void OnDisable()
    {
        FadeOut.SetTimeScale(TimeScale.Normal);
    }
}
