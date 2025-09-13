using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIInputRouter : MonoBehaviour
{ 
    [SerializeField] private InputSystemUIInputModule uiInputModule;

    private void Awake()
    {
        uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (uiInputModule == null) Debug.LogError("No InputSystemUIInputModule found on EventSystem. Shared UI navigation will not work.");

        PlayerInput[] players = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput player in players)
        {
            Debug.Log("registered player");
            RegisterPlayer(player);
        }
    }

    /// <summary>
    /// Link this to PlayerInputManager.playerJoined to auto-track new players.
    /// </summary>
    /// <param name="playerInput"></param>
    public void RegisterPlayer(PlayerInput playerInput)
    {
        playerInput.onActionTriggered += (ctx) =>
        {
            if (ctx.action.name == "Submit" || ctx.action.name == "Click")
            {
                SetControllingPlayer(playerInput);
            }
        };
    }

    private void SetControllingPlayer(PlayerInput newPlayer)
    {
        SharedUIManager.Instance.SetCurrentControllingPlayer(newPlayer);

        if (uiInputModule != null)
        {
            uiInputModule.actionsAsset = newPlayer.actions;
            uiInputModule.move = InputActionReference.Create(newPlayer.actions["Navigate"]);
            uiInputModule.submit = InputActionReference.Create(newPlayer.actions["Submit"]);
            uiInputModule.cancel = InputActionReference.Create(newPlayer.actions["Cancel"]);
        }

        Debug.Log($"Shared UI now controlled by {newPlayer.playerIndex} ({newPlayer.currentControlScheme})");
    }

    
}
