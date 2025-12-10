using UnityEngine;

public class OutlineVOTrigger : MonoBehaviour
{
    [Header("Which collider counts as the player?")]
    public string playerTag = "Controller";   // your controller trigger tag

    [Header("Only play once")]
    private bool _played = false;

    void OnTriggerEnter(Collider other)
    {
        if (_played) return;

        if (other.CompareTag(playerTag))
        {
            _played = true;

            if (SoundPuzzleVOController.Instance != null)
            {
                SoundPuzzleVOController.Instance.CueOutlineProximityHint();
            }
        }
    }
}
