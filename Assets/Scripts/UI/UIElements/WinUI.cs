using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct WinUIData
{
    public bool beatBestTime;
    public float bestTime;
    public bool beatHighScore;
    public float highScore;

    public WinUIData(bool beatBestTime, float bestTime, bool beatHighScore, float highScore)
    {
        this.beatBestTime = beatBestTime;
        this.bestTime = bestTime;
        this.beatHighScore = beatHighScore;
        this.highScore = highScore;
    }
}

public class WinUI : UIElement<WinUIData>
{
    [SerializeField] private Text timeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text newBestTimeText;
    [SerializeField] private Text newBestScoreText;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;

    protected override void Start()
    {
        base.Start();

        GameStateManager.Instance.onWin.AddListener((beatBestTime, currentTime, beatHighScore, currentScore) => 
        {
            SharedUIManager.Instance.DisableAllUI();
            EnableActions(new WinUIData(beatBestTime, currentTime, beatHighScore, currentScore));
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
        timeText.enabled = false;
        scoreText.enabled = false;
        newBestTimeText.enabled = false;
        newBestScoreText.enabled = false;

        StartCoroutine(PerformAfterRealDelay(1, () =>
        {
            timeText.enabled = true;

            System.TimeSpan time = System.TimeSpan.FromSeconds(data.bestTime);
            timeText.text = $"Time: {time:hh\\:mm\\:ss}";

            if (data.beatBestTime)
            {
                newBestTimeText.enabled = true;
            }
        }));

        StartCoroutine(PerformAfterRealDelay(2, () =>
        {
            scoreText.enabled = true;

            scoreText.text = $"Score: {data.highScore}";

            if (data.beatHighScore)
            {
                newBestScoreText.enabled = true;
            }
        }));

        firstSelected = this.GetComponentsInChildren<Button>()[0].gameObject;
    }

    protected override void EnableActions()
    {
        EnableActions(new WinUIData(false, 99999999, false, 0));
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    private IEnumerator PerformAfterRealDelay(float delay, UnityAction action)
    {
        yield return new WaitForSecondsRealtime(delay);

        action.Invoke();
    }
}
