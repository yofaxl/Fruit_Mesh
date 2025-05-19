using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInitializer : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerPrefab; // Karakter prefab'ı

    private void Start()
    {
        // Aktif sahnenin adından level numarasını al
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level"))
        {
            int levelNumber = int.Parse(sceneName.Substring(5)); // "Level" kelimesinden sonrasını al
            SpawnPlayer(levelNumber);
        }
        else
        {
            Debug.LogError("LevelInitializer: Scene name does not follow the 'LevelX' format!");
        }
    }

    private void SpawnPlayer(int levelNumber)
    {
        // Sahnedeki tüm spawn point'leri bul
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        
        // Bu level için olan spawn point'i bul
        SpawnPoint targetSpawnPoint = null;
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.levelNumber == levelNumber)
            {
                targetSpawnPoint = spawnPoint;
                break;
            }
        }

        if (targetSpawnPoint != null)
        {
            // Karakteri spawn et
            if (playerPrefab != null)
            {
                Instantiate(playerPrefab, targetSpawnPoint.transform.position, targetSpawnPoint.transform.rotation);
            }
            else
            {
                Debug.LogError("LevelInitializer: Player prefab is not assigned!");
            }
        }
        else
        {
            Debug.LogError($"LevelInitializer: No spawn point found for level {levelNumber}!");
        }
    }
} 