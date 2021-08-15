using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsController : MonoBehaviour
{
    public PlayerStats PlayerStats;
    public GameStats GameStats;
    public GameObject ShivIcon;
    public GameObject FlameIcon;

    public TextMeshProUGUI DisplayElement;
    public Image HealthDisplayElement;
    public PlayerStatKind ResourceToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        if (DisplayElement == null)
            DisplayElement = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (DisplayElement == null)
        //    return;


        string value = "0";
        switch (ResourceToDisplay)
        {
            case PlayerStatKind.Level:
                value = PlayerStats.Level;
                break;
            case PlayerStatKind.Health:

                float perc = (float)(PlayerStats.Health / 1000f);
                float a = (int)(255f * perc);
                float r = perc > .33 ? 255 : 255;
                float g = perc > .33 ? 255 : 0;
                float b = perc > .33 ? 255 : 0;

                HealthDisplayElement.color = new Color(r, g, b, perc);
                var scalePerc = Mathf.Lerp(.5f, 1f, perc);
                //HealthDisplayElement.transform.localScale = new Vector3(scalePerc, scalePerc, scalePerc);
                //value = Mathf.Max(0, (int)PlayerStats.Health).ToString();
                break;
            case PlayerStatKind.Gold:
                value = ((int)PlayerStats.Gold).ToString();
                break;
            case PlayerStatKind.CurrentWeapon:
                bool shiv = false;
                bool flam = false;
                if (PlayerStats.CurrentWeaponName.EqualsIgnoreCase("SV"))
                    shiv = true;
                else
                    flam = true;
                ShivIcon.SafeSetActive(shiv);
                FlameIcon.SafeSetActive(flam);
                value = string.Empty;
                break;
        }

        //DisplayElement.text = value;
    }
}
