using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Canvas canvas;
    [SerializeField] private UISceneProfile sceneUIProfile;

    private MultiplayerEventSystem playerEventSystem;

    private readonly Dictionary<System.Type, GameObject> uiElements = new Dictionary<System.Type, GameObject>();

    private PlayerInput playerInput;

    private string currentControlScheme;
    public GameObject FirstSelected { get; private set; }
    
    private void Awake()
    {
        foreach (var prefab in sceneUIProfile.playerUIPrefabs)
        {
            var go = Instantiate(prefab, canvas.transform.GetChild(0).transform);

            PlayerUIElement uiElement = go.GetComponent<PlayerUIElement>();
            uiElement.Initialize(player);
            uiElements[uiElement.GetType()] = go;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        canvas.worldCamera = Camera.main;

        playerEventSystem = player.GetComponentInChildren<MultiplayerEventSystem>();
        playerInput = player.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            OnControlsChanged();
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
        if (!panel.Enabled()) panel.Enable();
        else panel.Disable();
    }

    public void Enable<T>() where T : UIElement
    {
        var panel = GetUIElement<T>();
        panel?.Enable();

        SetFirstSelectedIfGamepad(panel.GetFirstSelected());
    }

    public void Disable<T>() where T : UIElement
    {
        var panel = GetUIElement<T>();
        panel?.Disable();

        RemoveFirstSelected();
    }

    public void Enable<T, TData>(TData data) where T : UIElement<TData>
    {
        var panel = GetUIElement<T, TData>();
        panel?.Enable(data);

        SetFirstSelectedIfGamepad(panel.GetFirstSelected());
    }
    public void Toggle<T, TData>(TData data) where T : UIElement<TData>
    {
        var panel = GetUIElement<T, TData>();
        panel.Toggle(data);
    }

    private void OnControlsChanged()
    {
        Debug.Log($"Controls Changed to {playerInput.currentControlScheme}");
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            Debug.Log($"Setting first selected: {FirstSelected}");
            playerEventSystem.SetSelectedGameObject(FirstSelected);
        }
        else
        {
            playerEventSystem.SetSelectedGameObject(null);
        }
    }

    private void SetFirstSelectedIfGamepad(GameObject obj)
    {
        FirstSelected = obj;

        // Only select first if player is using a gamepad
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            Debug.Log($"Setting first selected {FirstSelected}");
            playerEventSystem.SetSelectedGameObject(obj);
        }
    }

    private void RemoveFirstSelected()
    {
        FirstSelected = null;
        playerEventSystem.SetSelectedGameObject(null);
    }
}
