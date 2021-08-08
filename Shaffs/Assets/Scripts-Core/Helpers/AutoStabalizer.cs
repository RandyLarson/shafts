using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public class AutoStabalizer
	{
		static  public void StabalizeRotation(Transform transform)
		{
			if (!Mathf.Approximately(0, transform.rotation.z))
			{
				float velocity = 0f;
				float newZ = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, 0, ref velocity, 0.03f);

				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, newZ);
			}
		}


	}
}
