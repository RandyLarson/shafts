using Assets.Scripts.Extensions;
using Assets.Scripts.Player;
using UnityEngine;
using UnityStandardAssets._2D;

public class ShaftsLevelController : MonoBehaviour
{
    public GameStats GameStats;
    public GameObject PlayerStartLocation;
    public bool ForceNewPlayerSpawn;
    public ShaftPlayerController NewPlayerPrototype;

    // Start is called before the first frame update
    void Start()
    {
        //var gc = GameObject.FindGameObjectWithTag("GameController");
        InitializeLevel();
    }


    public void InitializeLevel()
    {
        if (ForceNewPlayerSpawn)
        {
            GameStats.CurrentPlayer.SafeDestroy();
            GameStats.CurrentPlayer = null;
        }

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
