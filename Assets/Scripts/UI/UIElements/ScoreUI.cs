using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : UIElement
{
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private float scoreCatchUpTime = 1f;
    [SerializeField] private float updateScoreCooldown = 0.1f;

    private Text _scoreText;
    private Transform _scoreContent;
    private readonly Dictionary<ScoreManager.ScoreReason, ScoreEntry> _uiEntries = new Dictionary<ScoreManager.ScoreReason, ScoreEntry>();

    private int _displayedScore = 0;
    private int _targetScore = 0;
    private float _lastUpdate = 0f;
    private int _scoreStep = 0;

    protected override void Start()
    {
        base.Start();
        _scoreText = transform.Find("Score").GetComponent<Text>();
        _scoreContent = transform.Find("SubScores/Content");

        var manager = ScoreManager.Instance;
        if (manager != null)
        {
            manager.OnEntryUpdated += HandleEntryUpdated;
            manager.OnEntryCommitted += HandleEntryCommitted;
            manager.OnTotalScoreChanged += HandleTotalScoreChanged;
        }
    }

    private void HandleEntryUpdated(ScoreManager.ScoreReason reason, int entryScore, int multiplier, float timeRemaining, float totalDuration)
    {
        if (!_uiEntries.TryGetValue(reason, out var uiEntry))
        {
            var go = Instantiate(scoreEntryPrefab, _scoreContent);
            uiEntry = go.GetComponent<ScoreEntry>();
            _uiEntries.Add(reason, uiEntry);
        }

        uiEntry.SetReason(reason.ToString());
        uiEntry.UpdateScore(entryScore, multiplier, timeRemaining, totalDuration);
    }

    private void HandleEntryCommitted(ScoreManager.ScoreReason reason, int committedAmount, int newTotal)
    {
        // remove UI entry for that reason (it finished)
        if (_uiEntries.TryGetValue(reason, out var uiEntry))
        {
            uiEntry.PlayCommitAndDestroy(); // graceful UI close (optional animation) then Destroy
            _uiEntries.Remove(reason);
        }

        // update total target score and animate catch-up
        _targetScore = newTotal;
        StartScoreCatchUp();
    }

    private void HandleTotalScoreChanged(int newTotal)
    {
        _targetScore = newTotal;
        StartScoreCatchUp();
    }

    private void StartScoreCatchUp()
    {
        int diff = _targetScore - _displayedScore;
        if (diff <= 0) { _scoreStep = 0; return; }
        _scoreStep = Mathf.Max(1, (int)(diff / (scoreCatchUpTime / updateScoreCooldown)));
    }

    private void Update()
    {
        if (_displayedScore < _targetScore && Time.time - _lastUpdate > updateScoreCooldown)
        {
            _lastUpdate = Time.time;
            _displayedScore += _scoreStep;
            if (_displayedScore > _targetScore) _displayedScore = _targetScore;
            _scoreText.text = _displayedScore.ToString();
        }
    }

    protected override void DisableActions() => gameObject.SetActive(false);
    protected override void InstantDisableActions() => DisableActions();
    protected override void EnableActions() => gameObject.SetActive(true);
    public override bool IsEnabled() => gameObject.activeSelf;
    public override bool IsDisabled() => !IsEnabled();
}
