using Assets.Scripts.Extensions;
using Assets.Scripts.Player;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets._2D;

public class ShaftsGameController : MonoBehaviour
{
    public GameObject UiTitleScreen;
    public GameObject UiPauseScreen;
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
        Assets.Scripts.Helpers.MoveToward.Initialize();

        GameStats.GameMode = GameMode.StartMenu;
        GameStats.DestroyPlayer();
        GameStats.GameController = null;
        GameStats.Fortune = string.Empty;

        PlayerStats.CurrentWeaponName = "SV";

        var fader = LevelStartingScreen.GetComponent<FadeOut>();
        if (fader)
        {
            fader.FadeOutDuration = LevelStartFadeoutTime;
            fader.OnLevelIntroComplete += OnLevelIntroComplete;
        }
        GameStats.GameController = this;
    }

    private void FixedUpdate()
    {
        var kbd = Keyboard.current;
        if (kbd != null)
        {
            if (kbd.escapeKey.isPressed)
            {
                SwitchToPauseUi();
            }
        }
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
                GameStats.DestroyPlayer();
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
                LoadOrInitializeScene(false, true, false);
                SwitchToGameMode(GameMode.StartingLevel);
                break;
            case GameMode.AboutGame:
                break;
            case GameMode.GameCredits:
                break;
            case GameMode.RestartLevel:
                LoadOrInitializeScene(false, false, true);
                SwitchToGameMode(GameMode.StartingLevel);
                break;
            case GameMode.StartTutorial:
                LoadOrInitializeScene(true, false, false);
                SwitchToGameMode(GameMode.StartingLevel);
                break;
        }
    }

    public enum SceneKind
    {
        tutorial,
        startingLevel,
        currentScene
    }


    private void LoadOrInitializeScene(bool loadTutorial, bool loadStartingScene, bool reloadCurrentScene)
    {
        if (Application.isEditor && SceneManager.sceneCount > 1)
            loadStartingScene = false;

        GameStats.DestroyPlayer();

        ShaftsLevelController currentLevel = null;
        var levelControllers = GameObject.FindGameObjectsWithTag("LevelController");
        for (int i = 0; i < levelControllers.Length; i++)
        {
            currentLevel = levelControllers[i].GetComponent<ShaftsLevelController>();

            if (currentLevel != null)
            {
                break;
            }
        }

        if (reloadCurrentScene && currentLevel != null)
        {
            SceneManager.LoadScene(currentLevel.gameObject.scene.name);
        }
        else if (loadStartingScene)
        {
            SceneManager.LoadScene(GameStats.StartingLevelName);
        }
        else if (loadTutorial)
        {
            SceneManager.LoadScene(GameStats.StartingTutorialLevel);
        }
        else if ( currentLevel != null )
        {
            currentLevel.InitializeLevel();
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
        bool uiPauseState = false;

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
                    fader.Initialize();
                break;
            case GameMode.GameOver:
                uiGameOverState = true;
                uiPlayerStatsState = true;
                break;
            case GameMode.Paused:
                uiPlayerStatsState = true;
                uiPauseState = true;
                break;
            case GameMode.RestartGameImmediate:
                break;
            case GameMode.AboutGame:
                uiAboutGameState = true;
                break;
            case GameMode.GameCredits:
                uiGameCreditsState = true;
                break;
            case GameMode.RestartLevel:
                break;
        }

        UiTitleScreen.SafeSetActive(uiTitleScreenState);
        LevelStartingScreen.SafeSetActive(uiLevelStartState);
        UiGameOverScreen.SafeSetActive(uiGameOverState);
        UiPlayerStatsScreen.SafeSetActive(uiPlayerStatsState);
        UiAboutGameScreen.SafeSetActive(uiAboutGameState);
        UiGameCreditsScreen.SafeSetActive(uiGameCreditsState);
        UiPauseScreen.SafeSetActive(uiPauseState);
    }

    private void OnLevelWasLoaded(int level)
    {
    }

    public void RestartGame()
    {
        SwitchToGameMode(GameMode.RestartGameImmediate);
    }

    public void TransitionToStartMenu()
    {
        SwitchToGameMode(GameMode.StartMenu);
    }


    public void StartTutorial()
    {
        SwitchToGameMode(GameMode.StartTutorial);
    }

    public void RestartLevel()
    {
        SwitchToGameMode(GameMode.RestartLevel);
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
