using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToHarbour : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(SaveLoadManager.Instance.GatherGameData());
        }

        SceneManager.LoadScene("Harbour");
    }
}