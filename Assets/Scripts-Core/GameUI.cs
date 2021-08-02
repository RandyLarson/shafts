using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityStandardAssets._2D;


public enum GameState
{
	StartMenu,
	MidGameMenu,
	NextLevel,
	GameOver,
	PlayPreparation,
	LevelIntro,
	LevelInterlude,
	Play
}
public interface IUserInterface
{
	void HideTransitionalScreens();
	bool TouchScreenUIVisible { get; set; }

	void EnterGamePhase(GameState switchTo, string reason = null);
	Joystick FlightControl { get; }
	ButtonJoystick WeaponsControl { get; }
	ButtonJoystick FreightControl { get; }
	Camera MainCamera { get; }
	TimerVisualizer GlobalTimer { get; }
	LevelIntroductionController LevelIntroController { get; }
	LevelCompletionController LevelOutroController { get; }
	StatisticsController StatsController { get; }
	void ConfigureMiniMap(GameObject toFollow, Rect levelBounds);
	void WirePlayerHealthVisualizer(HealthPoints playerHP, HealthPoints playerShield);

	TMPro.TextMeshProUGUI Diagnostics { get; }
	public TMPro.TextMeshProUGUI PlayerHealth { get; }
	public TMPro.TextMeshProUGUI PlayerTime { get;  }
	public TMPro.TextMeshProUGUI Level { get; }

}


public class GameUIController
{
	public static IUserInterface Controller;
}

public class GameUI : MonoBehaviour, IUserInterface
{
	public GameObject StartGameParent;
	public GameObject PauseParent;
	public GameObject GameOverParent;
	public GameObject LevelSuccessParent;
	public GameObject LevelIntroParent;
	public GameObject TouchInputControlParent;
	public Camera MainCamera;
	public Joystick FlightControl;
	public ButtonJoystick WeaponsControl;
	public ButtonJoystick FreightControl;
	public TMPro.TextMeshProUGUI PlayerHealth;
	public TMPro.TextMeshProUGUI PlayerTime;
	public TMPro.TextMeshProUGUI Level;

	public StatisticsController Statistics;

	public Camera2DFollow MiniMapCamera;
	public TimerVisualizer GlobalTimer;
	public TMPro.TextMeshProUGUI Diagnostics;


	Joystick IUserInterface.FlightControl => FlightControl;

	ButtonJoystick IUserInterface.WeaponsControl => WeaponsControl;

	ButtonJoystick IUserInterface.FreightControl => FreightControl;

	TMPro.TextMeshProUGUI IUserInterface.Diagnostics => Diagnostics;
	Camera IUserInterface.MainCamera => MainCamera;

	TimerVisualizer IUserInterface.GlobalTimer => GlobalTimer;

	StatisticsController IUserInterface.StatsController => Statistics;

	public LevelIntroductionController LevelIntroController => LevelIntroParent.GetComponent<LevelIntroductionController>();
	public LevelCompletionController LevelOutroController => LevelSuccessParent.GetComponent<LevelCompletionController>();

	public bool TouchScreenUIVisible
	{
		get => TouchInputControlParent.activeInHierarchy;
		set => TouchInputControlParent.SetActive(value);
	}

	TextMeshProUGUI IUserInterface.PlayerHealth => PlayerHealth;

	TextMeshProUGUI IUserInterface.PlayerTime => PlayerTime;

	TextMeshProUGUI IUserInterface.Level => Level;

    void IUserInterface.ConfigureMiniMap(GameObject toFollow, Rect levelBounds)
	{
		if (MiniMapCamera != null)
		{
			MiniMapCamera.ToFollow = toFollow.transform;
			MiniMapCamera.ConfigureViewBounds(levelBounds, 0f);
		}
	}

	public void WirePlayerHealthVisualizer(HealthPoints playerHP, HealthPoints playerShield)
	{
		if (null != playerHP)
			playerHP.Visualizer = PlayerHealth;

		if (null != playerShield)
			playerShield.Visualizer = PlayerTime;
	}

	public GameUI()
	{
		GameUIController.Controller = this;
	}

	public void Awake()
	{
		Object.DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		//TouchInputControlParent?.SetActive(Input.touchSupported);
	}

	public void HideTransitionalScreens()
	{
		GameOverParent.SetActive(false);
		LevelSuccessParent.SetActive(false);
	}

	/// <summary>
	/// Show or hide ui elements based on the phase we are entering.
	/// </summary>
	/// <param name="switchTo">The phase being entered.</param>
	/// <param name="reason">Auxiliary text to display for some phases.</param>
	public void EnterGamePhase(GameState switchTo, string reason = null)
	{
		bool startMenu = false;
		bool levelComplete = false;
		bool gameOver = false;
		bool levelIntro = false;
		bool midGameMenu = false;
		switch (switchTo)
		{
			case GameState.StartMenu:
				startMenu = true;
				break;
			case GameState.NextLevel:
				levelComplete = true;
				break;
			case GameState.GameOver:
				gameOver = true;
				var controller = GameOverParent.GetComponent<GameOverController>();
				if (controller != null)
					controller.SetText(reason);
				break;
			case GameState.PlayPreparation:
				break;
			case GameState.Play:
				break;
			case GameState.LevelInterlude:
			case GameState.LevelIntro:
				levelIntro = true;
				break;
			case GameState.MidGameMenu:
				midGameMenu = true;
				break;
		}

		GameOverParent.SafeSetActive(gameOver);
		StartGameParent.SafeSetActive(startMenu);
		LevelSuccessParent.SafeSetActive(levelComplete);
		LevelIntroParent.SafeSetActive(levelIntro);
		PauseParent.SafeSetActive(midGameMenu);
	}
}
