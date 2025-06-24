using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageParticle : MonoBehaviour
{
    private float duration;
    private float startTime;
    private SpriteRenderer spriteRenderer;
    private bool start = false;
    private Vector3 direction;
    private float velocity;

    // Update is called once per frame
    void Update()
    {
        if (!start) return;

        // Setting transparancy
        float p = Mathf.Clamp((Time.time - startTime) / duration, 0, 1);
        float a = 1-p;
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;

        // Movement
        transform.position += velocity * Time.deltaTime * direction;

        if (p == 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialise(float duration, Vector2 direction, float velocity, Color color)
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;

        this.startTime = Time.time;
        this.duration = duration;

        this.direction = direction;
        this.velocity = velocity;

        this.start = true;
    }
}
