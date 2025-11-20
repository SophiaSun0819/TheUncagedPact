using UnityEngine;

public class AnimatorEventRelay : MonoBehaviour
{
    public Animator animator;
    public string parameterName = "IsFlapping";

    // Call this to start flapping
    public void SetTrue()
    {
        if (animator)
            animator.SetBool(parameterName, true);
    }

    // Call this to stop flapping
    public void SetFalse()
    {
        if (animator)
            animator.SetBool(parameterName, false);
    }
}
