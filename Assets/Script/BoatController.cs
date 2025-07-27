using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

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

    private Rigidbody2D rb;
    private float currentSpeed;

    [Header("Lightning Effects")]
    public float lightningHitSlowdown = 0.7f; 
    public float slowdownDuration = 1.5f; 
    private float originalMoveSpeed;
    private bool isSlowed = false;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.5f;
    private bool isKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.drag = 1f; 
            rb.angularDrag = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        originalMoveSpeed = moveSpeed;
        currentSpeed = moveSpeed;

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
    }

    void FixedUpdate()
    {
        MoveAndRotateBoat();
    }

    void HandleInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        if (verticalInput < 0) verticalInput = 0;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        isMoving = Mathf.Abs(verticalInput) > 0.01f;
    }

    void MoveAndRotateBoat()
    {
        if (isKnockedBack) return; // Skip movement during knockback

        if (horizontalInput != 0)
        {
            float rotation = -horizontalInput * rotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }

        if (isMoving)
        {
            Vector2 movement = transform.up * verticalInput * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!isKnockedBack)
        {
            StartCoroutine(KnockbackRoutine(direction, force));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero; // Stop current movement
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero; // Stop knockback movement
        isKnockedBack = false;
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

            FlashRed();
            ApplyLightningSlowdown();

            if (currentHP <= 0)
            {
                Destroy(gameObject);
                Debug.Log("Boat is destroyed! Game Over.");
            }
        }
    }

    void ApplyLightningSlowdown()
    {
        if (isSlowed) return;

        isSlowed = true;
        currentSpeed = moveSpeed * lightningHitSlowdown;
        Invoke(nameof(ResetSpeed), slowdownDuration);
    }

    void ResetSpeed()
    {
        currentSpeed = moveSpeed;
        isSlowed = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void FlashRed()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color original = renderer.color;
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        renderer.color = original;
    }
}