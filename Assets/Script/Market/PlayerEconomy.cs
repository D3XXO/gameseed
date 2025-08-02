using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindGoldText();
    }

    public void FindGoldText()
    {
        GameObject goldTextObj = GameObject.FindGameObjectWithTag("GoldText");

        if (goldTextObj == null)
        {
            goldTextObj = GameObject.Find("GoldText");
        }

        if (goldTextObj != null)
        {
            goldText = goldTextObj.GetComponent<Text>();
            UpdateGoldUI();
        }
        else
        {
            Debug.LogWarning("Gold Text UI tidak ditemukan di scene ini!");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    public void SetGold(int amount)
    {
        currentGold = amount;
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