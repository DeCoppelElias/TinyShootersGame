using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Entity))]
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

    [SerializeField] private GameObject dashEffectPrefab;
    private GameObject dashEffect;

    private float dashStart = 0;
    private float chargeStart = 0;
    private float lastDash = 0;

    private Vector2 dashDirection;

    private Rigidbody2D rb;

    private Entity owner;

    public UnityEvent onPerformed;
    public UnityEvent onReady;

    private System.Action onComplete;

    private AudioManager audioManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        owner = GetComponent<Entity>();
        this.audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        InstantiateDashEffect();
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

                if (owner != null)
                {
                    // Increase contact damage
                    owner.ContactDamage *= contactDamageIncrease;
                }

                // Enable the dashing effect
                if (this.dashEffect != null) this.dashEffect.SetActive(true);

                // Play sound effect
                if (audioManager != null) audioManager.PlayDashSound();
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

                // Give some leeway
                StartCoroutine(PerformAfterDelay(0.25f, () =>
                {
                    if (owner != null)
                    {
                        // Decrease contact damage again
                        owner.ContactDamage /= contactDamageIncrease;
                    }
                }));
                

                // Disable dashing effect
                this.dashEffect.SetActive(false);
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

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        dashCooldown += powerup.dashCooldownDelta;
        dashDuration += powerup.dashDurationDelta;
        chargeDuration += powerup.chargeDurationDelta;
        dashSpeed += powerup.dashSpeedDelta;
    }

    public bool Dashing()
    {
        return this.dashingState == DashingState.Dashing;
    }

    public bool Charging()
    {
        return this.dashingState == DashingState.Charging;
    }

    private void InstantiateDashEffect()
    {
        if (this.dashEffectPrefab == null) return;
        if (this.dashEffect != null) return;

        this.dashEffect = Instantiate(this.dashEffectPrefab, this.transform);
        this.dashEffect.SetActive(false);
    }

    public void SetDashColor(Color color)
    {
        if (this.dashEffect == null) InstantiateDashEffect();
        this.dashEffect.GetComponentInChildren<TrailRenderer>().startColor = color;
        this.dashEffect.GetComponentInChildren<TrailRenderer>().endColor = color;
    }

    private IEnumerator PerformAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }
}
