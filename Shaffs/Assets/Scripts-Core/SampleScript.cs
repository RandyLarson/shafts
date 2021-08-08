using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Facing
{ Down, up, left,right }

public class SampleScript : MonoBehaviour {

	public string Title;

	[Range(0,10)]
	public int Level;
	public Facing OurFacing;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
