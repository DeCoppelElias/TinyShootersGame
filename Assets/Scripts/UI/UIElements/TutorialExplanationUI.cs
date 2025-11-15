using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialExplanationUI : UIElement
{
    private Text explanationTitle;
    private Text doneText;
    private TextMeshProUGUI explanationSubTitle;
    public override bool IsEnabled()
    {
        return this.gameObject.activeSelf;
    }
    public override bool IsDisabled()
    {
        return !IsEnabled();
    }

    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    protected override void InstantDisableActions()
    {
        this.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        explanationTitle = this.transform.Find("TitleContainer").Find("Title").GetComponent<Text>();
        doneText = this.transform.Find("TitleContainer").Find("Done").GetComponent<Text>();
        doneText.text = "";
        explanationSubTitle = this.transform.Find("SubTitle").GetComponent<TextMeshProUGUI>();

        DisableDoneText();
    }

    public void EnableDoneText()
    {
        this.doneText.text = "Done!";
    }

    public void DisableDoneText()
    {
        this.doneText.text = "";
    }

    public void SetTitle(string title, string subtitle)
    {
        explanationTitle.text = title;
        explanationSubTitle.text = subtitle;
    }
}
