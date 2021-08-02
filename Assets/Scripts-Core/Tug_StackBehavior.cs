using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tug_StackBehavior : StateMachineBehaviour
{
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		// 2 things based on power:
		//  * Vertical Height
		//  * Z-Rotation
		var currentPower = animator.GetFloat(GameConstants.PowerHash);
		float rotation = 0f;
		float scale = .4f;

		if (currentPower > .2f && currentPower <= .6)
		{
			scale = 1f;
			rotation = 10f;
		}
		else if (currentPower > .6)
		{
			scale = 1.5f;
			rotation = 45f;
		}

		if ( animator.transform.parent.transform.rotation.y != 0)
		{
			rotation = -rotation;
		}

		animator.gameObject.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
		animator.gameObject.transform.localScale = new Vector2(1, scale);
	}
}
