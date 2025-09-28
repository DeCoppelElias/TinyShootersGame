using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PVPStatusUI : UIElement
{
    [SerializeField] private GameObject countdownUIElement;
    [SerializeField] private Text countDownText;

    protected override void Start()
    {
        base.Start();

        DisableCountdown();
    }
    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    protected override void InstantDisableActions()
    {
        this.gameObject.SetActive(false);
    }

    public void PerformCountdown(int countdown)
    {
        ColorUtility.TryParseHtmlString("#E6E6E6", out Color newColor);
        countDownText.text = countdown.ToString();
        countDownText.color = newColor;
        countdownUIElement.SetActive(true);

        StartCoroutine(ReduceCountEverySecond(countdown, DisableCountdown));
    }
    private void DisableCountdown()
    {
        countDownText.text = "";
        countdownUIElement.SetActive(false);
    }

    private IEnumerator ReduceCountEverySecond(int count, UnityAction onComplete = null)
    {
        yield return new WaitForSeconds(1);
        if (countDownText.text != "")
        {
            if (count > 0)
            {
                countDownText.text = (count - 1).ToString();
                StartCoroutine(ReduceCountEverySecond(count - 1, onComplete));
            }
            else
            {
                if (onComplete != null) onComplete.Invoke();
            }
        }
    }
}
