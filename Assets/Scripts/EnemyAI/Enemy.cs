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

    [Header("On-Hit Color Change")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float colorChangeDuration = 1f;
    [SerializeField] private ColorChangeState colorChangeState = ColorChangeState.Nothing;
    private enum ColorChangeState { Nothing, ToDamageColor, ToOriginalColor }
    private SpriteRenderer spriteRenderer;
    private float startColorChange = 0;
    private Color originalColor;

    [Header("Debug Settings")]
    [SerializeField] private bool debug = false;

    public override void StartEntity()
    {
        base.StartEntity();

        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        targetPlayer = FindClosestPlayer();

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) throw new System.Exception("Cannot find size because collider is missing");
        size = collider.bounds.size.x;

        colorChangeState = ColorChangeState.Nothing;
    }

    private Player FindClosestPlayer()
    {
        lastPlayerRefresh = Time.time;
        Player[] players = Object.FindObjectsOfType<Player>();

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
        ColorUpdate();
    }

    private void RefreshPlayerUpdate()
    {
        if (Time.time - lastPlayerRefresh > refreshPlayerTargetCooldown)
        {
            this.targetPlayer = FindClosestPlayer();
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

    private void StartColorChange()
    {
        if (colorChangeState == ColorChangeState.Nothing) originalColor = spriteRenderer.color;
        startColorChange = Time.time;
        colorChangeState = ColorChangeState.ToDamageColor;
    }

    public override void TakeDamage(float amount, string sourceTag, DamageType damageType)
    {
        base.TakeDamage(amount, sourceTag, damageType);

        StartColorChange();
    }

    public Player GetTargetPlayer()
    {
        return this.targetPlayer;
    }
}
