using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Serializable class to store fish data
[System.Serializable]
public class Fish
{
    public string fishName;      // Name of the fish
    public float weight;          // Weight of the fish
    public float size;            // Size of the fish
    public ItemData itemData;     // Associated inventory item data
    public float strength;        // Fish's struggle strength (0-1)

    // Constructor to initialize fish properties
    public Fish(string name, float weight, float size, ItemData data = null, float strength = 0.5f)
    {
        this.fishName = name;
        this.weight = weight;
        this.size = size;
        this.itemData = data;
        this.strength = strength;
    }
}

public class Fishing : MonoBehaviour
{
    [Header("Fishing Settings")]
    private bool canFish = false;             // Flag if player can fish (boat stopped)
    private bool isFishing = false;           // Flag if currently fishing
    private bool fishCaught = false;          // Flag if fish was caught
    private bool isThrowing = false;          // Flag if throwing line
    private bool fishBiteDetected = false;    // Flag if fish is biting
    private bool waitingToReel = false;       // Flag waiting for player to reel
    public float fishingProgress;             // Progress towards catching fish (0-1)

    [Header("Throwing Settings")]
    public float throwDistance;               // Max distance to throw line
    public float throwPower;                  // Current throw power
    public float maxThrowPower;               // Max throw power
    public float throwPowerCycleSpeed;        // Speed of power meter cycling
    private int throwPowerDirection;          // Direction power meter is moving (1 or -1)
    // ADDED: Variables for the new throw target mechanic
    public RectTransform throwFishIndicator;  // UI for the fish target on the throw bar
    public float throwFishSpeed = 0.5f;       // Speed of the moving fish target
    private float throwFishPosition;          // Normalized position of the fish target (0-1)
    private int throwFishDirection;           // Movement direction of fish target
    private float throwAccuracy;              // Stores accuracy of the throw (0-1)

    [Header("Balancing Bar")]
    public RectTransform balancePlayerIndicator;  // UI element for player position
    public RectTransform balanceTargetIndicator; // UI element for target position
    public float balancePosition;             // Current player balance position (-1 to 1)
    public float balanceTarget;               // Target balance position (-1 to 1)
    public float balanceSpeed;                // Speed of balance movement
    public float balanceSensitivity;          // Sensitivity to player input
    public float balanceTargetChangeMinInterval; // Min time before target changes
    public float balanceTargetChangeMaxInterval; // Max time before target changes
    public float balanceRandomTargetMin;      // Min random target value
    public float balanceRandomTargetMax;      // Max random target value

    [Header("Reeling Bar")]
    public Image reelIndicator;               // UI element for reel power
    public float reelPower;                   // Current reel power (0-1)
    public float reelGainSpeed;               // Speed reel power increases
    public float reelDecaySpeed;              // Speed reel power decreases
    public float maxReelPower;                // Max reel power
    public float tensionIncreaseRate;         // Rate line tension increases
    public float tensionDecreaseRate;         // Rate line tension decreases
    public float maxLineTension;              // Max line tension before break
    public float currentLineTension;          // Current line tension

    // ADDED: Header and variables for the reel-to-balance interaction
    [Header("Reel-Balance Interaction")]
    public float weakReelThreshold = 0.2f;    // Normalized power threshold for "too weak"
    public float strongReelThreshold = 0.8f;  // Normalized power threshold for "too strong"
    public float reelBalanceInfluenceDelay = 2.0f; // Time until weak/strong reel affects balance
    public float reelBalanceInfluenceStrength = 0.5f; // How much it affects balance
    public float prolongedInfluenceDelay = 5.0f; // Time for the "heavy" effect
    public float prolongedInfluenceMultiplier = 2.0f; // Multiplier for the "heavy" effect
    private float timeHeldTooWeak = 0f;       // Timer for holding reel power too low
    private float timeHeldTooStrong = 0f;     // Timer for holding reel power too high

    [Header("UI Elements")]
    public Image progressBar;                 // Progress bar UI
    public GameObject fishingUI;              // Main fishing UI panel
    public Camera mainCamera;                 // Reference to main camera
    public float zoomedSize;                  // Camera size when fishing
    public float normalSize;                  // Normal camera size
    public float zoomSpeed;                   // Camera zoom speed
    public Image reelBarBackground;           // Reel bar background
    public Image balanceBarBackground;        // Balance bar background
    public Text tensionText;                  // Text showing line tension

