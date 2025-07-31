using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;

public class ReturnToSea : MonoBehaviour
{
    [Header("Pengaturan Spawn")]
    [Tooltip("Posisi di mana pemain akan muncul di scene laut")]
    public Vector3 targetSpawnPosition = new Vector3(0, 0, 0);

    private void OnMouseDown()
    {
        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.ResetTimeAndAdvanceDay();
        }

        SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
        if (saveLoadManager == null) return;

        GameData data = saveLoadManager.LoadGame();
        
        string sceneToLoad = "Gameplay";
        foreach (string scene in saveLoadManager.GameplayScenes)
        {
            if (scene == data.lastGameplaySceneName && scene != "Harbour")
            {
                sceneToLoad = scene;
                break;
            }
        }

        data.useOverrideSpawnPosition = true;
        data.overrideSpawnX = targetSpawnPosition.x;
        data.overrideSpawnY = targetSpawnPosition.y;
        data.overrideSpawnZ = targetSpawnPosition.z;
        
        data.lastGameplaySceneName = sceneToLoad;

        saveLoadManager.SaveGame(data);
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetSpawnPosition, 2f);
        Gizmos.DrawLine(transform.position, targetSpawnPosition);
    }
}