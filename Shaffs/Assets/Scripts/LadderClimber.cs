using UnityEngine;

public class LadderClimber : MonoBehaviour
{
    public bool IsOnLadder => OnLadders.Members.Count > 0;

    public GameObjectCollection OnLadders { get; private set; } = new GameObjectCollection();
}
