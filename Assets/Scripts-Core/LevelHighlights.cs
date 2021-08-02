using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Highlight
{
	public GameObject Item;
	public string Annotation;
}

public class LevelHighlights : MonoBehaviour
{
	public Highlight[] ItemsToFlyTo;
	public float PauseTime = 1f;

	// Start is called before the first frame update
    void Start()
    {
        
    }


}
