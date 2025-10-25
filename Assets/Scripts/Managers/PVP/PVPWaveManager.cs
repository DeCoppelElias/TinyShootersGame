using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyWaveSpawner))]
public class PVPWaveManager : MonoBehaviour
{
    public static PVPWaveManager Instance;

    private enum PVPBattleState { Initial, Easy, Normal, Hard }

    [Header("Wave Timing")]
    [SerializeField] private int stateCooldown = 60;
    [SerializeField] private int waveCooldown = 30;

    [Header("Spawn Settings")]
    [SerializeField] private EnemyRegistry enemyRegistry;
    [SerializeField] private Transform spawnParent;

    [SerializeField] private PVPBattleState pvpBattleState = PVPBattleState.Initial;
    private float lastStateUpgrade;
    private float lastWaveSpawn;
    private bool battleActive = false;

    private PVPStage currentLevel;
    private EnemyWaveSpawner enemySpawner;

    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        enemySpawner = GetComponent<EnemyWaveSpawner>();

        PVPBattleManager.Instance.OnBattleStarted.AddListener(HandleBattleStart);
        PVPBattleManager.Instance.OnBattleEnded.AddListener(HandleBattleEnd);
    }

    private void OnEnable()
    {
        PVPBattleManager.Instance?.OnBattleStarted.AddListener(HandleBattleStart);
        PVPBattleManager.Instance?.OnBattleEnded.AddListener(HandleBattleEnd);
    }

    private void OnDisable()
    {
        PVPBattleManager.Instance.OnBattleStarted.RemoveListener(HandleBattleStart);
        PVPBattleManager.Instance.OnBattleEnded.RemoveListener(HandleBattleEnd);
    }

    private void Update()
    {
        if (!battleActive || currentLevel == null)
            return;

        HandleStateTransitions();
        HandleWaveSpawning();
    }

    // Called by PVPBattleManager when level JSON is loaded
    public void Initialize(PVPStage level)
    {
        currentLevel = level;
        Debug.Log("[PVPWaveManager] Level data initialized.");
    }

    private void HandleBattleStart(PVPStage pvpStage)
    {
        currentLevel = pvpStage;

        Debug.Log("[PVPWaveManager] Battle started.");
        battleActive = true;
        lastStateUpgrade = Time.time;
        lastWaveSpawn = Time.time;
        pvpBattleState = PVPBattleState.Initial;
    }

    private void HandleBattleEnd()
    {
        Debug.Log("[PVPWaveManager] Battle ended.");
        battleActive = false;

        for(int i = 0; i < spawnParent.childCount; i++)
        {
            Destroy(spawnParent.GetChild(i).gameObject);
        }
    }

    private void HandleStateTransitions()
    {
        if (Time.time - lastStateUpgrade < stateCooldown)
            return;

        lastStateUpgrade = Time.time;

        switch (pvpBattleState)
        {
            case PVPBattleState.Initial:
                pvpBattleState = PVPBattleState.Easy;
                break;
            case PVPBattleState.Easy:
                pvpBattleState = PVPBattleState.Normal;
                break;
            case PVPBattleState.Normal:
                pvpBattleState = PVPBattleState.Hard;
                break;
            case PVPBattleState.Hard:
                break;
        }

        Debug.Log($"[PVPWaveManager] Difficulty upgraded to: {pvpBattleState}");
    }

    private void HandleWaveSpawning()
    {
        if (pvpBattleState == PVPBattleState.Initial) return;
        if (Time.time - lastWaveSpawn < waveCooldown) return;

        PVPStage.Wave wave = GetRandomWave();
        if (wave == null) return;

        lastWaveSpawn = Time.time;
        SpawnWave(wave);
    }

    private PVPStage.Wave GetRandomWave()
    {
        if (pvpBattleState == PVPBattleState.Easy)
        {
            return currentLevel.GetRandomEasyWave();
        }
        else if (pvpBattleState == PVPBattleState.Normal)
        {
            return currentLevel.GetRandomNormalWave();
        }
        else if (pvpBattleState == PVPBattleState.Hard)
        {
            return currentLevel.GetRandomHardWave();
        }
        return null;
    }

    private void SpawnWave(PVPStage.Wave wave)
    {
        Debug.Log($"[PVPWaveManager] Spawning wave ({pvpBattleState})");

        List<string> enemiesToSpawn = new List<string>();
        List<(string, Vector3)> customEnemySpawns = new List<(string, Vector3)>();
        foreach (PVPStage.EnemyCount enemyCount in wave.enemies)
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

        List<Vector3> spawnLocations = new List<Vector3>(currentLevel.GetEnemySpawnLocations());
        StartCoroutine(enemySpawner.SpawnEnemiesSequentially(enemiesToSpawn, spawnLocations, customEnemySpawns));
    }
}
