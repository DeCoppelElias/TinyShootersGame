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
    public GameObject owner;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private System.Action onComplete;

    private ParticleSystem trailParticleSystem;
    private ParticleSystem.MainModule mainModule;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (this.spriteRenderer) spriteRenderer.color = color;

        rb = GetComponent<Rigidbody2D>();

        this.trailParticleSystem = this.transform.Find("Trail").GetComponent<ParticleSystem>();
        this.mainModule = this.trailParticleSystem.main;
    }

    public void AssignOnComplete(System.Action action)
    {
        this.onComplete = action;
    }

    public void Initialize(string ownerTag, GameObject owner, Vector3 position, Quaternion rotation, Vector3 scale, float damage, float airTime, float velocity, int pierce, Color color)
    {
        this.ownerTag = ownerTag;
        this.owner = owner;

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

        if (this.trailParticleSystem == null) this.trailParticleSystem = this.transform.Find("Trail").GetComponent<ParticleSystem>();
        if (this.trailParticleSystem != null)
        {
            this.mainModule = this.trailParticleSystem.main;
            this.mainModule.startColor = color;

            float scaleChange = 1 + ((scale.x - 1) / 2f);
            ParticleSystem.MinMaxCurve lifetimeCurve = new ParticleSystem.MinMaxCurve(0.2f * scaleChange, 0.2f * scaleChange);
            this.mainModule.startLifetime = lifetimeCurve;

            ParticleSystem.MinMaxCurve sizeCurve = new ParticleSystem.MinMaxCurve(0.1f * scaleChange, 0.1f * scaleChange);
            this.mainModule.startSize = sizeCurve;
        }
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
        // rb.AddForce(transform.up * velocity, ForceMode2D.Impulse);
        rb.velocity = transform.up * velocity;
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
            ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.BulletExplosion, this.transform.position, Quaternion.identity, this.transform.localScale, this.color);
            Complete();
            return;
        }

        if (collision.CompareTag("Object"))
        {
            Object obj = collision.GetComponent<Object>();
            if (obj != null) obj.OnBulletHit(damage, rb.velocity.normalized);

            ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.BulletExplosion, this.transform.position, Quaternion.identity, this.transform.localScale, this.color);
            Complete();
            return;
        }

        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && !GameObject.ReferenceEquals(owner, collision.gameObject) && (ownerTag != collision.tag || ownerTag == "Player" && collision.tag == "Player") && entity.Health > 0)
        {
            Vector2 direction = (entity.transform.position - this.transform.position).normalized;
            float baseKnockbackForce = 30;
            float velocity = Vector2.Dot(rb.velocity, direction);

            Vector2 knockbackForce = direction.normalized * (
                baseKnockbackForce +
                0.4f * damage +
                0.4f * velocity
            );

            entity.TakeDamage(damage, ownerTag, Entity.DamageType.Ranged, knockbackForce);

            pierce--;
            if (pierce == 0)
            {
                ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.BulletExplosion, this.transform.position, Quaternion.identity, this.transform.localScale, this.color);
                Complete();
            }
            else if (splitOnHit)
            {
                Split();
            }

            return;
        }
    }

    private void Split()
    {
        if (splitAmount > 0)
        {
            Debug.Log(splitAmount);
            float angleInterval = 360f / splitAmount;
            float startAngle = (splitAmount % 2 == 0) ? angleInterval / 2f : 0f;
            for (int i = 0; i < splitAmount; i++)
            {
                float currentAngle = startAngle + i * angleInterval;
                CreateSplitBullet(currentAngle);
            }
        }
    }

    private void CreateSplitBullet(float angle)
    {
        Bullet bullet = BulletManager.Instance.TryGetBullet();
        if (bullet == null) return;
        bullet.AssignOnComplete(() => BulletManager.Instance.ReturnBullet(bullet));

        Vector3 direction = (Quaternion.Euler(0, 0, angle) * Vector3.up).normalized;
        bullet.Initialize(
            this.ownerTag, 
            this.owner,
            transform.position + direction * 0.1f, 
            Quaternion.Euler(0, 0, angle), 
            new Vector3(splitBulletSize, splitBulletSize, 1), 
            damage * splitDamagePercentage, splitRange / splitBulletSpeed, 
            splitBulletSpeed, 1, 
            this.color);
        bullet.Shoot();
    }
}
 