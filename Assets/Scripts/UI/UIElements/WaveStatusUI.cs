using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaveStatusUI : UIElement
{
    private GameObject waveCountdownUI;
    private Text waveCountdownText;
    private Text waveText;

    private Text waveNumberText;
    private Text levelNumberText;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        waveCountdownUI = this.transform.Find("WaveCountdown").gameObject;
        waveCountdownText = waveCountdownUI.transform.Find("Countdown").GetComponent<Text>();
        waveText = waveCountdownUI.transform.Find("Title").GetComponent<Text>();
        waveCountdownUI.SetActive(false);

        waveNumberText = this.transform.Find("CurrentWave").GetComponent<Text>();
        levelNumberText = this.transform.Find("CurrentLevel").GetComponent<Text>();
    }

    public void PerformWaveCountdown(int countdown, bool boss, int newLevel, int newWave)
    {
        if (waveCountdownUI.activeSelf) return;

        if (boss)
        {
            ColorUtility.TryParseHtmlString("#FA5C5C", out Color newColor);
            waveText.color = newColor;
            waveText.text = "BOSS wave starts in: ";
        }
        else
        {
            ColorUtility.TryParseHtmlString("#E6E6E6", out Color newColor);
            waveText.color = newColor;
            waveText.text = "Next wave starts in: ";
        }

        waveCountdownUI.SetActive(true);

        waveCountdownText.text = countdown.ToString();

        levelNumberText.text = $"Level: {newLevel}";
        waveNumberText.text = $"Wave: {newWave}";

        StartCoroutine(ReduceCountEverySecond(waveCountdownText, DisableCountdown));
    }

    public void EnableLevelCompletedText(int room)
    {
        if (waveCountdownUI.activeSelf) return;

        waveCountdownUI.SetActive(true);
        waveText.text = "You beat level " + room + "!";
    }

    private void DisableCountdown()
    {
        waveCountdownText.text = "";
        waveText.text = "";
        ColorUtility.TryParseHtmlString("#E6E6E6", out Color newColor);
        waveText.color = newColor;

        waveCountdownUI.SetActive(false);
    }

    private IEnumerator ReduceCountEverySecond(Text text, UnityAction onComplete = null)
    {
        yield return new WaitForSeconds(1);
        if (text.text != "")
        {
            int cooldown = int.Parse(text.text);
            if (cooldown > 0)
            {
                text.text = (cooldown - 1).ToString();
                StartCoroutine(ReduceCountEverySecond(text, onComplete));
            }
            else
            {
                if (onComplete != null) onComplete.Invoke();
            }
        }
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }
    protected override void InstantDisableActions()
    {
        DisableActions();
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }
}
