using UnityEngine;

public class HousePiece : MonoBehaviour
{
    public int pieceId = 0;   // set in Inspector (1,2,3...)
    [HideInInspector] public bool isLocked = false;
}
