using UnityEngine;

public class HousePiece : MonoBehaviour
{
    [Header("ID")]
    public int pieceId = 0;                  // 1 = base, 2 = pole, 3 = house etc.

    [HideInInspector] public bool isLocked;  // set by controller

    [Header("Disable when locked")]
    // Drag XR Grab Interactable, GrabInteractable, Grabbable, etc. here
    public Behaviour[] disableOnLock;
}
