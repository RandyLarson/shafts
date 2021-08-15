using Assets.Scripts.Extensions;
using Assets.Scripts.Player;
using UnityEditor;
using UnityEngine;


public enum GameMode
{
    StartTutorial,
    RestartGameImmediate,
    RestartLevel,
    StartMenu,
    StartingLevel,
    Playing,
    GameOver,
    Paused,
    AboutGame,
    GameCredits
}

[CreateAssetMenu(fileName = "GameStats", menuName = "ScriptableObjects/Shaffs/GameData", order = 1)]
public class GameStats : ScriptableObject
{
    public GameMode GameMode = GameMode.StartMenu;
    //    public string GameControllerScene = "GameControl";
    //    public string StartingLevel;
    public string GameControllerSceneName;
    public string StartingLevelName;

    public string CurrentLevel;

    public GameObject PlayerPrototype;

    // Runtime items:
    public ShaftsGameController GameController;
    public ShaftPlayerController CurrentPlayer;

    public string Fortune;

    public string StartingTutorialLevel;

    public void DestroyPlayer()
    {
        CurrentPlayer.SafeDestroy();
        CurrentPlayer = null;
    }
}

