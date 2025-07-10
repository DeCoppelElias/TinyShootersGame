using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 4;
    [SerializeField] private float currentVelocity;
    [SerializeField] private float acceleration = 5;
    private enum MovementState { Normal, Knockback, Reduced, }
    [SerializeField] private MovementState movementState = MovementState.Normal;

    private Vector2 currentMoveDirection = Vector2.zero;

    private Vector2 currentLookInput = Vector2.zero;

    private Player player;
    private Rigidbody2D playerRB;
    private PlayerInput playerInput;

    private DashAbility dashAbility;
    private ShootingAbility shootAbility;

    private SpriteRenderer spriteRenderer;

    private Vector3 knockbackDirection;
    private float knockbackSpeed = 10;
    private float knockbackStart = 0;
    private float knockbackDuration = 1;

    [SerializeField] private Camera customCamera;

    private enum TrailState { Idle, Trailing}
    private TrailState trailState = TrailState.Idle;
    private Vector3 previousTrailPosition;
    private Vector3 trailStartPosition;
    private ParticleManager particleManager;

    private void Start()
    {
        this.player = this.GetComponent<Player>();
        this.playerRB = this.GetComponent<Rigidbody2D>();
        this.playerInput = this.GetComponent<PlayerInput>();

        this.dashAbility = this.GetComponent<DashAbility>();
        this.shootAbility = this.GetComponent<ShootingAbility>();

        this.currentVelocity = moveSpeed;

        this.spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        this.previousTrailPosition = this.transform.position;
        this.particleManager = GameObject.Find("Particles").GetComponent<ParticleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Look();
        // DisplayTrail();
    }

    private void DisplayTrail()
    {
        if (trailState == TrailState.Idle && Vector3.Distance(this.transform.position, this.previousTrailPosition) > 2)
        {
            trailState = TrailState.Trailing;
            trailStartPosition = this.transform.position;
        }
        else if (trailState == TrailState.Trailing && Vector3.Distance(this.transform.position, trailStartPosition) > 0.5f)
        {
            Vector3 trailDirection = (this.transform.position - trailStartPosition).normalized;
            Quaternion trailRotation = Quaternion.FromToRotation(Vector3.up, trailDirection);
            this.particleManager.CreateParticle(ParticleManager.ParticleType.Trail, this.transform.position, trailRotation, this.transform.localScale, player.Color);

            trailState = TrailState.Idle;
            previousTrailPosition = this.transform.position;
        }
    }

    private void Move()
    {
        // Don't move if dashing
        if (dashAbility != null && (dashAbility.Dashing() || dashAbility.Charging())) return;

        // If knockback, move in knockback direction
        if (this.movementState == MovementState.Knockback)
        {
            Vector2 knockbackVelocity = knockbackDirection * knockbackSpeed;
            Vector2 inputVelocity = currentMoveDirection * moveSpeed * 0.5f;

            playerRB.velocity = knockbackVelocity + inputVelocity;

            // If knockback is over, return to normal movement
            if (Time.time - knockbackStart > knockbackDuration || playerRB.velocity == Vector2.zero)
            {
                this.movementState = MovementState.Normal;
            }
        }
        else if (this.movementState == MovementState.Normal)
        {
            currentVelocity = Mathf.Min(currentVelocity + (acceleration * Time.deltaTime), moveSpeed);
            playerRB.velocity = currentMoveDirection * currentVelocity;

            // Slow down if shooting
            if (shootAbility != null && shootAbility.shooting)
            {
                this.movementState = MovementState.Reduced;
                currentVelocity = shootAbility.GetShootingMoveSpeed();
            }
        }
        else if (this.movementState == MovementState.Reduced)
        {
            playerRB.velocity = currentMoveDirection * currentVelocity;

            // If shooting is done, accelerate to normal movespeed
            if (shootAbility != null && !shootAbility.shooting)
            {
                this.movementState = MovementState.Normal;
            }
        }
    }

    public void Look()
    {
        // Handle input depending on controller or mouse input
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad" && currentLookInput != Vector2.zero)
        {
            spriteRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, currentLookInput);
        }
        else if (Mouse.current != null && Mouse.current.enabled && playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = (customCamera != null) ? customCamera.ScreenToWorldPoint(mousePosition) : Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2 lookDirection = (worldMousePosition - transform.position);
            spriteRenderer.transform.localRotation = Quaternion.LookRotation(Vector3.forward, lookDirection);
        }
    }

    public void SetMoveDirection(Vector3 moveDirection)
    {
        this.currentMoveDirection = moveDirection.normalized;
    }
    public void SetLookInput(Vector3 lookInput)
    {
        this.currentLookInput = lookInput;
    }

    public void ApplyKnockBack(Vector3 knockbackDirection, float knockbackSpeed, float knockbackRange)
    {
        this.knockbackDirection = knockbackDirection;
        this.knockbackSpeed = knockbackSpeed;
        this.knockbackDuration = knockbackRange / knockbackSpeed;
        this.knockbackStart = Time.time;
        playerRB.velocity = knockbackDirection * knockbackSpeed;
        this.movementState = MovementState.Knockback;
    }

    public void ApplyStats(PlayerStats playerStats)
    {
        if (playerStats == null) return;

        moveSpeed = playerStats.normalMoveSpeed;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;

        moveSpeed += playerClass.normalMoveSpeedDelta;
    }

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        moveSpeed += powerup.normalMoveSpeedDelta;
    }
}
