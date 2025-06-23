using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    /// MAIN MENU 
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}

