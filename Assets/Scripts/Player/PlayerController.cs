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
    [SerializeField] private PlayerInput playerInput;

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

        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    }

    public void SetPlayerInput(PlayerInput newPlayerInput)
    {
        playerInput.onActionTriggered -= OnActionTriggered;
        playerInput = newPlayerInput;
        playerInput.onActionTriggered += OnActionTriggered;
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
        if (!collectInput) return;

        switch (context.action.name)
        {
            case "Dash":
                if (IsPaused()) return;

                if (dashAbility != null && context.performed)
                {
                    Vector3 moveDir = playerRB.velocity.normalized;
                    dashAbility.Dash(moveDir);
                }
                break;

            case "Fire":
                if (IsPaused()) return;

                if (shootAbility != null)
                {
                    if (context.performed)
                        shootAbility.StartShooting();
                    else if (context.canceled)
                        shootAbility.StopShooting();
                }
                break;

            case "LookGamepad":
                if (IsPaused()) return;

                if (playerMovement != null)
                {
                    Vector2 lookInput = context.ReadValue<Vector2>();
                    playerMovement.SetLookInput(lookInput);
                }
                break;

            case "LookMouse":
                if (IsPaused()) return;

                if (playerMovement != null && Camera.main != null)
                {
                    Vector2 mouseInput = context.ReadValue<Vector2>();
                    Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mouseInput);
                    playerMovement.SetLookPoint(worldMousePos);
                }
                break;

            case "Reflect":
                if (IsPaused()) return;

                if (reflectAbility != null && context.performed)
                {
                    reflectAbility.EnableReflectShield();
                }
                break;

            case "Move":
                if (playerMovement != null)
                {
                    playerMovement.SetMoveDirection(Vector2.zero);
                    if (IsPaused()) return;

                    if (context.performed)
                    {
                        playerMovement.SetMoveDirection(context.ReadValue<Vector2>());
                    }

                    onMove?.Invoke();
                }
                break;

            case "ClassAbility":
                if (IsPaused()) return;

                if (onClassAbility != null && context.performed)
                {
                    onClassAbility(player);
                }
                break;
            case "Pause":
                if (context.performed) onPause.Invoke();
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

    private bool IsPaused()
    {
        return GameStateManager.Instance != null && GameStateManager.Instance.IsPaused();
    }
}
