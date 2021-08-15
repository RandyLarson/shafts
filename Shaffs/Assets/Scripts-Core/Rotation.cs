using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
	X,
	Y,
	Z
}

public class Rotation : MonoBehaviour {

	public float RotationSpeed = 10f;
	public Axis RotationAxis = Axis.Z;
	public bool RotateWrtVelocity = false;

	private Vector3 RotationVector = Vector3.back;
	private Rigidbody2D OurRb { get; set; }

	private void Start()
	{
		OurRb = GetComponent<Rigidbody2D>();

		switch (RotationAxis)
		{
			case Axis.X:
				RotationVector = Vector3.left;
				break;
			case Axis.Y:
				RotationVector = Vector3.up;
				break;
			case Axis.Z:
				RotationVector = Vector3.back;
				break;
		}

	}

	void Update ()
	{
		float workingSpeed = RotationSpeed;

		if ( RotateWrtVelocity && OurRb != null)
        {
			workingSpeed = OurRb.velocity.magnitude * Mathf.Sign(OurRb.velocity.x);
        }

		transform.Rotate(RotationVector, workingSpeed*Time.deltaTime);
	}
}
