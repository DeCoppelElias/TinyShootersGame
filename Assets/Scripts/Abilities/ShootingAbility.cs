using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShootingAbility : MonoBehaviour
{
    public GameObject bullets;

    public float damage;
    public float attackCooldown = 0.5f;
    private float lastAttack;
    private float lastAttackRealTime;

    public float range = 1;
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

    public float shootingMoveSpeed = 2;

    public bool shooting = false;

    public bool workWithRealTime = false;

    public GameObject bulletPrefab;
    public Transform firePoint;

    public Entity owner;

    public UnityEvent onShoot;

    private void Start()
    {
        bullets = GameObject.Find("Bullets");
        if (owner == null) owner = GetComponent<Entity>();
    }

    private void Update()
    {
        if (shooting)
        {
            TryShootOnce();
        }
    }

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
            CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, 0);
            fan--;
            while (fan > 0)
            {
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, 45);
                fan--;
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, -45);
                fan--;
            }
        }
        else
        {
            while (fan > 0)
            {
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, 22.5f);
                fan--;
                CreateBulletGroup(split, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, -22.5f);
                fan--;
            }
        }

        PlayShootAnimation();

        if (onShoot != null) onShoot.Invoke();
    }
    public void CreateBulletGroup(float split, float airTime, float bulletSpeed, float bulletSize, float pierce, float damage, float rotation)
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
    public void CreateBullet(float airTime, float bulletSpeed, float bulletSize, float pierce, float damage, Vector3 position, float rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, firePoint.rotation,bullets.transform);
        bullet.transform.localScale = new Vector3(bulletSize, bulletSize, 1);

        bullet.GetComponent<Bullet>().pierce = pierce;
        bullet.GetComponent<Bullet>().damage = damage;
        bullet.GetComponent<Bullet>().ownerTag = owner.tag;
        bullet.GetComponent<Bullet>().airTime = airTime;

        bullet.GetComponent<Bullet>().splitOnHit = splitOnHit;
        bullet.GetComponent<Bullet>().splitAmount = splitAmount;
        bullet.GetComponent<Bullet>().splitRange = splitRange;
        bullet.GetComponent<Bullet>().splitBulletSize = splitBulletSize;
        bullet.GetComponent<Bullet>().splitBulletSpeed = splitBulletSpeed;
        bullet.GetComponent<Bullet>().splitDamagePercentage = splitDamagePercentage;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        Vector3 vector = Quaternion.AngleAxis(rotation, new Vector3(0, 0, 1)) * firePoint.up;

        rb.AddForce(vector * bulletSpeed, ForceMode2D.Impulse);
    }

    public void ShootBullet(float range, float bulletSpeed, float bulletSize, float pierce, float damage)
    {
        CreateBullet(range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, firePoint.position, 0);
    }

    public void ApplyStats(PlayerStats playerStats)
    {
        if (playerStats == null) return;
        if (!playerStats.hasShootAbility) return;

        damage = playerStats.damage;
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

        damage += playerClass.damageDelta;
        attackCooldown += playerClass.attackCooldownDelta;

        range += playerClass.rangeDelta;
        pierce += playerClass.pierceDelta;
        totalSplit += playerClass.totalSplitDelta;
        totalFan += playerClass.totalFanDelta;
        bulletSize += playerClass.bulletSizeDelta;
        bulletSpeed += playerClass.bulletSpeedDelta;

        splitOnHit = playerClass.splitOnHit;
        splitAmount += playerClass.splitAmountDelta;
        splitRange += playerClass.splitRangeDelta;
        splitBulletSize += playerClass.splitBulletSizeDelta;
        splitBulletSpeed += playerClass.splitBulletSpeedDelta;
        splitDamagePercentage += playerClass.splitDamagePercentageDelta;

        shootingMoveSpeed += playerClass.shootingMoveSpeedDelta;
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
}
