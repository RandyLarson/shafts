using UnityEngine;

public class ThrusterBehavior : StateMachineBehaviour
{
	public ThrusterInfo Thrusters;
	public ParticleSystem ThrusterParticles;


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
		base.OnStateEnter(animator, stateInfo, layerIndex);

		Debug.DebugBreak();

		// Wire maneuvering thrusters up to animator
		if (Thrusters == null)
		{
			Thrusters = animator.GetComponent<ThrusterInfo>();
		}

		if (ThrusterParticles == null)
		{
			ThrusterParticles = animator.GetComponentInChildren<ParticleSystem>();
		}

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
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		float currentPower = Mathf.Max(.15f, animator.GetFloat(GameConstants.PowerHash));
		animator.speed = .5f + currentPower * 2;

		if (ThrusterParticles != null)
		{
			var emissionModule = ThrusterParticles.emission.rateOverTime;
			emissionModule.constant = currentPower * 10f;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		SetEnabled(Thrusters?.TopThrusters, false);
		SetEnabled(Thrusters?.BottomThrusters, false);
		SetEnabled(Thrusters?.FrontThrusters, false);
		SetEnabled(Thrusters?.BackThrusters, false);
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that sets up animation IK (inverse kinematics)
	//}
}
