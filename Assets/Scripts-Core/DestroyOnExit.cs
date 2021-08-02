using UnityEngine;

public class DestroyOnExit : MonoBehaviour
{
	private void OnTriggerExit2D(Collider2D collision)
	{
		Destroy(collision.gameObject);
	}
}
