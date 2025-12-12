using UnityEngine;

public class OriBirdSpawnButton : MonoBehaviour
{
    [Header("Bird Setup")]
    public GameObject birdPrefab;        // Bird prefab (from Project)
    public Transform spawnPoint;         // Where the bird appears

    [Header("Water System")]
    public ShaderWaterLevelController waterLevelController;

    [Header("Delivery Target")]
    public Transform playerTarget;       // BirdDeliveryPoint under XR Camera

    [Header("Change Color Trigger (Pitcher Rim)")]
    [SerializeField] private GameObject changeColorTrigger;

    [Header("Home Perch (Where bird returns after giving page)")]
    [SerializeField] private Transform homePoint;

    [Header("Spawn Options")]
    public bool canSpawnAgain = false;   // Allow multiple birds or not

    private GameObject _spawnedBird;

    public void SpawnBird()
    {
        // Prevent multiple birds if not allowed
        if (!canSpawnAgain && _spawnedBird != null)
            return;

        if (!birdPrefab || !spawnPoint)
        {
            Debug.LogWarning("[OriBirdSpawnButton] Missing birdPrefab or spawnPoint!", this);
            return;
        }

        // Spawn bird
        _spawnedBird = Instantiate(
            birdPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("[OriBirdSpawnButton] Origami bird spawned.", this);

        // -----------------------------
        // Hook up BirdChangeCustom
        // -----------------------------
        var colorLogic = _spawnedBird.GetComponentInChildren<BirdChangeCustom>();
        if (colorLogic != null)
        {
            // Inject water controller
            if (waterLevelController != null)
            {
                colorLogic.Init(waterLevelController);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No waterLevelController assigned; bird won't react to water.", this);
            }

            // Inject ChangeColorTrigger (pitcher lip)
            if (changeColorTrigger != null)
            {
               // colorLogic.changeColorTrigger = changeColorTrigger;
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No changeColorTrigger assigned.", this);
            }
        }
        else
        {
            Debug.LogWarning("[OriBirdSpawnButton] BirdChangeCustom not found on spawned bird!", this);
        }

        // -----------------------------
        // Hook up BirdPickUp (delivery + home)
        // -----------------------------
        var courier = _spawnedBird.GetComponent<BirdPickUp>();
        if (courier != null)
        {
            // Set where bird should deliver the paper
            if (playerTarget != null)
            {
                courier.SetPlayerTarget(playerTarget);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No playerTarget assigned; courier cannot deliver page.");
            }

            // Set where the bird returns after delivering
            if (homePoint != null)
            {
                courier.SetHomePoint(homePoint);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No homePoint assigned; bird will return to spawn position.");
            }
        }
        else
        {
            Debug.Log("[OriBirdSpawnButton] BirdPickUp not found on spawned bird (OK if you don't use delivery yet).");
        }
    }
}
