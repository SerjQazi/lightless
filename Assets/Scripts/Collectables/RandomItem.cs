using UnityEngine;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> collectibles; // Assign in Inspector
    [SerializeField] private List<Transform> spawnPoints;   // Assign in Inspector

    public GameObject GetRandomCollectible()
    {
        if (collectibles == null || collectibles.Count == 0)
        {
            Debug.LogWarning("Collectible list is empty!");
            return null;
        }

        int index = Random.Range(0, collectibles.Count);
        return collectibles[index];
    }

    public Vector2 GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Spawn point list is empty!");
            return Vector2.zero;
        }

        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index].position;
    }

    public void SpawnRandomCollectible()
    {
        GameObject collectible = GetRandomCollectible();
        Vector2 spawnPos = GetRandomSpawnPoint();

        if (collectible != null)
        {
            Instantiate(collectible, spawnPos, Quaternion.identity);
        }
    }

    void Start()
    {
        SpawnRandomCollectible();
    }

}


