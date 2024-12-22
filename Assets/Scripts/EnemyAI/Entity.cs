using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public abstract class Entity : MonoBehaviour
{
    [Header("Basic Entity Stats")]
    public float maxHealth = 100;
    public float health = 100;
    public float damage = 10;
    public float moveSpeed = 3;

    public float contactDamage = 10;
    public float contactHitCooldown = 1f;
    private float lastContactHit = 0;

    public int onDeathScore = 100;

    [Header("Out Of Bounds Settings")]
    public Vector3 lastValidPosition;
    [SerializeField] private float updateValidPositionCooldown = 2f;
    private float lastValidPositionUpdate = 0;
    [SerializeField] private bool outOfBounds = false;
    [SerializeField] private float allowedOutOfBoundsDuration = 1f;
    private float outOfBoundsStart = 0;
    private int outOfBoundsCounter = 0;

    public enum DamageType { Ranged, Melee }
    private DamageType lastDamageType;
    private string lastDamageSourceTag;

    protected AudioManager audioManager;
    private WaveManager waveManager;

    private Transform healthBar;
    private Transform emptyHealthBar;

    private Tilemap walls;
    private Tilemap pits;

    private void Start()
    {
        walls = GameObject.Find("Walls").GetComponent<Tilemap>();
        pits = GameObject.Find("Pits").GetComponent<Tilemap>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();

        emptyHealthBar = transform.Find("EmptyHealthBar");
        healthBar = emptyHealthBar.GetChild(0);

        if (CheckOutOfBounds()) lastValidPosition = this.transform.position;

        StartEntity();
    }
    private void Update()
    {
        if (health <= 0)
        {
            OnDeath();
            return;
        }

        UpdateHealthBar();

        EnforceValidPosition();

        UpdateEntity();
    }

    private void UpdateHealthBar()
    {
        float scale = health / maxHealth;
        healthBar.localScale = new Vector3(scale, 1, 1);
    }


    public virtual void OnDeath()
    {
        // Give on death score to last damage source
        if (lastDamageSourceTag == "Player")
        {
            GameObject scoreManagerObj = GameObject.Find("ScoreManager");
            if (scoreManagerObj != null)
            {
                ScoreManager scoreManager = scoreManagerObj.GetComponent<ScoreManager>();
                scoreManager.AddScore(ScoreManager.ScoreReason.EnemyKill, this.onDeathScore);
                if (lastDamageType == DamageType.Melee) scoreManager.AddScore(ScoreManager.ScoreReason.MeleeKill, this.onDeathScore * 2);
            }
        }

        // Play death sound
        audioManager.PlayDieSound();

        Destroy(this.gameObject);
    }

    public virtual void UpdateEntity()
    {

    }

    public virtual void StartEntity()
    {

    }

    public virtual void TakeDamage(float amount, string sourceTag, DamageType damageType)
    {
        if (amount <= 0) return;

        this.health -= amount;

        lastDamageSourceTag = sourceTag;
        lastDamageType = damageType;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // An entity comes in contact with a player => player takes damage.
        // An enemy comes in contact with a player => enemy takes damage.
        // An enemy comes in contact with an other enemy => no damage is taken.
        if (collision.transform.CompareTag("Enemy") && this.transform.CompareTag("Enemy")) return;

        Entity entity = collision.gameObject.GetComponent<Entity>();
        if (entity != null && Time.time - lastContactHit > contactHitCooldown)
        {
            lastContactHit = Time.time;

            entity.TakeDamage(contactDamage, this.tag, DamageType.Melee);
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
}