    [Header("Bar Visuals Properties")]
    public float reelBarVisualRange;          // Visual range for reel bar
    public float balanceBarVisualRange;       // Visual range for balance bar

    [Header("Fish Data")]
    public List<Fish> fishList;               // List of possible fish to catch
    private Fish currentFish;                 // Currently hooked fish

    [Header("Bite Indicator")]
    public GameObject biteIndicator;          // UI indicator for fish bite

    [Header("Fishing Line Visuals")]
    public LineRenderer lineRenderer;         // Visual fishing line
    public Transform fishingRodTip;           // Fishing rod tip position
    public float lineOffsetZ = 0.1f;          // Z-offset for line rendering

    private BoatController boatMovement;      // Reference to boat controller
    private float randomTargetChangeTimer = 0f; // Timer for target changes
    private float currentRandomTargetChangeInterval; // Current interval for target change

    [SerializeField] private WorldTime.WorldTime worldTime; // Reference to world time system

    void Start()
    {
        // Initialize references and UI
        boatMovement = GetComponent<BoatController>();
        if (fishingUI != null) fishingUI.SetActive(false);
        if (biteIndicator != null) biteIndicator.SetActive(false);
        if (tensionText != null) tensionText.text = "";
        // ADDED: Ensure throw fish indicator is also hidden on start
        if (throwFishIndicator != null) throwFishIndicator.gameObject.SetActive(false);


        if (mainCamera == null) mainCamera = Camera.main;

        // Set up fishing line visuals
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        // Find fishing rod tip if not assigned
        if (fishingRodTip == null)
        {
            fishingRodTip = transform.Find("FishingRodTip") ?? transform;
        }
    }

    void Update()
    {
        // Control boat movement based on fishing state
        if (boatMovement != null)
        {
            boatMovement.SetMovementEnabled(!(isFishing || isThrowing || fishBiteDetected));
        }

        // Check if player can fish (boat must be stopped)
        if (boatMovement != null && !boatMovement.IsMoving())
        {
            canFish = true;
        }
        else
        {
            canFish = false;
            if (isFishing || isThrowing || fishBiteDetected) CancelFishing();
        }

        // Start fishing if conditions are met and F key pressed
        if (canFish && !isFishing && !isThrowing && !fishBiteDetected && Input.GetKeyDown(KeyCode.F))
        {
            StartThrowing();
        }

        // Handle different fishing states
        if (isThrowing)
        {
            HandleThrowing();
            UpdateFishingLineVisual();
        }
        else if (isFishing && !fishCaught)
        {
            HandleFishingProgress();
            HandleBalanceBar();
            HandleReelBar();
            UpdateFishingLineVisual();

            // Check for line break or fish escape
            if (currentLineTension >= maxLineTension || (currentLineTension <= 0f && fishingProgress < 0.99f))
            {
                CancelFishing();
            }

            // Check if fish was caught
            if (fishingProgress >= 1f) CatchFish();
        }

        // Handle reel input after bite detection
        if (waitingToReel && Input.GetKeyDown(KeyCode.Space))
        {
            BeginFishing();
        }

        // Update tension text display
        if (tensionText != null)
        {
            tensionText.text = $"Tension: {Mathf.RoundToInt(currentLineTension)} / {Mathf.RoundToInt(maxLineTension)}";
        }

        HandleCameraZoom();
    }

    // Start the throwing phase of fishing
    void StartThrowing()
    {
        isThrowing = true;
        isFishing = false;
        fishBiteDetected = false;
        fishCaught = false;

        // Activate UI elements for throwing
        if (fishingUI != null) fishingUI.SetActive(true);
        // MODIFIED: Start player and fish indicators at 0
        throwPower = 0f;
        throwPowerDirection = 1;
        throwFishPosition = 0f;
        throwFishDirection = 1;

        // Set up UI visibility
        if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(true);
        if (reelIndicator != null) reelIndicator.gameObject.SetActive(true);
        // ADDED: Show the fish target indicator for throwing
        if (throwFishIndicator != null) throwFishIndicator.gameObject.SetActive(true);

        if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(false);
        if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(false);
        if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);

