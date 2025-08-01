using UnityEngine;
using UnityEngine.UI;

public class BoatHealthUI : MonoBehaviour
{
    public Text healthText;

    private BoatController boatController;

    void Start()
    {
        boatController = FindObjectOfType<BoatController>();
        if (boatController != null)
        {
            boatController.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(boatController.maxHP, boatController.maxHP);
        }
    }

    void OnDestroy()
    {
        if (boatController != null)
        {
            boatController.OnHealthChanged -= UpdateHealthUI;
        }
    }

    public void UpdateHealthUI(int currentHP, int maxHP)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHP}/{maxHP}";
        }
    }
}