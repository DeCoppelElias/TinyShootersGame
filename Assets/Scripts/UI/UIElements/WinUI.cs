using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct WinUIData
{
    public bool beatHighScore;
    public float highScore;

    public WinUIData(bool beatHighScore, float highScore)
    {
        this.beatHighScore = beatHighScore;
        this.highScore = highScore;
    }
}

public class WinUI : UIElement<WinUIData>
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text newBestScoreText;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;

    protected override void Start()
    {
        base.Start();

        GameStateManager.Instance.onWin.AddListener((beatHighScore, currentScore) =>
        {
            SharedUIManager.Instance.DisableAllUI();
            EnableActions(new WinUIData(beatHighScore, currentScore));
        });

        mainMenuButton.onClick.AddListener(GameStateManager.Instance.QuitToMainMenu);
        restartButton.onClick.AddListener(GameStateManager.Instance.Restart);

        this.pausesGame = true;
        InstantDisableActions();
    }
    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }
    protected override void InstantDisableActions()
    {
        DisableActions();
    }

    protected override void EnableActions(WinUIData data)
    {
        this.gameObject.SetActive(true);
        scoreText.enabled = false;
        newBestScoreText.enabled = false;

        StartCoroutine(PerformAfterRealDelay(1, () =>
        {
            scoreText.enabled = true;
            float animateScoreDuration = 3f;
            StartCoroutine(AnimateScoreText(data.highScore, animateScoreDuration));

            StartCoroutine(PerformAfterRealDelay(animateScoreDuration, () => 
            {
                if (data.beatHighScore)
                {
                    newBestScoreText.enabled = true;
                }
            }));
            
        }));

        firstSelected = this.GetComponentsInChildren<Button>()[0].gameObject;
    }

    protected override void EnableActions()
    {
        EnableActions(new WinUIData(false, 0));
    }

    public override bool IsEnabled()
    {
        return this.gameObject.activeSelf;
    }
    public override bool IsDisabled()
    {
        return !IsEnabled();
    }


    private IEnumerator PerformAfterRealDelay(float delay, UnityAction action)
    {
        yield return new WaitForSecondsRealtime(delay);

        action.Invoke();
    }

    private IEnumerator AnimateScoreText(float targetScore, float duration)
    {
        float timeElapsed = 0f;
        float displayedScore = 0f;
        Vector3 originalScale = scoreText.transform.localScale;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime; // use unscaled time since game is paused
            float t = Mathf.Clamp01(timeElapsed / duration);

            // Smoothly interpolate score
            displayedScore = Mathf.Lerp(0, targetScore, t);
            scoreText.text = Mathf.RoundToInt(displayedScore).ToString();

            scoreText.transform.localScale = originalScale * ScaleOverTime(t);

            yield return null;
        }

        // Ensure it ends exactly at target score
        scoreText.text = Mathf.RoundToInt(targetScore).ToString();
        scoreText.transform.localScale = originalScale;
    }

    float ScaleOverTime(float t)
    {
        // Clamp t between 0 and 1
        t = Mathf.Clamp01(t);

        if (t <= 0.7f)
        {
            // First phase: grow from 1 → 2
            float progress = t / (2f / 3f);
            return Mathf.Lerp(1f, 2f, Mathf.SmoothStep(0f, 1f, progress));
        }
        else
        {
            // Second phase: shrink from 2 → 1
            float progress = (t - 2f / 3f) / (1f / 3f);
            return Mathf.Lerp(2f, 1f, Mathf.SmoothStep(0f, 1f, progress));
        }
    }
}
