using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int waveIndex = 0;
    [SerializeField] private int levelIndex = 0;
    [SerializeField] private int totalLevels = 3;
    [SerializeField] private int waveCooldown = 5;

    private Dictionary<int,Level> levels = new Dictionary<int,Level>();

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject baseMeleeEnemyEasy;
    [SerializeField] private GameObject baseRangedEnemyEasy;
    [SerializeField] private GameObject fastEnemyEasy;
    [SerializeField] private GameObject shotgunEnemyMedium;
    [SerializeField] private GameObject gunnerEnemyMedium;
    [SerializeField] private GameObject tripleEnemyMedium;
    [SerializeField] private GameObject heavyEnemyMedium;
    [SerializeField] private GameObject sniperEnemyMedium;
    [SerializeField] private GameObject chargeMeleeEnemyHard;
    [SerializeField] private GameObject dodgeMeleeEnemyHard;
    [SerializeField] private GameObject dodgeRangedEnemyHard;
    [SerializeField] private GameObject reflectMeleeEnemyHard;

    [Header("Boss Prefabs")]
    [SerializeField] private GameObject dashBoss;
    [SerializeField] private GameObject splitBoss;
    [SerializeField] private GameObject rangedBoss;


    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap warningTilemap;
    [SerializeField] private Tile warningTile;

    private enum WaveState { Fighting, Ready, Cooldown, Done }
    [Header("State")]
    [SerializeField] private WaveState waveState = WaveState.Cooldown;
    [SerializeField] private GameObject enemies;

    private UIManager uiManager;
    private GameStateManager gameStateManager;
    private ScoreManager scoreManager;
    private Player player;

    private float lastWaveTime = 0;
    private float minFightingDuration = 3f;
    private float fightingStart = 0;
    private float playerHealthBeforeWave = 0;


    private void Start()
    {
        lastWaveTime = Time.time;

        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        gameStateManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        player = GameObject.Find("Player").GetComponent<Player>();

        SetupCurrentLevel();
    }

    void Update()
    {
        if (waveIndex >= 0)
        {
            if (waveState == WaveState.Ready)
            {
                playerHealthBeforeWave = player.health;
                SpawnWave(this.levelIndex, this.waveIndex);
                waveState = WaveState.Fighting;
                fightingStart = Time.time;
            }
            else if (waveState == WaveState.Fighting)
            {
                if (enemies.transform.childCount == 0 && Time.time - fightingStart > minFightingDuration)
                {
                    // Give Score if player did not lose health
                    if (player.health == playerHealthBeforeWave)
                    {
                        scoreManager.AddScore(ScoreManager.ScoreReason.PerfectWave, 1000);
                    }

                    if (CheckLastWave())
                    {
                        waveState = WaveState.Done;
                        uiManager.EnableLevelCompletedText(levelIndex + 1);
                        if (CheckLastLevel())
                        {
                            StartCoroutine(PerformAfterDelay(5, () => gameStateManager.GameWon()));
                        }
                        else
                        {
                            StartCoroutine(PerformAfterDelay(5, NextLevel));
                        }
                    }
                    else
                    {
                        waveIndex++;

                        waveState = WaveState.Cooldown;
                        lastWaveTime = Time.time;

                        Level level = GetLevel(this.levelIndex);
                        Wave wave = level.GetWave(waveIndex);
                        uiManager.PerformWaveCountdown(waveCooldown, wave.boss);

                        if (waveIndex == 4)
                        {
                            uiManager.EnableUpgradeUI();
                        }
                    }
                }
            }
            else if (waveState == WaveState.Cooldown)
            {
                if (Time.time > lastWaveTime + waveCooldown)
                {
                    waveState = WaveState.Ready;
                    uiManager.DisableWaveUI();
                }
            }
        }
    }

    private void LoadLevel(int levelIndex)
    {
        string path = $"Levels/level{levelIndex}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError("Level json does not exist: " + path);
            return;
        }

        string json = jsonFile.text;
        Level level = JsonUtility.FromJson<Level>(json);

        if (level == null)
        {
            Debug.LogError("Level json is not correct: " + path);
            return;
        }

        levels.Add(levelIndex, level);
        level.Log();
    }

    private void SetupCurrentLevel()
    {
        Level level = GetLevel(this.levelIndex);

        Camera.main.transform.position = level.GetCameraLocation();
        player.transform.position = level.GetPlayerSpawnLocation();
    }

    private Level GetLevel(int levelIndex)
    {
        if (!levels.ContainsKey(levelIndex))
        {
            LoadLevel(levelIndex);
        }

        return levels[levelIndex];
    }

    private void SpawnWave(int levelIndex, int waveIndex)
    {
        Level level = GetLevel(levelIndex);
        Wave wave = level.GetWave(waveIndex);
        List<EnemyCount> enemies = wave.enemies;

        int totalCount = 0;
        foreach (EnemyCount enemyCount in enemies)
        {
            if (!enemyCount.customSpawn) totalCount += enemyCount.amount;
        }
        List<Vector3> spawnLocations = FindSpawnLocations(totalCount);

        int count = 0;
        foreach (EnemyCount enemyCount in enemies)
        {
            GameObject prefab = StringToPrefab(enemyCount.type);
            for (int i = 0; i < enemyCount.amount; i++)
            {
                if (!enemyCount.customSpawn)
                {
                    CreateEnemy(prefab, spawnLocations[count]);
                    count += 1;
                }
                else
                {
                    CreateEnemy(prefab, level.roomLocation.ToVector3() + enemyCount.customSpawnLocation.ToVector3());
                }
            }
        }
    }

    public void NextLevel()
    {
        waveState = WaveState.Cooldown;
        lastWaveTime = Time.time;

        waveIndex = 0;
        levelIndex++;

        Level level = GetLevel(levelIndex);
        Wave wave = level.GetWave(waveIndex);

        uiManager.DisableWaveUI();
        uiManager.PerformWaveCountdown(waveCooldown, wave.boss);

        SetupCurrentLevel();
    }

    public Vector3 GetSafePosition()
    {
        Level level = GetLevel(this.levelIndex);

        return level.GetPlayerSpawnLocation();
    }

    public bool InsideLevel(Vector3 position)
    {
        Level level = GetLevel(this.levelIndex);

        return level.InsideLevel(position);
    }

    private bool CheckLastWave()
    {
        Level level = GetLevel(this.levelIndex);
        return this.waveIndex == level.GetWaveCount() - 1;
    }

    private bool CheckLastLevel()
    {
        return (levelIndex == totalLevels - 1);
    }

    private List<Vector3> FindSpawnLocations(int amount)
    {
        Level level = GetLevel(levelIndex);
        List<Vector3> spawnLocations = new List<Vector3>(level.GetSpawnLocations());
        if (spawnLocations.Count < amount) throw new Exception("Cannot spawn enemies, number of spawn locations is too small");

        return spawnLocations.OrderBy(x => UnityEngine.Random.Range(0, spawnLocations.Count)).Take(amount).ToList();
    }

    private void CreateEnemy(GameObject prefab, Vector3 spawnLocation)
    {
        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), warningTile);
        StartCoroutine(CreateEnemyAfterDelay(prefab, spawnLocation, 1));
    }

    private IEnumerator CreateEnemyAfterDelay(GameObject prefab, Vector3 spawnLocation, int delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject enemy = Instantiate(prefab, spawnLocation, Quaternion.identity);
        enemy.transform.SetParent(enemies.transform);

        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), null);
    }

    public GameObject StringToPrefab(string s)
    {
        if (s == "MeleeEnemy")
        {
            return baseMeleeEnemyEasy;
        }
        else if (s == "RangedEnemy")
        {
            return baseRangedEnemyEasy;
        }
        else if (s == "FastEnemy")
        {
            return fastEnemyEasy;
        }

        else if (s == "ShotgunEnemy")
        {
            return shotgunEnemyMedium;
        }
        else if (s == "GunnerEnemy")
        {
            return gunnerEnemyMedium;
        }
        else if (s == "SniperEnemy")
        {
            return sniperEnemyMedium;
        }
        else if (s == "HeavyEnemy")
        {
            return heavyEnemyMedium;
        }
        else if (s == "TripleEnemy")
        {
            return tripleEnemyMedium;
        }

        else if (s == "DodgeMeleeEnemy")
        {
            return dodgeMeleeEnemyHard;
        }
        else if (s == "DodgeRangedEnemy")
        {
            return dodgeRangedEnemyHard;
        }
        else if (s == "ChargeMeleeEnemy")
        {
            return chargeMeleeEnemyHard;
        }
        else if (s == "ReflectMeleeEnemy")
        {
            return reflectMeleeEnemyHard;
        }

        else if (s == "SplitBoss")
        {
            return splitBoss;
        }
        else if (s == "DashBoss")
        {
            return dashBoss;
        }
        else if (s == "RangedBoss")
        {
            return rangedBoss;
        }

        return baseMeleeEnemyEasy;
    }

    private IEnumerator PerformAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }

    [System.Serializable]
    public class Level
    {
        public Location roomLocation;
        public Location cameraLocation;
        public Location playerSpawnLocation;
        public List<Location> spawnLocations;
        public List<Wave> waves;

        public float roomWidth;
        public float roomHeight;

        public Wave GetWave(int index)
        {
            if (index < waves.Count && index >= 0)
            {
                return waves[index];
            }
            throw new Exception("Wave does not exist!");
        }

        public List<Vector3> GetSpawnLocations()
        {
            List<Vector3> result = new List<Vector3>();
            foreach (Location location in spawnLocations)
            {
                result.Add(roomLocation.ToVector3() + location.ToVector3());
            }
            return result;
        }

        public Vector3 GetCameraLocation()
        {
            return roomLocation.ToVector3() + cameraLocation.ToVector3();
        }
        public Vector3 GetPlayerSpawnLocation()
        {
            return roomLocation.ToVector3() + playerSpawnLocation.ToVector3();
        }

        public int GetWaveCount()
        {
            return waves.Count;
        }

        public void Log()
        {
            Debug.Log($"Loaded Level: {this.roomLocation.ToVector3()}");
            Debug.Log($"Camera Location: {this.cameraLocation.ToVector3()}");
            Debug.Log($"Player Spawn Location: {this.playerSpawnLocation.ToVector3()}");
            Debug.Log($"Enemy Spawn Locations: {this.spawnLocations.Count}");

            foreach (var wave in this.waves)
            {
                Debug.Log($"Wave {wave.waveNumber}");
                foreach (var enemy in wave.enemies)
                {
                    Debug.Log($"Enemy Type: {enemy.type}, Amount: {enemy.amount}");
                }
            }
        }

        public bool InsideLevel(Vector3 position)
        {
            if (position.x < this.roomLocation.x - this.roomWidth) return false;
            if (position.x > this.roomLocation.x + this.roomWidth) return false;

            if (position.y < this.roomLocation.y - this.roomHeight) return false;
            if (position.y > this.roomLocation.y + this.roomHeight) return false;

            return true;
        }
    }

    [System.Serializable]
    public class Wave
    {
        public int waveNumber;
        public List<EnemyCount> enemies;
        public bool boss = false;
    }

    [System.Serializable]
    public class EnemyCount
    {
        public string type;
        public int amount;
        public bool customSpawn = false;
        public Location customSpawnLocation;
    }

    [System.Serializable]
    public class Location
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
