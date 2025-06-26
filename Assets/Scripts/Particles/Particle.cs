using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Particle : MonoBehaviour
{
    private Action onComplete;

    public void AssignOnComplete(Action action)
    {
        this.onComplete = action;
    }

    public abstract void Play();

    protected void Complete()
    {
        onComplete();
    }
}
