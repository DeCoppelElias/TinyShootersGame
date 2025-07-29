using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player : Entity
{
    [Header("Player Settings")]
    public PlayerClass playerClass;
    public PlayerStats baseStats = null;

    [SerializeField] private bool invulnerable = false;
    [SerializeField] private float invulnerableDuration = 0.5f;
    private float invulnerableStart;

    private ShootingAbility shootingAbility;
    private DashAbility dashAbility;

    public UnityEvent onHitEvent;
    public UnityEvent onDeath;

    public bool isPVP = false;

    private PlayerController playerController;

    [SerializeField] private List<Sprite> alternativeSprites = new List<Sprite>();
    private int alternativeSpriteIndex = 0;

    private PlayerMovement playerMovement;

    public Color Color { get; set; }

    public override void StartEntity()
    {
        base.StartEntity();

        shootingAbility = GetComponent<ShootingAbility>();
        dashAbility = GetComponent<DashAbility>();
        dashAbility.onStartDash.AddListener(() => invulnerable = true);
        dashAbility.onLateComplete.AddListener(() => invulnerable = false);

        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();

        SetupColor(new Color(59 / 255f, 93 / 255f, 201 / 255f));
        ApplyStats(baseStats);
        ApplyClass(playerClass);
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            alternativeSpriteIndex++;
            SpriteRenderer renderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                if (alternativeSpriteIndex == alternativeSprites.Count + 1)
                {
                    alternativeSpriteIndex = 0;

                    Animator animator = transform.Find("Sprite").GetComponent<Animator>();
                    animator.enabled = true;
                }
                else
                {
                    if (alternativeSpriteIndex == 1)
                    {
                        Animator animator = transform.Find("Sprite").GetComponent<Animator>();
                        animator.enabled = false;
                    }
                    renderer.sprite = alternativeSprites[alternativeSpriteIndex - 1];
                }
            }
        }
    }

    public List<PlayerClass> GetUpgrades()
    {
        List<PlayerClass> upgrades = new List<PlayerClass>();
        if (this.playerClass == null)
        {
            if (this.baseStats == null) return upgrades;

            upgrades = this.baseStats.upgrades;
        }
        else
        {
            upgrades = this.playerClass.upgrades;
        }
        return upgrades;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        this.playerClass = playerClass;

        Animator animator = transform.Find("Sprite").GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = playerClass.animatorController;
        }

        if (!isPVP) this.MaxHealth += playerClass.healthDelta;
        else this.MaxHealth += playerClass.pvpHealthDelta;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.MaxHealth - 100) / 500f), 1, 1);

        this.Health = this.MaxHealth;

        if (!isPVP) this.invulnerableDuration += playerClass.invulnerableDurationDelta;

        this.ContactDamage += playerClass.contactDamageDelta;
        this.ContactHitCooldown += playerClass.contactHitCooldownDelta;

        AbilityBehaviour abilityBehaviour = GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null && playerClass.classAbility != null && playerController != null)
        {
            abilityBehaviour.LinkAbility(playerClass.classAbility);
            playerController.classAbility = abilityBehaviour.UseAbility;
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.ApplyClass(playerClass);
        if (shootingAbility != null) shootingAbility.ApplyClass(playerClass);
        if (dashAbility != null) dashAbility.ApplyClass(playerClass);
    }

    public void ApplyStats(PlayerStats playerStats)
    {
        if (playerStats == null) return;

        Animator animator = transform.Find("Sprite").GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = playerStats.animatorController;
        }

        if (!isPVP) this.MaxHealth = playerStats.maxHealth;
        else this.MaxHealth = playerStats.pvpMaxHealth;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.MaxHealth - 100) / 500f), 1, 1);

        this.Health = this.MaxHealth;

        if (!isPVP) this.invulnerableDuration = playerStats.invulnerableDuration;

        this.ContactDamage = playerStats.contactDamage;
        this.ContactHitCooldown = playerStats.contactHitCooldown;

        AbilityBehaviour abilityBehaviour = GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null && playerStats.classAbility != null && playerController != null)
        {
            abilityBehaviour.LinkAbility(playerStats.classAbility);
            playerController.classAbility = abilityBehaviour.UseAbility;
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.ApplyStats(playerStats);
        if (shootingAbility != null) shootingAbility.ApplyPlayerStats(playerStats);
        if (dashAbility != null) dashAbility.ApplyStats(playerStats);
    }

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        this.MaxHealth += powerup.healthDelta;
        this.Health += powerup.healthDelta;

        if (powerup.recoverHealth) this.Health = this.MaxHealth;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.MaxHealth - 100) / 500f), 1, 1);

        if (!isPVP) this.invulnerableDuration += powerup.invulnerableDurationDelta;

        this.ContactDamage += powerup.contactDamageDelta;
        this.ContactHitCooldown += powerup.contactHitCooldownDelta;

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.ApplyPowerup(powerup);
        if (shootingAbility != null) shootingAbility.ApplyPowerup(powerup);
        if (dashAbility != null) dashAbility.ApplyPowerup(powerup);
    }

    public override void TakeDamage(float amount, string sourceTag, DamageType damageType, Vector2 direction)
    {
        if (amount <= 0) return;
        if (invulnerable) return;
        if (Time.time - invulnerableStart < invulnerableDuration) return;
        invulnerableStart = Time.time;

        base.TakeDamage(amount, sourceTag, damageType, direction);

        if (onHitEvent != null) onHitEvent.Invoke();
    }

    public override void OnDeath()
    {
        base.OnDeath();
        onDeath.Invoke();
    }

    public override void AddKnockback(Vector2 force)
    {
        if (knockbackImmune) return;
        if (this.playerMovement != null) this.playerMovement.ApplyKnockBack(force);
    }

    private void SetupColor(Color color)
    {
        if (shootingAbility != null) shootingAbility.SetBulletColor(color);
        if (dashAbility != null) dashAbility.SetDashColor(color);

        this.Color = color;
    }
}
