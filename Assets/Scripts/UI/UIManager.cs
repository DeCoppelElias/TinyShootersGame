using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class UIManager : MonoBehaviour
{
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected EventSystem eventSystem;

    protected readonly Dictionary<System.Type, GameObject> uiElements = new Dictionary<System.Type, GameObject>();

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
        if (panel.PausesGame) GameStateManager.Instance.RequestPause();
        FirstSelected = panel.GetFirstSelected();
    }
    private void OnDisableActions(UIElement panel)
    {
        if (panel.PausesGame) GameStateManager.Instance.ReleasePause();
        if (FirstSelected == panel.GetFirstSelected()) FirstSelected = null;
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

    protected void EnableFirstSelected()
    {
        Debug.Log($"Enabling first selected: {FirstSelected}");
        eventSystem.SetSelectedGameObject(FirstSelected);
    }

    protected void DisableFirstSelected()
    {
        Debug.Log($"Disabling first selected: {FirstSelected}");
        eventSystem.SetSelectedGameObject(null);
    }

    protected bool IsController(PlayerInput playerInput)
    {
        return playerInput != null && Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad";
    }
}
