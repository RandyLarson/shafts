using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverallGameController : MonoBehaviour
{
    public GameStats GameStats;

    void Start()
    {
        Debug.Log("Overall Game controller");
        GameStats.GameMode = GameMode.StartMenu;

        if (GameStats.GameControllerSceneName != null)
            SceneManager.LoadScene(GameStats.GameControllerSceneName);
    }
}
