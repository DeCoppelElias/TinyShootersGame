using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIElement : MonoBehaviour
{
    protected GameObject firstSelected;

    [SerializeField] protected UnityEvent<UIElement> onDisable = new UnityEvent<UIElement>();
    [SerializeField] protected UnityEvent<UIElement> onEnable = new UnityEvent<UIElement>();

    [SerializeField] protected bool pausesGame = false;
    public bool PausesGame => pausesGame;

    protected virtual void Start()
    {

    }

    public virtual void Enable()
    {
        if (Enabled()) return;
        EnableActions();
        if (onEnable != null) onEnable.Invoke(this);
    }
    public virtual void Disable()
    {
        if (!Enabled()) return;
        DisableActions();
        if (onDisable != null) onDisable.Invoke(this);
    }
    public virtual void InstantDisable()
    {
        if (!Enabled()) return;
        InstantDisableActions();
        if (onDisable != null) onDisable.Invoke(this);
    }
    protected abstract void EnableActions();
    protected abstract void DisableActions();
    protected abstract void InstantDisableActions();
    public abstract bool Enabled();

    public void Toggle()
    {
        if (!Enabled()) Enable();
        else Disable();
    }

    public virtual GameObject GetFirstSelected()
    {
        return firstSelected;
    }

    public void AddOnDisableAction(UnityAction<UIElement> action)
    {
        onDisable.AddListener(action);
    }
    public void RemoveOnDisableAction(UnityAction<UIElement> action)
    {
        onDisable.RemoveListener(action);
    }
    public void AddOnEnableAction(UnityAction<UIElement> action)
    {
        onEnable.AddListener(action);
    }
    public void RemoveOnEnableAction(UnityAction<UIElement> action)
    {
        onEnable.RemoveListener(action);
    }
}

public abstract class UIElement<T> : UIElement
{
    public void Enable(T data)
    {
        if (Enabled()) return;
        EnableActions(data);
        if (onEnable != null) onEnable.Invoke(this);
    }
    protected abstract void EnableActions(T data);

    public void Toggle(T data)
    {
        if (!Enabled()) Enable(data);
        else Disable();
    }
}

public abstract class PlayerUIElement : UIElement
{
    protected Player player;

    protected bool initialized = false;
    protected UnityEvent onInitialized = new UnityEvent();

    public virtual void Initialize(Player player)
    {
        if (player == null) return;
        
        this.player = player;
        initialized = true;
        onInitialized?.Invoke();
    }

    public override void Enable()
    {
        if (initialized) base.Enable();
    }

    public override void Disable()
    {
        if (initialized) base.Disable();
    }

    public override void InstantDisable()
    {
        if (initialized) base.InstantDisable();
    }

    protected void AddOnInitializedListener(UnityAction onInitializedAction)
    {
        if (initialized) onInitializedAction.Invoke();
        else this.onInitialized.AddListener(onInitializedAction);
    }
}
