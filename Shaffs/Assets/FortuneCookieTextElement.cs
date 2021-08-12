using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;

public class FortuneCookieTextElement : MonoBehaviour
{
    public string[] Wisdoms;
    public TextMeshProUGUI DisplayElement;
    public GameStats GameStats;

    private void OnEnable()
    {
        string content = GameStats.Fortune;
        if (content.HasNoContent() )
            content = Wisdoms[Random.Range(0, Wisdoms.Length - 1)];

        DisplayElement.text = content;
    }
}
