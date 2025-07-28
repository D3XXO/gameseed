using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fishing : MonoBehaviour
{
    [Header("Fishing Settings")]
    public bool canFish = false;
    public bool isFishing = false;
    public bool fishCaught = false;
    public bool isThrowing = false; // New state for throwing
    public bool fishBiteDetected = false; // New state for fish bite detection
    public float fishingProgress = 0f;
    public float fishingDuration = 10f;

    [Header("Throwing Settings")]
    public float throwDistance = 10f; // Distance the line can be thrown
    public float throwPower = 0f; // Power based on how many times space is pressed
    public float maxThrowPower = 5f; // Maximum power for throwing

    [Header("Balancing Bar")]
    public Image balanceBar;
    public float balancePosition = 0f;
    public float balanceTarget = 0f;
    public float balanceSpeed = 2f;
    public float balanceSensitivity = 0.5f;

    [Header("Reeling Bar")]
    public Image reelBar;
    public float reelPower = 0f;
    public float reelGainSpeed = 1f;
    public float reelDecaySpeed = 0.5f;
    public float maxReelPower = 100f;

    [Header("UI Elements")]
    public Image progressBar;
    public GameObject fishingUI;
    public Camera mainCamera;
    public float zoomedFOV = 30f;
    public float normalFOV = 60f;
    public float zoomSpeed = 2f;

    [Header("Fish Data")]
    public List<Fish> fishList;

    [Header("Bite Indicator")]
    public GameObject biteIndicator; // UI element to indicate a fish bite

    private BoatController boatMovement;
    private float randomDirectionChangeTimer = 0f;
    private float randomDirectionChangeInterval = 1.5f;

    void Start()
    {
        boatMovement = GetComponent<BoatController>();
        fishingUI.SetActive(false);
        biteIndicator.SetActive(false); // Hide bite indicator initially
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        InitializeFishList();
    }

    void Update()
    {
        if (!isFishing && boatMovement != null && !boatMovement.IsMoving())
        {
            canFish = true;
        }
        else if (boatMovement != null && boatMovement.IsMoving())
        {
            canFish = false;
            if (isFishing || isThrowing)
            {
                CancelFishing();
            }
        }

        if (canFish && !isFishing && !isThrowing && Input.GetKeyDown(KeyCode.F))
        {
            StartFishing();
        }

        if (isThrowing)
        {
            HandleThrowing();
        }

        if (isFishing && !fishCaught)
        {
            HandleFishingProgress();
            HandleBalanceBar();
            HandleReelBar();

            if (fishingProgress >= 1f)
            {
                CatchFish();
            }
        }

        if (fishBiteDetected)
        {
            // Wait for player to acknowledge the bite
            if (Input.GetKeyDown(KeyCode.E)) // Press E to acknowledge the bite
            {
                fishBiteDetected = false; // Reset bite detection
                fishingProgress = 0f; // Reset fishing progress
                reelPower = 0f; // Reset reel power
                balancePosition = 0f; // Reset balance position
                biteIndicator.SetActive(false); // Hide bite indicator
                Debug.Log("Fish is biting! Start reeling!");
            }
        }

        HandleCameraZoom();
    }

    void StartFishing()
    {
        isThrowing = true; // Start throwing state
        fishingUI.SetActive(true);
        mainCamera.fieldOfView = zoomedFOV; // Zoom in when fishing starts
        throwPower = 0f; // Reset throw power
    }

    void HandleThrowing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            throwPower += 1f; // Increase throw power for each space press
            throwPower = Mathf.Clamp(throwPower, 0f, maxThrowPower); // Clamp to max throw power
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Throw the line when space is released
            isThrowing = false; // Exit throwing state
            StartCoroutine(DetectFishBite()); // Start detecting fish bite
            Debug.Log($"Line thrown with power: {throwPower}, Distance: {throwDistance}");

            // Reset throw power for the next throw
            throwPower = 0f;
        }
    }

    IEnumerator DetectFishBite()
    {
        // Simulate a delay for fish to bite
        yield return new WaitForSeconds(2f); // Wait for 2 seconds (you can adjust this)

        // Random chance for a fish to bite
        if (Random.value < 0.5f) // 50% chance to detect a bite
        {
            fishBiteDetected = true; // Set bite detected
            biteIndicator.SetActive(true); // Show bite indicator
            Debug.Log("A fish has bitten the bait!");
        }
        else
        {
            Debug.Log("No fish bite detected.");
        }
    }

    void CancelFishing()
    {
        isFishing = false;
        isThrowing = false; // Reset throwing state
        fishCaught = false;
        fishBiteDetected = false; // Reset fish bite detection
        biteIndicator.SetActive(false); // Hide bite indicator
        fishingUI.SetActive(false);
    }

    void CatchFish()
    {
        fishCaught = true;
        isFishing = false;
        fishingUI.SetActive(false);
        
        Fish caughtFish = fishList[Random.Range(0, fishList.Count)];
        Debug.Log($"Fish caught: {caughtFish.fishName}, Weight: {caughtFish.weight}, Size: {caughtFish.size}");
    }

    void HandleFishingProgress()
    {
        float progressRate = 0.1f;
        progressRate += reelPower / maxReelPower * 0.2f;
        
        float balanceAccuracy = 1f - Mathf.Abs(balancePosition - balanceTarget);
        progressRate *= balanceAccuracy;
        
        fishingProgress += progressRate * Time.deltaTime / fishingDuration;
        fishingProgress = Mathf.Clamp(fishingProgress, 0f, 1f);
        
        if (progressBar != null)
        {
            progressBar.fillAmount = fishingProgress;
        }
    }

    void HandleBalanceBar()
    {
        randomDirectionChangeTimer += Time.deltaTime;
        if (randomDirectionChangeTimer >= randomDirectionChangeInterval)
        {
            randomDirectionChangeTimer = 0f;
            float changeAmount = Random.Range(-0.5f, 0.5f);
            balanceTarget = Mathf.Clamp(balanceTarget + changeAmount, -1f, 1f);
            randomDirectionChangeInterval = Random.Range(0.8f, 2f);
        }

        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= balanceSensitivity;
        if (Input.GetKey(KeyCode.D)) input += balanceSensitivity;

        balancePosition += input * Time.deltaTime * balanceSpeed;
        balancePosition = Mathf.Clamp(balancePosition, -1f, 1f);

        balancePosition = Mathf.MoveTowards(balancePosition, balanceTarget, Time.deltaTime * 0.5f);

        if (balanceBar != null)
        {
            balanceBar.fillAmount = 0.5f + balancePosition * 0.5f;
        }
    }

    void HandleReelBar()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            reelPower += reelGainSpeed * Time.deltaTime * maxReelPower;
        }
        else
        {
            reelPower -= reelDecaySpeed * Time.deltaTime * maxReelPower;
        }
        reelPower = Mathf.Clamp(reelPower, 0f, maxReelPower);

        if (reelBar != null)
        {
            reelBar.fillAmount = reelPower / maxReelPower;
        }
    }

    void HandleCameraZoom()
    {
        if (isFishing)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, normalFOV, Time.deltaTime * zoomSpeed);
        }
    }

    void InitializeFishList()
    {
        fishList = new List<Fish>
        {
            new Fish("Aleefish", 2.5f, 30f),
            new Fish("Dafish", 5.0f, 60f),
            new Fish("Hafeesh", 3.0f, 40f),
            new Fish("Nyarvish", 7.0f, 80f),
            new Fish("Absolish", 4.5f, 50f)
        };
    }
}
