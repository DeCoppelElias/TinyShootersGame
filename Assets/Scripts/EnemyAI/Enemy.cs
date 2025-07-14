using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Target Player Settings")]
    [SerializeField] protected Player targetPlayer;
    [SerializeField] private float refreshPlayerTargetCooldown = 2;
    private float lastPlayerRefresh = 0;

    [Header("Collider Size")]
    protected float size = 0;

    [Header("On-Death Body Settings")]
    private float bodyDuration = 0.25f;
    [SerializeField] protected bool spawnBody = true;

    [Header("Debug Settings")]
    [SerializeField] private bool debug = false;

    public override void StartEntity()
    {
        base.StartEntity();

        targetPlayer = FindClosestPlayer();

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) throw new System.Exception("Cannot find size because collider is missing");
        size = collider.bounds.size.x;
    }

    private Player FindClosestPlayer()
    {
        lastPlayerRefresh = Time.time;
        Player[] players = UnityEngine.Object.FindObjectsOfType<Player>();

        Player closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Player player in players)
        {
            float distance = Vector3.Distance(this.transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        RefreshPlayerUpdate();
    }

    private void RefreshPlayerUpdate()
    {
        if (Time.time - lastPlayerRefresh > refreshPlayerTargetCooldown)
        {
            this.targetPlayer = FindClosestPlayer();
        }
    }

    public Player GetTargetPlayer()
    {
        return this.targetPlayer;
    }

    public override void OnDeath()
    {
        base.OnDeath();

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

        // Display score
        // DisplayScore();

        // Spawn Body
        if (spawnBody)
        {
            Transform spriteTransform = transform.Find("Sprite");
            spriteRenderer.color = this.originalColor;
            spriteRenderer.sortingOrder = -1;
            spriteTransform.SetParent(transform.parent);
            DeadBodyBehaviour deadBody = spriteTransform.gameObject.AddComponent<DeadBodyBehaviour>();
            deadBody.Initialise(bodyDuration, lastDamageDirection);
        }

        Destroy(this.gameObject);
    }

    /*private void DisplayScore()
    {
        if (particleManager == null || particleManager.scoreTextPrefab == null) return;

        var scoreDisplay = Instantiate(particleManager.scoreTextPrefab, transform.position, Quaternion.identity, particleManager.particleParent);
        scoreDisplay.GetComponent<ScoreDisplay>().Initialise(1f, this.onDeathScore);
    }*/
}
