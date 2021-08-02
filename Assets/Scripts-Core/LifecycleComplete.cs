using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifecycleComplete : MonoBehaviour
{
    public float BornOn;
    public float DurationUntilWin = 10;
    public bool IsDurationUp { get => Time.time - BornOn > DurationUntilWin; }
    // Start is called before the first frame update
    void Start()
    {
        BornOn = Time.time;        
    }
}
