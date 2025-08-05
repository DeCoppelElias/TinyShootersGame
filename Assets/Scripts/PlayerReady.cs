using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReady : MonoBehaviour
{
    public bool IsReady { get; private set; } = false;
    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    private void OnActionTriggered(InputAction.CallbackContext ctx)
    {
        if (ctx.action.name == "Ready" && ctx.performed && !IsReady)
        {
            IsReady = true;

            playerInput.onActionTriggered -= OnActionTriggered;

            FindObjectOfType<PVPLobbyManager>().TryStartGame(playerInput);
        }
    }
}
