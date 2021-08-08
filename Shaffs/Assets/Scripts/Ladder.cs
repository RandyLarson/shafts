using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var climber = collision.gameObject.GetComponent<LadderClimber>();
        if (climber != null)
        {
            climber.OnLadders.RememberObject(gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        var climber = collision.gameObject.GetComponent<LadderClimber>();
        if (climber != null)
        {
            climber.OnLadders.ForgetObject(gameObject);
        }
    }
}
