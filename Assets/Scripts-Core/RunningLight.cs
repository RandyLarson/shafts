using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class RunningLight : MonoBehaviour
{
    public float BlinkDuration = 1f;
    private float NextTransitionTime = 0f;
    private float TargetIntensity = 0;
    private float InitialIntensity = 0;
    public Light2D TheLight = null;

    private float i = 0f;
    private void Start()
    {
        TheLight = GetComponent<Light2D>();
        if (TheLight != null)
        {
            InitialIntensity = TheLight.intensity;
        }
    }
    void Update()
    {
        if (TheLight != null)
        {
            //float progression = Time.time / NextTransitionTime;
            i += 1f * Time.deltaTime;
            TheLight.intensity = Mathf.Lerp(InitialIntensity, TargetIntensity, i);
            if (i >= 1)
            {
                float tmp = InitialIntensity;
                InitialIntensity = TargetIntensity;
                TargetIntensity = tmp;

                NextTransitionTime = Time.time + BlinkDuration;
                i = 0f;
            }
        }
    }
}
