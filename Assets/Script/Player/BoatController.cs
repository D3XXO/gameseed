using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public float originalMoveSpeed;
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
    private Animator animator;

    void Start()
    {
        isoUp = new Vector2(Mathf.Sin(Mathf.PI / 6), Mathf.Cos(Mathf.PI / 6)).normalized;
        isoRight = new Vector2(Mathf.Cos(Mathf.PI / 6), -Mathf.Sin(Mathf.PI / 6)).normalized;
        isoDown = -isoUp;
        isoLeft = -isoRight;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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

        if (animator != null)
        {
            animator.SetFloat("LastInputX", 0);
            animator.SetFloat("LastInputY", 1); // Default to facing up
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
            animator.SetBool("isWalking", false);
        }

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
            moveSpeed = data.currentMoveSpeed;
            originalMoveSpeed = data.currentMoveSpeed;
            SetHealth(data.playerCurrentHP, data.playerMaxHP);
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

            // Update animation parameters
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
                animator.SetFloat("InputX", inputDirection.x);
                animator.SetFloat("InputY", inputDirection.y);

                // Only update last input when actually moving
                animator.SetFloat("LastInputX", inputDirection.x);
                animator.SetFloat("LastInputY", inputDirection.y);
            }
        }
        else
        {
            _isMoving = false;
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                // Reset current input but keep last input
                animator.SetFloat("InputX", 0);
                animator.SetFloat("InputY", 0);
            }
        }
    }

    public static BoatController Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;
        originalMoveSpeed += amount;
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

    public void LoadFromGameData(GameData data)
    {
        // Load stat movement
        moveSpeed = data.currentMoveSpeed;
        originalMoveSpeed = data.currentMoveSpeed;

        // Load stat health
        maxHP = data.playerMaxHP;
        currentHP = data.playerCurrentHP;

        // Update UI health jika ada
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public GameData SaveToGameData(GameData data)
    {
        data.currentMoveSpeed = moveSpeed;
        data.playerMaxHP = maxHP;
        data.playerCurrentHP = currentHP;
        return data;
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