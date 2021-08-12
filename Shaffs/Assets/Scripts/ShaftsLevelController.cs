using Assets.Scripts.Extensions;
using Assets.Scripts.Player;
using UnityEngine;
using UnityStandardAssets._2D;

public class ShaftsLevelController : MonoBehaviour
{
    public GameStats GameStats;
    public PlayerStats PlayerStats;
    public GameObject PlayerStartLocation;
    public bool ForceNewPlayerSpawn;
    public string LevelName;
    public ShaftPlayerController NewPlayerPrototype;
    public string Fortune;

    // Start is called before the first frame update
    void Start()
    {
        InitializeLevel();
    }


    public void InitializeLevel()
    {
        GameStats.Fortune = Fortune;
        GameStats.GameController.SwitchToGameMode(GameMode.StartingLevel);

        if (ForceNewPlayerSpawn)
        {
            GameStats.CurrentPlayer.SafeDestroy();
            GameStats.CurrentPlayer = null;
        }

        if (LevelName.HasNoContent())
        { 
            int numAt = gameObject.scene.name.IndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
            if ( numAt != -1 )
            {
                LevelName = gameObject.scene.name.Remove(0, numAt);
            }
            else
            {
                LevelName = "foo";
            }
        }

        PlayerStats.Level = LevelName;
        GameStats.CurrentLevel = LevelName;
        if (GameStats.CurrentPlayer == null)
        {
            GameObject playerPrototype = NewPlayerPrototype != null ? NewPlayerPrototype.gameObject : GameStats.PlayerPrototype;
            if (playerPrototype != null)
            {
                if (playerPrototype.SafeInstantiate<ShaftPlayerController>(PlayerStartLocation.transform.position, out var created))
                {
                    GameStats.CurrentPlayer = created;
                }
            }
        }

        if (GameStats.CurrentPlayer != null)
        {
            GameStats.CurrentPlayer.transform.position = PlayerStartLocation.transform.position;
            var followerScript = Camera.main.GetComponent<Camera2DFollow>();
            if (followerScript != null)
            {
                followerScript.ToFollow = GameStats.CurrentPlayer.transform;
            }
        }
    }

}