        // Initialize balance target change timer
        randomTargetChangeTimer = 0f;
        currentRandomTargetChangeInterval = Random.Range(balanceTargetChangeMinInterval, balanceTargetChangeMaxInterval);
    }

    // Handle the throwing mechanics
    void HandleThrowing()
    {
        // Cycle throw power between 0 and max
        throwPower += throwPowerDirection * throwPowerCycleSpeed * Time.deltaTime;
        throwPower = Mathf.Clamp(throwPower, 0f, maxThrowPower);
        if (throwPower >= maxThrowPower || throwPower <= 0f)
        {
            throwPowerDirection *= -1;
        }

        // Update throw power UI
        if (reelIndicator != null)
        {
            reelIndicator.fillAmount = throwPower / maxThrowPower;
        }

        // ADDED: Move the fish target indicator randomly on the bar
        throwFishPosition += throwFishDirection * throwFishSpeed * Time.deltaTime;
        throwFishPosition = Mathf.Clamp01(throwFishPosition);
        if (throwFishPosition >= 1f || throwFishPosition <= 0f)
        {
            throwFishDirection *= -1;
        }

        // ADDED: Update the visual position of the fish target indicator
        if (throwFishIndicator != null)
        {
            float fishIndicatorX = Mathf.Lerp(-reelBarVisualRange / 2f, reelBarVisualRange / 2f, throwFishPosition);
            throwFishIndicator.anchoredPosition = new Vector2(fishIndicatorX, throwFishIndicator.anchoredPosition.y);
        }


        // Handle throw release
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isThrowing = false;
            // ADDED: Hide the fish throw indicator after throwing
            if (throwFishIndicator != null) throwFishIndicator.gameObject.SetActive(false);

            // ADDED: Calculate throw accuracy based on alignment with fish target
            float playerPowerNormalized = throwPower / maxThrowPower;
            throwAccuracy = 1f - Mathf.Abs(playerPowerNormalized - throwFishPosition);
            throwAccuracy = Mathf.Clamp01(throwAccuracy);

            // Show all fishing UI elements
            if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(true);
            if (reelIndicator != null) reelIndicator.gameObject.SetActive(true);
            if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(true);
            if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(true);
            if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(true);
            if (progressBar != null) progressBar.gameObject.SetActive(true);

            // MODIFIED: Check accuracy instead of raw power
            if (throwAccuracy < 0.1f)
            {
                Debug.Log("Bad throw. Missed the target.");
                CancelFishing();
                return;
            }

            // Start fish bite detection
            StartCoroutine(DetectFishBite());
        }
    }

    // Coroutine to detect if a fish bites
    IEnumerator DetectFishBite()
    {
        fishCaught = false;

        // Show fishing line
        if (lineRenderer != null && fishingRodTip != null)
        {
            lineRenderer.enabled = true;
            Vector3 start = fishingRodTip.position;
            Vector3 end = fishingRodTip.position + transform.right * throwPower * throwDistance / maxThrowPower;
            start.z += lineOffsetZ;
            end.z += lineOffsetZ;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        // MODIFIED: Calculate bite delay based on throw accuracy (better accuracy = faster bite)
        float biteDelay = 1f + (1f - throwAccuracy) * 4f; // Perfect throw waits 1s, worst waits 5s
        yield return new WaitForSeconds(biteDelay);

        // MODIFIED: Calculate bite chance based on throw accuracy
        float biteChance = throwAccuracy * 0.9f; // Max 90% chance of bite on a perfect throw
        if (Random.value < biteChance)
        {
            // Fish bit - wait for player to reel
            fishBiteDetected = true;
            waitingToReel = true;
            if (biteIndicator != null) biteIndicator.SetActive(true);
            Debug.Log("Fish is biting! Press Space to reel!");

            // Give player limited time to respond
            yield return new WaitForSeconds(2f);

            if (waitingToReel)
            {
                Debug.Log("No reel input detected. Cancelling fishing.");
                CancelFishing();
            }
        }
        else
        {
            Debug.Log("No fish bite. Try again.");
            CancelFishing();
        }
    }

    // Begin the fishing minigame after bite detection
    void BeginFishing()
    {
        waitingToReel = false;
        fishBiteDetected = false;
        isFishing = true;

        // Pause game time while fishing
        if (worldTime != null) worldTime.SetPaused(true);

        // Select random fish from list
        currentFish = fishList[Random.Range(0, fishList.Count)];
        if (currentFish == null)
        {
            Debug.LogError("No fish found.");
            CancelFishing();
            return;
        }

        // Initialize fishing variables
        fishingProgress = 0f;
        reelPower = maxReelPower / 2f;
        balancePosition = 0f;
        balanceTarget = Random.Range(balanceRandomTargetMin, balanceRandomTargetMax);
        currentLineTension = maxLineTension / 2f;

        // ADDED: Reset reel-balance interaction timers
        timeHeldTooWeak = 0f;
        timeHeldTooStrong = 0f;


        if (biteIndicator != null) biteIndicator.SetActive(false);

        Debug.Log($"Fishing started! Target: {currentFish.fishName}");
    }

    // Cancel fishing and reset all states
    void CancelFishing()
    {
        isFishing = false;
        isThrowing = false;
        fishCaught = false;
        fishBiteDetected = false;
        waitingToReel = false;

        // ADDED: Reset timers on cancel as well
        timeHeldTooWeak = 0f;
        timeHeldTooStrong = 0f;

        ResetUIElements();

        // Resume game time
        if (worldTime != null) worldTime.SetPaused(false);
    }

    // Successful fish catch
    void CatchFish()
    {
        fishCaught = true;
        isFishing = false;

        ResetUIElements();

        // Add fish to inventory if possible
        if (fishList != null && fishList.Count > 0 && currentFish != null)
        {
            if (currentFish.itemData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(currentFish.itemData, 1);
                Debug.Log($"Caught {currentFish.fishName}!");
            }
        }

        // Resume game time
        if (worldTime != null) worldTime.SetPaused(false);
    }

    // Handle fishing progress based on balance accuracy
    void HandleFishingProgress()
    {
        if (currentFish == null) return;

        // Calculate how close player is to target (0-1)
        float balanceAccuracy = 1f - Mathf.Abs(balancePosition - balanceTarget);
        float baseChange = Time.deltaTime * 0.1f;
        float progressRate;

        // Different progress rates based on accuracy
        if (balanceAccuracy > 0.7f)
        {
            progressRate = balanceAccuracy * (1f - currentFish.strength) * baseChange;
        }
        else if (balanceAccuracy > 0.4f)
        {
            progressRate = balanceAccuracy * (0.5f - currentFish.strength * 0.5f) * baseChange;
        }
        else
        {
            progressRate = -currentFish.strength * baseChange;
        }

        // Update progress and clamp between 0-1
        fishingProgress += progressRate;
        fishingProgress = Mathf.Clamp(fishingProgress, 0f, 1f);

        // Update progress bar UI
        if (progressBar != null)
            progressBar.fillAmount = fishingProgress;
    }

    // Handle reel power mechanics
    void HandleReelBar()
    {
        // Increase reel power when holding space, decrease otherwise
        if (Input.GetKey(KeyCode.Space))
        {
            reelPower += reelGainSpeed * Time.deltaTime * maxReelPower;
        }
        else
        {
            reelPower -= reelDecaySpeed * Time.deltaTime * maxReelPower;
        }

        // Clamp reel power and update UI
        reelPower = Mathf.Clamp(reelPower, 0f, maxReelPower);
        if (reelIndicator != null)
        {
            reelIndicator.fillAmount = reelPower / maxReelPower;
        }

        if (currentFish == null) return;

        // Calculate line tension based on reel power and fish strength
        float powerNormalized = reelPower / maxReelPower;
        float tensionChange = 0f;

        if (powerNormalized >= 0.99f || powerNormalized <= 0.01f)
        {
            // Extreme reel positions increase tension quickly
            tensionChange = tensionIncreaseRate * 10f * Time.deltaTime * currentFish.strength;
        }
        else
        {
            if (powerNormalized > 0.9f)
            {
                // High reel position increases tension
                tensionChange = tensionIncreaseRate * (powerNormalized - 0.9f) * Time.deltaTime * currentFish.strength;
            }
            else if (powerNormalized < 0.1f)
            {
                // Low reel position increases tension
                tensionChange = tensionIncreaseRate * (0.1f - powerNormalized) * Time.deltaTime * currentFish.strength;
            }
            else
            {
                // Middle position decreases tension
                tensionChange = -tensionDecreaseRate * Time.deltaTime;
            }
        }

        // Update and clamp line tension
        currentLineTension += tensionChange;
        currentLineTension = Mathf.Clamp(currentLineTension, 0f, maxLineTension);

        // ADDED: Logic for tracking time held in weak/strong zones
        if (powerNormalized < weakReelThreshold)
        {
            timeHeldTooWeak += Time.deltaTime;
            timeHeldTooStrong = 0f; // Reset other timer
        }
        else if (powerNormalized > strongReelThreshold)
        {
            timeHeldTooStrong += Time.deltaTime;
            timeHeldTooWeak = 0f; // Reset other timer
        }
        else
        {
            // If in the safe zone, reset both timers
            timeHeldTooWeak = 0f;
            timeHeldTooStrong = 0f;
        }
    }

    // Handle balance bar mechanics
    void HandleBalanceBar()
    {
        if (currentFish == null) return;

        // Change target position at random intervals
        randomTargetChangeTimer += Time.deltaTime;
        if (randomTargetChangeTimer >= currentRandomTargetChangeInterval)
        {
            randomTargetChangeTimer = 0f;
            balanceTarget = Random.Range(balanceRandomTargetMin, balanceRandomTargetMax);
            currentRandomTargetChangeInterval = Random.Range(balanceTargetChangeMinInterval, balanceTargetChangeMaxInterval);
        }

        // Get player input
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= balanceSensitivity;
        if (Input.GetKey(KeyCode.D)) input += balanceSensitivity;

        // Add random fish influence based on fish strength
        float fishInfluence = (Random.value * 2f - 1f) * currentFish.strength * 0.1f * Time.deltaTime;
        
        // MODIFIED: Added reel influence to the balance calculation
        float reelInfluence = 0f;
        if (timeHeldTooWeak > reelBalanceInfluenceDelay)
        {
            float currentInfluence = -reelBalanceInfluenceStrength;
            // Apply "heavy" multiplier if held for too long
            if (timeHeldTooWeak > prolongedInfluenceDelay)
            {
                currentInfluence *= prolongedInfluenceMultiplier;
            }
            reelInfluence = currentInfluence;
        }
        else if (timeHeldTooStrong > reelBalanceInfluenceDelay)
        {
            float currentInfluence = reelBalanceInfluenceStrength;
            // Apply "heavy" multiplier if held for too long
            if (timeHeldTooStrong > prolongedInfluenceDelay)
            {
                currentInfluence *= prolongedInfluenceMultiplier;
            }
            reelInfluence = currentInfluence;
        }
        
        balancePosition += (input * balanceSpeed + reelInfluence) * Time.deltaTime + fishInfluence;
        balancePosition = Mathf.Clamp(balancePosition, -1f, 1f);

        // Update balance bar UI indicators
        if (balancePlayerIndicator != null && balanceTargetIndicator != null)
        {
            float px = Mathf.Lerp(-balanceBarVisualRange / 2f, balanceBarVisualRange / 2f, (balancePosition + 1f) / 2f);
            balancePlayerIndicator.anchoredPosition = new Vector2(px, balancePlayerIndicator.anchoredPosition.y);

            float tx = Mathf.Lerp(-balanceBarVisualRange / 2f, balanceBarVisualRange / 2f, (balanceTarget + 1f) / 2f);
            balanceTargetIndicator.anchoredPosition = new Vector2(tx, balanceTargetIndicator.anchoredPosition.y);
        }
    }

    // Handle camera zoom during fishing
    void HandleCameraZoom()
    {
        if (mainCamera == null || !mainCamera.orthographic) return;

        // Zoom in when fishing, zoom out otherwise
        float targetSize = (isFishing || isThrowing || fishBiteDetected) ? zoomedSize : normalSize;
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    // Update fishing line visuals
    void UpdateFishingLineVisual()
    {
        if (lineRenderer != null && fishingRodTip != null)
        {
            // Calculate line positions based on throw power
            Vector3 start = fishingRodTip.position;
            Vector3 end = fishingRodTip.position + transform.right * throwPower * throwDistance / maxThrowPower;
            start.z += lineOffsetZ;
            end.z += lineOffsetZ;

            // Update line renderer positions
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }

    // Reset all UI elements to default state
    private void ResetUIElements()
    {
        if (fishingUI != null) fishingUI.SetActive(false);
        if (biteIndicator != null) biteIndicator.SetActive(false);
        if (tensionText != null) tensionText.text = "";
        
        // ADDED: Ensure throw fish indicator is hidden on reset
        if (throwFishIndicator != null) throwFishIndicator.gameObject.SetActive(false);

        // Hide all fishing UI elements
        if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(false);
        if (reelIndicator != null) reelIndicator.gameObject.SetActive(false);
        if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(false);
        if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(false);
        if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);

        // Reset camera and line visuals
        if (mainCamera != null) mainCamera.orthographicSize = normalSize;
        if (lineRenderer != null) lineRenderer.enabled = false;
    }
}