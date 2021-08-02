using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Database.Models;
using Assets.Scripts.Extensions;
using Assets.Scripts.MissionPlanning;
using Assets.Scripts.SavedGame;
using Assets.Scripts.UI;
using Milkman;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets._2D;

public enum TimeScale
{
    SuperSlow,
    Slow,
    Normal,
    Paused
}


public class GameController : MonoBehaviour
{
    public string StartingLevel = "Level-1.0+";

    public string StartMenuLevel = GameConstants.LevelStart;

    public int BaseSwimHealth = 10;
    public int BaseSwimHealthGoal = 30;
    public int SwimHealthDeltaPerLevel = 5;
    public int SwimHealthGoalMax = 150;
    public float BaseTimeToLive = 60;
    public float MinTimeToLive = 10;
    public float TimeToLiveDeltaPerLevel = 10;
    public int CouplingBase = 5;
    public int CouplingDeltaPerLevel = 2;

    public Camera2DFollow MiniMapCamera;
    public MissionPlannerConfig BaseMissionLimits;
    public PlayerInputStyle PlayerInputMode = PlayerInputStyle.TouchScreen;

    public DamageVisualization SmokeEffect;
    public DiagnosticVisualizer DiagnosticVisualizer;
    public bool DiagnosticPathFindingVisualize = false;

    public GameObject FlyingSpawnLocation = null;

    public int CurrentLevel = 0;


    internal void SwitchPlayer(PlayerShip nextForm)
    {
        ConfigureForPlayer(nextForm, true);
    }

    public GameObject DiagnosticHitMarker;
    /// <summary>
    /// The active player object.
    /// </summary>
    public PlayerShip Player
    {
        get => InnerPlayer;
        set => InnerPlayer = value;
    }

    private PlayerShip BackedUpPlayerShip { get; set; }

    internal Transform GetNextSpawnLocation()
    {
        return FlyingSpawnLocation.transform;
    }

    /// <summary>
    /// The active boundary for the current play area.
    /// </summary>
    public Rect LevelBoundary { get; set; }

    /// <summary>
    /// Global access to this game controller.
    /// </summary>
    public static GameController TheController { get; protected set; }
    public static PlayerShip ThePlayer { get => TheController?.Player; }
    public static void PlayerDestroyed()
    {
        //TheController.Player = null;
    }
    public static DiagnosticController DiagnosticsController { get => TheController != null ? TheController.Diagnostics : null; }
    public DiagnosticController Diagnostics;

    /// <summary>
    /// Helper to get to the game-play stats for the level - people saved, freight delivered, etc.
    /// </summary>
    public static GameLevelStats LevelStatistics { get => TheController?.LevelStats; }

    /// <summary>
    /// Keeping the tally of progress and activity for the current level.
    /// </summary>
    private GameLevelStats LevelStats { get; set; } = new GameLevelStats();

    /// <summary>
    /// Access to the Game's database - persistence for player information.
    /// </summary>
    DatabaseController DbController { get; set; }

    /// <summary>
    /// Phases can be things like Play, Preparation, GameOver
    /// </summary>
    public GameState GamePhase { get; private set; } = GameState.StartMenu;
    public bool RestorePlayerShipFromSave { get; private set; }

    public float MinPatrolAltitude = 100;
    public float MinCivilianAltitude = 40;

    public float AutoForceDragMultiplier = 3.1f;


    public event Action<string, float> OnGameEvent;

    /// <summary>
    /// Invokes the methods bound to the GameEvent.
    /// </summary>
    /// <param name="named"></param>
    protected virtual void SendGameEvent(string named, float value)
    {
        OnGameEvent?.Invoke(named, value);
    }

    /// <summary>
    /// Invokes the methods bound to the GameEvent.
    /// </summary>
    /// <param name="eventName"></param>
    public void SignalGameEvent(string eventName, float value)
    {
        Debug.Log($"Signaling event: {eventName}, {value}");
        SendGameEvent(eventName, value);
    }

    public void SignalGameEvent(string eventName)
    {
        Debug.Log($"Signaling event: {eventName}, (no-value, 0 implied)");
        SendGameEvent(eventName, 0);
    }

    private void Awake()
    {
        UnityEngine.Random.InitState((int)(DateTime.Now.Ticks & 0xFFFF));
        GameConstants.Init();

        TheController = this;
        DontDestroyOnLoad(this);

        LoadGameDatabase();
        LoadPlayerGameLevelOptions();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }


