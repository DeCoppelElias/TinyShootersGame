using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIFadeTransition))]
public class PauseUI : UIElement
{
    private UITransition uiTransition;

    private Button resumeButton;
    private Button quitButton;
    private UITransition UiTransition { 
        get
        {
            if (uiTransition == null) return this.GetComponent<UITransition>();
            return uiTransition;
        }
        set => uiTransition = value;
    }

    protected override void Start()
    {
        base.Start();
        UiTransition = GetComponent<UIFadeTransition>();

        resumeButton = this.transform.Find("ResumeButton").GetComponent<Button>();
        resumeButton.onClick.AddListener(Disable);

        quitButton = this.transform.Find("MainMenuButton").GetComponent<Button>();
        quitButton.onClick.AddListener(GameStateManager.Instance.QuitToMainMenu);

        InstantDisableActions();
    }

    protected override void EnableActions()
    {
        firstSelected = this.transform.Find("ResumeButton").gameObject;
        UiTransition.Transition();
    }

    protected override void DisableActions()
    {
        UiTransition.ReverseTransition();
    }
    protected override void InstantDisableActions()
    {
        UiTransition.InstantReverseTransition();
    }

    public override bool IsEnabled()
    {
        return UiTransition.IsEnabled();
    }
    public override bool IsDisabled()
    {
        return UiTransition.IsDisabled();
    }
}
