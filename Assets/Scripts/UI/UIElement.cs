using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIElement : MonoBehaviour
{
    protected Player player;
    protected UIManager uiManager;

    protected virtual void Start()
    {
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }
    public virtual void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public virtual UIElement Disable()
    {
        this.gameObject.SetActive(false);
        return this;
    }

    public virtual bool Enabled()
    {
        return this.gameObject.activeSelf;
    }
}
