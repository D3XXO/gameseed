using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI References")]
    public GameObject upgradePanel;
    public Button speedUpgradeButton;
    public Button healthUpgradeButton;
    public Text speedCostText;
    public Text healthCostText;
    public Text currentSpeedText;
    public Text currentHealthText;
    public Text currentGoldText;

    [Header("Interaction Settings")]
    public LayerMask interactableLayer; // Layer for clickable objects
    public float interactionRadius = 3f;


    [Header("Upgrade Settings")]
    public int baseSpeedUpgradeCost = 100;
    public int speedCostIncrease = 50;
    public float speedIncreaseAmount = 0.5f;

    public int baseHealthUpgradeCost = 150;
    public int healthCostIncrease = 75;
    public int healthIncreaseAmount = 10;

    public int currentSpeedUpgradeLevel = 0;
    public int currentHealthUpgradeLevel = 0;
    private int currentSpeedUpgradeCost;
    private int currentHealthUpgradeCost;
    private bool isPanelOpen = false;

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

        currentSpeedUpgradeCost = baseSpeedUpgradeCost;
        currentHealthUpgradeCost = baseHealthUpgradeCost;
    }

    void Start()
    {
        speedUpgradeButton.onClick.AddListener(UpgradeSpeed);
        healthUpgradeButton.onClick.AddListener(UpgradeHealth);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            CheckForUpgradeTrigger();
        }

        // Close panel if ESC is pressed
        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUpgradePanel();
        }
    }

    void CheckForUpgradeTrigger()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRadius, interactableLayer))
        {
            if (hit.collider.CompareTag("UpgradeTrigger"))
            {
                ToggleUpgradePanel();
            }
        }
    }

    public void ToggleUpgradePanel()
    {
        isPanelOpen = !upgradePanel.activeSelf;
        upgradePanel.SetActive(isPanelOpen);

        if (isPanelOpen)
        {
            UpdateUI();
            BoatController.Instance.SetMovementEnabled(false);
            Time.timeScale = 0f;
        }
        else
        {
            BoatController.Instance.SetMovementEnabled(true);
            Time.timeScale = 1f;
        }
    }

    void UpgradeSpeed()
    {
        if (PlayerEconomy.Instance.currentGold >= currentSpeedUpgradeCost)
        {
            PlayerEconomy.Instance.AddGold(-currentSpeedUpgradeCost);

            // Upgrade stat
            BoatController.Instance.IncreaseMoveSpeed(speedIncreaseAmount);
            currentSpeedUpgradeLevel++;
            currentSpeedUpgradeCost = baseSpeedUpgradeCost + (speedCostIncrease * currentSpeedUpgradeLevel);

            // Simpan data
            if (SaveLoadManager.Instance != null)
            {
                GameData data = SaveLoadManager.Instance.LoadGame();
                data = SaveToGameData(data);
                data = BoatController.Instance.SaveToGameData(data);
                SaveLoadManager.Instance.SaveGame(data);
            }

            UpdateUI();
        }
    }

    void UpgradeHealth()
    {
        if (PlayerEconomy.Instance.currentGold >= currentHealthUpgradeCost)
        {
            PlayerEconomy.Instance.AddGold(-currentHealthUpgradeCost);

            // Upgrade stat
            BoatController.Instance.IncreaseMaxHealth(healthIncreaseAmount);
            currentHealthUpgradeLevel++;
            currentHealthUpgradeCost = baseHealthUpgradeCost + (healthCostIncrease * currentHealthUpgradeLevel);

            // Simpan data
            if (SaveLoadManager.Instance != null)
            {
                GameData data = SaveLoadManager.Instance.LoadGame();
                data = SaveToGameData(data);
                data = BoatController.Instance.SaveToGameData(data);
                SaveLoadManager.Instance.SaveGame(data);
            }

            UpdateUI();
        }
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void UpdateUI()
    {
        speedCostText.text = $"{currentSpeedUpgradeCost}G";
        healthCostText.text = $"{currentHealthUpgradeCost}G";

        currentSpeedText.text = $"Speed: {BoatController.Instance.moveSpeed:F1}";
        currentHealthText.text = $"Health: {BoatController.Instance.currentHP}/{BoatController.Instance.maxHP}";

        if (PlayerEconomy.Instance != null)
        {
            currentGoldText.text = $"Gold: {PlayerEconomy.Instance.currentGold}G";
        }
    }

    public void LoadFromGameData(GameData data)
    {
        if (data == null) return;

        currentSpeedUpgradeLevel = data.speedUpgradeLevel;
        currentHealthUpgradeLevel = data.healthUpgradeLevel;

        // Hitung ulang harga upgrade
        currentSpeedUpgradeCost = baseSpeedUpgradeCost + (speedCostIncrease * currentSpeedUpgradeLevel);
        currentHealthUpgradeCost = baseHealthUpgradeCost + (healthCostIncrease * currentHealthUpgradeLevel);

        UpdateUI();
    }

    public GameData SaveToGameData(GameData data)
    {
        data.speedUpgradeLevel = currentSpeedUpgradeLevel;
        data.healthUpgradeLevel = currentHealthUpgradeLevel;
        return data;
    }


}