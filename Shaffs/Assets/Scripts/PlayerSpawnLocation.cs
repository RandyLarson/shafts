using UnityEngine;

public class PlayerSpawnLocation : MonoBehaviour
{



	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 2);
	}
}
