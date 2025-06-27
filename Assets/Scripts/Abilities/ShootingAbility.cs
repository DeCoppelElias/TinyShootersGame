using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShootingAbility : MonoBehaviour
{
    public GameObject bullets;

    [SerializeField] private float damage;
    public float attackCooldown = 0.5f;
    private float lastAttack;
    private float lastAttackRealTime;

    public float range = 1;
    public int pierce = 1;
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

    public float shootingMoveSpeed = 2;

    public bool shooting = false;

    public bool workWithRealTime = false;

    public GameObject bulletPrefab;
    private Color bulletColor = Color.white;
    public Transform firePoint;

    public Entity owner;

    public UnityEvent onShoot;

    private BulletManager bulletManager;

    private void Start()
    {
        bullets = GameObject.Find("Bullets");
        if (owner == null) owner = GetComponent<Entity>();

        bulletManager = GameObject.Find("Bullets").GetComponent<BulletManager>();
    }

    private void Update()
    {
        if (shooting)
        {
            TryShootOnce();
        }
    }

    #region Properties
    public float Damage
    {
        get { return damage; }
        set
        {
            if (value < 1) damage = 1;
            else damage = value;
        }
    }

    //TODO
    #endregion

    public void StartShooting()
    {
        shooting = true;
    }

    public void StopShooting()
    {
        shooting = false;
    }

    public void TryShootOnce()
    {
        if (!workWithRealTime && Time.time - lastAttack <= attackCooldown) return;
        else if (workWithRealTime && Time.realtimeSinceStartup - lastAttackRealTime <= attackCooldown) return;

        lastAttack = Time.time;
        lastAttackRealTime = Time.realtimeSinceStartup;

        float fan = totalFan;
        float split = totalSplit;
        if (fan % 2 == 1)
        {
            CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, Damage, 0);
            fan--;
            while (fan > 0)
            {
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, Damage, 45);
                fan--;
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, Damage, -45);
                fan--;
            }
        }
        else
        {
            while (fan > 0)
            {
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, Damage, 22.5f);
                fan--;
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, Damage, -22.5f);
                fan--;
            }
        }

        PlayShootAnimation();

        if (onShoot != null) onShoot.Invoke();
    }
    public void CreateBulletGroup(float split, float airTime, float bulletSpeed, float bulletSize, int pierce, float damage, float rotation)
    {
        if (split % 2 != 0)
        {
            CreateBullet(airTime, bulletSpeed, bulletSize, pierce, damage, firePoint.position, rotation);
            split--;
            int counter = 1;
            while (split != 0)
            {
                CreateBullet(airTime, bulletSpeed, bulletSize, pierce, damage, firePoint.position + new Vector3(firePoint.up.y, -firePoint.up.x, 0) * 0.5f * counter * bulletSize, rotation);
                split--;
                if (split != 0)
                {
                    CreateBullet(airTime, bulletSpeed, bulletSize, pierce, damage, firePoint.position + new Vector3(-firePoint.up.y, firePoint.up.x, 0) * 0.5f * counter * bulletSize, rotation);
                    split--;
                }
                counter++;
            }
        }
        else
        {
            int counter = 0;
            while (split != 0)
            {
                CreateBullet(airTime, bulletSpeed, bulletSize, pierce, damage, firePoint.position + new Vector3(firePoint.up.y, -firePoint.up.x, 0) * 0.25f + new Vector3(firePoint.up.y, -firePoint.up.x, 0) * 0.5f * counter * bulletSize, rotation);
                split--;
                if (split != 0)
                {
                    CreateBullet(airTime, bulletSpeed, bulletSize, pierce, damage, firePoint.position + new Vector3(-firePoint.up.y, firePoint.up.x, 0) * 0.25f + new Vector3(firePoint.up.y, -firePoint.up.x, 0) * 0.5f * counter * bulletSize, rotation);
                    split--;
                }
                counter++;
            }
        }
    }
    public void CreateBullet(float airTime, float bulletSpeed, float bulletSize, int pierce, float damage, Vector3 position, float rotation)
    {
        Bullet bullet = bulletManager.TryGetBullet();
        if (bullet == null) return;
        bullet.AssignOnComplete(() => bulletManager.ReturnBullet(bullet));

        Quaternion finalRotation = Quaternion.Euler(0, 0, firePoint.eulerAngles.z + rotation);
        bullet.Initialize(owner.tag, position, finalRotation, new Vector3(bulletSize, bulletSize, 1), damage, airTime, bulletSpeed, pierce, bulletColor);
        if (splitOnHit) bullet.InitializeSplitting(splitAmount, splitRange, splitBulletSize, splitBulletSpeed, splitDamagePercentage);
        bullet.Shoot();
    }

    public void ShootBullet(float range, float bulletSpeed, float bulletSize, int pierce, float damage)
    {
        CreateBullet(range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, firePoint.position, 0);
    }

    public void ApplyStats(PlayerStats playerStats)
    {
        if (playerStats == null) return;
        if (!playerStats.hasShootAbility) return;

        Damage = playerStats.damage;
        attackCooldown = playerStats.attackCooldown;

        range = playerStats.range;
        pierce = playerStats.pierce;
        totalSplit = playerStats.totalSplit;
        totalFan = playerStats.totalFan;
        bulletSize = playerStats.bulletSize;
        bulletSpeed = playerStats.bulletSpeed;

        splitOnHit = playerStats.splitOnHit;
        splitAmount = playerStats.splitAmount;
        splitRange = playerStats.splitRange;
        splitBulletSize = playerStats.splitBulletSize;
        splitBulletSpeed = playerStats.splitBulletSpeed;
        splitDamagePercentage = playerStats.splitDamagePercentage;

        shootingMoveSpeed = playerStats.shootingMoveSpeed;
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        if (!playerClass.hasShootAbility) return;

        Damage += playerClass.damageDelta;
        attackCooldown += playerClass.attackCooldownDelta;

        range += playerClass.rangeDelta;
        pierce += playerClass.pierceDelta;
        totalSplit += playerClass.totalSplitDelta;
        totalFan += playerClass.totalFanDelta;
        bulletSize += playerClass.bulletSizeDelta;
        bulletSpeed += playerClass.bulletSpeedDelta;

        splitOnHit = splitOnHit || playerClass.splitOnHit;
        splitAmount += playerClass.splitAmountDelta;
        splitRange += playerClass.splitRangeDelta;
        splitBulletSize += playerClass.splitBulletSizeDelta;
        splitBulletSpeed += playerClass.splitBulletSpeedDelta;
        splitDamagePercentage += playerClass.splitDamagePercentageDelta;

        shootingMoveSpeed += playerClass.shootingMoveSpeedDelta;
    }

    public void ApplyPowerup(Powerup powerup)
    {
        if (powerup == null) return;

        Damage += powerup.damageDelta;
        attackCooldown += powerup.attackCooldownDelta;

        range += powerup.rangeDelta;
        pierce += powerup.pierceDelta;
        totalSplit += powerup.totalSplitDelta;
        totalFan += powerup.totalFanDelta;
        bulletSize += powerup.bulletSizeDelta;
        bulletSpeed += powerup.bulletSpeedDelta;

        splitOnHit = splitOnHit || powerup.splitOnHit;
        splitAmount += powerup.splitAmountDelta;
        splitRange += powerup.splitRangeDelta;
        splitBulletSize += powerup.splitBulletSizeDelta;
        splitBulletSpeed += powerup.splitBulletSpeedDelta;
        splitDamagePercentage += powerup.splitDamagePercentageDelta;

        shootingMoveSpeed += powerup.shootingMoveSpeedDelta;
    }

    private void PlayShootAnimation()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.speed = 1 / attackCooldown;
        animator.SetBool("Shooting", true);

        StartCoroutine(ResetShootingAnimation(animator, 0.2f * attackCooldown));
    }

    private IEnumerator ResetShootingAnimation(Animator animator, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetBool("Shooting", false);
        }
    }

    public void SetBulletColor(Color color)
    {
        this.bulletColor = color;
    }

    public Color GetBulletColor()
    {
        return this.bulletColor;
    }
}
