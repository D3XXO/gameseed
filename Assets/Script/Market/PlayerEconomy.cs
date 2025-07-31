using UnityEngine;
using UnityEngine.UI;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance;
    
    public int currentGold;
    public Text goldText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{currentGold}G";
        }
    }
}