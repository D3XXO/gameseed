using UnityEngine;
using System.Collections.Generic;

public class ThunderManager : MonoBehaviour
{
    [Header("Thunder Settings")]
    public float activeDuration = 4f;
    public float knockbackForce = 5f;
    public int damage = 3;
    public int maxActiveThunders = 5;
    public float spawnDistanceMin = 2f;
    public float spawnDistanceMax = 5f;

    private Transform player;
    private List<GameObject> activeThunders = new List<GameObject>();

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure you have a GameObject with tag 'Player'");
        }
    }

    // Called by DisasterManager to trigger a thunder strike
    public void TriggerThunderStrike(Vector2 position)
    {
        if (activeThunders.Count < maxActiveThunders)
        {
            GameObject thunder = InstantiateThunder(position);
            activeThunders.Add(thunder);
        }
    }

    // Called by DisasterManager to trigger multiple strikes around player
    public void TriggerThunderStorm(int strikeCount)
    {
        for (int i = 0; i < strikeCount && activeThunders.Count < maxActiveThunders; i++)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
            Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;
            
            GameObject thunder = InstantiateThunder((Vector2)player.position + offset);
            activeThunders.Add(thunder);
        }
    }

    private GameObject InstantiateThunder(Vector2 position)
    {
        // Create thunder object (you'll need to assign a prefab or create one programmatically)
        GameObject thunder = new GameObject("ThunderStrike");
        thunder.transform.position = position;
        
        // Add components
        ThunderAttack attack = thunder.AddComponent<ThunderAttack>();
        attack.knockbackForce = knockbackForce;
        attack.damage = damage;

        // Add automatic cleanup
        ThunderInstance instance = thunder.AddComponent<ThunderInstance>();
        instance.Initialize(this);

        return thunder;
    }

    public void RemoveThunder(GameObject thunder)
    {
        if (activeThunders.Contains(thunder))
        {
            activeThunders.Remove(thunder);
            Destroy(thunder);
        }
    }

    public void ClearAllThunders()
    {
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