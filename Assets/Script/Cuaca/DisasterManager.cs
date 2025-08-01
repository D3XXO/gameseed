using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WorldTime
{
    public class DisasterManager : MonoBehaviour
    {
        [Header("Disaster Settings")]
        [SerializeField] private float minDisasterInterval = 30f;
        [SerializeField] private float maxDisasterInterval = 120f;
        [SerializeField] private float disasterWarningTime = 5f;
        [SerializeField][Range(0f, 1f)] public float dailyDisasterChance = 0.3f; 

        [Header("Thunder Settings")]
        [SerializeField] private GameObject thunderDisasterPrefab;
        [SerializeField] private float thunderDuration = 4f;
        [SerializeField] private int maxThunders = 5;
        [SerializeField] private float minSpawnDistance = 4f;
        [SerializeField] private float maxSpawnDistance = 8f;

        private Transform player;
        private bool isDisasterActive = false;
        private Coroutine disasterRoutine;
        private List<GameObject> activeThunders = new List<GameObject>();
        public static DisasterManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
            player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null)
            {
                Debug.LogError("Player not found!");
                return;
            }
        }

        private void Start()
        {
            disasterRoutine = StartCoroutine(DisasterScheduler());
        }

        private IEnumerator DisasterScheduler()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minDisasterInterval, maxDisasterInterval));

                // Only trigger if:
                // 1. It's day 3 or later
                // 2. Random chance succeeds
                // 3. No active disaster
                if (WorldTime.Instance.CurrentDay >= 3 &&
                    Random.value <= dailyDisasterChance &&
                    !isDisasterActive)
                {
                    StartCoroutine(TriggerRandomDisaster());
                }
            }
        }

        private IEnumerator TriggerRandomDisaster()
        {
            isDisasterActive = true;
            Debug.LogWarning("A disaster is coming!");

            // Show warning (add visual/audio here)
            yield return new WaitForSeconds(disasterWarningTime);

            TriggerThunderDisaster();

            yield return new WaitForSeconds(10f); // Cooldown between disasters
            isDisasterActive = false;
        }

        private void TriggerThunderDisaster()
        {
            int strikeCount = Random.Range(3, 6);
            for (int i = 0; i < strikeCount && activeThunders.Count < maxThunders; i++)
            {
                Vector2 pos = GetRandomPositionAroundPlayer();
                GameObject thunder = Instantiate(thunderDisasterPrefab, pos, Quaternion.identity);
                activeThunders.Add(thunder);
                Destroy(thunder, thunderDuration);
            }
        }

        private Vector2 GetRandomPositionAroundPlayer()
        {
            float angle = Random.Range(0, 360);
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            return (Vector2)player.position +
                   new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                              Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;
        }

        public void StopAllDisasters()
        {
            if (disasterRoutine != null) StopCoroutine(disasterRoutine);
            isDisasterActive = false;
            foreach (var thunder in activeThunders) Destroy(thunder);
            activeThunders.Clear();
        }
    }
}