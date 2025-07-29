using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToSea : MonoBehaviour
{
    [Header("Pengaturan Spawn")]
    [Tooltip("Posisi di mana pemain akan muncul di scene 'Gameplay'")]
    public Vector3 targetSpawnPosition = new Vector3(0, 0, 0);

    private void OnMouseDown()
    {
        Debug.Log("Objek Kembali ke Laut diklik. Mempersiapkan untuk berlayar...");

        SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
        if (saveLoadManager == null)
        {
            Debug.LogError("SaveLoadManager tidak ditemukan di scene!");
            return;
        }

        GameData data = saveLoadManager.LoadGame();

        string sceneToLoad = string.IsNullOrEmpty(data.lastGameplaySceneName) ? "Gameplay" : data.lastGameplaySceneName;

        data.useOverrideSpawnPosition = true;
        data.overrideSpawnX = targetSpawnPosition.x;
        data.overrideSpawnY = targetSpawnPosition.y;
        data.overrideSpawnZ = targetSpawnPosition.z;

        saveLoadManager.SaveGame(data);
        Debug.Log($"Posisi spawn baru ({targetSpawnPosition}) disimpan. Memuat scene: {sceneToLoad}");

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetSpawnPosition, 2f);
        Gizmos.DrawLine(transform.position, targetSpawnPosition);
    }
}