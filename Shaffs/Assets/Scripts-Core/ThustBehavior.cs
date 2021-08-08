using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThustBehavior : ThrustBehaviorBase
{
	private AudioSource EngineAudio;
	public ThrusterInfo Thrusters;


	private void SetEnabled(GameObject whichObject, bool setTo)
	{
		if (whichObject != null)
		{
			whichObject.SetActive(setTo);
		}
	}

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EngineAudio = animator.gameObject.GetComponent<AudioSource>();
		//base.OnStateEnter(animator, stateInfo, layerIndex);

		// Wire maneuvering thrusters up to animator
		if (Thrusters == null)
		{
			Thrusters = animator.GetComponent<ThrusterInfo>();
		}

	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//base.OnStateUpdate(animator, stateInfo, layerIndex);

		float currentPower = Mathf.Max(.15f, animator.GetFloat(GameConstants.PowerHash));
		float sqrVelocity = animator.GetFloat(GameConstants.SqrVelocityHash);

		float velocityFactor = Mathf.Min(1, sqrVelocity / 1000 * .5f);
		animator.speed = .5f + currentPower + sqrVelocity + velocityFactor;

		int hThrusterStatus = animator.GetInteger(GameConstants.HorzThrusterHash);
		int vThrusterStatus = animator.GetInteger(GameConstants.VertThrusterHash);

		bool bottomStatus = vThrusterStatus == 1;
		bool topStatus = vThrusterStatus == 2;
		bool frontStatus = hThrusterStatus == 2;
		bool rearStatus = hThrusterStatus == 1;

		SetEnabled(Thrusters?.TopThrusters, topStatus);
		SetEnabled(Thrusters?.BottomThrusters, bottomStatus);
		SetEnabled(Thrusters?.FrontThrusters, frontStatus);
		SetEnabled(Thrusters?.BackThrusters, rearStatus);

		if (EngineAudio != null)
		{
			if (!EngineAudio.isPlaying)
				EngineAudio.Play();

			EngineAudio.volume = Audio.volume;
			EngineAudio.pitch = Audio.pitch + velocityFactor;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//base.OnStateExit(animator, stateInfo, layerIndex);

		SetEnabled(Thrusters?.TopThrusters, false);
		SetEnabled(Thrusters?.BottomThrusters, false);
		SetEnabled(Thrusters?.FrontThrusters, false);
		SetEnabled(Thrusters?.BackThrusters, false);
	}


}
