using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "AR Escape Room/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Puzzle 2: Origami Bird")]
    public AudioClip P2_Success;                 // “It looks weak… and really thirsty.”
    public AudioClip P2_OOO_ReadBookFirst;       // “It’s just paper for now… maybe something in that book…”
    public AudioClip P2_OOO_PitcherFirst;        // “Water’s nice, but who’s it for?”
    public AudioClip P2_OOO_CageFirst;           // “A cage with no bird… what good is that?”
    public AudioClip P2_WrongOrigami;            // subtle negative sound

    // Add more for P3–P5 later
}
