using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public abstract class Entity : MonoBehaviour
{
    [Header("Basic Entity Stats")]
    [SerializeField] protected EntityState entityState = EntityState.Alive;
    protected enum EntityState { Alive, Dead }

    public UnityEvent onDeath;

    [SerializeField] public System.Guid EntityID { get; private set; }

    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health = 100;

    [SerializeField] private float contactDamage = 10;
    [SerializeField] private float contactHitCooldown = 1f;

    [SerializeField] private int healOnMeleeKill = 0;
    private Dictionary<System.Guid, float> contactHits = new Dictionary<System.Guid, float>();

    public bool knockbackImmune = false;

    public int onDeathScore = 100;
    [SerializeField] protected Color color;

    [Header("On-Hit Color Change Settings")]
    [SerializeField] protected Color damageColor = Color.red;
    [SerializeField] private float colorChangeDuration = 0.25f;
    [SerializeField] private ColorChangeState colorChangeState = ColorChangeState.Nothing;
    private enum ColorChangeState { Nothing, ToDamageColor, ToOriginalColor }
    protected SpriteRenderer spriteRenderer;
    private float startColorChange = 0;
    protected Color originalColor;

    [Header("Out Of Bounds Settings")]
    public Vector3 lastValidPosition;
    [SerializeField] private float updateValidPositionCooldown = 2f;
    private float lastValidPositionUpdate = 0;
    [SerializeField] private bool outOfBounds = false;
    [SerializeField] private float allowedOutOfBoundsDuration = 1f;
    private float outOfBoundsStart = 0;
    private int outOfBoundsCounter = 0;

    // [Header("Contact Knockback Settings")]
    private int baseContactKnockback = 100;
    private float velocityKnockbackMultiplier = 10;
    private float damageKnockbackMultiplier = 1.1f;

    public enum DamageType { Ranged, Melee }
    protected DamageType lastDamageType;
    protected string lastDamageSourceTag;
    protected Vector2 lastDamageDirection;

    private WaveManager waveManager;

    private Transform healthBar;
    private Transform emptyHealthBar;

    private Tilemap walls;
    private Tilemap pits;

    protected Rigidbody2D rb;

    private void Awake()
    {
        EntityID = System.Guid.NewGuid();
    }

    private void Start()
    {
        walls = GameObject.Find("Walls")?.GetComponent<Tilemap>();
        pits = GameObject.Find("Pits")?.GetComponent<Tilemap>();
        waveManager = GameObject.Find("WaveManager")?.GetComponent<WaveManager>();
        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        emptyHealthBar = transform.Find("EmptyHealthBar");
        healthBar = emptyHealthBar.GetChild(0);

        entityState = EntityState.Alive;
        colorChangeState = ColorChangeState.Nothing;

        rb = this.GetComponent<Rigidbody2D>();

        if (CheckOutOfBounds()) lastValidPosition = this.transform.position;

        StartEntity();

        OnColorChange(this.color);
    }
    private void Update()
    {
        if (entityState == EntityState.Dead) return;

        UpdateHealthBar();
        EnforceValidPosition();
        ColorUpdate();

        UpdateEntity();
    }

    #region Properties
    public float MaxHealth
    {
        get { return maxHealth; }
        set 
        {
            if (value < 1) maxHealth = 1;
            else maxHealth = value;
        }
    }

    public float Health
    {
        get { return health; }
        set
        {
            if (value <= 0)
            {
                health = 0;
                if (entityState == EntityState.Alive) OnDeath();
            }
            else health = Mathf.Min(value, MaxHealth);
        }
    }

    public float ContactDamage 
    {
        get { return contactDamage; }
        set 
        {
            if (value < 0) contactDamage = 0;
            else contactDamage = value;
        }
    }
    public float ContactHitCooldown 
    { 
        get { return contactHitCooldown; }
        set
        {
            if (value < 0.01f) contactHitCooldown = 0.01f;
            else contactHitCooldown = value;
        }
    }

    public Color Color { 
        get => color;
        set
        {
            color = value;
            OnColorChange(value);
        }
    }

    public int HealOnMeleeKill
    {
        get => healOnMeleeKill;
        set
        {
            healOnMeleeKill = Mathf.Max(0, value);
        }
    }
    #endregion

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float scale = Health / MaxHealth;
        healthBar.localScale = new Vector3(scale, 1, 1);
    }


    public virtual void OnDeath()
    {
        this.entityState = EntityState.Dead;

        // visual effects
        ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.Damage, transform.position, Quaternion.identity, this.transform.localScale, damageColor, 10);
        ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.Blood, transform.position, Quaternion.identity, this.transform.localScale, damageColor);

        // sound
        AudioManager.Instance.PlayDieSound();

        // Callback
        if (onDeath != null) onDeath.Invoke();
    }

    public virtual void UpdateEntity()
    {

    }

    public virtual void StartEntity()
    {

    }

    public virtual void TakeDamage(float damage, string sourceTag, DamageType damageType, Vector2 knockback)
    {
        if (damage <= 0) return;

        lastDamageSourceTag = sourceTag;
        lastDamageType = damageType;
        lastDamageDirection = knockback.normalized;

        this.Health -= damage;

        // Give knockback
        AddKnockback(knockback);

        // Color change
        StartColorChange();

        // Create damage particles
        int damageParticleAmount = UnityEngine.Random.Range(1, 3);
        ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.Damage, transform.position, Quaternion.identity, this.transform.localScale, damageColor, damageParticleAmount);

        // Create blood
        float r = UnityEngine.Random.Range(0, 1f);
        if (r < 0.05f) ParticleManager.Instance.CreateParticle(ParticleManager.ParticleType.Blood, transform.position, Quaternion.identity, this.transform.localScale, damageColor);

        // Play damage sound effect
        AudioManager.Instance.PlayDamageSound();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // An entity comes in contact with a player => player takes damage.
        // An enemy comes in contact with a player => enemy takes damage.
        // An enemy comes in contact with an other enemy => no damage is taken.
        if (collision.transform.CompareTag("Enemy") && this.transform.CompareTag("Enemy")) return;
        if (entityState == EntityState.Dead) return;

        Entity other = collision.gameObject.GetComponent<Entity>();
        if (other != null && Time.time - LastContactHit(other) > ContactHitCooldown)
        {
            AddContactHit(other);

            Vector2 direction = (collision.transform.position - this.transform.position).normalized;
            float damage = contactDamage;
            float velocity = Vector2.Dot(rb.velocity, direction);

            Vector2 knockbackForce = direction.normalized * (
                baseContactKnockback +
                other.damageKnockbackMultiplier * damage +
                other.velocityKnockbackMultiplier * velocity
            );

            other.TakeDamage(contactDamage, this.tag, DamageType.Melee, knockbackForce);

            // Possibly heal when entity is killed
            if (other.entityState == EntityState.Dead) this.Health += healOnMeleeKill;
        }
    }

    private float LastContactHit(Entity other)
    {
        if (contactHits.ContainsKey(other.EntityID)) return contactHits[other.EntityID];
        return 0;
    }

    private void AddContactHit(Entity other)
    {
        if (contactHits.ContainsKey(other.EntityID)) contactHits[other.EntityID] = Time.time;
        else
        {
            contactHits.Add(other.EntityID, Time.time);
            CleanupOldContactHits();
        }
    }

    private void CleanupOldContactHits()
    {
        Debug.Log("cleaning up old contact hits. Before: " + contactHits.Count.ToString());
        var expired = new List<System.Guid>();
        foreach (var pair in contactHits)
        {
            if (Time.time - pair.Value > ContactHitCooldown)
                expired.Add(pair.Key);
        }

        foreach (var e in expired)
            contactHits.Remove(e);
        Debug.Log("After: " + contactHits.Count.ToString());
    }

    private bool CheckOutOfBounds()
    {
        if (walls != null && walls.GetTile(Vector3Int.FloorToInt(this.transform.position)) != null) return true;
        if (pits != null && pits.GetTile(Vector3Int.FloorToInt(this.transform.position)) != null) return true;
        if (waveManager != null && !waveManager.InsideLevel(this.transform.position)) return true;

        return false;
    }

    private void EnforceValidPosition()
    {
        if (CheckOutOfBounds())
        {
            if (!outOfBounds)
            {
                outOfBounds = true;
                outOfBoundsStart = Time.time;
            }
            else
            {
                if (Time.time - outOfBoundsStart > allowedOutOfBoundsDuration)
                {
                    if (outOfBoundsCounter < 3)
                    {
                        this.transform.position = lastValidPosition;
                        outOfBoundsCounter += 1;
                    }
                    else
                    {
                        Vector3 safePosition = new Vector3(0, 0, 0);
                        if (waveManager != null)
                        {
                            safePosition = waveManager.GetSafePosition();
                        }
                        
                        this.transform.position = safePosition;
                        outOfBoundsCounter = 0;
                    }
                    this.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
                    outOfBounds = false;
                }
            }
        }
        else
        {
            outOfBounds = false;

            if (Time.time - lastValidPositionUpdate > updateValidPositionCooldown)
            {
                lastValidPosition = this.transform.position;
                lastValidPositionUpdate = Time.time;
            }
        }
    }

    private void ColorUpdate()
    {
        if (colorChangeState == ColorChangeState.ToDamageColor)
        {
            float p = Mathf.Clamp((Time.time - startColorChange) / (0.5f * colorChangeDuration), 0, 1);
            spriteRenderer.color = Color.Lerp(originalColor, damageColor, p);

            if (p == 1)
            {
                startColorChange = Time.time;
                colorChangeState = ColorChangeState.ToOriginalColor;
            }
        }
        else if (colorChangeState == ColorChangeState.ToOriginalColor)
        {
            float p = Mathf.Clamp((Time.time - startColorChange) / (0.5f * colorChangeDuration), 0, 1);
            spriteRenderer.color = Color.Lerp(damageColor, originalColor, p);

            if (p == 1)
            {
                colorChangeState = ColorChangeState.Nothing;
            }
        }
    }

    protected void StartColorChange()
    {
        if (colorChangeState == ColorChangeState.Nothing) originalColor = spriteRenderer.color;
        startColorChange = Time.time;
        colorChangeState = ColorChangeState.ToDamageColor;
    }

    public virtual void AddKnockback(Vector2 force)
    {
        if (knockbackImmune) return;
        if (rb != null) rb.AddForce(force, ForceMode2D.Impulse);
    }

    public virtual void Revive()
    {
        if (this.entityState != EntityState.Dead) return;

        this.health = maxHealth;
        this.entityState = EntityState.Alive;
    }

    protected virtual void OnColorChange(Color newColor)
    {

    }
}
