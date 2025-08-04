using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;

    [Header("Confirmation UI")]
    public GameObject confirmationCanvas;

    public GameObject pauseMenuUI;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (ItemDatabase.Instance == null)
        {
            ItemDatabase masterDb = Resources.Load<ItemDatabase>("MasterItemDatabase");
            if (masterDb != null)
            {
                masterDb.Init();
            }
        }

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Update()
    {
        HandleInventoryInput();
        if (SceneManager.GetActiveScene().name != "Main Menu" && pauseMenuUI != null && Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle pause menu
            if (pauseMenuUI.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedSetup(scene));

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (scene.name == "Main Menu")
        {
            if (mainCanvas != null)
            {
                Transform canvasRoot = mainCanvas.transform;

                Transform newGameBtnObj = canvasRoot.Find("New Game");
                if (newGameBtnObj != null)
                    newGameButton = newGameBtnObj.GetComponent<Button>();

                Transform loadGameBtnObj = canvasRoot.Find("Load Game");
                if (loadGameBtnObj != null)
                    loadGameButton = loadGameBtnObj.GetComponent<Button>();

                Transform confirmationObj = canvasRoot.Find("Confirmation Canvas");
                if (confirmationObj != null)
                    confirmationCanvas = confirmationObj.gameObject;
            }

            if (newGameButton != null || loadGameButton != null)
            {
                InitializeMainMenuButtons();
            }

            if (confirmationCanvas != null)
            {
                confirmationCanvas.SetActive(false);
            }

            pauseMenuUI = null;
        }
        else
        {
            if (mainCanvas != null)
            {
                Transform canvasRoot = mainCanvas.transform;
                Transform pauseObj = canvasRoot.Find("PauseCanvas");
                if (pauseObj != null)
                {
                    pauseMenuUI = pauseObj.gameObject;
                    pauseMenuUI.SetActive(false);
                }
            }

            newGameButton = null;
            loadGameButton = null;
            confirmationCanvas = null;
        }
    }
    private IEnumerator DelayedSetup(Scene scene)
    {
        yield return null; // Tunggu 1 frame

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (scene.name == "Main Menu")
        {
            // Main menu logic seperti biasa...
            pauseMenuUI = null;
        }
        else
        {
            if (mainCanvas != null)
            {
                Transform canvasRoot = mainCanvas.transform;
                Transform pauseObj = canvasRoot.Find("PauseCanvas");
                if (pauseObj != null)
                {
                    pauseMenuUI = pauseObj.gameObject;
                    pauseMenuUI.SetActive(false);
                    Debug.Log("PauseCanvas assigned after delay.");
                }
                else
                {
                    Debug.LogWarning("PauseCanvas not found in gameplay scene.");
                }
            }
            else
            {
                Debug.LogWarning("MainCanvas not found in gameplay scene.");
            }
        }
    }


    
    void HandleInventoryInput()
    {
        if (SceneManager.GetActiveScene().name == "Harbour") return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryUI.Instance.ToggleInventory();
        }
    }
    void InitializeMainMenuButtons()
    {
        bool saveFileExists = SaveLoadManager.Instance.DoesSaveFileExist();

        if (newGameButton != null)
        {
            newGameButton.interactable = true;
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (loadGameButton != null)
        {
            loadGameButton.interactable = saveFileExists;
            loadGameButton.onClick.RemoveAllListeners();
            loadGameButton.onClick.AddListener(LoadExistingGame);
        }
    }

    public void OnNewGameClicked()
    {
        if (SaveLoadManager.Instance.DoesSaveFileExist())
        {
            if (confirmationCanvas != null)
            {
                confirmationCanvas.SetActive(true);
            }
            Time.timeScale = 0f;
        }
        else
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        SaveLoadManager.Instance.DeleteSaveFile();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetInventoryState();
        }

        SceneManager.LoadScene("Gameplay");
        Time.timeScale = 1f;
        if (confirmationCanvas != null)
        {
            confirmationCanvas.SetActive(false);
        }
    }

    public void LoadExistingGame()
    {
        GameData loadedData = SaveLoadManager.Instance.LoadGame();
        string sceneToLoad = string.IsNullOrEmpty(loadedData.lastGameplaySceneName) ? "Gameplay" : loadedData.lastGameplaySceneName;
        SceneManager.sceneLoaded -= OnGameplaySceneLoadedAndApplyData;
        SceneManager.sceneLoaded += OnGameplaySceneLoadedAndApplyData;
        SceneManager.LoadScene(sceneToLoad);
        Time.timeScale = 1f;
    }

    private void OnGameplaySceneLoadedAndApplyData(Scene scene, LoadSceneMode mode)
    {
        string[] gameplayScenes = { "Gameplay", "Harbour" };
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
            GameData dataToApply = SaveLoadManager.Instance.LoadGame();
            SaveLoadManager.Instance.ApplyGameData(dataToApply);
        }
        SceneManager.sceneLoaded -= OnGameplaySceneLoadedAndApplyData;
    }

    public void QuitGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(SaveLoadManager.Instance.GatherGameData());
        }
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void LoadMainMenu()
    {
        if (SaveLoadManager.Instance != null)
        {
            GameData currentData = SaveLoadManager.Instance.GatherGameData();

            if (SceneManager.GetActiveScene().name == "Harbour")
            {
                currentData.lastGameplaySceneName = "Harbour";
            }

            SaveLoadManager.Instance.SaveGame(currentData);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}