using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct GameOverUIData
{
    public bool beatHighScore;
    public float highScore;

    public GameOverUIData(bool beatHighScore, float highScore)
    {
        this.beatHighScore = beatHighScore;
        this.highScore = highScore;
    }
}

public class GameOverUI : UIElement<GameOverUIData>
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

    protected override void EnableActions(GameOverUIData data)
    {
        this.gameObject.SetActive(true);
        GameObject scoreGameObject = this.gameObject.transform.Find("Scores").Find("Score").gameObject;
        scoreGameObject.SetActive(false);
        GameObject newHighScoreGameObject = this.gameObject.transform.Find("Scores").Find("NewHighScore").gameObject;
        newHighScoreGameObject.SetActive(false);

        StartCoroutine(PerformAfterRealDelay(1, () =>
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
        EnableActions(new GameOverUIData(false, 0));
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
