using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyRegistry", menuName = "Game/Enemy Registry")]
public class EnemyRegistry : ScriptableObject
{
    [System.Serializable]
    public class EnemyEntry
    {
        public string enemyName;
        public GameObject prefab;
    }

    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    private Dictionary<string, GameObject> enemyDict;

    public void Initialize()
    {
        enemyDict = new Dictionary<string, GameObject>();
        foreach (var entry in enemies)
        {
            if (!enemyDict.ContainsKey(entry.enemyName))
                enemyDict[entry.enemyName] = entry.prefab;
            else
                Debug.LogWarning($"Duplicate enemy name in registry: {entry.enemyName}");
        }
    }

    public GameObject GetPrefab(string enemyName)
    {
        if (enemyDict == null) Initialize();
        if (enemyDict.TryGetValue(enemyName, out var prefab))
            return prefab;

        Debug.LogError($"Enemy type '{enemyName}' not found in registry!");
        return null;
    }
}
