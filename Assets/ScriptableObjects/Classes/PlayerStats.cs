using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player starting stats
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    // General Settings
    public string className;
    public RuntimeAnimatorController animatorController;
    public Sprite UISprite;

    public float maxHealth = 100;
    public float pvpMaxHealth = 100;
    public float health = 100;
    public float damage = 10;

    public float normalMoveSpeed = 5f;
    public float shootingMoveSpeed = 2f;

    public float invulnerableDuration = 0.5f;

    public float contactDamage = 10;
    public float contactHitCooldown = 1f;

    public ClassAbility classAbility;

    // Ranged Combat Settings
    public bool hasShootAbility = false;
    public float attackCooldown = 0.5f;
    public float range = 5;
    public float pierce = 1;
    public float totalSplit = 1;
    public float totalFan = 1;
    public float bulletSize = 1;
    public float bulletSpeed = 6;
    public bool splitOnHit = false;
    public float splitAmount = 0;
    public float splitRange = 1;
    public float splitBulletSize = 0.5f;
    public float splitBulletSpeed = 6;
    public float splitDamagePercentage = 0.5f;

    // Dash Settings
    public bool hasDashAbility = false;
    public int dashCooldown = 2;
    public float dashDuration = 0.1f;
    public float chargeDuration = 0f;
    public float dashSpeed = 20;

    // Upgrades
    public List<PlayerClass> upgrades = new List<PlayerClass>();
}
