using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UITransition))]
public class PauseUI : UIElement
{
    private UITransition uiTransition;

    protected override void Start()
    {
        base.Start();
        uiTransition = GetComponent<UITransition>();
    }

    public override void Enable()
    {
        uiManager.SetFirstSelectedIfGamepad(this.transform.Find("ResumeButton").gameObject);
        uiTransition.FadeIn();
    }

    public override UIElement Disable()
    {
        uiTransition.FadeOut();
        return this;
    }

    public override bool Enabled()
    {
        return uiTransition.Enabled();
    }
}
