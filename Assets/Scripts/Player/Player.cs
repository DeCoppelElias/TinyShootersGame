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

    public override void StartEntity()
    {
        shootingAbility = GetComponent<ShootingAbility>();
        dashAbility = GetComponent<DashAbility>();
        playerController = GetComponent<PlayerController>();

        ApplyStats(baseStats);
        ApplyClass(playerClass);
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("lol1");
            alternativeSpriteIndex++;
            SpriteRenderer renderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Debug.Log("lol2");
                if (alternativeSpriteIndex == alternativeSprites.Count + 1)
                {

                    Debug.Log("lol3");
                    alternativeSpriteIndex = 0;

                    Animator animator = transform.Find("Sprite").GetComponent<Animator>();
                    animator.enabled = true;
                }
                else
                {
                    Debug.Log("lol4");
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

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        this.playerClass = playerClass;

        Animator animator = transform.Find("Sprite").GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = playerClass.animatorController;
        }

        if (!isPVP) this.maxHealth += playerClass.healthDelta;
        else this.maxHealth += playerClass.pvpHealthDelta;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.maxHealth - 100) / 500f), 1, 1);

        this.health = this.maxHealth;

        if (!isPVP) this.invulnerableDuration += playerClass.invulnerableDurationDelta;

        this.contactDamage += playerClass.contactDamageDelta;
        this.contactHitCooldown += playerClass.contactHitCooldownDelta;

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

        if (!isPVP) this.maxHealth = playerStats.maxHealth;
        else this.maxHealth = playerStats.pvpMaxHealth;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.maxHealth - 100) / 500f), 1, 1);

        this.health = this.maxHealth;

        if (!isPVP) this.invulnerableDuration = playerStats.invulnerableDuration;

        this.contactDamage = playerStats.contactDamage;
        this.contactHitCooldown = playerStats.contactHitCooldown;

        AbilityBehaviour abilityBehaviour = GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null && playerStats.classAbility != null && playerController != null)
        {
            abilityBehaviour.LinkAbility(playerStats.classAbility);
            playerController.classAbility = abilityBehaviour.UseAbility;
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.ApplyStats(playerStats);
        if (shootingAbility != null) shootingAbility.ApplyStats(playerStats);
        if (dashAbility != null) dashAbility.ApplyStats(playerStats);
    }

    public override void TakeDamage(float amount, string sourceTag, DamageType damageType)
    {
        if (amount <= 0) return;
        if (Time.time - invulnerableStart < invulnerableDuration) return;
        if (dashAbility != null && dashAbility.Dashing()) return;

        this.health -= amount;

        StartColorChange();

        invulnerableStart = Time.time;

        if (onHitEvent != null) onHitEvent.Invoke();
    }

    public override void OnDeath()
    {
        base.OnDeath();

        audioManager.PlayDieSound();
        onDeath.Invoke();
    }
}
