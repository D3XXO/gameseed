using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
    public float gridSize;
    private Vector2 inputDirection;
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

    private Vector2 isoUp;
    private Vector2 isoRight;
    private Vector2 isoDown;
    private Vector2 isoLeft;

    void Start()
    {
        isoUp = new Vector2(Mathf.Sin(Mathf.PI / 6), Mathf.Cos(Mathf.PI / 6)).normalized;
        isoRight = new Vector2(Mathf.Cos(Mathf.PI / 6), -Mathf.Sin(Mathf.PI / 6)).normalized;
        isoDown = -isoUp;
        isoLeft = -isoRight;

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

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (SaveLoadManager.Instance != null)
        {
            GameData data = SaveLoadManager.Instance.LoadGame();
            string currentScene = SceneManager.GetActiveScene().name;

            if (SaveLoadManager.Instance != null &&
            Array.Exists(SaveLoadManager.Instance.GameplayScenes,
            scene => scene == currentScene && scene != "Harbour"))
            {
                data.lastGameplaySceneName = currentScene;
                SaveLoadManager.Instance.SaveGame(data);
            }
        }
    }

    void Update()
    {
        HandleInput();
        HandleLightingInput();
        HandleInventoryInput();
    }

    void FixedUpdate()
    {
        MoveBoat();
    }

    void HandleInput()
    {
        inputDirection = Vector2.zero;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0) inputDirection += isoUp;
        if (vertical < 0) inputDirection += isoDown;
        if (horizontal > 0) inputDirection += isoRight;
        if (horizontal < 0) inputDirection += isoLeft;

        if (inputDirection.magnitude > 0)
        {
            inputDirection.Normalize();
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHP += amount;
        currentHP += amount; // Also heal the boat
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
    void MoveBoat()
    {
        if (isKnockedBack || !movementEnabled) return;

        if (_isMoving)
        {
            Vector2 movement = inputDirection * currentSpeed * Time.fixedDeltaTime;
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

    public void SetHealth(int current, int max)
    {
        currentHP = Mathf.Clamp(current, 0, max);
        maxHP = max;
        OnHealthChanged?.Invoke(currentHP, maxHP);
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
                SceneManager.LoadScene("Harbour");
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

    void HandleInventoryInput()
    {
        if (SceneManager.GetActiveScene().name == "Harbour") return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryUI.Instance.ToggleInventory();
        }
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