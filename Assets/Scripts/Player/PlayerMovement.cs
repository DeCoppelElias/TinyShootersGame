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
    [SerializeField] private float targetVelocity;
    [SerializeField] private float acceleration = 5;
    private enum MovementState { Normal, Knockback }
    [SerializeField] private MovementState movementState = MovementState.Normal;

    private Vector2 currentMoveDirection = Vector2.zero;

    private enum LookBehaviour { LookInput, LookPoint}
    private LookBehaviour lookBehaviour = LookBehaviour.LookInput;

    private Vector2 currentLookInput = Vector2.zero;
    private Vector2 currentLookPoint = Vector2.zero;

    private Player player;
    private Rigidbody2D playerRB;

    private DashAbility dashAbility;
    private ShootingAbility shootAbility;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private Camera customCamera;

    private enum TrailState { Idle, Trailing}
    private TrailState trailState = TrailState.Idle;
    private Vector3 previousTrailPosition;
    private Vector3 trailStartPosition;
    private Rigidbody2D rb;

    private void Start()
    {
        this.player = this.GetComponent<Player>();
        this.playerRB = this.GetComponent<Rigidbody2D>();

        this.dashAbility = this.GetComponent<DashAbility>();
        this.shootAbility = this.GetComponent<ShootingAbility>();
        if (shootAbility)
        {
            shootAbility.OnShootStart += () => {
                targetVelocity = shootAbility.GetShootingMoveSpeed();
                currentVelocity = shootAbility.GetShootingMoveSpeed();
            };

            shootAbility.OnShootEnd += () => {
                targetVelocity = moveSpeed;
            };
        }

        this.currentVelocity = moveSpeed;
        this.targetVelocity = moveSpeed;

        this.spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        this.previousTrailPosition = this.transform.position;
        this.rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
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
            ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.Trail, this.transform.position, trailRotation, this.transform.localScale, player.Color);

            trailState = TrailState.Idle;
            previousTrailPosition = this.transform.position;
        }
    }

    private void Move()
    {
        if (dashAbility != null && (dashAbility.Dashing() || dashAbility.Charging())) return;

        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (movementState == MovementState.Knockback)
        {
            if (playerRB.velocity.magnitude > moveSpeed)
            {
                Vector2 inputForce = currentMoveDirection * rb.mass;
                playerRB.AddForce(inputForce, ForceMode2D.Force);
            }
            else
            {
                movementState = MovementState.Normal;
            }
        }
        else
        {
            playerRB.velocity = currentMoveDirection * currentVelocity;
        }
    }

    public void Look()
    {
        Vector2 lookInput = currentLookInput;
        if (lookBehaviour == LookBehaviour.LookPoint)
        {
            lookInput = currentLookPoint - new Vector2(transform.position.x, transform.position.y);
        }
        if (lookInput != Vector2.zero) spriteRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookInput);
    }

    public void SetMoveDirection(Vector3 moveDirection)
    {
        this.currentMoveDirection = moveDirection.normalized;
    }
    public void SetLookInput(Vector3 lookInput)
    {
        this.currentLookInput = lookInput;
        this.lookBehaviour = LookBehaviour.LookInput;
    }
    public void SetLookPoint(Vector3 lookPoint)
    {
        this.currentLookPoint = lookPoint;
        this.lookBehaviour = LookBehaviour.LookPoint;
    }
    public void ApplyKnockBack(Vector2 force)
    {
        if (rb != null && !dashAbility.Dashing())
        {
            rb.AddForce(force, ForceMode2D.Impulse);
            this.movementState = MovementState.Knockback;
        }
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
