using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReady : MonoBehaviour
{
    public bool IsReady { get; private set; }
    public static event System.Action<PlayerInput> OnPlayerReady;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponentInChildren<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Ready" && context.performed)
        {
            OnReady();
        }
    }

    private void OnReady()
    {
        if (IsReady) return;

        IsReady = true;
        OnPlayerReady?.Invoke(playerInput);
        Debug.Log($"{playerInput.gameObject.name} is ready!");
    }
}
