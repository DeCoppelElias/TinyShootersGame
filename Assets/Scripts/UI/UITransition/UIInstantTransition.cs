using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInstantTransition : UITransition
{
    public override void InstantReverseTransition()
    {
        this.gameObject.SetActive(false);
    }

    public override bool IsDisabled()
    {
        return !IsEnabled();
    }

    public override bool IsEnabled()
    {
        return this.gameObject.activeSelf;
    }

    public override void ReverseTransition()
    {
        this.gameObject.SetActive(false);
    }

    public override void Transition()
    {
        this.gameObject.SetActive(true);
    }
}
