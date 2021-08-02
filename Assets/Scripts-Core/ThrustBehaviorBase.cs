using UnityEngine;

public class ThrustBehaviorBase : StateMachineBehaviour
{
	public static int nextId = 0;
	public int ourId = nextId++;
	public Sound Audio;

	private void Awake()
	{
	}
	private void OnDestroy()
	{
	}

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Audio?.Initialize(animator.gameObject);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Audio != null)
		{
			if (Audio.audioSource.isPlaying == false)
				Audio.audioSource.Play();

			Audio.audioSource.volume = Audio.volume;
			Audio.audioSource.pitch = Audio.pitch;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Audio?.audioSource.Stop();
	}
}
