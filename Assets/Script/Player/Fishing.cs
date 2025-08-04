using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Fish
{
    public string fishName;
    public float weight;
    public float size;
    public ItemData itemData;
    public float strength;

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
    private bool canFish = false;
    private bool isFishing = false;
    private bool fishCaught = false;
    private bool isThrowing = false;
    private bool fishBiteDetected = false;
    private bool waitingToReel = false;
    public float fishingProgress;
    private EnemyAI[] enemyAIs;

    [Header("Throwing Settings")]
    public float throwDistance;
    public float throwPower;
    public float maxThrowPower;
    public float throwPowerCycleSpeed;
    private int throwPowerDirection;

    [Header("Balancing Bar")]
    public RectTransform balancePlayerIndicator;
    public RectTransform balanceTargetIndicator;
    public float balancePosition;
    public float balanceTarget;
    public float balanceSpeed;
    public float balanceSensitivity;
    public float balanceTargetChangeMinInterval;
    public float balanceTargetChangeMaxInterval;
    public float balanceRandomTargetMin;
    public float balanceRandomTargetMax;

    [Header("Fish Caught UI")]
    public GameObject fishCaughtUI; // Parent panel untuk tampilan ikan yang ditangkap
    public Image fishImage; // Komponen Image untuk menampilkan gambar ikan
    public Text fishNameText; // Komponen Text untuk menampilkan nama ikan
    public float displayDuration = 3f;

    [Header("Reeling Bar")]
    public Image reelIndicator;
    public float reelPower;
    public float reelGainSpeed;
    public float reelDecaySpeed;
    public float maxReelPower;
    public float tensionIncreaseRate;
    public float tensionDecreaseRate;
    public float maxLineTension;
    public float currentLineTension;

    [Header("UI Elements")]
    public Image progressBar;
    public GameObject fishingUI;
    public Camera mainCamera;
    public float zoomedSize;
    public float normalSize;
    public float zoomSpeed;
    public Image reelBarBackground;
    public Image balanceBarBackground;
    public Text tensionText;

    [Header("Bar Visuals Properties")]
    public float reelBarVisualRange;
    public float balanceBarVisualRange;

    [Header("Fish Data")]
    public List<Fish> fishList;
    private Fish currentFish;

    [Header("Bite Indicator")]
    public GameObject biteIndicator;

    [Header("Fishing Line Visuals")]
    public LineRenderer lineRenderer;
    public Transform fishingRodTip;
    public float lineOffsetZ = 0.1f;

    private BoatController boatMovement;
    private float randomTargetChangeTimer = 0f;
    private float currentRandomTargetChangeInterval;

    [Header("Fishing SFX")]
    public FishingSFX fishingSFX; // Referensi ke komponen FishingSFX
    public AudioClip[] splashSounds; // Suara saat umpan masuk air
    public AudioClip[] pullSounds; // Suara saat ikan menarik
    private AudioSource audioSource; // Komponen AudioSource tambahan

    [SerializeField] private WorldTime.WorldTime worldTime;

    void Start()
    {
        boatMovement = GetComponent<BoatController>();
        enemyAIs = FindObjectsOfType<EnemyAI>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (fishingUI != null) fishingUI.SetActive(false);
        if (biteIndicator != null) biteIndicator.SetActive(false);
        if (tensionText != null) tensionText.text = "";

        if (mainCamera == null) mainCamera = Camera.main;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        if (fishingRodTip == null)
        {
            fishingRodTip = transform.Find("FishingRodTip") ?? transform;
        }

        if (fishingSFX == null)
        {
            fishingSFX = GetComponent<FishingSFX>();
            if (fishingSFX == null)
            {
                Debug.LogWarning("FishingSFX component not found!");
            }
        }
    }

    void Update()
    {
        if (boatMovement != null)
        {
            boatMovement.SetMovementEnabled(!(isFishing || isThrowing || fishBiteDetected));
        }

        if (boatMovement != null && !boatMovement.IsMoving())
        {
            canFish = true;
        }
        else
        {
            canFish = false;
            if (isFishing || isThrowing || fishBiteDetected) CancelFishing();
        }

        if (isThrowing && Input.GetKeyDown(KeyCode.F))
        {
            CancelFishing();
            return; // Keluar agar tidak masuk ke StartThrowing() lagi
        }

        if (canFish && !isFishing && !isThrowing && !fishBiteDetected && Input.GetKeyDown(KeyCode.F))
        {
            StartThrowing();
        }

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

            if (currentLineTension >= maxLineTension || (currentLineTension <= 0f && fishingProgress < 0.99f))
            {
                CancelFishing();
            }

            if (fishingProgress >= 1f) CatchFish();
        }

        if (waitingToReel && Input.GetKeyDown(KeyCode.Space))
        {
            BeginFishing();
        }

        if (tensionText != null)
        {
            tensionText.text = $"Tension: {Mathf.RoundToInt(currentLineTension)} / {Mathf.RoundToInt(maxLineTension)}";
        }

        if (fishCaught && boatMovement != null && boatMovement.IsMoving())
        {
            if (fishingSFX != null)
            {
                fishingSFX.StopReelingSound();
            }
        }

        HandleCameraZoom();
    }

    void StartThrowing()
    {
        isThrowing = true;
        isFishing = false;
        fishBiteDetected = false;
        fishCaught = false;

        if (fishingUI != null) fishingUI.SetActive(true);
        throwPower = 0f;
        throwPowerDirection = 1;

        if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(true);
        if (reelIndicator != null) reelIndicator.gameObject.SetActive(true);
        if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(false);
        if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(false);
        if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (fishingSFX != null) fishingSFX.PlayCastingSound();

        randomTargetChangeTimer = 0f;
        currentRandomTargetChangeInterval = Random.Range(balanceTargetChangeMinInterval, balanceTargetChangeMaxInterval);
    }

    void HandleThrowing()
    {
        throwPower += throwPowerDirection * throwPowerCycleSpeed * Time.deltaTime;
        if (throwPower >= maxThrowPower)
        {
            throwPower = maxThrowPower;
            throwPowerDirection = -1;
        }
        else if (throwPower <= 0f)
        {
            throwPower = 0f;
            throwPowerDirection = 1;
        }

        if (reelIndicator != null)
        {
            reelIndicator.fillAmount = throwPower / maxThrowPower;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isThrowing = false;

            if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(true);
            if (reelIndicator != null) reelIndicator.gameObject.SetActive(true);
            if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(true);
            if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(true);
            if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(true);
            if (progressBar != null) progressBar.gameObject.SetActive(true);

            if (splashSounds.Length > 0)
            {
                AudioClip randomSplash = splashSounds[Random.Range(0, splashSounds.Length)];
                audioSource.PlayOneShot(randomSplash);
            }
            if (throwPower <= 0.1f)
            {
                Debug.Log("Throw too weak.");
                CancelFishing();
                return;
            }

            StartCoroutine(DetectFishBite());
        }
    }

    IEnumerator DetectFishBite()
    {
        fishCaught = false;

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

        float biteDelay = Mathf.Clamp(throwDistance / (throwPower + 1f), 0.5f, 5f);
        yield return new WaitForSeconds(biteDelay);

        float biteChance = 0.5f + (throwPower / maxThrowPower * 0.3f);
        if (Random.value < biteChance)
        {
            fishBiteDetected = true;
            waitingToReel = true;
            if (biteIndicator != null) biteIndicator.SetActive(true);
            Debug.Log("Fish is biting! Press Space to reel!");

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

    void BeginFishing()
    {
        waitingToReel = false;
        fishBiteDetected = false;
        isFishing = true;

        if (worldTime != null) worldTime.SetPaused(true);
        if (boatMovement != null) boatMovement.SetMovementEnabled(false);
        if (enemyAIs != null)
            if (fishingSFX != null) fishingSFX.StartReelingSound();
        {
            foreach (var enemy in enemyAIs)
            {
                if (enemy != null) enemy.enabled = false;
            }
        }

        if (pullSounds.Length > 0)
        {
            AudioClip randomPull = pullSounds[Random.Range(0, pullSounds.Length)];
            audioSource.PlayOneShot(randomPull);
        }
        currentFish = fishList[Random.Range(0, fishList.Count)];
        if (currentFish == null)
        {
            Debug.LogError("No fish found.");
            CancelFishing();
            return;
        }

        fishingProgress = 0f;
        reelPower = maxReelPower / 2f;
        balancePosition = 0f;
        balanceTarget = Random.Range(balanceRandomTargetMin, balanceRandomTargetMax);
        currentLineTension = maxLineTension / 2f;

        if (biteIndicator != null) biteIndicator.SetActive(false);

        Debug.Log($"Fishing started! Target: {currentFish.fishName}");
    }

    void CancelFishing()
    {
        isFishing = false;
        isThrowing = false;
        fishCaught = false;
        fishBiteDetected = false;
        waitingToReel = false;

        if (boatMovement != null) boatMovement.SetMovementEnabled(true);
        if (fishingSFX != null) fishingSFX.StopReelingSound();
        if (enemyAIs != null)
        {
            foreach (var enemy in enemyAIs)
            {
                if (enemy != null) enemy.enabled = true;
            }
        }

        ResetUIElements();

        if (worldTime != null) worldTime.SetPaused(false);
    }

    void CatchFish()
    {
        fishCaught = true;
        isFishing = false;

        if (boatMovement != null) boatMovement.SetMovementEnabled(true);
        if (fishingSFX != null) fishingSFX.StopReelingSound();

        if (splashSounds.Length > 0)
        {
            AudioClip randomSplash = splashSounds[Random.Range(0, splashSounds.Length)];
            audioSource.PlayOneShot(randomSplash);
        }

        if (enemyAIs != null)
        {
            foreach (var enemy in enemyAIs)
            {
                if (enemy != null) enemy.enabled = true;
            }
        }

        ResetUIElements();

        if (fishList != null && fishList.Count > 0 && currentFish != null)
        {
            if (currentFish.itemData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(currentFish.itemData, 1);
                Debug.Log($"Caught {currentFish.fishName}!");

                // Tampilkan UI ikan yang ditangkap
                if (fishCaughtUI != null) fishCaughtUI.SetActive(true);
                if (fishImage != null) fishImage.sprite = currentFish.itemData.icon;
                if (fishNameText != null) fishNameText.text = currentFish.fishName;

                // Sembunyikan UI setelah beberapa detik
                StartCoroutine(HideFishCaughtUI());
            }
        }

        if (worldTime != null) worldTime.SetPaused(false);
    }

    IEnumerator HideFishCaughtUI()
    {
        yield return new WaitForSeconds(displayDuration);
        if (fishCaughtUI != null) fishCaughtUI.SetActive(false);
    }

    void HandleFishingProgress()
    {
        if (currentFish == null) return;

        float balanceAccuracy = 1f - Mathf.Abs(balancePosition - balanceTarget);
        float baseChange = Time.deltaTime * 0.1f;
        float progressRate;

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

        fishingProgress += progressRate;
        fishingProgress = Mathf.Clamp(fishingProgress, 0f, 1f);

        if (progressBar != null)
            progressBar.fillAmount = fishingProgress;
    }

    void HandleReelBar()
    {
        bool wasReeling = reelPower > 0.1f;
        if (Input.GetKey(KeyCode.Space))
        {
            reelPower += reelGainSpeed * Time.deltaTime * maxReelPower;
        }
        else
        {
            reelPower -= reelDecaySpeed * Time.deltaTime * maxReelPower;
        }

        bool isReeling = reelPower > 0.1f;

        reelPower = Mathf.Clamp(reelPower, 0f, maxReelPower);
        if (reelIndicator != null)
        {
            reelIndicator.fillAmount = reelPower / maxReelPower;
        }

        if (currentFish == null) return;

        float powerNormalized = reelPower / maxReelPower;
        float tensionChange = 0f;

        if (powerNormalized >= 0.99f || powerNormalized <= 0.01f)
        {
            tensionChange = tensionIncreaseRate * 10f * Time.deltaTime * currentFish.strength;
        }
        else
        {
            if (powerNormalized > 0.9f)
            {
                tensionChange = tensionIncreaseRate * (powerNormalized - 0.9f) * Time.deltaTime * currentFish.strength;
            }
            else if (powerNormalized < 0.1f)
            {
                tensionChange = tensionIncreaseRate * (0.1f - powerNormalized) * Time.deltaTime * currentFish.strength;
            }
            else
            {
                tensionChange = -tensionDecreaseRate * Time.deltaTime;
            }
        }

        if (fishingSFX != null)
        {
            if (isReeling && !wasReeling)
            {
                fishingSFX.StartReelingSound();
            }
            else if (!isReeling && wasReeling)
            {
                fishingSFX.StopReelingSound();
            }
        }

        if (fishingSFX != null)
        {
            float tensionNormalized = currentLineTension / maxLineTension;
            fishingSFX.SetReelingPitch(Mathf.Lerp(0.9f, 1.1f, tensionNormalized));
        }

        if (fishCaught)
        {
            if (fishingSFX != null && fishingSFX.reelingSource.isPlaying)
            {
                fishingSFX.StopReelingSound();
            }
            return;
        }
        currentLineTension += tensionChange;
        currentLineTension = Mathf.Clamp(currentLineTension, 0f, maxLineTension);
    }

    void HandleBalanceBar()
    {
        if (currentFish == null) return;

        randomTargetChangeTimer += Time.deltaTime;
        if (randomTargetChangeTimer >= currentRandomTargetChangeInterval)
        {
            randomTargetChangeTimer = 0f;
            balanceTarget = Random.Range(balanceRandomTargetMin, balanceRandomTargetMax);
            currentRandomTargetChangeInterval = Random.Range(balanceTargetChangeMinInterval, balanceTargetChangeMaxInterval);
        }

        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= balanceSensitivity;
        if (Input.GetKey(KeyCode.D)) input += balanceSensitivity;

        float fishInfluence = (Random.value * 2f - 1f) * currentFish.strength * 0.1f * Time.deltaTime;
        balancePosition += input * Time.deltaTime * balanceSpeed + fishInfluence;
        balancePosition = Mathf.Clamp(balancePosition, -1f, 1f);

        if (balancePlayerIndicator != null && balanceTargetIndicator != null)
        {
            float px = Mathf.Lerp(-balanceBarVisualRange / 2f, balanceBarVisualRange / 2f, (balancePosition + 1f) / 2f);
            balancePlayerIndicator.anchoredPosition = new Vector2(px, balancePlayerIndicator.anchoredPosition.y);

            float tx = Mathf.Lerp(-balanceBarVisualRange / 2f, balanceBarVisualRange / 2f, (balanceTarget + 1f) / 2f);
            balanceTargetIndicator.anchoredPosition = new Vector2(tx, balanceTargetIndicator.anchoredPosition.y);
        }
    }

    void HandleCameraZoom()
    {
        if (mainCamera == null || !mainCamera.orthographic) return;

        float targetSize = (isFishing || isThrowing || fishBiteDetected) ? zoomedSize : normalSize;
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    void UpdateFishingLineVisual()
    {
        if (lineRenderer != null && fishingRodTip != null)
        {
            Vector3 start = fishingRodTip.position;
            Vector3 end = fishingRodTip.position + transform.right * throwPower * throwDistance / maxThrowPower;
            start.z += lineOffsetZ;
            end.z += lineOffsetZ;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }

    private void ResetUIElements()
    {
        if (fishingUI != null) fishingUI.SetActive(false);
        if (biteIndicator != null) biteIndicator.SetActive(false);
        if (tensionText != null) tensionText.text = "";

        if (reelBarBackground != null) reelBarBackground.gameObject.SetActive(false);
        if (reelIndicator != null) reelIndicator.gameObject.SetActive(false);
        if (balanceBarBackground != null) balanceBarBackground.gameObject.SetActive(false);
        if (balancePlayerIndicator != null) balancePlayerIndicator.gameObject.SetActive(false);
        if (balanceTargetIndicator != null) balanceTargetIndicator.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);

        if (mainCamera != null) mainCamera.orthographicSize = normalSize;
        if (lineRenderer != null) lineRenderer.enabled = false;

        if (fishCaughtUI != null) fishCaughtUI.SetActive(false);
    }
}