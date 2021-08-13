using UnityEngine;

public enum PlayerStatKind
{
    Level,
    Health,
    Gold,
    CurrentWeapon
}


[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/Shaffs/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    public string Level;
    public float Health;
    public float Gold;
    public string CurrentWeaponName;
}