    /// <summary>
    /// Load (or create) the game's database for persistence.
    /// </summary>
    private void LoadGameDatabase(bool destroyIfLoaded = false)
    {
        if (DbController == null || destroyIfLoaded)
        {
            DbController = new DatabaseController();
            DbController.Load();
        }
    }

    private void StoreGameDatabase()
    {
        DbController?.Save();
    }

    void Start()
    {
        Assets.Scripts.Helpers.MoveToward.Initialize();
        GameUIController.Controller.HideTransitionalScreens();
        LoadStartSceneIfNeeded();
    }

    private void OnEnable()
    {
        TheController = this;
    }

    private void OnDestroy()
    {
        TheController = null;
    }


    void Update()
    {
        if (TheController == null)
            TheController = this;


    }

    private void LateUpdate()
    {
        // UI Visuals
        if (Player != null)
        {
            Player.TimeToLive -= Time.deltaTime;
            if (GameUIController.Controller != null)
            {
                var mayFly = Player.GetComponent<Mayfly>();
                if (mayFly != null)
                    GameUIController.Controller.PlayerHealth.text = "Coupling ~ " + mayFly.CouplingCount + "/" + mayFly.MaxCouplingCount;
                else
                {
                    var evolve = Player.GetComponent<Evolve>();
                    GameUIController.Controller.PlayerHealth.text = "Health ~ " + (math.max(0, (int)Player.Health.HP)) + "/" + evolve.HealthAmtToEvolve;
                }

                GameUIController.Controller.PlayerTime.text = ((int)Player.TimeToLive) + " ~ Time";
                GameUIController.Controller.Level.text = "Level " + CurrentLevel.ToString();
            }
        }

        CheckForGlobalGameControls();
        CheckForGameProgressions();

        if (GameUIController.Controller?.Diagnostics)
        {
            MissionTracker.DrawTargetMarkers();
            MissionTracker.GatherDiagnostics();
        }

    }

    private void CheckForGlobalGameControls()
    {
        // Mid-Game Menu/Pause
        bool midGameMenu = false; //  Input.GetButtonDown("ActivateMenu");

        if (midGameMenu)
        {
            if (GamePhase == GameState.MidGameMenu)
                EnterGamePhase(GameState.Play);
            else if (GamePhase == GameState.Play)
                EnterGamePhase(GameState.MidGameMenu);
        }
    }

    private void LoadStartSceneIfNeeded()
    {
        // Will be 1 if only the ControllerScene is loaded.
        // This is the expected state when launched standalone.
        if (SceneManager.sceneCount == 1)
        {
            LoadStartMenuLevel();
        }
    }

    private void SceneManager_sceneLoaded(Scene theScene, LoadSceneMode loadMode)
    {
        EnterGamePhase(GameState.PlayPreparation, $"Level loaded: {theScene.name}");
        Debug.Log("Scene Loaded - " + theScene.name + " Mode: " + loadMode);
        bool res = SceneManager.SetActiveScene(theScene);

        if (loadMode == LoadSceneMode.Single)
        {
            MissionTracker.ResetTargetList();
            MessageContoller.Clear();
        }

        WireUpGameComponents();

        if (GamePhase == GameState.PlayPreparation && Player != null)
        {
            EnterGamePhase(GameState.Play, "Scene loaded: " + theScene.name + " Mode: " + loadMode);
        }
    }

    private void SceneManager_activeSceneChanged(Scene orgScene, Scene nextScene)
    {
        Debug.Log("Active scene changed- " + orgScene.name + " to: " + nextScene.name);
    }

    private void SceneManager_sceneUnloaded(Scene theScene)
    {
        Debug.Log("Scene unloaded: " + theScene.name);
    }

