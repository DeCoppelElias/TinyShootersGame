using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(InputSystemUIInputModule))]
public class SharedEventSystem : MonoBehaviour
{
    private InputSystemUIInputModule sharedUIInputModule;

    private PlayerInput currentControllingPlayer;

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += RegisterPlayerInput;
        Debug.Log($"Start listen for navigation or point input");
        sharedUIInputModule = GetComponent<InputSystemUIInputModule>();
    }

    void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= RegisterPlayerInput;
    }

    void RegisterPlayerInput(PlayerInput playerInput)
    {
        Debug.Log($"Start listen for navigation or point input");

        // Listen for navigation input
        InputAction navigate = playerInput.actions["Navigate"];
        navigate.performed += ctx => OnPlayerNavigated(playerInput);

        InputAction point = playerInput.actions["Point"];
        point.performed += ctx => OnPlayerNavigated(playerInput);
    }

    void OnPlayerNavigated(PlayerInput playerInput)
    {
        if (currentControllingPlayer == playerInput)
            return;

        HIER WAS IK BEZIG, IK PROBEERDE TE ZORGEN DAT DE SHARED UI ZOU WISSELEN TUSSEN DE MEERDERE SPELERS AFHANKELIJK VAN WIE DAT ALS LAATSTE EEN INPUT GEGEVEN HEEFT.
        Debug.Log($"Shared UI now controlled by: {playerInput.name}");
        currentControllingPlayer = playerInput;
        sharedUIInputModule.actionsAsset = playerInput.actions;
    }
}
