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

    [SerializeField] private float dashCooldown = 2;
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

    public UnityEvent onStartDash;
    public UnityEvent onPerformed;
    public UnityEvent onReady;
    public UnityEvent onLateComplete;

    private System.Action onComplete;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        owner = GetComponent<Entity>();

        InstantiateDashEffect();
    }

    public void Dash(Vector2 direction, System.Action action = null)
    {
        if (dashingState != DashingState.Ready) return;
        if (direction == Vector2.zero) return;

        this.onComplete = action;
        dashingState = DashingState.Charging;
        this.dashDirection = direction;

        rb.velocity = Vector2.zero;

        chargeStart = Time.time;
        if (chargeDuration > 0)
        {
            rb.AddForce(0.1f * DashSpeed * rb.mass * -dashDirection.normalized, ForceMode2D.Impulse);
        }
    }

    public bool Ready()
    {
        return dashingState == DashingState.Ready;
    }

    public void SetReady()
    {
        dashingState = DashingState.Ready;

        if (onReady != null)
        {
            onReady.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (dashingState == DashingState.Cooldown)
        {
            if (Time.time - lastDash > DashCooldown)
            {
                SetReady();
            }
        }
        else if (dashingState == DashingState.Charging)
        {
            if (Time.time - chargeStart > ChargeDuration)
            {
                dashingState = DashingState.Dashing;

                // Perform dash
                rb.velocity = Vector2.zero;
                rb.AddForce(DashSpeed * rb.mass * dashDirection.normalized, ForceMode2D.Impulse);
                dashStart = Time.time;

                if (owner != null)
                {
                    // Increase contact damage
                    owner.ContactDamage *= ContactDamageIncrease;
                }

                // Enable the dashing effect
                if (this.dashEffect != null) this.dashEffect.SetActive(true);

                // Play sound effect
                if (AudioManager.Instance != null) AudioManager.Instance.PlayDashSound();

                // Make knockback immune
                this.owner.knockbackImmune = true;

                // Callback
                this.onStartDash.Invoke();
            }
        }
        else if (dashingState == DashingState.Dashing)
        {
            // End dash
            if (Time.time - dashStart > DashDuration || rb.velocity == Vector2.zero)
            {
                dashingState = DashingState.Cooldown;
                lastDash = Time.time;

                rb.velocity = Vector2.zero;

                onPerformed?.Invoke();
                onComplete?.Invoke();

                // Give some leeway
                StartCoroutine(PerformAfterDelay(0.25f, () =>
                {
                    if (owner != null)
                    {
                        // Decrease contact damage again
                        owner.ContactDamage /= ContactDamageIncrease;

                        this.onLateComplete.Invoke();
                    }
                }));

                // Disable dashing effect
                this.dashEffect.SetActive(false);

                // Enable knockback again
                this.owner.knockbackImmune = false;
            }
        }
    }

    public float GetDashingDistance()
    {
        float force = DashSpeed * rb.mass;
        if (rb.drag <= 0f)
            return (force / rb.mass) * DashDuration;

        float v0 = force / rb.mass;
        return v0 / rb.drag * (1f - Mathf.Exp(-rb.drag * DashDuration));
    }

    public float GetDashCooldown()
    {
        return this.DashCooldown;
    }

    public void ApplyStats(PlayerStats playerStats) 
    {
        if (playerStats == null) return;
        if (!playerStats.hasDashAbility) return;

        DashCooldown = playerStats.dashCooldown;
        DashDuration = playerStats.dashDuration;
        ChargeDuration = playerStats.chargeDuration;
        DashSpeed = playerStats.dashSpeed;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        if (!playerClass.hasDashAbility) return;

        DashCooldown += playerClass.dashCooldownDelta;
        DashDuration += playerClass.dashDurationDelta;
        ChargeDuration += playerClass.chargeDurationDelta;
        DashSpeed += playerClass.dashSpeedDelta;
    }

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        DashCooldown += powerup.dashCooldownDelta;
        DashDuration += powerup.dashDurationDelta;
        ChargeDuration += powerup.chargeDurationDelta;
        DashSpeed += powerup.dashSpeedDelta;
        ContactDamageIncrease += powerup.contactDamageIncreaseDelta;
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

    #region Properties
    public float DashCooldown
    {
        get { return dashCooldown; }
        set { dashCooldown = Mathf.Max(value, 0.5f); }
    }
    public float DashDuration
    {
        get { return dashDuration; }
        set { dashDuration = Mathf.Clamp(value, 0, 0.5f); }
    }
    public float ChargeDuration
    {
        get { return chargeDuration; }
        set { chargeDuration = Mathf.Clamp(value, 0, 5); }
    }
    public float DashSpeed
    {
        get { return dashSpeed; }
        set { dashSpeed = Mathf.Clamp(value, 5, 50); }
    }
    public float ContactDamageIncrease
    {
        get { return contactDamageIncrease; }
        set { contactDamageIncrease = Mathf.Max(value, 1); }
    }
    #endregion
}
