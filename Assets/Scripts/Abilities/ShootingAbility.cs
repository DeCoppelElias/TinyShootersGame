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

    [SerializeField] private Color bulletColor = Color.white;
    public List<Transform> firepoints = new List<Transform>();
    private List<ParticleSystem> muzzleFlashes = new List<ParticleSystem>();

    public Entity owner;

    public UnityAction OnShootStart;
    public UnityAction OnShootEnd;

    public UnityAction onShoot;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (owner == null) owner = GetComponent<Entity>();

        if (this.baseStats != null) this.runtimeStats = new RuntimeShootingStats(this.baseStats);

        InitializeFirepoints();
        InitializeMuzzleFlashes();
    }

    private void InitializeFirepoints()
    {
        Transform spriteTransform = this.transform.Find("Sprite");
        if (spriteTransform == null) return;

        this.firepoints.Clear();
        for (int i = 0; i < spriteTransform.childCount; i++)
        {
            Transform firepointTransform = spriteTransform.GetChild(i);
            this.firepoints.Add(firepointTransform);
        }
    }

    private void InitializeMuzzleFlashes()
    {
        this.muzzleFlashes.Clear();
        foreach (Transform firepoint in this.firepoints)
        {
            ParticleSystem muzzleFlash = firepoint.GetComponentInChildren<ParticleSystem>();
            this.muzzleFlashes.Add(muzzleFlash);
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
        if (OnShootStart != null) OnShootStart.Invoke();
    }

    public void StopShooting()
    {
        shooting = false;
        if (OnShootEnd != null) OnShootEnd.Invoke();
    }

    public void TryShootOnce()
    {
        if (!workWithRealTime && Time.time - lastAttack <= this.runtimeStats.AttackCooldown) return;
        else if (workWithRealTime && Time.realtimeSinceStartup - lastAttackRealTime <= this.runtimeStats.AttackCooldown) return;

        lastAttack = Time.time;
        lastAttackRealTime = Time.realtimeSinceStartup;

        Shoot();

        if (onShoot != null) onShoot.Invoke();
    }

    private void Shoot()
    {
        for (int i = 0; i < this.firepoints.Count; i++)
        {
            Transform firepoint = this.firepoints[i];
            ShootFromFirepoint(firepoint);

            // Trigger animation
            PlayShootAnimation();

            // Play muzzle flash effect
            ParticleSystem muzzleFlash = this.muzzleFlashes[i];
            if (muzzleFlash != null) muzzleFlash.Play();
        }

        // Play sound effect
        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSound();
    }

    private void ShootFromFirepoint(Transform firepoint, float knockbackMultiplier=1)
    {
        float fan = this.runtimeStats.Fan;
        float split = this.runtimeStats.Split;
        if (fan % 2 == 1)
        {
            CreateBulletGroup(firepoint, split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 0, knockbackMultiplier);
            fan--;
            while (fan > 0)
            {
                CreateBulletGroup(firepoint, split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 45, knockbackMultiplier);
                fan--;
                CreateBulletGroup(firepoint, split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, -45, knockbackMultiplier);
                fan--;
            }
        }
        else
        {
            while (fan > 0)
            {
                CreateBulletGroup(firepoint, split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, 22.5f, knockbackMultiplier);
                fan--;
                CreateBulletGroup(firepoint, split, this.runtimeStats.Range / this.runtimeStats.BulletVelocity, this.runtimeStats.BulletVelocity, this.runtimeStats.BulletSize, this.runtimeStats.Pierce, this.runtimeStats.Damage, -22.5f, knockbackMultiplier);
                fan--;
            }
        }
    }

    private void CreateBulletGroup(Transform firepoint, float split, float airTime, float bulletSpeed, float bulletSize, int pierce, float damage, float rotation, float knockbackMultiplier=1)
    {
        if (split % 2 != 0)
        {
            CreateBullet(firepoint, airTime, bulletSpeed, bulletSize, pierce, damage, firepoint.position, rotation, knockbackMultiplier);
            split--;
            int counter = 1;
            while (split != 0)
            {
                CreateBullet(firepoint, airTime, bulletSpeed, bulletSize, pierce, damage, firepoint.position + new Vector3(firepoint.up.y, -firepoint.up.x, 0) * 0.5f * counter * bulletSize, rotation, knockbackMultiplier);
                split--;
                if (split != 0)
                {
                    CreateBullet(firepoint, airTime, bulletSpeed, bulletSize, pierce, damage, firepoint.position + new Vector3(-firepoint.up.y, firepoint.up.x, 0) * 0.5f * counter * bulletSize, rotation, knockbackMultiplier);
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
                CreateBullet(firepoint, airTime, bulletSpeed, bulletSize, pierce, damage, firepoint.position + new Vector3(firepoint.up.y, -firepoint.up.x, 0) * 0.25f + new Vector3(firepoint.up.y, -firepoint.up.x, 0) * 0.5f * counter * bulletSize, rotation, knockbackMultiplier);
                split--;
                if (split != 0)
                {
                    CreateBullet(firepoint, airTime, bulletSpeed, bulletSize, pierce, damage, firepoint.position + new Vector3(-firepoint.up.y, firepoint.up.x, 0) * 0.25f + new Vector3(firepoint.up.y, -firepoint.up.x, 0) * 0.5f * counter * bulletSize, rotation, knockbackMultiplier);
                    split--;
                }
                counter++;
            }
        }
    }

    private void CreateBullet(Transform firepoint, float airTime, float bulletSpeed, float bulletSize, int pierce, float damage, Vector3 position, float rotation, float knockbackMultiplier=1)
    {
        Bullet bullet = BulletManager.Instance.TryGetBullet();
        if (bullet == null) return;
        bullet.AssignOnComplete(() => BulletManager.Instance.ReturnBullet(bullet));

        Quaternion finalRotation = Quaternion.Euler(0, 0, firepoint.eulerAngles.z + rotation);
        bullet.Initialize(owner.tag, owner.gameObject, position, finalRotation, new Vector3(bulletSize, bulletSize, 1), damage, airTime, bulletSpeed, pierce, bulletColor);
        if (this.runtimeStats.Explode) bullet.InitializeSplitting(this.runtimeStats.ExplodeBulletAmount, this.runtimeStats.ExplodeBulletRange, this.runtimeStats.ExplodeBulletSize, this.runtimeStats.ExplodeBulletVelocity, this.runtimeStats.ExplodeDamagePercentage);
        bullet.Shoot();

        // Give Knockback to self
        Vector2 direction = -(Vector2)(finalRotation * Vector3.up).normalized;
        float baseKnockbackForce = 20;
        float velocity = bulletSpeed;

        Vector2 knockbackForce = direction.normalized * knockbackMultiplier * (
            baseKnockbackForce +
            0.4f * damage +
            0.4f * velocity
        );
        if (this.owner != null) this.owner.AddKnockback(knockbackForce);
    }

    public void ShootBullet(float range, float bulletSpeed, float bulletSize, int pierce, float damage, float knockbackMultiplier)
    {
        for (int i = 0; i < this.firepoints.Count; i++)
        {
            Transform firepoint = this.firepoints[i];
            CreateBullet(firepoint, range / bulletSpeed, bulletSpeed, bulletSize, pierce, damage, firepoint.position, 0, knockbackMultiplier);

            // Trigger animation
            PlayShootAnimation();

            // Play muzzle flash effect
            ParticleSystem muzzleFlash = this.muzzleFlashes[i];
            if (muzzleFlash != null) muzzleFlash.Play();
        }
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

    public bool IsShootable(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        if (distance > this.runtimeStats.Range) return false;

        Vector3 direction = (to - from).normalized;
        float radius = (this.runtimeStats.BulletSize / 2.66f) / 2; // A bullet is 6 pixels, which means that 2.66 bullets is 1 in game distance unit, then devide to get radius.
        LayerMask obstacleLayerMask = LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.CircleCast(from, radius, direction, distance, obstacleLayerMask);
        return hit.collider == null;
    }

    protected bool RaycastContainsWall(RaycastHit2D[] rays)
    {
        foreach (RaycastHit2D ray in rays)
        {
            if (ray.transform.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}
