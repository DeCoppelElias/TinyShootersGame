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
        firstSelected = this.transform.Find("ResumeButton").gameObject;
        uiTransition.FadeIn();
    }

    public override UIElement Disable()
    {
        uiTransition.FadeOut();
        onDisable.Invoke();
        return this;
    }

    public override bool Enabled()
    {
        return uiTransition.Enabled();
    }
}
