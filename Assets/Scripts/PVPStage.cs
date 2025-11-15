using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PVPStage
{
    public Location roomLocation;
    public Location cameraLocation;
    public float roomWidth;
    public float roomHeight;
    public List<Location> playerSpawnLocations;
    public List<Location> enemySpawnLocations;
    public List<Wave> easyWaves;
    public List<Wave> normalWaves;
    public List<Wave> hardWaves;

    [Serializable]
    public class Wave
    {
        public List<EnemyCount> enemies;
    }

    [Serializable]
    public class EnemyCount
    {
        public string type;
        public int amount;
        public bool customSpawn = false;
        public Location customSpawnLocation;
    }

    [Serializable]
    public class Location
    {
        public float x, y, z;
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    public Wave GetRandomEasyWave()
    {
        int r = UnityEngine.Random.Range(0, easyWaves.Count);
        return easyWaves[r];
    }
    public Wave GetRandomNormalWave()
    {
        int r = UnityEngine.Random.Range(0, normalWaves.Count);
        return normalWaves[r];
    }
    public Wave GetRandomHardWave()
    {
        int r = UnityEngine.Random.Range(0, hardWaves.Count);
        return hardWaves[r];
    }

    public List<Vector3> GetEnemySpawnLocations()
    {
        List<Vector3> result = new List<Vector3>();
        foreach (var loc in enemySpawnLocations)
            result.Add(roomLocation.ToVector3() + loc.ToVector3());
        return result;
    }

    public List<Vector3> GetPlayerSpawnLocations()
    {
        List<Vector3> result = new List<Vector3>();
        foreach (var loc in playerSpawnLocations)
            result.Add(roomLocation.ToVector3() + loc.ToVector3());
        return result;
    }
    public Vector3 GetCameraLocation()
    {
        return roomLocation.ToVector3() + cameraLocation.ToVector3();
    }
}
