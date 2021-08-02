using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A goal that is successful if a GameObject enters it -- the player reaching a destination.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RegionTrigger : GoalBase
{
	public bool ShowVisualization = false;
	public bool RegionEntered = false;
	public bool SuccessOnPlayerEnter = true;
	public string[] SuccessTags = null;
	public GameObject Visualization;

	public RegionTrigger()
	{
		Name = "Region";
		UnresolvedMessage = "You need to reach the designated area.";
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (RegionEntered)
			return;

		if (SuccessOnPlayerEnter)
		{
			if (GameController.TheController?.Player != null && collision.gameObject == GameController.TheController.Player.gameObject)
				RegionEntered = true;
		}
		else
		{
			RegionEntered = SuccessTags?.Any(tagName => collision.gameObject.CompareTag(tagName)) == true;
		}

		if (RegionEntered)
		{
			OnSuccess();
		}
	}

	
	private void OnDrawGizmos()
	{
		Collider2D Region = GetComponent<Collider2D>();
		if (Region != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(Region.bounds.center, Region.bounds.size);

			Vector2 box2 = new Vector2(Region.bounds.size.x - 2, Region.bounds.size.y - 2);
			Gizmos.DrawWireCube(Region.bounds.center, box2);
		}
	}


	override public GoalStatus GetGoalStatus(out string statusMessage)
	{
		statusMessage = string.Empty;
		if (RegionEntered)
		{
			statusMessage = SuccessMessage;
			return GoalStatus.Successful;
		}
		else
		{
			statusMessage = UnresolvedMessage;
			return GoalStatus.Unresolved;
		}
	}

}
