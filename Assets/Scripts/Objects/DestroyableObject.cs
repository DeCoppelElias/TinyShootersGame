using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DestroyableObject : Object
{
    [SerializeField] private List<Sprite> destroySprites = new List<Sprite>();
    private int currentSpriteIndex = -1;

    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();

        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public override void OnBulletHit(float damage, Vector3 direction)
    {
        base.OnBulletHit(damage, direction);

        // Change Sprite
        currentSpriteIndex++;
        Sprite newSprite = this.destroySprites[currentSpriteIndex];
        this.spriteRenderer.sprite = newSprite;

        if (currentSpriteIndex >= destroySprites.Count - 1)
        {
            this.active = false;
            this.rb.bodyType = RigidbodyType2D.Static;
            Collider2D colider = this.GetComponent<Collider2D>();
            colider.enabled = false;
        }
    }
}
