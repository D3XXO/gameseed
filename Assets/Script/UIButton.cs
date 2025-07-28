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

    private void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (ItemDatabase.Instance == null)
        {
            ItemDatabase masterDb = Resources.Load<ItemDatabase>("MasterItemDatabase");
            if (masterDb != null)
            {
                masterDb.Init();
            }
        }

        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            InitializeMainMenuButtons();
            if (confirmationCanvas != null)
            {
                confirmationCanvas.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Main Menu" && Time.timeScale > 0.1f && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
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
        string[] gameplayScenes = {"Gameplay", "House", "Beach"};

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
            SaveLoadManager.Instance.SaveGame(SaveLoadManager.Instance.GatherGameData());
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}