using UnityEngine;
using System.Collections;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public float spawnInterval;
    public int maxObjectsInScene;

    [Header("Spawn Area (Circle)")]
    public float spawnRadius;
    public LayerMask groundLayer;

    private int currentObjectsCount = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentObjectsCount < maxObjectsInScene)
            {
                SpawnObject();
            }
            else
            {
                Debug.Log("Max objects reached, waiting for space.");
            }
        }
    }

    void SpawnObject()
    {
        Vector3 randomSpawnPos = Vector3.zero;
        bool foundValidSpot = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!foundValidSpot && attempts < maxAttempts)
        {
            Vector2 randomCirclePoint = Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePos = transform.position + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0);

            Collider2D hit = Physics2D.OverlapPoint(candidatePos, groundLayer);
            if (hit != null)
            {
                randomSpawnPos = candidatePos;
                randomSpawnPos.z = 0;
                foundValidSpot = true;
            }
            attempts++;
        }

        if (foundValidSpot)
        {
            GameObject newObject = Instantiate(prefabToSpawn, randomSpawnPos, Quaternion.identity);
            currentObjectsCount++;

            CollectibleItem collectible = newObject.GetComponent<CollectibleItem>();
            if (collectible != null)
            {
                collectible.OnItemDestroyed += OnObjectDestroyed;
            }

            Debug.Log($"Spawned {prefabToSpawn.name} at: {randomSpawnPos}");
        }
        else
        {
            Debug.LogWarning($"Could not find a valid spawn spot for {prefabToSpawn.name} after {maxAttempts} attempts.");
        }
    }

    public void OnObjectDestroyed()
    {
        currentObjectsCount--;
        Debug.Log($"Object destroyed. Current count: {currentObjectsCount}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}