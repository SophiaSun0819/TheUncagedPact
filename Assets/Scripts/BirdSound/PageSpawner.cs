using UnityEngine;

public class PageSpawner : MonoBehaviour
{
    public GameObject pagePrefab;
    public bool puzzleSuccess=false;
    public Transform spawnTransform;
    private bool hasSpawned = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (puzzleSuccess && !hasSpawned)
        {
            SpawnPage();
        }
    }
    private void SpawnPage()
    {
        if (pagePrefab == null || spawnTransform == null)
        {
           
            return;
        }

        
        Instantiate(pagePrefab, spawnTransform.position, spawnTransform.rotation);

        hasSpawned = true; 

       
    }
}
