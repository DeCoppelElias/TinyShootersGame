using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShootingAbility : MonoBehaviour
{
    [Header("Shooting Stats")]
    [SerializeField] private BaseShootingStats baseStats;
    [SerializeField] private RuntimeShootingStats runtimeStats;
    public RuntimeShootingStats RuntimeStats { get => runtimeStats.GetStats(); }

    private float lastAttack;
    private float lastAttackRealTime;

    [Header("ShootAbility Parameters")]
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
        if (owner == null) owner = GetComponent<Entity>();

        bulletManager = GameObject.Find("Bullets").GetComponent<BulletManager>();
        
        if (runtimeStats == null)
        {
            if (this.baseStats != null) this.runtimeStats = new RuntimeShootingStats(this.baseStats);
            else this.runtimeStats = new RuntimeShootingStats();
        }
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
        if (!workWithRealTime && Time.time - lastAttack <= this.runtimeStats.AttackCooldown) return;
        else if (workWithRealTime && Time.realtimeSinceStartup - lastAttackRealTime <= this.runtimeStats.AttackCooldown) return;

        lastAttack = Time.time;
        lastAttackRealTime = Time.realtimeSinceStartup;

        float fan = this.runtimeStats.Fan;
        float split = this.runtimeStats.Split;
        if (fan % 2 == 1)
        {
            CreateBulletGroup(split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 0);
            fan--;
            while (fan > 0)
            {
                CreateBulletGroup(split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 45);
                fan--;
                CreateBulletGroup(split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, -45);
                fan--;
            }
        }
        else
        {
            while (fan > 0)
            {
                CreateBulletGroup(split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 22.5f);
                fan--;
                CreateBulletGroup(split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, -22.5f);
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
        if (this.runtimeStats.Explode) bullet.InitializeSplitting(this.runtimeStats.ExplodeBulletAmount, this.runtimeStats.ExplodeBulletRange, this.runtimeStats.ExplodeBulletSize, this.runtimeStats.ExplodeBulletVelocity, this.runtimeStats.ExplodeDamagePercentage);
        bullet.Shoot();
    }

    public void ShootBullet(float range, float bulletSpeed, float bulletSize, int pierce, float damage)
    {
        CreateBullet(range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, firePoint.position, 0);
    }

    public void ApplyPlayerStats(PlayerStats playerStats)
    {
        this.runtimeStats.ApplyStats(playerStats);
    }

    public void ApplyShootingStats(RuntimeShootingStats stats)
    {
        this.runtimeStats.ApplyStats(stats);
    }

    public void ApplyClass(PlayerClass playerClass)
    {
        this.runtimeStats.ApplyClass(playerClass);
    }

    public void ApplyPowerup(Powerup powerup)
    {
        this.runtimeStats.ApplyPowerup(powerup);
    }

    private void PlayShootAnimation()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.speed = 1 / this.runtimeStats.AttackCooldown;
        animator.SetBool("Shooting", true);

        StartCoroutine(ResetShootingAnimation(animator, 0.2f * this.runtimeStats.AttackCooldown));
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

    public float GetShootingMoveSpeed()
    {
        return this.runtimeStats.ShootingMoveSpeed;
    }

    public float GetDamage()
    {
        return this.runtimeStats.Damage;
    }

    public float GetRange()
    {
        return this.runtimeStats.Range;
    }
}
