using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sherbert.Framework.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyWaveSpawner))]
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Level Settings")]
    [SerializeField] private int waveIndex = 0;
    [SerializeField] private int levelIndex = 0;
    [SerializeField] private int totalLevels = 3;
    [SerializeField] private int waveCooldown = 5;
    [SerializeField] private int levelCooldown = 5;
    [SerializeField] private int initialCooldown = 5;

    private Dictionary<int,Level> levels = new Dictionary<int,Level>();

    [Header("Enemy Prefabs")]
    private Dictionary<string, GameObject> enemyDict = new Dictionary<string, GameObject>();
    [SerializeField] private List<string> enemyNames = new List<string>();
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap warningTilemap;
    [SerializeField] private Tile warningTile;

    private enum WaveState { Initial, Fighting, Ready, Spawning, WaveCooldown, LevelCooldown, Done }
    [Header("State")]
    [SerializeField] private WaveState waveState = WaveState.Initial;
    [SerializeField] private GameObject enemies;

    private Player player;
    private PlayerUIManager playerUIManager;

    private float startTime = 0;
    private float lastWaveTime = 0;
    private float playerHealthBeforeWave = 0;
    private float betweenEnemySpawnDelay = 0.2f;
    private float enemySpawnDelay = 1;

    private float classUpgradeCooldown = 10;
    private float classUpgradeCounter = 0;

    private float powerupCooldown = 10;
    private float powerupCounter = 5;

    private EnemyWaveSpawner enemySpawner;

    private void Awake()
    {
        Instance = this;
    }

    private void SelectLevel()
    {
        if (SceneTransitionManager.Instance != null)
        {
            levelIndex = SceneTransitionManager.Instance.Level - 1;
        }

        StartCoroutine(ShowLevelUps(levelIndex));
    }

    private IEnumerator ShowLevelUps(int levelIndex)
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < levelIndex; i++)
        {
            // 1. Show Powerup first
            playerUIManager.Enable<PowerupUI>();
            yield return new WaitForSeconds(0.2f);

            // 2. Show Upgrade if available, else Powerup
            if (player.GetUpgrades().Count > 0)
                playerUIManager.Enable<UpgradeUI>();
            else
                playerUIManager.Enable<PowerupUI>();

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Start()
    {
        enemySpawner = GetComponent<EnemyWaveSpawner>();
        startTime = Time.time;

        player = GameObject.Find("Player").GetComponent<Player>();
        playerUIManager = player.GetComponentInChildren<PlayerUIManager>();

        InitializeDict();
        SelectLevel();
        SetupCurrentLevel();
    }

    void Update()
    {
        if (waveIndex >= 0)
        {
            if (waveState == WaveState.Initial)
            {
                if (Time.time - startTime > initialCooldown)
                {
                    waveState = WaveState.Ready;
                }
            }
            else if (waveState == WaveState.Ready)
            {
                SpawnWave(this.levelIndex, this.waveIndex);
            }
            else if (waveState == WaveState.Fighting)
            {
                if (enemies.transform.childCount == 0)
                {
                    EndWave();
                }
            }
            else if (waveState == WaveState.WaveCooldown)
            {
                if (Time.time - lastWaveTime > waveCooldown)
                {
                    waveState = WaveState.Ready;
                }
            }
            else if (waveState == WaveState.LevelCooldown)
            {
                if (Time.time - lastWaveTime > levelCooldown)
                {
                    waveState = WaveState.Ready;
                    NextLevel();
                }
            }
        }
    }

    private void EndWave()
    {
        // Give Score if player did not lose health
        if (player.Health == playerHealthBeforeWave)
        {
            ScoreManager.Instance.AddScore(ScoreManager.ScoreReason.PerfectWave, 1000);
        }

        WaveStatusUI waveStatusUI = SharedUIManager.Instance.GetUIElement<WaveStatusUI>();

        if (CheckLastLevel() && CheckLastWave())
        {
            waveState = WaveState.Done;
            waveStatusUI.EnableLevelCompletedText(levelIndex + 1);
            StartCoroutine(PerformAfterDelay(5, () => GameStateManager.Instance.GameWon()));
        }
        else if (!CheckLastLevel() && CheckLastWave())
        {
            waveState = WaveState.LevelCooldown;
            waveStatusUI.EnableLevelCompletedText(levelIndex + 1);
            lastWaveTime = Time.time;

            string levelString = $"Level {levelIndex + 1}";
            if (!PlayerPrefs.HasKey(levelString))
            {
                PlayerPrefs.SetString(levelString, "true");
            }

            RewardPlayer();
        }
        else if (!CheckLastWave())
        {
            waveIndex++;

            waveState = WaveState.WaveCooldown;
            lastWaveTime = Time.time;

            Level level = GetLevel(this.levelIndex);
            Wave wave = level.GetWave(waveIndex);
            waveStatusUI.PerformWaveCountdown(waveCooldown, wave.boss, levelIndex, waveIndex);

            RewardPlayer();
        }
    }

    private void RewardPlayer()
    {
        classUpgradeCounter++;
        if (classUpgradeCounter == classUpgradeCooldown)
        {
            classUpgradeCounter = 0;

            if (player.GetUpgrades().Count > 0) playerUIManager.Enable<UpgradeUI>();
            else playerUIManager.Enable<PowerupUI>();
        }

        powerupCounter++;
        if (powerupCounter == powerupCooldown)
        {
            powerupCounter = 0;
            playerUIManager.Enable<PowerupUI>();
        }
    }

    private void InitializeDict()
    {
        this.enemyDict.Clear();

        for (int i = 0; i < this.enemyPrefabs.Count; i++)
        {
            this.enemyDict.Add(this.enemyNames[i], this.enemyPrefabs[i]);
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

        CameraManager.Instance.TransitionCamera(level.GetCameraLocation());
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
        if (waveState != WaveState.Ready) return;

        waveState = WaveState.Spawning;

        playerHealthBeforeWave = player.Health;

        Level level = GetLevel(levelIndex);
        Wave wave = level.GetWave(waveIndex);
        List<EnemyCount> enemies = wave.enemies;

        int totalCount = 0;
        foreach (EnemyCount enemyCount in enemies)
        {
            if (!enemyCount.customSpawn) totalCount += enemyCount.amount;
        }

        List<string> enemiesToSpawn = new List<string>();
        List<(string, Vector3)> customEnemySpawns = new List<(string, Vector3)>();
        foreach (EnemyCount enemyCount in enemies)
        {
            if (enemyCount.customSpawn)
            {
                customEnemySpawns.Add((enemyCount.type, enemyCount.customSpawnLocation.ToVector3()));
            }
            else
            {
                for (int i = 0; i < enemyCount.amount; i++) enemiesToSpawn.Add(enemyCount.type);
            }
        }

        List<Vector3> spawnLocations = new List<Vector3>(level.GetSpawnLocations());
        StartCoroutine(enemySpawner.SpawnEnemiesSequentially(enemiesToSpawn, spawnLocations, customEnemySpawns));

        StartCoroutine(PerformAfterDelay((this.betweenEnemySpawnDelay * totalCount) + this.enemySpawnDelay, () => waveState = WaveState.Fighting));
    }

    public void NextLevel()
    {
        waveState = WaveState.WaveCooldown;
        lastWaveTime = Time.time;

        this.waveIndex = 0;
        this.levelIndex++;

        Level level = GetLevel(levelIndex);
        Wave wave = level.GetWave(waveIndex);

        SetupCurrentLevel();

        WaveStatusUI waveStatusUI = SharedUIManager.Instance.GetUIElement<WaveStatusUI>();
        waveStatusUI.PerformWaveCountdown(waveCooldown, wave.boss, waveIndex, levelIndex);

        player.Health = player.MaxHealth;
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
