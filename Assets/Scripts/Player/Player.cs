using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player : Entity
{
    public Class playerClass;
    [SerializeField]
    private Class defaultPlayerClass = null;

    [SerializeField]
    private float invulnerableDuration = 0.5f;
    private float invulnerableStart;

    private ShootingAbility shootingAbility;
    private DashAbility dashAbility;

    public GameObject bulletWall;

    public UnityEvent onHitEvent;
    public UnityEvent onDeath;

    public bool isPVP = false;

    private PlayerController playerController;

    [SerializeField] private List<Sprite> alternativeSprites = new List<Sprite>();

    public override void StartEntity()
    {
        shootingAbility = GetComponent<ShootingAbility>();
        dashAbility = GetComponent<DashAbility>();
        playerController = GetComponent<PlayerController>();

        if (playerClass == null)
        {
            playerClass = defaultPlayerClass;
        }
        ApplyClass(playerClass);
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        if (playerClass == null) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SpriteRenderer renderer = this.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                if (renderer.sprite == playerClass.blueSprite)
                {
                    renderer.sprite = alternativeSprites[0];
                }
                else
                {
                    for (int i = 0; i < alternativeSprites.Count; i++)
                    {
                        if (renderer.sprite == alternativeSprites[i])
                        {
                            if (i == alternativeSprites.Count - 1)
                            {
                                renderer.sprite = playerClass.blueSprite;
                            }
                            else
                            {
                                renderer.sprite = alternativeSprites[i + 1];
                            }
                            return;
                        }
                    }
                }
            }
        }
    }

    public void ApplyClass(Class playerClass)
    {
        this.playerClass = playerClass;

        SpriteRenderer renderer = this.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = playerClass.blueSprite;
        }

        if (!isPVP) this.maxHealth = playerClass.maxHealth;
        else this.maxHealth = playerClass.pvpMaxHealth;

        Transform healthbar = this.transform.Find("EmptyHealthBar");
        healthbar.localScale = new Vector3(1 + ((this.maxHealth - 100) / 500f), 1, 1);

        this.health = this.maxHealth;

        this.damage = playerClass.damage;

        this.moveSpeed = playerClass.normalMoveSpeed;

        if (!isPVP) this.invulnerableDuration = playerClass.invulnerableDuration;

        this.contactDamage = playerClass.contactDamage;
        this.contactHitCooldown = playerClass.contactHitCooldown;

        AbilityBehaviour abilityBehaviour = GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null && playerClass.classAbility != null && playerController != null)
        {
            abilityBehaviour.LinkAbility(playerClass.classAbility);
            playerController.classAbility = abilityBehaviour.UseAbility;
        }

        if (shootingAbility != null)
        {
            shootingAbility.attackCooldown = playerClass.attackCooldown;

            shootingAbility.range = playerClass.range;
            shootingAbility.pierce = playerClass.pierce;
            shootingAbility.totalSplit = playerClass.totalSplit;
            shootingAbility.totalFan = playerClass.totalFan;
            shootingAbility.bulletSize = playerClass.bulletSize;
            shootingAbility.bulletSpeed = playerClass.bulletSpeed;

            shootingAbility.splitOnHit = playerClass.splitOnHit;
            shootingAbility.splitAmount = playerClass.splitAmount;
            shootingAbility.splitRange = playerClass.splitRange;
            shootingAbility.splitBulletSize = playerClass.splitBulletSize;
            shootingAbility.splitBulletSpeed = playerClass.splitBulletSpeed;
            shootingAbility.splitDamagePercentage = playerClass.splitDamagePercentage;

            shootingAbility.shootingMoveSpeed = playerClass.shootingMoveSpeed;

            shootingAbility.damage = damage;
        }

        DashAbility dashAbility = GetComponent<DashAbility>();
        if (dashAbility != null) dashAbility.ApplyClass(playerClass);
    }

    public override void TakeDamage(float amount, string sourceTag, DamageType damageType)
    {
        if (amount <= 0) return;
        if (Time.time - invulnerableStart < invulnerableDuration) return;
        if (dashAbility != null && dashAbility.Dashing()) return;

        this.health -= amount;

        invulnerableStart = Time.time;

        if (onHitEvent != null) onHitEvent.Invoke();
    }

    public override void OnDeath()
    {
        audioManager.PlayDieSound();
        this.health = 1;
        onDeath.Invoke();
    }
}
