using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SharedUIManager : UIManager
{
    public static SharedUIManager Instance { get; private set; }

    [SerializeField] private UISceneProfile sceneUIProfile;

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
    }

    private void LinkToPlayers()
    {
        PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController playerController in playerControllers)
        {
            playerController.onPause.AddListener(() => {
                Toggle<PauseUI>();
            });
        }
    }
}