    private void WireUpGameComponents()
    {
        // Can be overridden as we walk through the levels, they will set during wire-up.
        if (BaseMissionLimits != null)
            MissionTracker.PlannerConfig = BaseMissionLimits;

        // Walk through each loaded scene and collect mandatory goals
        // and other items that need wiring into the overall game.
        string levelName = string.Empty;

        // Sort the active scenes by level and index
        // so they are layered correctly.
        var orderedLevel = Enumerable.Range(0, SceneManager.sceneCount)
            .Select(idx => SceneManager.GetSceneAt(idx))
            .Select(theScene => (theScene, traits: ParseSceneName(theScene.name)))
            .Where(sceneInfo => sceneInfo.traits.HasValue)
            .Where(sceneInfo => sceneInfo.theScene.isLoaded)
            .OrderBy(sceneInfo => sceneInfo.traits.Value.level * 100 + sceneInfo.traits.Value.scene)
            .ToArray();

        Rect ultimateLevelBoundary = Rect.zero;
        PlayerShip ultimatePlayerShip = null;
        bool forceUseOfPlayer = false;
        LevelAnnotation introLevelDetails = null;
        EndLevelAnnotation endLevelDetails = null;
        StatisticsUIConfig statisticsUIConfig = null;

        for (int i = 0; i < orderedLevel.Length; i++)
        {
            var sceneInfo = orderedLevel[i];
            try
            {
                // A scene may be null if it is loaded into the designer, but has not been "loaded" at start-play yet.
                if (sceneInfo.theScene == null)
                    continue;

                levelName = sceneInfo.theScene.name;
                Debug.Log(string.Format("Wiring scene name: {0}", levelName));

                // Locate the LevelController
                var rootObjects = sceneInfo.theScene.GetRootGameObjects();
                foreach (var go in rootObjects)
                {
                    if (go.GetComponent(out LevelController levelController) && levelController != null)
                    {

                        // We want to collect the latest variant of the level boundary. Then, when all 
                        // scenes have been processed, apply the boundary and update the camera. I think doing so
                        // for each level was causing some jitter in the camera.
                        if (levelController.LevelBounds != Rect.zero)
                            ultimateLevelBoundary = levelController.LevelBounds;

                        if (levelController.MinimumPatrolAltitude != 0)
                            MinPatrolAltitude = levelController.MinimumPatrolAltitude;

                        if (levelController.Player != null)
                        {
                            ultimatePlayerShip = levelController.Player;
                            forceUseOfPlayer = levelController.UsePlayerAlways;
                        }

                        if (levelController.LevelDescription?.HasContent == true)
                            introLevelDetails = levelController.LevelDescription;

                        if (levelController.EndLevelDescription?.HasContent == true)
                            endLevelDetails = levelController.EndLevelDescription;

                        if (levelController.StatisticsUIConfig != null)
                            statisticsUIConfig = levelController.StatisticsUIConfig;

                        levelController.WireUpLevel();

                        if (levelController.IsStartScene)
                            ShowStartMenu();

                        // We are only looking for the level controller.
                        break;
                    }
                }
            }
            catch (Exception caught)
            {
                Debug.LogError($"Exception wiring scenes. Msg: `{caught.Message}`.");
            }
        }

        // Apply the effective boundary - the last one if processing multiple game scenes
        LevelBoundary = ultimateLevelBoundary;

        // Restore the player's ship from saved data if this is the initial/restore load.
        if (RestorePlayerShipFromSave)
        {
            ShipModel savedShip = GetCurrentPlayerShipModel(false);
            if (savedShip != null)
            {
                PlayerShip restoredShip = RestorePlayerShip(savedShip, ultimatePlayerShip?.transform);
                ultimatePlayerShip.SafeDestroy();
                ultimatePlayerShip = restoredShip;
                forceUseOfPlayer = true;
            }
            RestorePlayerShipFromSave = false;
        }

        ConfigureForPlayer(ultimatePlayerShip, forceUseOfPlayer);

        GameUIController.Controller.LevelIntroController.Clear();
        GameUIController.Controller.LevelOutroController.Clear();

        if (introLevelDetails?.HasContent == true)
        {
            GameUIController.Controller.LevelIntroController.Activate(introLevelDetails, GameState.LevelIntro);
        }

        if (endLevelDetails?.HasContent == true)
        {
            GameUIController.Controller.LevelOutroController.Details = endLevelDetails;
        }

        if (Player == null)
        {
            Debug.LogError("No player after wiring levels, error.");
        }


        // A scene may have set up a timer goal as part of its wiring. 
        // Find it within the list of active goals an display the timer visual if desired.
        //
        //if ( GameUIController.Controller.LevelTimer != null )
        //{
        //	TimeGoal levelTimer = GameUIController.Controller.LevelTimer;
        //	if (levelTimer.Duration > 0f && levelTimer.MandatoryForSuccess)
        //		AddLevelGoal(levelTimer);
        //
        //	levelTimer.gameObject.SetActive(levelTimer.Duration > 0f);
        //}

        if (LevelStats == null)
            LevelStats = new GameLevelStats();

        // levelName will contain the last level processed (if more than 1) and will represent
        // the level we want to collect stats for. Reset and start collecting for this level.

        LevelStats.Clear();
        LevelStats.LevelName = levelName;
        LevelStats.TimeStarted = Time.time;
        LevelStats.LevelAttempt = 1 + GetNumberOfAttemptsForLevel(levelName);

        // Remember this as the farthest level in, unless we are at the start menu.
        if (!string.IsNullOrWhiteSpace(levelName) && levelName != StartMenuLevel)
        {
            GetCurrentPlayerModel().CurrentLevelName = levelName;
            StoreGameDatabase();
        }

        if (GameUIController.Controller.StatsController != null)
        {
            GameUIController.Controller.StatsController.SetLevelStatistics(LevelStats);

            if (statisticsUIConfig != null)
            {
                GameUIController.Controller.StatsController.gameObject.SetActive(statisticsUIConfig.ShowStatsPanel);
                GameUIController.Controller.StatsController.StatisticsUIConfig = statisticsUIConfig;
            }

            GameUIController.Controller.StatsController.TriggerUpdates();
        }

        Debug.Log(string.Format("Player found: {0}", Player == null ? "no" : "yes"));
        Debug.Log(string.Format("Player health found: {0}", Player?.Health == null ? "no" : "yes"));
    }

