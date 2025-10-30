using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SharedUIManager : UIManager
{
    public static SharedUIManager Instance { get; private set; }

    [SerializeField] private UISceneProfile sceneUIProfile;

    private List<PlayerInput> playerInputs;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    protected override void LoadUIPanels()
    {
        foreach (var prefab in sceneUIProfile.sharedUIPrefabs)
        {
            var go = Instantiate(prefab, canvas.transform);

            UIElement uiElement = go.GetComponent<UIElement>();
            uiElements[uiElement.GetType()] = go;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LinkToPlayers();
        InitializeUIElements();
    }

    private void InitializeUIElements()
    {
        foreach (GameObject uiGO in uiElements.Values)
        {
            UIElement uiElement = uiGO.GetComponent<UIElement>();

            // Add callback
            uiElement.AddOnEnableAction((UIElement panel) => HandleFirstSelected());
        }
    }

    private void LinkToPlayers()
    {
        this.playerInputs = new List<PlayerInput>(FindObjectsOfType<PlayerInput>());
        List<PlayerController> playerControllers = new List<PlayerController>(FindObjectsOfType<PlayerController>());
        foreach (PlayerController playerController in playerControllers)
        {
            PlayerInput playerInput = playerController.GetComponent<PlayerInput>();
            playerController.onPause.AddListener(() => {
                Toggle<PauseUI>();
            });
        }
    }

    private void HandleFirstSelected()
    {
        if (this.playerInputs.Any((PlayerInput input) => IsController(input))) EnableFirstSelected();
        else DisableFirstSelected();
    }

    public void DisableAllUI()
    {
        foreach (GameObject go in uiElements.Values)
        {
            UIElement uiElement = go.GetComponent<UIElement>();
            uiElement?.InstantDisable();
        }
    }
}
