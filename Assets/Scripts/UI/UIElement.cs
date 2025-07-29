using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIElement : MonoBehaviour
{
    public virtual void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public virtual UIElement Disable()
    {
        this.gameObject.SetActive(false);
        return this;
    }
}
