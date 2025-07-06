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

    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health = 100;

    [SerializeField] private float contactDamage = 10;
    [SerializeField] private float contactHitCooldown = 1f;
    private float lastContactHit = 0;

    public int onDeathScore = 100;

    [Header("On-Hit Color Change Settings")]
    [SerializeField] private Color damageColor = Color.red;
    private float colorChangeDuration = 0.25f;
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
    
    public enum DamageType { Ranged, Melee }
    protected DamageType lastDamageType;
    protected string lastDamageSourceTag;
    protected Vector2 lastDamageDirection;

    protected AudioManager audioManager;
    private WaveManager waveManager;

    private Transform healthBar;
    private Transform emptyHealthBar;

    private Tilemap walls;
    private Tilemap pits;

    protected Rigidbody2D rb;

    private void Start()
    {
        walls = GameObject.Find("Walls")?.GetComponent<Tilemap>();
        pits = GameObject.Find("Pits")?.GetComponent<Tilemap>();
        audioManager = GameObject.Find("AudioManager")?.GetComponent<AudioManager>();
        waveManager = GameObject.Find("WaveManager")?.GetComponent<WaveManager>();
        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        emptyHealthBar = transform.Find("EmptyHealthBar");
        healthBar = emptyHealthBar.GetChild(0);

        entityState = EntityState.Alive;
        colorChangeState = ColorChangeState.Nothing;

        rb = this.GetComponent<Rigidbody2D>();

        if (CheckOutOfBounds()) lastValidPosition = this.transform.position;

        StartEntity();
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
                OnDeath();
            }
            else health = value;
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
    #endregion

    private void UpdateHealthBar()
    {
        float scale = Health / MaxHealth;
        healthBar.localScale = new Vector3(scale, 1, 1);
    }


    public virtual void OnDeath()
    {
        this.entityState = EntityState.Dead;
    }

    public virtual void UpdateEntity()
    {

    }

    public virtual void StartEntity()
    {

    }

    public virtual void TakeDamage(float amount, string sourceTag, DamageType damageType, Vector2 direction)
    {
        if (amount <= 0) return;

        this.Health -= amount;

        lastDamageSourceTag = sourceTag;
        lastDamageType = damageType;
        lastDamageDirection = direction;

        StartColorChange();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // An entity comes in contact with a player => player takes damage.
        // An enemy comes in contact with a player => enemy takes damage.
        // An enemy comes in contact with an other enemy => no damage is taken.
        if (collision.transform.CompareTag("Enemy") && this.transform.CompareTag("Enemy")) return;

        Entity entity = collision.gameObject.GetComponent<Entity>();
        if (entity != null && Time.time - lastContactHit > ContactHitCooldown)
        {
            lastContactHit = Time.time;

            Vector2 direction = (collision.transform.position - this.transform.position).normalized;
            entity.TakeDamage(contactDamage, this.tag, DamageType.Melee, direction);
        }
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

    public virtual void AddKnockback(float force, Vector3 direction)
    {
        if (rb != null) rb.AddForce(100 * force * direction, ForceMode2D.Impulse);
    }
}
