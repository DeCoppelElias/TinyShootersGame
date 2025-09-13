using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class UIManager : MonoBehaviour
{
    [SerializeField] protected Canvas canvas;
    protected readonly Dictionary<System.Type, GameObject> uiElements = new Dictionary<System.Type, GameObject>();

    [SerializeField] protected PlayerInput currentControllingPlayer;
    private string currentControlScheme;

    [Header("Panel Statistics")]
    [SerializeField] private int enabledPanels = 0;
    [SerializeField] private int disabledPanels = 0;
    [SerializeField] private int enabledPausePanels = 0;

    public GameObject FirstSelected { get; private set; }

    public virtual void Awake()
    {
        LoadUIPanels();
        RegisterCallbacks();
    }

    /// <summary>
    /// Load UI elements in class variable.
    /// </summary>
    protected abstract void LoadUIPanels();

    private void RegisterCallbacks()
    {
        foreach (GameObject uiGO in uiElements.Values)
        {
            UIElement uiElement = uiGO.GetComponent<UIElement>();

            uiElement.AddOnEnableAction(OnEnableActions);
            uiElement.AddOnDisableAction(OnDisableActions);
        }
    }

    public T GetUIElement<T>() where T : UIElement
    {
        if (uiElements.TryGetValue(typeof(T), out var go))
            return go.GetComponent<T>();
        return null;
    }

    public T GetUIElement<T, TData>() where T : UIElement<TData>
    {
        if (uiElements.TryGetValue(typeof(T), out var go))
            return go.GetComponent<T>();
        return null;
    }

    public void Toggle<T>() where T : UIElement
    {
        var panel = GetUIElement<T>();
        panel.Toggle();
    }

    public void Enable<T>() where T : UIElement
    {
        var panel = GetUIElement<T>();
        panel?.Enable();
    }

    private void OnEnableActions(UIElement panel)
    {
        enabledPanels++;
        if (panel.PausesGame) enabledPausePanels++;
        if (enabledPausePanels == 1) GameStateManager.Instance.ToPaused();

        SetFirstSelectedIfGamepad(panel.GetFirstSelected());
    }
    private void OnDisableActions(UIElement panel)
    {
        enabledPanels--;
        if (panel.PausesGame) enabledPausePanels--;
        if (enabledPausePanels == 0)
        {
            GameStateManager.Instance.ToRunning();
            RemoveFirstSelected();
        }
    }

    public void Disable<T>() where T : UIElement
    {
        var panel = GetUIElement<T>();
        panel?.Disable();
    }

    public void Enable<T, TData>(TData data) where T : UIElement<TData>
    {
        var panel = GetUIElement<T, TData>();
        panel?.Enable(data);
    }
    public void Toggle<T, TData>(TData data) where T : UIElement<TData>
    {
        var panel = GetUIElement<T, TData>();
        panel.Toggle(data);
    }

    private void SetFirstSelectedIfGamepad(GameObject obj)
    {
        FirstSelected = obj;

        // Only select first if player is using a gamepad
        if (currentControllingPlayer != null && Gamepad.current != null && Gamepad.current.enabled && currentControllingPlayer.currentControlScheme == "Gamepad") EventSystem.current.SetSelectedGameObject(obj);
    }

    private void RemoveFirstSelected()
    {
        FirstSelected = null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetCurrentControllingPlayer(PlayerInput controllingPlayer)
    {
        if (controllingPlayer == null) return;
        if (currentControllingPlayer == controllingPlayer) return;

        this.currentControllingPlayer = controllingPlayer;
        this.currentControlScheme = controllingPlayer.currentControlScheme;
    }

    private void Update()
    {
        if (currentControllingPlayer != null && currentControllingPlayer.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = currentControllingPlayer.currentControlScheme;
            OnControlsChanged();
        }
    }

    private void OnControlsChanged()
    {
        if (Gamepad.current != null && Gamepad.current.enabled && currentControllingPlayer.currentControlScheme == "Gamepad")
        {
            EventSystem.current.SetSelectedGameObject(FirstSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
