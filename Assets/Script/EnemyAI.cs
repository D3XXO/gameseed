using UnityEngine;
using System;

public class EnemyAI : MonoBehaviour
{
    public event Action OnSharkDestroyed;

    [Header("Movement")]
    public float patrolMoveSpeed;
    public float chaseMoveSpeed;
    public float rotationSpeed;

    [Header("Patrol Settings (Circular)")]
    public float patrolOrbitRadius;
    public float patrolOrbitSpeed;
    public float minPatrolMoveDistance;
    public float patrolTurnChangeInterval;

    [Header("Detection")]
    public float chaseRange;
    public LayerMask playerLayer;

    [Header("Damage")]
    public int damageAmount;
    public float collisionCooldown;

    [Header("Lifespawn")]
    public float lifeDuration;

    private Transform playerTarget;
    private State currentState;
    public enum State { Patrolling, Chasing }

    private float lastCollisionTime;
    private float lifeTimer;
    private float patrolTurnTimer;
    private int patrolTurnDirection = 1;

    private Vector3 initialSpawnPosition;

    private float currentPatrolAngle = 0f;

    private EnemySpawner mySpawner;

    public void Init(EnemySpawner spawner, Vector3 spawnPos)
    {
        mySpawner = spawner;
        initialSpawnPosition = spawnPos;
        lifeTimer = lifeDuration;
        currentState = State.Patrolling;
        patrolTurnTimer = patrolTurnChangeInterval;
        patrolTurnDirection = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
    }

    void Start()
    {
        if (playerTarget == null)
        {
            BoatController playerBoat = FindObjectOfType<BoatController>();
            if (playerBoat != null) playerTarget = playerBoat.transform;
            else Debug.LogWarning("Player (BoatController) not found for EnemyAI.");
        }
    }

    void OnDisable()
    {
        OnSharkDestroyed = null;
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            DestroyShark();
            return;
        }

        CheckForPlayer();

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolState();
                break;
            case State.Chasing:
                HandleChaseState();
                break;
        }
    }

    void CheckForPlayer()
    {
        if (playerTarget == null)
        {
            BoatController playerBoat = FindObjectOfType<BoatController>();
            if (playerBoat != null) playerTarget = playerBoat.transform;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= chaseRange && currentState != State.Chasing)
        {
            currentState = State.Chasing;
            Debug.Log("Shark: Player detected! Chasing.");
        }
        else if (distanceToPlayer > chaseRange && currentState == State.Chasing)
        {
            currentState = State.Patrolling;
            Debug.Log("Shark: Player out of range. Returning to patrol.");

            currentPatrolAngle = Vector2.SignedAngle(Vector2.right, (Vector2)transform.position - (Vector2)initialSpawnPosition);
            lifeTimer = lifeDuration;
        }
    }

    void HandlePatrolState()
    {
        currentPatrolAngle += patrolOrbitSpeed * patrolTurnDirection * Time.deltaTime;
        Vector3 targetPos = initialSpawnPosition + new Vector3(
            Mathf.Cos(currentPatrolAngle * Mathf.Deg2Rad) * patrolOrbitRadius,
            Mathf.Sin(currentPatrolAngle * Mathf.Deg2Rad) * patrolOrbitRadius,
            0f
        );

        transform.position = Vector3.MoveTowards(transform.position, targetPos, patrolMoveSpeed * Time.deltaTime);

        Vector3 directionToTarget = (targetPos - transform.position).normalized;
        RotateShark(directionToTarget);

        patrolTurnTimer -= Time.deltaTime;
        if (patrolTurnTimer <= 0)
        {
            patrolTurnDirection *= -1;
            patrolTurnTimer = patrolTurnChangeInterval;
            Debug.Log("Shark: Changed patrol turn direction.");
        }
    }

    void HandleChaseState()
    {
        if (playerTarget == null)
        {
            currentState = State.Patrolling;
            return;
        }

        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, chaseMoveSpeed * Time.deltaTime);

        RotateShark(directionToPlayer);
    }

    void RotateShark(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastCollisionTime + collisionCooldown)
        {
            BoatController playerBoat = collision.gameObject.GetComponent<BoatController>();
            if (playerBoat != null)
            {
                playerBoat.TakeDamage(damageAmount);
                lastCollisionTime = Time.time;

                Debug.Log($"Shark collided with player! Player HP: {playerBoat.currentHP}");

                DestroyShark();
            }
        }
    }

    void DestroyShark()
    {
        if (mySpawner != null)
        {
            mySpawner.SpawnImmediate();
        }
        OnSharkDestroyed?.Invoke();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(initialSpawnPosition, patrolOrbitRadius);

        Vector3 patrolPoint = initialSpawnPosition + new Vector3(Mathf.Cos(currentPatrolAngle * Mathf.Deg2Rad) * patrolOrbitRadius, Mathf.Sin(currentPatrolAngle * Mathf.Deg2Rad) * patrolOrbitRadius, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(initialSpawnPosition, patrolPoint);
        Gizmos.DrawSphere(patrolPoint, 0.2f);

        if (playerTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }
}