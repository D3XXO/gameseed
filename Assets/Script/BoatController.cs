using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class BoatController : MonoBehaviour
{
    [Header("Layer Options")]
    public LayerMask groundLayer;
    public LayerMask collectibleLayer;

    [Header("Boat Health")]
    public int maxHP;
    public int currentHP;
    public event Action<int, int> OnHealthChanged;
    public int collisionDamage;

    [Header("Movement")]
    public float moveSpeed;
    public float rotationSpeed;
    public float gridSize;
    private float verticalInput;
    private float horizontalInput;
    private bool isMoving = false;
    public Transform collectPivot;

    void Start()
    {
        transform.position = SnapToGrid(transform.position);

        if (collectPivot == null)
        {
            collectPivot = transform.Find("Collect Pivot");
            if (collectPivot == null)
            {
                GameObject newPivot = new GameObject("Collect Pivot");
                newPivot.transform.SetParent(transform);
                newPivot.transform.localPosition = Vector3.zero;
                collectPivot = newPivot.transform;
            }
        }

        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Update()
    {
        HandleInput();
        MoveAndRotateBoat();
    }

    void HandleInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        if (verticalInput < 0)
        {
            verticalInput = 0;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        isMoving = Mathf.Abs(verticalInput) > 0.01f;
    }

    void MoveAndRotateBoat()
    {
        if (horizontalInput != 0)
        {
            transform.Rotate(0, 0, -horizontalInput * rotationSpeed * Time.deltaTime);
        }

        if (isMoving)
        {
            Vector3 movement = transform.up * verticalInput * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    public Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & collectibleLayer) != 0)
        {
            CollectibleItem item = other.GetComponent<CollectibleItem>();
            if (item != null)
            {
                item.Collect();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(collisionDamage);
        }
    }

    public Transform GetCollectPivot()
    {
        return collectPivot;
    }

    public void Heal(int amount)
    {
        if (currentHP < maxHP)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            OnHealthChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"Boat healed for {amount} HP. Current HP: {currentHP}/{maxHP}");
        }
        else
        {
            Debug.Log("Boat is already at full HP.");
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHP > 0)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHealthChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"Boat took {amount} damage. Current HP: {currentHP}/{maxHP}");

            if (currentHP <= 0)
            {
                Destroy(gameObject);
                Debug.Log("Boat is destroyed! Game Over.");
            }
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}