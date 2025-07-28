using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using UnityEngine.Rendering.Universal;

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
    private bool _isMoving = false;
    private bool movementEnabled = true;
    public Transform collectPivot;

    private Rigidbody2D rb;
    private float currentSpeed;

    [Header("Lightning Effects")]
    public float lightningHitSlowdown;
    public float slowdownDuration;
    private float originalMoveSpeed;
    private bool isSlowed = false;

    [Header("Knockback Settings")]
    public float knockbackDuration;
    private bool isKnockedBack = false;

    [Header("Lighting")]
    public Light2D boatLight;
    private bool isLightOn = false;

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

        if (boatLight == null)
        {
            boatLight = GetComponentInChildren<Light2D>();
        }

        if (boatLight != null)
        {
            boatLight.enabled = false;
        }

        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Update()
    {
        HandleInput();
        HandleLightingInput();
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
        _isMoving = Mathf.Abs(verticalInput) > 0.01f;
    }

    void MoveAndRotateBoat()
    {
        if (isKnockedBack || !movementEnabled) return;

        if (horizontalInput != 0)
        {
            float rotation = -horizontalInput * rotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }

        if (_isMoving)
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
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
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
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            TakeDamage(collisionDamage, false);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage, false);
        }
        else if (collision.gameObject.CompareTag("Thunder"))
        {
            TakeDamage(collisionDamage, true);
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
        }
    }

    public void TakeDamage(int amount, bool applyFlash = true)
    {
        if (currentHP > 0)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            OnHealthChanged?.Invoke(currentHP, maxHP);

            if (applyFlash)
            {
                FlashRed();
            }
            ApplyLightningSlowdown();

            if (currentHP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    void HandleLightingInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            ToggleBoatLight();
        }
    }

    void ToggleBoatLight()
    {
        if (boatLight != null)
        {
            isLightOn = !isLightOn;
            boatLight.enabled = isLightOn;
        }
    }

    public bool IsLightOn()
    {
        return isLightOn;
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
        return _isMoving && movementEnabled;
    }

    public void SetMovementEnabled(bool enable)
    {
        movementEnabled = enable;
        if (!enable)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
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