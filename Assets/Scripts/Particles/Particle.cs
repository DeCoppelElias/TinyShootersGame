using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Particle : MonoBehaviour
{
    private Action onComplete;

    public void AssignOnComplete(Action action)
    {
        this.onComplete += action;
    }

    public abstract void Play();
    public abstract void Initialise(Vector3 position, Quaternion rotation, Color color);

    protected void Complete()
    {
        onComplete();
    }
}
