using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float airTime;
    public float velocity;
    public int pierce;
    
    private enum BulletState { Idle, Initialized, Shoot}
    private BulletState bulletState = BulletState.Idle;
    private float shootTime;

    public bool splitOnHit = false;
    public float splitAmount = 0;
    public float splitRange = 1;
    public float splitBulletSize = 0.5f;
    public float splitBulletSpeed = 6;
    public float splitDamagePercentage = 0.5f;

    public Color color;

    public bool reflected = false;

    public string ownerTag;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private System.Action onComplete;
    private BulletManager bulletManager;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (this.spriteRenderer) spriteRenderer.color = color;

        rb = GetComponent<Rigidbody2D>();
        bulletManager = GameObject.Find("Bullets").GetComponent<BulletManager>();
    }

    public void AssignOnComplete(System.Action action)
    {
        this.onComplete = action;
    }

    public void Initialize(string ownerTag, Vector3 position, Quaternion rotation, Vector3 scale, float damage, float airTime, float velocity, int pierce, Color color)
    {
        this.ownerTag = ownerTag;

        this.transform.position = position;
        this.transform.rotation = rotation;
        this.transform.localScale = scale;

        this.damage = damage;
        this.airTime = airTime;
        this.velocity = velocity;
        this.pierce = pierce;

        this.color = color;
        if (this.spriteRenderer) spriteRenderer.color = color;

        this.splitOnHit = false;

        bulletState = BulletState.Initialized;
    }

    public void InitializeSplitting(float splitAmount, float splitRange, float splitBulletSize, float splitBulletSpeed, float splitDamagePercentage)
    {
        this.splitOnHit = true;

        this.splitAmount = splitAmount;
        this.splitRange = splitRange;
        this.splitBulletSize = splitBulletSize;
        this.splitBulletSpeed = splitBulletSpeed;
        this.splitDamagePercentage = splitDamagePercentage;
    }

    public void Shoot()
    {
        if (bulletState != BulletState.Initialized) throw new System.Exception("Bullet can only be fired when in initialized state.");

        shootTime = Time.time;
        bulletState = BulletState.Shoot;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * velocity, ForceMode2D.Impulse);
    }

    private void Complete()
    {
        if (splitOnHit)
        {
            Split();
        }

        bulletState = BulletState.Idle;
        if (onComplete != null) onComplete();
    }

    public void RemoveBullet()
    {
        bulletState = BulletState.Idle;
        onComplete();
    }

    private void Update()
    {
        if (bulletState == BulletState.Shoot)
        {
            float p = Mathf.Clamp((Time.time - shootTime - (0.8f * airTime)) / (0.2f * airTime), 0, 1);
            Color originalColor = spriteRenderer.color;
            originalColor.a = 1f - p;
            spriteRenderer.color = originalColor;

            if (Time.time - shootTime > airTime)
            {
                Complete();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Complete();
            return;
        }

        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && ownerTag != collision.tag && entity.Health > 0)
        {
            entity.TakeDamage(damage, ownerTag, Entity.DamageType.Ranged, rb.velocity.normalized);

            pierce--;
            if (pierce == 0)
            {
                Complete();
            }
            else if (splitOnHit)
            {
                Split();
            }
        }
    }

    private void Split()
    {
        if (splitAmount > 0)
        {
            float angleInterval = 360 / splitAmount;
            float currentAngle = 0;
            for (int i = 0; i < splitAmount; i++)
            {
                CreateSplitBullet(currentAngle);
                currentAngle += angleInterval;
            }
        }
    }

    private void CreateSplitBullet(float angle)
    {
        Bullet bullet = bulletManager.TryGetBullet();
        if (bullet == null) return;
        bullet.AssignOnComplete(() => bulletManager.ReturnBullet(bullet));

        Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
        bullet.Initialize(this.ownerTag, transform.position + direction / 3, Quaternion.Euler(0, 0, angle), new Vector3(splitBulletSize, splitBulletSize, 1), damage * splitDamagePercentage, splitRange / splitBulletSpeed, splitBulletSpeed, 1, this.color);
        bullet.Shoot();
    }
}
 