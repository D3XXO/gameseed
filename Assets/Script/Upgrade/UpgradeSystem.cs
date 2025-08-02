using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance;

    [Header("UI References")]
    public GameObject upgradePanel;
    public Text upgradeNameText;
    public Text descriptionText;
    public Text costText;
    public Text currentStatsText;
    public Button upgradeButton;

    [Header("Speed Upgrade Settings")]
    public string speedUpgradeName = "Boat Speed";
    public int speedBaseCost = 100;
    public float speedCostMultiplier = 1.5f;
    public float speedUpgradePercent = 0.2f;
    public int speedMaxLevel = 5;

    [Header("Health Upgrade Settings")]
    public string healthUpgradeName = "Boat Health";
    public int healthBaseCost = 150;
    public float healthCostMultiplier = 2f;
    public int healthUpgradeAmount = 1;
    public int healthMaxLevel = 3;

    public int speedCurrentLevel = 0;
    public int healthCurrentLevel = 0;
    private BoatController boatController;
    private UpgradeType currentUpgradeType;

    private enum UpgradeType { Speed, Health }

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

        boatController = FindObjectOfType<BoatController>();
        upgradePanel.SetActive(false);
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            ShowUpgradePanel();
        }
    }

    public void ShowUpgradePanel()
    {
        // Default to speed upgrade when opening panel
        ShowUpgradeType(UpgradeType.Speed);
        upgradePanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void ShowSpeedUpgrade()
    {
        ShowUpgradeType(UpgradeType.Speed);
    }

    public void ShowHealthUpgrade()
    {
        ShowUpgradeType(UpgradeType.Health);
    }

    private void ShowUpgradeType(UpgradeType type)
    {
        currentUpgradeType = type;

        if (type == UpgradeType.Speed)
        {
            if (speedCurrentLevel >= speedMaxLevel)
            {
                upgradeNameText.text = $"{speedUpgradeName} (MAX)";
                descriptionText.text = "Maximum speed reached";
                costText.text = "MAX LEVEL";
                currentStatsText.text = $"Current Speed: {boatController.moveSpeed}";
                upgradeButton.interactable = false;
            }
            else
            {
                int currentCost = CalculateUpgradeCost(type);
                float newSpeed = boatController.moveSpeed * (1 + speedUpgradePercent);
                upgradeNameText.text = $"{speedUpgradeName} (Level {speedCurrentLevel + 1})";
                descriptionText.text = $"Increase speed by {speedUpgradePercent * 100}%";
                costText.text = $"{currentCost}G";
                currentStatsText.text = $"Current: {boatController.moveSpeed}\nNext: {newSpeed}";
                upgradeButton.interactable = PlayerEconomy.Instance.currentGold >= currentCost;
            }
        }
        else // Health upgrade
        {
            if (healthCurrentLevel >= healthMaxLevel)
            {
                upgradeNameText.text = $"{healthUpgradeName} (MAX)";
                descriptionText.text = "Maximum health reached";
                costText.text = "MAX LEVEL";
                currentStatsText.text = $"Current HP: {boatController.currentHP}/{boatController.maxHP}";
                upgradeButton.interactable = false;
            }
            else
            {
                int currentCost = CalculateUpgradeCost(type);
                int newMaxHP = boatController.maxHP + healthUpgradeAmount;
                upgradeNameText.text = $"{healthUpgradeName} (Level {healthCurrentLevel + 1})";
                descriptionText.text = $"Increase max HP by {healthUpgradeAmount}";
                costText.text = $"{currentCost}G";
                currentStatsText.text = $"Current: {boatController.maxHP}\nNext: {newMaxHP}";
                upgradeButton.interactable = PlayerEconomy.Instance.currentGold >= currentCost;
            }
        }
    }

    public void HideUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void PerformUpgrade()
    {
        int currentCost = CalculateUpgradeCost(currentUpgradeType);

        if (PlayerEconomy.Instance.currentGold >= currentCost)
        {
            PlayerEconomy.Instance.AddGold(-currentCost);

            if (currentUpgradeType == UpgradeType.Speed)
            {
                boatController.moveSpeed *= (1f + speedUpgradePercent);
                speedCurrentLevel++;
            }
            else // Health upgrade
            {
                boatController.maxHP += healthUpgradeAmount;
                boatController.currentHP += healthUpgradeAmount; // Also heal the boat
                boatController.IncreaseMaxHealth(healthUpgradeAmount);
                healthCurrentLevel++;
            }

            // Refresh UI
            ShowUpgradeType(currentUpgradeType);
        }
    }

    private int CalculateUpgradeCost(UpgradeType type)
    {
        if (type == UpgradeType.Speed)
        {
            return Mathf.RoundToInt(speedBaseCost * Mathf.Pow(speedCostMultiplier, speedCurrentLevel));
        }
        else
        {
            return Mathf.RoundToInt(healthBaseCost * Mathf.Pow(healthCostMultiplier, healthCurrentLevel));
        }
    }
}