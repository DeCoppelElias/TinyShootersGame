using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public enum ScoreReason { EnemyKill, PerfectWave, MeleeKill, NoShotsMissed }

    // Events:
    // Fired whenever an entry for a reason is created/updated
    public event Action<ScoreReason, int /*entryScore*/, int /*multiplier*/, float /*timeRemaining*/, float /*totalDuration*/> OnEntryUpdated;
    // Fired when an entry finishes and the amount is committed to the total score
    public event Action<ScoreReason, int /*committedAmount*/, int /*newTotalScore*/> OnEntryCommitted;
    // Fired whenever total score changes
    public event Action<int /*totalScore*/> OnTotalScoreChanged;

    private class Entry
    {
        public int score;
        public int multiplier;
        public float expiryTime;
        public float duration;
        public float durationAdd;
    }

    private readonly Dictionary<ScoreReason, Entry> _entries = new Dictionary<ScoreReason, Entry>();
    private int _totalScore = 0;

    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private float defaultDurationAdd = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // check expirations
        var now = Time.time;
        var expired = new List<ScoreReason>();
        foreach (var kv in _entries)
        {
            var reason = kv.Key;
            var e = kv.Value;
            if (now >= e.expiryTime)
                expired.Add(reason);
            else
            {
                // notify UI about remaining time
                float remaining = e.expiryTime - now;
                OnEntryUpdated?.Invoke(reason, e.score * e.multiplier, e.multiplier, remaining, e.duration);
            }
        }

        foreach (var reason in expired)
        {
            CommitEntry(reason);
        }
    }

    private void CommitEntry(ScoreReason reason)
    {
        if (!_entries.TryGetValue(reason, out var e)) return;

        int committed = e.score * e.multiplier;
        _entries.Remove(reason);

        _totalScore += committed;
        OnEntryCommitted?.Invoke(reason, committed, _totalScore);
        OnTotalScoreChanged?.Invoke(_totalScore);
    }

    /// <summary>
    /// Add a score for a reason. If an entry for that reason exists and has not expired, it will
    /// be combined (score sum, multiplier++) and expiry extended by entry.durationAdd.
    /// When the entry expires, its (score * multiplier) is added to the total and the entry resets.
    /// </summary>
    public void AddScore(ScoreReason reason, int amount)
    {
        if (amount <= 0) return;

        var now = Time.time;
        if (_entries.TryGetValue(reason, out var e))
        {
            // still active: add and extend
            e.score += amount;
            e.multiplier += 1;
            e.expiryTime = now + e.duration + e.durationAdd;

            // notify UI
            OnEntryUpdated?.Invoke(reason, e.score * e.multiplier, e.multiplier, e.expiryTime - now, e.duration);
        }
        else
        {
            // create new entry
            var entry = new Entry
            {
                score = amount,
                multiplier = 1,
                duration = defaultDuration,
                durationAdd = defaultDurationAdd,
                expiryTime = now + defaultDuration
            };
            _entries.Add(reason, entry);

            // notify UI that a new entry exists
            OnEntryUpdated?.Invoke(reason, entry.score * entry.multiplier, entry.multiplier, entry.expiryTime - now, entry.duration);
        }
    }

    public int GetTotalScore() => _totalScore;

    /// <summary>
    /// Returns the total score including active (not yet committed) entries.
    /// </summary>
    public int GetTotalScoreIncludingActive()
    {
        int total = _totalScore;
        foreach (var entry in _entries.Values)
            total += entry.score * entry.multiplier;
        return total;
    }

    /// <summary>
    /// Force commit all active entries
    /// </summary>
    public void CommitAll()
    {
        var keys = new List<ScoreReason>(_entries.Keys);
        foreach (var k in keys) CommitEntry(k);
    }
}
