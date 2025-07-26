using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject sharkPrefab;
    public float spawnInterval;
    public int maxSharks;
    public float spawnRadius;
    public LayerMask groundLayer;

    private int currentSharkCount;
    private Coroutine spawnRoutine;

    void Start()
    {
        spawnRoutine = StartCoroutine(SpawnSharkRoutine());
    }

    IEnumerator SpawnSharkRoutine()
    {
        if (currentSharkCount < maxSharks)
        {
            SpawnImmediate();
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentSharkCount < maxSharks)
            {
                SpawnImmediate();
            }
        }
    }

    public void SpawnImmediate()
    {
        if (currentSharkCount >= maxSharks)
        {
            Debug.Log("Max sharks reached, not spawning new one immediately.");
            return;
        }

        Vector3 randomSpawnPos = Vector3.zero;
        bool foundValidSpot = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!foundValidSpot && attempts < maxAttempts)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

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
            GameObject newSharkGO = Instantiate(sharkPrefab, randomSpawnPos, Quaternion.identity);
            currentSharkCount++;

            EnemyAI newSharkAI = newSharkGO.GetComponent<EnemyAI>();
            if (newSharkAI != null)
            {
                newSharkAI.Init(this, randomSpawnPos);
                newSharkAI.OnSharkDestroyed += OnSharkDestroyed;
            }
            Debug.Log($"Shark spawned at: {randomSpawnPos}. Current sharks: {currentSharkCount}");
        }
        else
        {
            Debug.LogWarning("Could not find a valid spawn spot for shark. Trying again in 2 seconds.");
            Invoke("SpawnShark", 2f);
        }
    }

    void OnSharkDestroyed()
    {
        currentSharkCount--;
        Debug.Log($"Shark destroyed. Remaining sharks: {currentSharkCount}");
    }
}