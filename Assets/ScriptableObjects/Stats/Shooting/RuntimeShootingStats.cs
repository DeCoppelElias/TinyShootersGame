using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeShootingStats
{
    [SerializeField] private float damage = 5;
    [SerializeField] private float range = 1;
    [SerializeField] private int pierce = 1;
    [SerializeField] private float split = 1;
    [SerializeField] private float fan = 1;
    [SerializeField] private float bulletSize = 1;
    [SerializeField] private float bulletVelocity = 6;
    [SerializeField] private float attackCooldown = 0.5f;

    [SerializeField] private bool explode = false;
    [SerializeField] private float explodeBulletAmount = 0;
    [SerializeField] private float explodeDamagePercentage = 0.5f;
    [SerializeField] private float explodeBulletRange = 1;
    [SerializeField] private float explodeBulletSize = 0.5f;
    [SerializeField] private float explodeBulletVelocity = 6;

    [SerializeField] private float shootingMoveSpeed = 2;

    public RuntimeShootingStats(){ }
    public RuntimeShootingStats(PlayerStats stats)
    {
        ApplyStats(stats);
    }
    public RuntimeShootingStats(RuntimeShootingStats stats)
    {
        ApplyStats(stats);
    }
    public RuntimeShootingStats(BaseShootingStats stats)
    {
        ApplyStats(stats);
    }

    public void ApplyStats(PlayerStats baseStats)
    {
        if (baseStats == null) return;

        Damage = baseStats.damage;
        AttackCooldown = baseStats.attackCooldown;

        Range = baseStats.range;
        Pierce = baseStats.pierce;
        Split = baseStats.totalSplit;
        Fan = baseStats.totalFan;
        BulletSize = baseStats.bulletSize;
        BulletVelocity = baseStats.bulletSpeed;

        Explode = baseStats.splitOnHit;
        ExplodeBulletAmount = baseStats.splitAmount;
        ExplodeBulletRange = baseStats.splitRange;
        ExplodeBulletSize = baseStats.splitBulletSize;
        ExplodeBulletVelocity = baseStats.splitBulletSpeed;
        ExplodeDamagePercentage = baseStats.splitDamagePercentage;

        ShootingMoveSpeed = baseStats.shootingMoveSpeed;
    }

    public void ApplyStats(RuntimeShootingStats stats)
    {
        Damage = stats.Damage;
        AttackCooldown = stats.AttackCooldown;

        Range = stats.Range;
        Pierce = stats.Pierce;
        Split = stats.Split;
        Fan = stats.Fan;
        BulletSize = stats.BulletSize;
        BulletVelocity = stats.BulletVelocity;

        Explode = stats.Explode;
        ExplodeBulletAmount = stats.ExplodeBulletAmount;
        ExplodeBulletRange = stats.ExplodeBulletRange;
        ExplodeBulletSize = stats.ExplodeBulletSize;
        ExplodeBulletVelocity = stats.ExplodeBulletVelocity;
        ExplodeDamagePercentage = stats.ExplodeDamagePercentage;

        ShootingMoveSpeed = stats.ShootingMoveSpeed;
    }

    public void ApplyStats(BaseShootingStats stats)
    {
        Damage = stats.Damage;
        AttackCooldown = stats.AttackCooldown;

        Range = stats.Range;
        Pierce = stats.Pierce;
        Split = stats.Split;
        Fan = stats.Fan;
        BulletSize = stats.BulletSize;
        BulletVelocity = stats.BulletVelocity;

        Explode = stats.Explode;
        ExplodeBulletAmount = stats.ExplodeBulletAmount;
        ExplodeBulletRange = stats.ExplodeBulletRange;
        ExplodeBulletSize = stats.ExplodeBulletSize;
        ExplodeBulletVelocity = stats.ExplodeBulletVelocity;
        ExplodeDamagePercentage = stats.ExplodeDamagePercentage;

        ShootingMoveSpeed = stats.ShootingMoveSpeed;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        if (!playerClass.hasShootAbility) return;

        this.Damage += playerClass.damageDelta;
        this.AttackCooldown += playerClass.attackCooldownDelta;

        this.Range += playerClass.rangeDelta;
        this.Pierce += playerClass.pierceDelta;
        this.Split += playerClass.totalSplitDelta;
        this.Fan += playerClass.totalFanDelta;
        this.BulletSize += playerClass.bulletSizeDelta;
        this.BulletVelocity += playerClass.bulletSpeedDelta;

        this.Explode = this.Explode || playerClass.splitOnHit;
        this.ExplodeBulletAmount += playerClass.splitAmountDelta;
        this.ExplodeBulletRange += playerClass.splitRangeDelta;
        this.ExplodeBulletSize += playerClass.splitBulletSizeDelta;
        this.ExplodeBulletVelocity += playerClass.splitBulletSpeedDelta;
        this.ExplodeDamagePercentage += playerClass.splitDamagePercentageDelta;

        this.ShootingMoveSpeed += playerClass.shootingMoveSpeedDelta;
    }

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        this.Damage += powerup.damageDelta;
        this.AttackCooldown += powerup.attackCooldownDelta;

        this.Range += powerup.rangeDelta;
        this.Pierce += powerup.pierceDelta;
        this.Split += powerup.totalSplitDelta;
        this.Fan += powerup.totalFanDelta;
        this.BulletSize += powerup.bulletSizeDelta;
        this.BulletVelocity += powerup.bulletSpeedDelta;

        this.Explode = this.Explode || powerup.splitOnHit;
        this.ExplodeBulletAmount += powerup.splitAmountDelta;
        this.ExplodeBulletRange += powerup.splitRangeDelta;
        this.ExplodeBulletSize += powerup.splitBulletSizeDelta;
        this.ExplodeBulletVelocity += powerup.splitBulletSpeedDelta;
        this.ExplodeDamagePercentage += powerup.splitDamagePercentageDelta;

        ShootingMoveSpeed += powerup.shootingMoveSpeedDelta;
    }

    public RuntimeShootingStats GetStats()
    {
        return new RuntimeShootingStats(this);
    }

    #region Properties
    public float Damage
    {
        get { return damage; }
        set { damage = Mathf.Max(value, 1); }
    }
    public float Range
    {
        get { return range; }
        set { range = Mathf.Max(value, 1); }
    }
    public int Pierce
    {
        get { return pierce; }
        set { pierce = Mathf.Max(value, 1); }
    }
    public float Split
    {
        get { return split; }
        set { split = Mathf.Max(value, 1); }
    }
    public float Fan
    {
        get { return fan; }
        set { fan = Mathf.Max(value, 1); }
    }
    public float BulletSize
    {
        get { return bulletSize; }
        set { bulletSize = Mathf.Max(value, 0.1f); }
    }
    public float BulletVelocity
    {
        get { return bulletVelocity; }
        set { bulletVelocity = Mathf.Max(value, 0.1f); }
    }
    public float AttackCooldown
    {
        get { return attackCooldown; }
        set { attackCooldown = Mathf.Max(value, 0.1f); }
    }

    public bool Explode
    {
        get => explode;
        set => explode = value;
    }
    public float ExplodeBulletAmount
    {
        get { return explodeBulletAmount; }
        set { explodeBulletAmount = Mathf.Max(value, 0.1f); }
    }
    public float ExplodeDamagePercentage
    {
        get { return explodeDamagePercentage; }
        set { explodeDamagePercentage = Mathf.Max(value, 0.1f); }
    }
    public float ExplodeBulletRange
    {
        get { return explodeBulletRange; }
        set { explodeBulletRange = Mathf.Max(value, 0.1f); }
    }
    public float ExplodeBulletSize
    {
        get { return explodeBulletSize; }
        set { explodeBulletSize = Mathf.Max(value, 0.1f); }
    }
    public float ExplodeBulletVelocity
    {
        get { return explodeBulletVelocity; }
        set { explodeBulletVelocity = Mathf.Max(value, 0.1f); }
    }

    public float ShootingMoveSpeed
    {
        get { return shootingMoveSpeed; }
        set { shootingMoveSpeed = Mathf.Max(value, 0.1f); }
    }
    #endregion
}
