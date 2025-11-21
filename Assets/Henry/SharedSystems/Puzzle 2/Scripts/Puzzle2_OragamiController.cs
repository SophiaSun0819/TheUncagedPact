using UnityEngine;

public class Puzzle2_OrigamiController : MonoBehaviour
{
    [Header("Shared Systems (drag assets/objects here)")]
    public PuzzleState puzzleState;     // drag PuzzleState_Main (asset)
    public AudioLibrary audioLibrary;   // drag AudioLibrary_Main (asset)
    public AudioSource roomAudio;       // drag RoomAudio (scene object)

    [Header("Scene Refs")]
    public Transform crowSpawnPoint;    // empty under TableAnchor
    public GameObject crowGhostPrefab;  // transparent crow prefab

    [Header("Origami Refs")]
    public GameObject correctOrigami;   // the one with QR (or scan trigger)

    [Header("Settings")]
    public bool requireReadStory = true;
    public bool allowSingleSpawn = true;

    public void HandleCorrectScan()
{
    // Guard: require story first?
    if (requireReadStory && !puzzleState.ReadStory)
    {
        if (audioLibrary && audioLibrary.P2_OOO_ReadBookFirst && roomAudio)
            roomAudio.PlayOneShot(audioLibrary.P2_OOO_ReadBookFirst);
        return;
    }

    // Already spawned?
    if (puzzleState.CrowSpawned && allowSingleSpawn) return;

    // Spawn the crow
    if (crowGhostPrefab && crowSpawnPoint)
    {
        Instantiate(crowGhostPrefab, crowSpawnPoint.position, crowSpawnPoint.rotation, crowSpawnPoint.parent);
    }

    // Set state + success VO
    puzzleState.CrowSpawned = true;
    if (audioLibrary && audioLibrary.P2_Success && roomAudio)
        roomAudio.PlayOneShot(audioLibrary.P2_Success);
}

}
