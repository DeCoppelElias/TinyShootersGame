using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIElement : MonoBehaviour
{
    protected GameObject firstSelected;

    public UnityEvent onDisable;

    protected virtual void Start()
    {
        
    }
    public virtual void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public virtual UIElement Disable()
    {
        this.gameObject.SetActive(false);
        onDisable.Invoke();
        return this;
    }

    public virtual bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    public virtual GameObject GetFirstSelected()
    {
        return firstSelected;
    }
}
