using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public enum ScoreReason { EnemyKill, PerfectWave, MeleeKill, NoShotsMissed }

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(ScoreReason reason, int amount)
    {
        if (amount <= 0) return;

        this.score += amount;

        SharedUIManager.Instance?.GetUIElement<ScoreUI>()?.AddScore(reason, amount);
    }

    public int GetScore()
    {
        return this.score;
    }
}
