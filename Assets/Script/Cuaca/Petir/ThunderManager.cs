using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThunderManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject thunderPrefab; // Serialized private field
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 10f;
    public int maxActiveThunders = 3;
    public float spawnDistanceMin = 2f;
    public float spawnDistanceMax = 5f;

    private static ThunderManager _instance;
    private Transform player;
    private Coroutine spawnRoutine;
    private List<GameObject> activeThunders = new List<GameObject>();

    public static ThunderManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ThunderManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ThunderManager");
                    _instance = obj.AddComponent<ThunderManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Load prefab from Resources if not assigned
        if (thunderPrefab == null)
        {
            Debug.LogError("Thunder prefab not assigned in inspector!");
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure you have a GameObject with tag 'Player'");
            return;
        }
        spawnRoutine = StartCoroutine(SpawnThunderRoutine());
    }

    private IEnumerator SpawnThunderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            if (activeThunders.Count < maxActiveThunders)
            {
                SpawnThunder();
            }
        }
    }

    private void SpawnThunder()
    {
        if (thunderPrefab == null)
        {
            Debug.LogError("Thunder prefab reference is missing!");
            return;
        }

        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
        Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;

        GameObject newThunder = Instantiate(thunderPrefab, (Vector2)player.position + offset, Quaternion.identity);
        activeThunders.Add(newThunder);

        // Add automatic cleanup component
        ThunderInstance thunderInstance = newThunder.AddComponent<ThunderInstance>();
        thunderInstance.Initialize(this);
    }

    public void RemoveThunder(GameObject thunder)
    {
        if (activeThunders.Contains(thunder))
        {
            activeThunders.Remove(thunder);
        }
    }

    public void StopAllThunder()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        foreach (var thunder in activeThunders)
        {
            if (thunder != null)
            {
                Destroy(thunder);
            }
        }
        activeThunders.Clear();
    }
}