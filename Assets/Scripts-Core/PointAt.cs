using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAt : MonoBehaviour
{
	public GameObject Target;

	void Start()
    {
        
    }

    void Update()
    {
		if (Target != null)
		{
			transform.up = Target.transform.position - transform.position;
		}
        
    }
}
