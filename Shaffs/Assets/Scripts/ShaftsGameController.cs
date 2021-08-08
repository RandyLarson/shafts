using Assets.Scripts.Extensions;
using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets._2D;

public class ShaftsGameController : MonoBehaviour
{
    public GameObject UiTitleScreen;
    public GameObject UiPlayerStatsScreen;
    public GameObject UiAboutGameScreen;
    public GameObject UiGameCreditsScreen;
    public GameObject LevelStartingScreen;
    public GameObject UiGameOverScreen;
    public PlayerStats PlayerStats;
    public GameStats GameStats;

    public float LevelStartFadeoutTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        SwitchToGameMode(GameStats.GameMode);
    }

    private void Initialize()
    {
        GameStats.GameMode = GameMode.StartMenu;
        GameStats.CurrentPlayer = null;
        GameStats.GameController = null;

        var fader = LevelStartingScreen.GetComponent<FadeOut>();
        if (fader)
        {
            fader.FadeOutDuration = LevelStartFadeoutTime;
            fader.OnLevelIntroComplete += OnLevelIntroComplete;
        }
        GameStats.GameController = this;
    }

    private void LateUpdate()
    {
        CheckForGameProgression();
    }

    private void CheckForGameProgression()
    {
        if (GameStats.GameMode == GameMode.Playing)
        {
            if (PlayerStats.Health <= 0)
                SwitchToGameMode(GameMode.GameOver);
        }
    }

    private void OnLevelIntroComplete()
    {
        SwitchToGameMode(GameMode.Playing);
    }

    public void SwitchToAboutUi() => SwitchToGameMode(GameMode.AboutGame);
    public void SwitchToStartMenu() => SwitchToGameMode(GameMode.StartMenu);
    public void SwitchToPauseUi() => SwitchToGameMode(GameMode.Paused);
    public void SwitchToPlayingUi() => SwitchToGameMode(GameMode.Playing);
    public void SwitchToGameCreditsUi() => SwitchToGameMode(GameMode.GameCredits);

    public void SwitchToGameMode(GameMode nextMode)
    {
        GameStats.GameMode = nextMode;
        AdjustUiElements();

        switch (nextMode)
        {
            case GameMode.StartMenu:
                GameStats.CurrentPlayer.SafeDestroy();
                GameStats.CurrentPlayer = null;
                break;
            case GameMode.StartingLevel:
                break;
            case GameMode.Playing:
                break;
            case GameMode.GameOver:
                break;
            case GameMode.Paused:
                break;
            case GameMode.RestartGameImmediate:
                GameStats.CurrentPlayer.SafeDestroy();
                GameStats.CurrentPlayer = null;
                SceneManager.LoadScene(GameStats.StartingLevel.name);
                SwitchToGameMode(GameMode.StartingLevel);
                break;
            case GameMode.AboutGame:
                break;
            case GameMode.GameCredits:
                break;
        }
    }

    private void AdjustUiElements()
    {
        bool uiTitleScreenState = false;
        bool uiLevelStartState = false;
        bool uiGameOverState = false;
        bool uiPlayerStatsState = false;
        bool uiAboutGameState = false;
        bool uiGameCreditsState = false;

        switch (GameStats.GameMode)
        {
            case GameMode.StartMenu:
                uiTitleScreenState = true;
                break;
            case GameMode.Playing:
                uiPlayerStatsState = true;
                break;
            case GameMode.StartingLevel:
                uiLevelStartState = true;
                var fader = LevelStartingScreen.GetComponent<FadeOut>();
                if (fader != null)
                {
                    fader.StartFade();
                }
                break;
            case GameMode.GameOver:
                uiGameOverState = true;
                uiPlayerStatsState = true;
                break;
            case GameMode.Paused:
                uiPlayerStatsState = true;
                break;
            case GameMode.RestartGameImmediate:
                break;
            case GameMode.AboutGame:
                uiAboutGameState = true;
                break;
            case GameMode.GameCredits:
                uiGameCreditsState = true;
                break;
        }

        UiTitleScreen.SafeSetActive(uiTitleScreenState);
        LevelStartingScreen.SafeSetActive(uiLevelStartState);
        UiGameOverScreen.SafeSetActive(uiGameOverState);
        UiPlayerStatsScreen.SafeSetActive(uiPlayerStatsState);
        UiAboutGameScreen.SafeSetActive(uiAboutGameState);
        UiGameCreditsScreen.SafeSetActive(uiGameCreditsState);
    }

    private void OnLevelWasLoaded(int level)
    {
    }

    public void RestartGame()
    {
        SwitchToGameMode(GameMode.RestartGameImmediate);
    }

    public void StartLevel()
    {
        SwitchToGameMode(GameMode.StartingLevel);
    }


    public void GameOver()
    {
        SwitchToGameMode(GameMode.GameOver);
    }

    public void PauseGame()
    {
        SwitchToGameMode(GameMode.Paused);
    }

    public void ResumeGame()
    {
        SwitchToGameMode(GameMode.Playing);
    }

}
