using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap warningTilemap;
    [SerializeField] private Tile warningTile;
    [SerializeField] private EnemyRegistry enemyRegistry;
    [SerializeField] private Transform enemyParent;

    [Header("Spawn Settings")]
    [SerializeField] private float betweenEnemySpawnDelay = 0.2f;
    [SerializeField] private float enemySpawnDelay = 1f;

    private readonly HashSet<Vector3> _activeSpawnLocations = new HashSet<Vector3>();

    /// <summary>
    /// Returns a random spawn location that is not currently active.
    /// Throws an error if no free position is available.
    /// </summary>
    private Vector3 GetRandomFreeSpawnLocation(List<Vector3> spawnLocations)
    {
        List<Vector3> freePositions = spawnLocations.FindAll(pos => !_activeSpawnLocations.Contains(pos));

        if (freePositions.Count == 0)
            throw new System.Exception("No free spawn locations available!");

        return freePositions[Random.Range(0, freePositions.Count)];
    }

    public IEnumerator SpawnEnemyWithWarning(string enemyType, Vector3 spawnPosition)
    {
        if (_activeSpawnLocations.Contains(spawnPosition))
            throw new System.Exception($"Spawn position {spawnPosition} is already in use!");

        _activeSpawnLocations.Add(spawnPosition);

        Vector3Int tilePos = Vector3Int.FloorToInt(spawnPosition);
        warningTilemap.SetTile(tilePos, warningTile);

        yield return new WaitForSeconds(enemySpawnDelay);

        GameObject prefab = enemyRegistry.GetPrefab(enemyType);
        if (prefab != null)
        {
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (enemyParent != null)
                enemy.transform.SetParent(enemyParent);
        }
        else
        {
            Debug.LogError($"Enemy type '{enemyType}' not found in registry!");
        }

        warningTilemap.SetTile(tilePos, null);
        _activeSpawnLocations.Remove(spawnPosition);
    }

    public IEnumerator SpawnEnemiesSequentially(List<string> enemyTypes, List<Vector3> spawnPositions, List<(string, Vector3)> customEnemySpawns)
    {
        for (int i = 0; i < enemyTypes.Count; i++)
        {
            Vector3 spawnPos = GetRandomFreeSpawnLocation(spawnPositions);

            StartCoroutine(SpawnEnemyWithWarning(enemyTypes[i], spawnPos));
            yield return new WaitForSeconds(betweenEnemySpawnDelay);
        }

        foreach (var (enemyType, spawnPos) in customEnemySpawns)
        {
            var prefab = enemyRegistry.GetPrefab(enemyType);
            if (prefab == null) continue;

            StartCoroutine(SpawnEnemyWithWarning(enemyType, spawnPos));
            yield return new WaitForSeconds(betweenEnemySpawnDelay);
        }
    }
}
