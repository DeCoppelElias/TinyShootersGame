using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Ability Settings")]
    [SerializeField] private DashingState dashingState = DashingState.Ready;
    private enum DashingState { Ready, Charging, Dashing, Cooldown };

    [SerializeField] private int dashCooldown = 2;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float chargeDuration = 0f;
    [SerializeField] private float dashSpeed = 20;
    [SerializeField] private float contactDamageIncrease = 5;

    private float dashStart = 0;
    private float chargeStart = 0;
    private float lastDash = 0;

    private Vector2 dashDirection;

    private Rigidbody2D rb;

    private Entity entity;

    public UnityEvent onPerformed;
    public UnityEvent onReady;

    private System.Action onComplete;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        entity = GetComponent<Entity>();
    }
    public void Dash(Vector2 direction, System.Action action = null)
    {
        if (dashingState != DashingState.Ready) return;
        if (direction == Vector2.zero) return;

        this.onComplete = action;
        dashingState = DashingState.Charging;
        this.dashDirection = direction;
        chargeStart = Time.time;
    }

    public bool Ready()
    {
        return dashingState == DashingState.Ready;
    }

    private void Update()
    {
        if (dashingState == DashingState.Cooldown)
        {
            if (Time.time - lastDash > dashCooldown)
            {
                dashingState = DashingState.Ready;

                if (onReady != null)
                {
                    onReady.Invoke();
                }
            }
        }
        else if (dashingState == DashingState.Charging)
        {
            if (Time.time - chargeStart > chargeDuration)
            {
                dashingState = DashingState.Dashing;
                rb.velocity = dashDirection * dashSpeed;
                dashStart = Time.time;

                // Increase contact damage
                if (entity != null)
                {
                    entity.contactDamage *= contactDamageIncrease;
                }
            }
            else
            {
                rb.velocity = 0.1f * dashSpeed * -dashDirection;
            }
        }
        else if (dashingState == DashingState.Dashing)
        {
            if (Time.time - dashStart > dashDuration || rb.velocity == Vector2.zero)
            {
                dashingState = DashingState.Cooldown;
                lastDash = Time.time;

                if (onPerformed != null)
                {
                    onPerformed.Invoke();
                }
                onComplete?.Invoke();

                // Decrease contact damage again
                if (entity != null)
                {
                    entity.contactDamage /= contactDamageIncrease;
                }
            }
            else
            {
                rb.velocity = dashDirection * dashSpeed;
            }
        }
    }

    public float GetDashingDistance()
    {
        return dashSpeed * dashDuration;
    }

    public int GetDashCooldown()
    {
        return this.dashCooldown;
    }

    public void ApplyStats(PlayerStats playerStats) 
    {
        if (playerStats == null) return;
        if (!playerStats.hasDashAbility) return;

        dashCooldown = playerStats.dashCooldown;
        dashDuration = playerStats.dashDuration;
        chargeDuration = playerStats.chargeDuration;
        dashSpeed = playerStats.dashSpeed;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        if (!playerClass.hasDashAbility) return;

        dashCooldown += playerClass.dashCooldownDelta;
        dashDuration += playerClass.dashDurationDelta;
        chargeDuration += playerClass.chargeDurationDelta;
        dashSpeed += playerClass.dashSpeedDelta;
    }

    public bool Dashing()
    {
        return this.dashingState == DashingState.Dashing;
    }

    public bool Charging()
    {
        return this.dashingState == DashingState.Charging;
    }
}
