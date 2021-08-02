using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
	public TextMeshProUGUI FailReason;

    public void SetText(string content)
    {
		if (FailReason != null )
			FailReason.text = content ?? string.Empty;		
    }
}
