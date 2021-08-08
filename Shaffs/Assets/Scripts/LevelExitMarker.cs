using Assets.Scripts.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitMarker : MonoBehaviour
{
    public SceneAsset NextLevel;
    public float DelayUntilLoad = 1;
    public GameObject SpawnOnEntry;
    private float LoadNextLevelTime = 0;

    private void LateUpdate()
    {
        if (NextLevel == null)
            return;

        if (LoadNextLevelTime > 0 && Time.time > LoadNextLevelTime)
        {
            SceneManager.LoadScene(NextLevel.name, LoadSceneMode.Single);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (SpawnOnEntry != null)
            {
                SpawnOnEntry.SafeInstantiate<GameObject>(transform.position, out _, DelayUntilLoad);
            }

            LoadNextLevelTime = Time.time + DelayUntilLoad;
        }
    }

}
