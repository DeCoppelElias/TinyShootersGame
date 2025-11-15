using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UITransition : MonoBehaviour
{
    public abstract bool IsEnabled();
    public abstract bool IsDisabled();
    public abstract void Transition();

    public abstract void ReverseTransition();

    public abstract void InstantReverseTransition();
}
