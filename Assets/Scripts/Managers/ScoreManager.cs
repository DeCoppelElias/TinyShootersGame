using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int score = 0;
    private int displayedScore = 0;

    private float scoreCatchUpTime = 1;
    private float updateScoreCooldown = 0.1f;
    private int scoreJumpAmount = 0;
    private float lastUpdate = 0;

    [SerializeField] private Text scoreText;
    [SerializeField] private Transform scoreContent;
    [SerializeField] private GameObject scoreEntryPrefab;

    public enum ScoreReason { EnemyKill, PerfectWave, MeleeKill, NoShotsMissed}
    private Dictionary<ScoreReason, ScoreEntry> entries = new Dictionary<ScoreReason, ScoreEntry>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(ScoreReason reason, int amount)
    {
        if (amount <= 0) return;

        ScoreEntry entry = null;
        if (entries.ContainsKey(reason))
        {
            entry = entries[reason];
            entry.AddScore(amount);
        }
        else
        {
            GameObject entryGameObject = Instantiate(scoreEntryPrefab, scoreContent);
            entry = entryGameObject.GetComponent<ScoreEntry>();
            entry.Initialise(reason.ToString(), amount, () =>
            {
                entries.Remove(reason);
                this.score += entry.GetScore();
                int difference = score - displayedScore;
                this.scoreJumpAmount = (int)(difference / (scoreCatchUpTime / updateScoreCooldown));
            });
            entries.Add(reason, entry);
        }
    }

    public int GetScore()
    {
        int totalscore = this.score;
        foreach (ScoreEntry scoreEntry in entries.Values)
        {
            totalscore += scoreEntry.GetScore();
        }

        return totalscore;
    }

    private void AddDisplayedScore(int amount)
    {
        displayedScore += amount;

        if (displayedScore > score) displayedScore = score;

        scoreText.text = displayedScore.ToString();
    }

    private void Update()
    {
        if (displayedScore < score && Time.time - lastUpdate > updateScoreCooldown)
        {
            lastUpdate = Time.time;

            AddDisplayedScore(scoreJumpAmount);
        }
    }
}
