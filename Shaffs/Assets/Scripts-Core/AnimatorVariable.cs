using UnityEngine;

public class AnimatorVariable : MonoBehaviour
{
    public Animator Animator;
    public string Variable = "Position";
    public int IntValue = 0;


    void Start()
    {
        if (Animator != null)
        {
            Animator.SetInteger(Variable, IntValue);
        }
    }
}
