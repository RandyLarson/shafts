using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public bool DestroyAfterEntry = true;
	public bool DestroyParent = true;
	public string TargetTag = "Player";
	protected int fadeOutId = Animator.StringToHash("FadeOut");
	protected Animator ShieldAnimator = null;

	private void Start()
	{
		ShieldAnimator = GetComponent<Animator>();

	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ( collision.gameObject.CompareTag(TargetTag))
		{
			if (DestroyAfterEntry)
			{
				ShieldAnimator.SetTrigger(fadeOutId);
			}
		}
	}

	private void TriggerDestroy()
	{
		if (DestroyAfterEntry)
		{
			if (DestroyParent)
				Destroy(transform.parent.gameObject);
			else
				Destroy(gameObject);
		}
	}
}
