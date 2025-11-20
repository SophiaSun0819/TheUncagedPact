using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleState", menuName = "AR Escape Room/Puzzle State")]
public class PuzzleState : ScriptableObject
{
    [Header("Story & Bird")]
    public bool ReadStory;     // set true when book is read
    public bool CrowSpawned;   // true after scanning the correct origami
    public bool CrowHealed;    // true after pitcher/water

    [Header("Progress")]
    public bool CageBuilt;     // true after cage puzzle
    public bool BirdsongSolved; // true after song puzzle
}
