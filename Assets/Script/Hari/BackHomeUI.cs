using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BackHomeUI : MonoBehaviour
{
    public static BackHomeUI Instance { get; private set; }

    [Header("Notification Canvases")]
    private GameObject returnHomeNotificationCanvas;
    public string returnHomeCanvasName = "ReturnHomeNotificationCanvas";

    [Header("Notification Buttons Names")]
    public string goHomeButtonName = "GoHomeButton";
    public string stayButtonName = "StayButton";

    [Header("Big Wave Logic")]
    public GameObject bigWavePrefab;
    public string playerTag = "Player";

    private BoatController boatController;
    private Fishing fishingController;
    private List<EnemyAI> activeEnemies = new List<EnemyAI>();

    private void Awake()
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

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.WorldTimeEndNight -= OnWorldTimeEndNightHandler;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string[] gameplayScenes = {"Gameplay"};

        bool isGameplayScene = false;
        foreach (string sceneName in gameplayScenes)
        {
            if (scene.name == sceneName)
            {
                isGameplayScene = true;
                break;
            }
        }

        if (isGameplayScene)
        {
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                Transform foundCanvasTransform = mainCanvas.transform.Find(returnHomeCanvasName);
                if (foundCanvasTransform != null)
                {
                    returnHomeNotificationCanvas = foundCanvasTransform.gameObject;
                    returnHomeNotificationCanvas.SetActive(false);

                    Button goHomeButton = returnHomeNotificationCanvas.transform.Find(goHomeButtonName)?.GetComponent<Button>();
                    Button stayButton = returnHomeNotificationCanvas.transform.Find(stayButtonName)?.GetComponent<Button>();

                    if (goHomeButton != null && stayButton != null)
                    {
                        goHomeButton.onClick.RemoveAllListeners();
                        stayButton.onClick.RemoveAllListeners();

                        goHomeButton.onClick.AddListener(OnGoHomeButtonClicked);
                        stayButton.onClick.AddListener(OnStayButtonClicked);
                    }
                }
                else
                {
                    returnHomeNotificationCanvas = null;
                }
            }
            else
            {
                returnHomeNotificationCanvas = null;
            }

            if (WorldTime.WorldTime.Instance != null && returnHomeNotificationCanvas != null)
            {
                WorldTime.WorldTime.Instance.WorldTimeEndNight -= OnWorldTimeEndNightHandler;
                WorldTime.WorldTime.Instance.WorldTimeEndNight += OnWorldTimeEndNightHandler;
            }
        }
        else
        {
            if (returnHomeNotificationCanvas != null)
            {
                returnHomeNotificationCanvas.SetActive(false);
                returnHomeNotificationCanvas = null;
            }
            if (WorldTime.WorldTime.Instance != null)
            {
                WorldTime.WorldTime.Instance.WorldTimeEndNight -= OnWorldTimeEndNightHandler;
            }
        }
    }


    private void OnWorldTimeEndNightHandler(object sender, System.EventArgs e)
    {
        ShowReturnHomeNotification();
    }

    public void ShowReturnHomeNotification()
    {
        if (returnHomeNotificationCanvas == null) return;

        boatController = FindObjectOfType<BoatController>();
        if (boatController != null)
        {
            boatController.SetMovementEnabled(false);
        }

        fishingController = FindObjectOfType<Fishing>();
        if (fishingController != null)
        {
            fishingController.enabled = false;
        }

        activeEnemies.Clear();
        activeEnemies.AddRange(FindObjectsOfType<EnemyAI>());
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.enabled = false;
            }
        }
        
        returnHomeNotificationCanvas.SetActive(true);
    }

    public void HideReturnHomeNotification()
    {
        if (returnHomeNotificationCanvas == null) return;
        
        returnHomeNotificationCanvas.SetActive(false);

        if (boatController != null)
        {
            boatController.SetMovementEnabled(true);
        }

        if (fishingController != null)
        {
            fishingController.enabled = true;
        }

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.enabled = true;
            }
        }
        activeEnemies.Clear();
    }

    public void OnGoHomeButtonClicked()
    {
        HideReturnHomeNotification();

        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.ResetTimeAndAdvanceDay();
        }
        SceneManager.LoadScene("Harbour");
    }
    
    public void OnStayButtonClicked()
    {
        HideReturnHomeNotification();
        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.ContinueTimeAfterNightEnd();
            StartCoroutine(BigWaveSequence());
        }
    }
    
    private IEnumerator BigWaveSequence()
    {
        yield return new WaitUntil(() => WorldTime.WorldTime.Instance.CurrentTime.Hours >= 3 && WorldTime.WorldTime.Instance.CurrentTime.Minutes >= 10);

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            yield break;
        }

        Vector3 playerPosition = player.transform.position;
        Vector2 randomDirection = Random.insideUnitCircle.normalized * 30f;
        Vector3 spawnPosition = new Vector3(playerPosition.x + randomDirection.x, playerPosition.y, playerPosition.z + randomDirection.y);

        if (bigWavePrefab != null)
        {
            GameObject waveInstance = Instantiate(bigWavePrefab, spawnPosition, Quaternion.identity);
            BigWaveController waveController = waveInstance.GetComponent<BigWaveController>();
            
            if (waveController != null)
            {
                waveController.SetTarget(player.transform);
            }
        }
    }
}