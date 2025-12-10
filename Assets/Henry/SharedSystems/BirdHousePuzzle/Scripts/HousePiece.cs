using UnityEngine;

public class HousePiece : MonoBehaviour
{
    [Header("ID")]
    public int pieceId = 0;

    [HideInInspector] public bool isLocked;

    [Header("Disable when locked")]
    public Behaviour[] disableOnLock;

    private bool _pickedVOFired = false;

    // Hook this from your XR Grab Interactable / Grabbable OnSelectEntered event
    public void OnPickedUp()
    {
        if (_pickedVOFired) return;
        _pickedVOFired = true;

        if (SoundPuzzleVOController.Instance != null)
        {
            SoundPuzzleVOController.Instance.CuePickUpFirstPiece();
        }
    }
}