    public void ActivateLevelInterlude(LevelAnnotation details)
    {
        GameUIController.Controller.LevelIntroController.Activate(details, GameState.LevelInterlude);
    }

    public void LevelDetailsActive(GameState gameState)
    {
        EnterGamePhase(gameState);
    }

    public void LevelDetailsDismissed(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.LevelIntro:
            case GameState.LevelInterlude:
                EnterGamePhase(GameState.Play);
                break;
        }
    }

    void ConfigurePlayerInput(PlayerInputStyle wishedStyle)
    {
        if (wishedStyle == PlayerInputStyle.TouchScreen && Input.touchSupported == false)
            wishedStyle = PlayerInputStyle.Keyboard;

        PlayerInputMode = wishedStyle;

        switch (PlayerInputMode)
        {
            case PlayerInputStyle.TouchScreen:
                //GameUIController.Controller.TouchScreenUIVisible = true;
                Cursor.visible = false;
                break;
            case PlayerInputStyle.Keyboard:
            case PlayerInputStyle.GamePad:
                //GameUIController.Controller.TouchScreenUIVisible = false;
                Cursor.visible = true;
                break;
        }
    }

    private void BackupPlayerShip()
    {
        if (BackedUpPlayerShip != null)
            DestroyImmediate(BackedUpPlayerShip);

        BackedUpPlayerShip = Instantiate(Player, Player.transform.position, Player.transform.rotation);
        BackedUpPlayerShip.transform.SetParent(null, true);
        BackedUpPlayerShip.gameObject.SetActive(false);
    }

    private void RestoreBackedUpPlayerShip()
    {
        if (BackedUpPlayerShip == null)
        {
            Debug.LogError("No backed up player ship to restore");
            return;
        }

        PlayerShip tmpHolder = BackedUpPlayerShip;
        BackedUpPlayerShip = null;
        tmpHolder.gameObject.SetActive(true);
        ConfigureForPlayer(tmpHolder, true);
    }


    private void ConfigureForPlayer(PlayerShip nextPlayerShip, bool forcePlayerUse)
    {
        if (nextPlayerShip != null && Player != nextPlayerShip)
        {
            if (forcePlayerUse || Player == null)
            {
                // Take a reference to the player and clone it so we can restore the player upon destruction
                // and reloading scene.
                Player.SafeDestroy();
                Player = nextPlayerShip;
                DontDestroyOnLoad(Player);
            }
            else
            {
                // There is already an active player ship that we'll use. 
                // Dispose of this one.
                Destroy(nextPlayerShip.gameObject);
            }
        }

        if (Player == null)
        {
            //if (BackupPlayerShip != null)
            //{
            //	RestorePlayerAndContinue
            //	RestoreBackedUpPlayerShip();
            //}
            //else
            {
                return;
            }
        }

        //GameUIController.Controller.WirePlayerHealthVisualizer(Player.Health, Player.ShieldHealth);

        //Player.PlayerBoundary = levelBounds;
        // Wire up the joystick and other UI controls
        //PlayerController.playerJoystick = GameUIController.Controller.FlightControl;
        //PlayerController.weaponsControl = GameUIController.Controller.WeaponsControl;
        //PlayerController.freightControl = GameUIController.Controller.FreightControl;

        Player.TimeToLive = math.max(MinTimeToLive, BaseTimeToLive - CurrentLevel * TimeToLiveDeltaPerLevel);
        Camera mainCamera = GameUIController.Controller.MainCamera;
        if (mainCamera != null)
        {
            var cameraFollow = mainCamera.GetComponent<Camera2DFollow>();
            cameraFollow.ConfigureViewBounds(LevelBoundary, 10f);
            cameraFollow.ToFollow = Player.transform;
        }

        GameUIController.Controller.ConfigureMiniMap(Player.gameObject, LevelBoundary);

        BackupPlayerShip();
    }

    GoalStatus? OverrideGoalStatus = GoalStatus.Unresolved;
    private PlayerShip InnerPlayer;

    public void ForceGoalsToSuccess(string setTo)
    {
        if (Enum.TryParse<GoalStatus>(setTo, out var asStatus))
            OverrideGoalStatus = asStatus;
    }

    public void PlayerDestruction()
    {
        if (Player == null)
            return;

        GameObject.Destroy(Player);
        Player = null;

        EndGame("Maybe you should have weaved instead of bobbed.", false);
        SetTimeScale(TimeScale.Slow);
    }

    private void CheckForGameProgressions()
    {
        DiagnosticController.Add($"GamePhase: {GamePhase} ");
        DiagnosticController.Add($"Scene: {SceneManager.GetActiveScene().name}");

        if (GamePhase != GameState.Play)
        {
            if (GamePhase == GameState.LevelIntro ||
                GamePhase == GameState.LevelInterlude)
            {
                SetTimeScale(TimeScale.SuperSlow);
            }

            GoalManager.GatherGoalDiagnostics();
            return;
        }

        // Player destruction
        if (Player != null && (Player.TimeToLive <= 0 || Player.Health.HP <= 0))
        {
            Player.Health.VisualzeDeath();

            string message = ":(";

            if (Player.TimeToLive <= 0)
                message = "Life is short. You've run out of time";
            else
                message = "The web of life has consumed you.";

            GameObject.Destroy(Player.gameObject);
            Player = null;

            EndGame(message, false);
            SetTimeScale(TimeScale.Slow);
        }

        if (Player == null)
            return;

        var finalForm = Player.GetComponent<LifecycleComplete>();
        if (finalForm != null)
        {

            if (finalForm.IsDurationUp)
            {

                ProgressToNextScene(false);
            }
        }
#if false

        // Level goals
        var overallStatus = OverrideGoalStatus.HasValue ? (status: OverrideGoalStatus.Value, reason: "Overridden") : GoalManager.OverallStatus;
        OverrideGoalStatus = null;

        switch (overallStatus.status)
        {
            case GoalStatus.Failed:
                EndGame(overallStatus.reason, false);
                break;

            case GoalStatus.Successful:
                ProgressToNextScene(false);
                break;

            default:
                break;
        } 
#endif
    }

    public void EnterGamePhase(GameState nextPhase, string reason = null)
    {
        Debug.Log(string.Format("Changing Game Phase: {0}=>{1}, reason: {2}", GamePhase, nextPhase, reason.LogValue()));

        GamePhase = nextPhase;
        if (GamePhase == GameState.Play || GamePhase == GameState.StartMenu)
        {
            SetTimeScale(TimeScale.Normal);
        }

        GameUIController.Controller.EnterGamePhase(GamePhase, reason);
    }

    IEnumerator LoadScene(string sceneName, LoadSceneMode loadMode)
    {
        if (loadMode == LoadSceneMode.Single)
        {
            GoalManager.ResetGoalList();
        }

        var loadTask = SceneManager.LoadSceneAsync(sceneName, loadMode);

        while (!loadTask.isDone)
        {
            yield return null;
        }
    }


    IEnumerator LoadScene(int atBuildIndex, LoadSceneMode loadMode)
    {
        Scene toLoad = SceneManager.GetSceneByBuildIndex(atBuildIndex);

        if (loadMode == LoadSceneMode.Single)
        {
            GoalManager.ResetGoalList();
        }

        var loadTask = SceneManager.LoadSceneAsync(atBuildIndex, loadMode);

        while (!loadTask.isDone)
        {
            yield return null;
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void LoadStartMenuLevel()
    {
        SceneManager.LoadScene(StartMenuLevel);
    }

    /// <summary>
    /// Referenced by the Editor start menu button configuration.
    /// Resumes from the player's saved point, if it exists.
    /// </summary>
    public void LoadInitialLevel()
    {
        MessageContoller.Clear();

        string levelToLoad = StartingLevel;

        // Starting level can be set by persistence.
        PlayerModel currentPlayer = GetCurrentPlayerModel();
        if (currentPlayer.CurrentLevelName != null)
        {
            string rootLevelScene = LocateRootSceneForLevel(currentPlayer.CurrentLevelName);
            if (rootLevelScene != null)
                levelToLoad = rootLevelScene;
        }

        SceneManager.LoadScene(levelToLoad);

        RestorePlayerShipFromSave = true;
    }

    public void BeginNewGame()
    {
        MessageContoller.Clear();

        CurrentLevel = 1;
        RestorePlayerShipFromSave = false;
        string levelToLoad = StartingLevel;
        SceneManager.LoadScene(levelToLoad);
    }

    public void RestorePlayerAndContinue()
    {
        RestoreBackedUpPlayerShip();
        EnterGamePhase(GameState.PlayPreparation);
    }

    /// <summary>
    /// Parses the standard level name (Level-{level-num}.{scene-num}.{terminal-marker}
    ///		level-num: integer => ordinal of levels
    ///		scene-num: integer => ordinal of parts within level
    ///		terminal-marker: +-   (where `+` => level not complete, `-` => final scene of level)
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns>null if the name does not conform to the standard layout, otherwise the parts</returns>
    public (int level, int scene, bool isTerminal)? ParseSceneName(string sceneName)
    {
        var matchResults = Regex.Match(sceneName, @"Level-(?'level'\d+)\.(?'scene'\d+)(?'term'.)");
        var level = matchResults.Groups["level"];
        var scene = matchResults.Groups["scene"];
        var terminalMarker = matchResults.Groups["term"];

        if (int.TryParse(level.Value, out int idLevel) &&
             int.TryParse(scene.Value, out int idScene))
        {
            bool isTerminalScene = terminalMarker.Value == "-";
            return (idLevel, idScene, isTerminalScene);
        }

        return null;
    }

    public string LocateRootSceneForLevel(string sceneName)
    {
        string rootLevelName = sceneName;

        var parsedSceneName = ParseSceneName(sceneName);
        if (parsedSceneName.HasValue)
        {
            // Build the non-terminal scene name if the scene ordinal of the level is beyond 0.
            if (parsedSceneName.Value.scene != 0)
            {
                rootLevelName = $"Level-{parsedSceneName.Value.level}.0+";
            }
        }
        return rootLevelName;
    }


    /// <summary>
    /// Reload the starting scene of the current level. Parses the current scene name and resets
    /// to the 0th scene within the section.
    /// </summary>
    public void RestartLevel()
    {
        MessageContoller.Clear();
        Scene currentScene = SceneManager.GetActiveScene();

        // Reboot the current scene of the current level, or reload the initial scene of the 
        // level. If we reboot the current scene, the backed-up player needs to be restored.
        string levelRootScene = LocateRootSceneForLevel(currentScene.name);
        if (levelRootScene != null)
        {
            RestorePlayerShipFromSave = true;
            EnterGamePhase(GameState.PlayPreparation);
            StartCoroutine(LoadScene(levelRootScene, LoadSceneMode.Single));
        }
    }

    /// <summary>
    /// Do a single scene load of the given name.
    /// </summary>
    /// <param name="sceneName">The scene to load.</param>
    internal void JumpToScene(string sceneName)
    {
        var parsedSceneName = ParseSceneName(sceneName);
        if (parsedSceneName.HasValue)
        {
            var toLoad = SceneManager.GetSceneByName(sceneName);
            EnterGamePhase(GameState.PlayPreparation);
            SceneManager.LoadScene(sceneName);

            //if (toLoad.buildIndex >= 0)
            //	StartCoroutine(LoadScene(toLoad.buildIndex, LoadSceneMode.Single));
            //else
            //	Debug.LogError($"Cannot find scene {sceneName}. Build index is: {toLoad.buildIndex}");
        }
    }

    public void SaveCurrentPlayer(bool saveDb)
    {
        LoadGameDatabase();
        StorePlayerShip(Player, false);
        if (saveDb)
            StoreGameDatabase();
    }

    private void SaveLevelStatistics(GameLevelStats stats, bool saveDB = true)
    {
        LoadGameDatabase();
        DbController.SaveLevelStatistics(GetCurrentPlayerModel(), stats);
        if (saveDB)
            StoreGameDatabase();
    }


    public void SaveCurrentState()
    {
        SaveCurrentPlayer(false);
        SaveLevelStatistics(LevelStatistics, false);
        StoreGameDatabase();
    }

    /// <summary>
    /// Level success either loads a new scene if we are at the end of a scene chapter, or
    /// it can load an additive level with new objectives and objects.
    /// </summary>
    public void ProgressToNextScene(bool forceLoad)
    {
        // Save current scene/level statistics
        //LevelStatistics.TimeOfCompletion = Time.time;
        //LevelStatistics.LevelComplete = true;
        //SaveCurrentState();

        Scene currentScene = SceneManager.GetActiveScene();
        var parsedSceneName = ParseSceneName(currentScene.name);

        CurrentLevel++;
        EnterGamePhase(GameState.PlayPreparation);
        SetTimeScale(TimeScale.Normal);
        StartCoroutine(LoadScene(currentScene.buildIndex, LoadSceneMode.Single));

        //if (parsedSceneName.HasValue)
        //{
        //    // Do we want to advance to the next scene by build index order or by logical name.
        //    // If by logical name, then take the parsed name and adjust the major and minor components
        //    // to the next step. We'll need to ensure that there is a scene present by that name.
        //    //int nextMajorLevelNum = parsedSceneName.Value.level;
        //    //int nextSceneNum = parsedSceneName.Va
        //    //if (parsedSceneName.Value.isTerminal)
        //    //{
        //    //	EnterGamePhase(GameState.NextLevel);
        //    //	nextMajorLevelNum++;
        //    //}

        //    // Advance by build index for now.
        //    LoadSceneMode loadMode = parsedSceneName.Value.isTerminal ? LoadSceneMode.Single : LoadSceneMode.Additive;

        //    if (loadMode == LoadSceneMode.Single)
        //    {
        //        EnterGamePhase(GameState.NextLevel);
        //        SetTimeScale(TimeScale.Slow);
        //    }
        //    else
        //    {
        //        EnterGamePhase(GameState.PlayPreparation);
        //        SetTimeScale(TimeScale.Normal);
        //    }

        //    if (loadMode == LoadSceneMode.Additive || forceLoad)
        //    {
        //        int activeScenes = SceneManager.sceneCount;
        //        var theScene = SceneManager.GetSceneAt(activeScenes - 1);
        //        int itsBuildIndex = theScene.buildIndex;

        //        StartCoroutine(LoadScene(itsBuildIndex + 1, loadMode));
        //    }
        //}
    }

    public void SetTimeScale(TimeScale setTo)
    {
        float newScale = 1;
        switch (setTo)
        {
            case TimeScale.SuperSlow:
                newScale = .05f;
                break;
            case TimeScale.Slow:
                newScale = .20f;
                break;
            case TimeScale.Normal:
                newScale = 1f;
                break;
            case TimeScale.Paused:
                newScale = 0f;
                break;
        }

        Time.timeScale = newScale;
        // Adjust fixed delta time according to timescale
        // The fixed delta time will now be 0.02 frames per real-time second
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    /// <summary>
    /// Identify and load player state:
    ///   * Player's ships
    ///   * Player's progress
    /// </summary>
    private ShipModel GetCurrentPlayerShipModel(bool createIfNotPresent)
    {
        PlayerModel lastPlayer = GetCurrentPlayerModel();
        ShipModel theirShip = null;

        // Last ship used
        if (lastPlayer.CurrentShipId.HasValue)
        {
            theirShip = lastPlayer.AvailableShips.Where(sm => sm.Id == lastPlayer.CurrentShipId).FirstOrDefault();
            if (theirShip == null)
                theirShip = lastPlayer.AvailableShips.FirstOrDefault();
        }

        if (theirShip == null && createIfNotPresent)
        {
            theirShip = new ShipModel();
            lastPlayer.AvailableShips.Add(theirShip);
            lastPlayer.CurrentShipId = theirShip.Id;
        }

        return theirShip;
    }

    public bool CanCurrentPlayerContinueSavedGame()
    {
        PlayerModel currentPlayer = GetCurrentPlayerModel();
        return !string.IsNullOrWhiteSpace(currentPlayer.CurrentLevelName);
    }

    PlayerModel GetCurrentPlayerModel()
    {
        LoadGameDatabase();
        PlayerModel lastPlayer = DbController.GetDefaultPlayer();
        return lastPlayer;
    }

    /// <summary>
    /// Global options associated with the saved player:
    ///   * User Input style (touch/keyboard/...)
    ///   
    /// Potential:
    ///   * User name
    ///   * Stats and such for score display
    /// </summary>
    private void LoadPlayerGameLevelOptions()
    {
        LoadGameDatabase();

        // Validate input style is acceptable
        // Fall back to the game controller's default if nothing set in player.
        PlayerModel lastPlayer = DbController.GetDefaultPlayer();
        ConfigurePlayerInput(lastPlayer.UserInputStyle ?? PlayerInputMode);
    }

    private int GetNumberOfAttemptsForLevel(string levelName)
    {
        LoadGameDatabase();
        LevelStatsModel mostRecentAttempt = DbController.GetMostStatisticsForLevel(GetCurrentPlayerModel(), levelName);
        return mostRecentAttempt?.LevelAttempt ?? 0;
    }

    /// <summary>
    /// Add or update the backing model for this ship.
    /// </summary>
    /// <param name="toSave"></param>
    private void StorePlayerShip(PlayerShip toSave, bool saveDB = true)
    {
        if (toSave == null)
            return;

        LoadGameDatabase();
        ShipModel currentShip = GetCurrentPlayerShipModel(true);

        currentShip.ShipName = toSave.CanonicalName();

        // Shield
        if (toSave.Shield != null)
        {
            currentShip.ShieldInstalled = new ShieldModel();
            currentShip.ShieldInstalled.ShieldObjectName = toSave.Shield.CanonicalName();
            currentShip.ShieldInstalled.HP = toSave.ShieldHealth?.HP ?? 0;
        }

        // Weapon rack
        currentShip.MunitionRack = new List<MunitionModel>();
        currentShip.MunitionRack.AddRange(toSave.WeaponRack.Select(munition =>
                new MunitionModel()
                {
                    MunitionObjectName = munition.Munition.CanonicalName(),
                    Count = munition.Count,
                    Capacity = munition.Munition.Capacity
                })
        );

        DbController.SaveCurrentPlayerShip(currentShip, true);
        if (saveDB)
            DbController.Save();
    }


    private PlayerShip RestorePlayerShip(ShipModel playerModelToRestore, Transform prototype)
    {
        if (prototype == null)
        {
            Debug.LogError($"No ship model prototype supplied to pull position from.");
            throw new ArgumentNullException("prototype");
        }

        if (playerModelToRestore == null)
        {
            Debug.LogError($"No ship model provided to restore.");
            throw new ArgumentNullException("playerToRestore");
        }

        PlayerShip shipResource = Resources.Load<PlayerShip>(playerModelToRestore.ShipName);
        if (shipResource == null)
        {
            Debug.LogError($"Could not locate player's ship `{playerModelToRestore?.ShipName}`, or any ship.");
            return null;
        }

        // This resource overrides all others, if non-null.
        // It is the restored resource from the Game Database.
        // If present, instantiate it in the same location as the Player.
        PlayerShip restoredPlayer = null;
        if (prototype != null)
        {
            restoredPlayer = Instantiate(shipResource, prototype.position, prototype.rotation, prototype.parent);
            if (restoredPlayer != null)
            {
                restoredPlayer.name = shipResource.name;

                // Restore sub-components:

                // Weapon rack
                restoredPlayer.AvailableWeapons.Clear();
                foreach (MunitionModel model in playerModelToRestore.MunitionRack)
                {
                    Munition munitionResource = Resources.Load<Munition>(model.MunitionObjectName);
                    if (munitionResource != null)
                    {
                        munitionResource.Capacity = model.Capacity;
                        munitionResource.Count = model.Count;

                        restoredPlayer.AvailableWeapons.Add(munitionResource);
                    }
                }

                // Shield
                //if (!string.IsNullOrWhiteSpace(playerModelToRestore.ShieldInstalled?.ShieldObjectName))
                //{
                //	GameObject shieldResource = Resources.Load<GameObject>(playerModelToRestore.ShieldInstalled.ShieldObjectName);
                //	if (shieldResource != null)
                //	{
                //		restoredPlayer.Shield = Instantiate(shieldResource, restoredPlayer.transform);
                //		ShieldController shieldController = restoredPlayer.Shield.GetComponent<ShieldController>();
                //		shieldController.SetHealth(playerModelToRestore.ShieldInstalled.HP);
                //	}
                //}
            }
        }
        return restoredPlayer;
    }


    public void EndGame(string reason = null, bool isLevelComplete = false)
    {
        LevelStatistics.TimeOfCompletion = Time.time;
        LevelStatistics.LevelComplete = isLevelComplete;
        SaveLevelStatistics(LevelStatistics, true);

        if (isLevelComplete)
        {
            // Completion of game -- 
        }
        else
        {
            EnterGamePhase(GameState.GameOver, reason);
        }
    }

    public void ShowStartMenu()
    {
        EnterGamePhase(GameState.StartMenu);
    }
}
