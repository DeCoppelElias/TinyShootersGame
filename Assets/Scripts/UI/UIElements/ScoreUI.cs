using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : UIElement
{
    private int actualScore = 0;
    private int displayedScore = 0;

    private Dictionary<ScoreManager.ScoreReason, ScoreEntry> entries = new Dictionary<ScoreManager.ScoreReason, ScoreEntry>();

    private float scoreCatchUpTime = 1;
    private float updateScoreCooldown = 0.1f;
    private int scoreJumpAmount = 0;
    private float lastUpdate = 0;

    private Text scoreText;
    private Transform scoreContent;
    [SerializeField] private GameObject scoreEntryPrefab;

    protected override void Start()
    {
        base.Start();

        scoreText = this.transform.Find("Score").GetComponent<Text>();
        scoreContent = this.transform.Find("SubScores").Find("Content");
    }

    public void AddScore(ScoreManager.ScoreReason reason, int amount)
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
                this.actualScore += entry.GetScore();
                int difference = actualScore - displayedScore;
                this.scoreJumpAmount = (int)(difference / (scoreCatchUpTime / updateScoreCooldown));
            });
            entries.Add(reason, entry);
        }
    }
    private void AddDisplayedScore(int amount)
    {
        displayedScore += amount;

        if (displayedScore > actualScore) displayedScore = actualScore;

        scoreText.text = displayedScore.ToString();
    }

    private void Update()
    {
        if (displayedScore < actualScore && Time.time - lastUpdate > updateScoreCooldown)
        {
            lastUpdate = Time.time;

            AddDisplayedScore(scoreJumpAmount);
        }
    }
    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }
    protected override void InstantDisableActions()
    {
        DisableActions();
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    public override bool IsEnabled()
    {
        return this.gameObject.activeSelf;
    }
    public override bool IsDisabled()
    {
        return !IsEnabled();
    }
}
