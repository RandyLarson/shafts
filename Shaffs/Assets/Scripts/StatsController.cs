using TMPro;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    public PlayerStats PlayerStats;
    public GameStats GameStats;

    public TextMeshProUGUI DisplayElement;
    public PlayerStatKind ResourceToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        if ( DisplayElement == null )
            DisplayElement = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DisplayElement == null)
            return;


        string value = "0";
        switch (ResourceToDisplay)
        {
            case PlayerStatKind.Level:
                value = PlayerStats.Level;
                break;
            case PlayerStatKind.Health:
                value = Mathf.Max(0, (int)PlayerStats.Health).ToString();
                break;
            case PlayerStatKind.Gold:
                value = ((int)PlayerStats.Gold).ToString();
                break;
        }

        DisplayElement.text = value;
    }
}
