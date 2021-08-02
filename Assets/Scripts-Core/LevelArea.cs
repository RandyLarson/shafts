using UnityEngine;

public class LevelArea : MonoBehaviour
{
	public Rect LevelBounds;
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(LevelBounds.center, LevelBounds.size);

		Vector2 box2 = new Vector2(LevelBounds.size.x - 2, LevelBounds.size.y - 2);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(LevelBounds.center, box2);
	}
}
