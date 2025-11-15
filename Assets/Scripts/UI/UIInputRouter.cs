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
        PlayerInput[] players = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput player in players)
        {
            RegisterPlayer(player);
        }
    }

    /// <summary>
    /// Link this to PlayerInputManager.playerJoined to auto-track new players.
    /// </summary>
    /// <param name="playerInput"></param>
    public void RegisterPlayer(PlayerInput playerInput)
    {
        playerInput.actions["Submit"].performed += ctx =>
        {
            SetControllingPlayer(playerInput);
        };
        playerInput.actions["Click"].performed += ctx => SetControllingPlayer(playerInput);
    }

    private void SetControllingPlayer(PlayerInput newPlayer)
    {
        /*bool success = SharedUIManager.Instance.SetCurrentControllingPlayer(newPlayer);
        if (!success) return;

        if (uiInputModule != null)
        {
            uiInputModule.actionsAsset = newPlayer.actions;
            uiInputModule.move = InputActionReference.Create(newPlayer.actions["Navigate"]);
            uiInputModule.submit = InputActionReference.Create(newPlayer.actions["Submit"]);
            uiInputModule.cancel = InputActionReference.Create(newPlayer.actions["Cancel"]);
            uiInputModule.leftClick = InputActionReference.Create(newPlayer.actions["Click"]);
            uiInputModule.point = InputActionReference.Create(newPlayer.actions["Point"]);
        }

        Debug.Log($"Shared UI now controlled by {newPlayer.playerIndex} ({newPlayer.currentControlScheme})");*/

        Debug.Log("This method is depricated and should be removed.");
    }

    
}
