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
    public float fishingProgress = 0f;
    public float fishingDuration = 10f; // Time to catch fish
    
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

    private BoatMovement boatMovement;
    private float randomDirectionChangeTimer = 0f;
    private float randomDirectionChangeInterval = 1.5f;

    void Start()
    {
        boatMovement = GetComponent<BoatMovement>();
        fishingUI.SetActive(false);
    }

    void Update()
    {
        if (!isFishing && boatMovement != null && boatMovement.IsMoving() == false)
        {
            canFish = true;
        }
        else if (boatMovement != null && boatMovement.IsMoving())
        {
            canFish = false;
            if (isFishing)
            {
                CancelFishing();
            }
        }

        if (canFish && !isFishing && Input.GetKeyDown(KeyCode.F))
        {
            StartFishing();
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
    }

    void StartFishing()
    {
        isFishing = true;
        fishCaught = false;
        fishingProgress = 0f;
        reelPower = 0f;
        balancePosition = 0f;
        fishingUI.SetActive(true);
        
        balanceTarget = Random.Range(-0.8f, 0.8f);
    }

    void CancelFishing()
    {
        isFishing = false;
        fishCaught = false;
        fishingUI.SetActive(false);
    }

    void CatchFish()
    {
        fishCaught = true;
        isFishing = false;
        fishingUI.SetActive(false);
        Debug.Log("Fish caught!");
        
        // Tambahin ikan sama berat nanti disini gw malas
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
}
