using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerUIManager : UIManager
{
    [SerializeField] private UISceneProfile sceneUIProfile;

    // Start is called before the first frame update
    void Start()
    {
        canvas.worldCamera = Camera.main;
        InitializeUIElements();
    }

    protected override void LoadUIPanels()
    {
        foreach (var prefab in sceneUIProfile.playerUIPrefabs)
        {
            var go = Instantiate(prefab, canvas.transform.GetChild(0).transform);

            PlayerUIElement uiElement = go.GetComponent<PlayerUIElement>();
            uiElements[uiElement.GetType()] = go;
        }
    }

    private void InitializeUIElements()
    {
        foreach (GameObject uiGO in uiElements.Values)
        {
            PlayerUIElement uiElement = uiGO.GetComponent<PlayerUIElement>();
            Player player = currentControllingPlayer.GetComponent<Player>();
            uiElement.Initialize(player);
        }
    }
}
