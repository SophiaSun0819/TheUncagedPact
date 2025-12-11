using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSequenceManager : MonoBehaviour
{
    [Header("Player / Camera")]
    [Tooltip("Usually your XR camera / center eye transform.")]
    public Transform playerHead;

    [Header("Things that will poof (drag from Hierarchy)")]
    public List<GameObject> poofTargets = new List<GameObject>();
    public GameObject poofVfxPrefab;          // VFX prefab that also has an AudioSource

    [Header("Guide Birds")]
    [Tooltip("Drag 6 different bird prefabs here.")]
    public List<GameObject> birdPrefabs = new List<GameObject>();
    public int birdsToSpawn = 6;
    public float birdSpawnRadius = 0.5f;

    [Header("Door")]
    [Tooltip("Door prefab with Animator that has an 'Open' trigger.")]
    public GameObject doorPrefab;
    public float doorDistanceInFront = 2f;

    [Header("Voice Over")]
    public AudioSource voSource;              // AudioSource on EndSequence object
    public AudioClip cageCrumblingVO;         // "Oh no, the cage is crumbling!"
    public AudioClip manyBirdsVO;            // "Wow, so many birds..."
    public AudioClip followUsVO;             // "Do they want me to follow them?"

    [Header("Timings")]
    public float poofDelayBetweenObjects = 0.08f;
    public float delayBeforeBirds = 0.4f;
    public float delayBeforeDoor = 2.0f;
    public float delayAfterDoorOpens = 2.0f;

    bool sequenceStarted = false;

    public void PlayEndSequence()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;
        StartCoroutine(EndSequenceRoutine());
    }

    IEnumerator EndSequenceRoutine()
    {
        // 1) VO: cage crumbling
        PlayVO(cageCrumblingVO);

        // 2) Poof all selected objects
        yield return StartCoroutine(DissolveWorldRoutine());

        yield return new WaitForSeconds(delayBeforeBirds);

        // 3) Spawn birds around player + VO about birds
        SpawnBirdsAroundPlayer();
        PlayVO(manyBirdsVO);

        yield return new WaitForSeconds(delayBeforeDoor);

        // 4) Spawn door in front of player, open it, tell birds to lead to it + VO follow line
        GameObject doorInstance = SpawnDoorInFrontOfPlayer();
        if (doorInstance != null)
        {
            AssignDoorToBirds(doorInstance.transform);
            OpenDoor(doorInstance);
        }

        PlayVO(followUsVO);

        yield return new WaitForSeconds(delayAfterDoorOpens);

        // 5) (Optional) here you could trigger scene change / fade out etc.
        // e.g. SceneManager.LoadScene("NextScene");
    }

    IEnumerator DissolveWorldRoutine()
    {
        // work on a copy so we can safely destroy during loop
        var targets = new List<GameObject>(poofTargets);

        foreach (GameObject obj in targets)
        {
            if (obj == null) continue;

            if (poofVfxPrefab != null)
                Instantiate(poofVfxPrefab, obj.transform.position, Quaternion.identity);

            Destroy(obj);
            yield return new WaitForSeconds(poofDelayBetweenObjects);
        }
    }

    void SpawnBirdsAroundPlayer()
    {
        if (playerHead == null || birdPrefabs.Count == 0) return;

        for (int i = 0; i < birdsToSpawn; i++)
        {
            GameObject prefab = birdPrefabs[Mathf.Clamp(i, 0, birdPrefabs.Count - 1)];
            // or use Random.Range: birdPrefabs[Random.Range(0, birdPrefabs.Count)];

            Vector3 offset = Random.onUnitSphere;
            offset.y = Mathf.Abs(offset.y);        // keep them above-ish
            offset *= birdSpawnRadius;

            Vector3 spawnPos = playerHead.position + offset;
            GameObject birdObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            GuideBird guide = birdObj.GetComponent<GuideBird>();
            if (guide != null)
            {
                guide.player = playerHead;
            }
        }
    }

    GameObject SpawnDoorInFrontOfPlayer()
    {
        if (doorPrefab == null || playerHead == null) return null;

        Vector3 spawnPos = playerHead.position + playerHead.forward * doorDistanceInFront;
        spawnPos.y = playerHead.position.y - 0.1f; // a bit below eye level

        Quaternion rot = Quaternion.LookRotation(-playerHead.forward, Vector3.up);

        return Instantiate(doorPrefab, spawnPos, rot);
    }

    void AssignDoorToBirds(Transform doorTransform)
    {
        GuideBird[] birds = FindObjectsByType<GuideBird>(FindObjectsSortMode.None);
        foreach (GuideBird b in birds)
        {
            b.doorTarget = doorTransform;
            b.StartLeading();
        }
    }

    void OpenDoor(GameObject doorInstance)
    {
        Animator anim = doorInstance.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Open");   // make sure your Animator has a trigger called "Open"
        }
    }

    void PlayVO(AudioClip clip)
    {
        if (voSource != null && clip != null)
        {
            voSource.Stop();
            voSource.PlayOneShot(clip);
        }
    }
}
