using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Player player;
    private Rigidbody2D playerRB;

    public bool collectInput = true;

    private PlayerMovement playerMovement;
    private DashAbility dashAbility;
    private ShootingAbility shootAbility;
    private ReflectShieldAbility reflectAbility;
    private GameStateManager gameStateManager;
    private PlayerInput playerInput;

    public UnityEvent onPause;
    private Action onMove;
    public Action<Player> onClassAbility;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerRB = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        dashAbility = GetComponent<DashAbility>();
        shootAbility = GetComponent<ShootingAbility>();
        reflectAbility = GetComponent<ReflectShieldAbility>();

        gameStateManager = FindObjectOfType<GameStateManager>();

        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered += OnActionTriggered;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Dash":
                if (gameStateManager != null && gameStateManager.IsPaused()) return;
                if (!collectInput) return;

                if (dashAbility != null && context.performed)
                {
                    Vector3 moveDir = playerRB.velocity.normalized;
                    dashAbility.Dash(moveDir);
                }
                break;

            case "Fire":
                if (gameStateManager != null && gameStateManager.IsPaused()) return;
                if (!collectInput) return;

                if (shootAbility != null)
                {
                    if (context.performed)
                        shootAbility.StartShooting();
                    else if (context.canceled)
                        shootAbility.StopShooting();
                }
                break;

            case "Look":
                if (gameStateManager != null && gameStateManager.IsPaused()) return;
                if (!collectInput) return;

                if (playerMovement != null)
                {
                    Vector2 lookInput = context.ReadValue<Vector2>();
                    playerMovement.SetLookInput(lookInput);
                }
                break;

            case "Reflect":
                if (gameStateManager != null && gameStateManager.IsPaused()) return;
                if (!collectInput) return;

                if (reflectAbility != null && context.performed)
                {
                    reflectAbility.EnableReflectShield();
                }
                break;

            case "Move":
                if (playerMovement != null)
                {
                    playerMovement.SetMoveDirection(Vector2.zero);
                    if (gameStateManager != null && gameStateManager.IsPaused()) return;
                    if (!collectInput) return;

                    if (context.performed)
                    {
                        playerMovement.SetMoveDirection(context.ReadValue<Vector2>());
                    }

                    onMove?.Invoke();
                }
                break;

            case "ClassAbility":
                if (onClassAbility != null && context.performed)
                {
                    onClassAbility(player);
                }
                break;
            case "Pause":
                onPause.Invoke();
                break;
        }
    }

    public void AddOnMoveCallback(Action action)
    {
        onMove += action;
    }

    public void RemoveOnMoveCallback(Action action)
    {
        onMove -= action;
    }
}
