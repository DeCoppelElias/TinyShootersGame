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
    protected override void Start()
    {
        base.Start();

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
        GameObject timeGameObject = this.gameObject.transform.Find("Scores").Find("Time").gameObject;
        timeGameObject.SetActive(false);
        GameObject scoreGameObject = this.gameObject.transform.Find("Scores").Find("Score").gameObject;
        scoreGameObject.SetActive(false);
        GameObject newBestTimeGameObject = this.gameObject.transform.Find("Scores").Find("NewBestTime").gameObject;
        newBestTimeGameObject.SetActive(false);
        GameObject newHighScoreGameObject = this.gameObject.transform.Find("Scores").Find("NewHighScore").gameObject;
        newHighScoreGameObject.SetActive(false);

        StartCoroutine(PerformAfterRealDelay(1, () =>
        {
            timeGameObject.SetActive(true);

            Text timeText = timeGameObject.GetComponent<Text>();
            System.TimeSpan time = System.TimeSpan.FromSeconds(data.bestTime);
            timeText.text = $"Time: {time:hh\\:mm\\:ss}";

            if (data.beatBestTime)
            {
                newBestTimeGameObject.gameObject.SetActive(true);
            }
        }));

        StartCoroutine(PerformAfterRealDelay(2, () =>
        {
            scoreGameObject.SetActive(true);

            Text scoreText = scoreGameObject.GetComponent<Text>();
            scoreText.text = $"Score: {data.highScore}";

            if (data.beatHighScore)
            {
                newHighScoreGameObject.SetActive(true);
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
